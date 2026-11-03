// External acceptance tool, never linked into or shipped with the application.
// XRes local-client credentials establish PID ownership; titles and _NET_WM_PID
// are not an ownership oracle (Mono does not publish _NET_WM_PID).
#include <sys/types.h>
#include <X11/Xlib.h>
#include <X11/Xatom.h>
#include <X11/Xutil.h>
#include <X11/extensions/XRes.h>
#include <errno.h>
#include <limits.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

static int xerror;
static unsigned visited;
#define MAX_VISITS 8192
#define MAX_NAME_BYTES 4096
typedef struct {
    Window window, parent;
    int map_state, x, y, width, height, can_close;
    char *name;
} WindowRecord;
static WindowRecord records[MAX_VISITS];
static unsigned record_count;
static int on_error(Display *display, XErrorEvent *event)
{
    (void)display;
    xerror = event->error_code;
    return 0;
}

static pid_t owner_pid(Display *display, Window window)
{
    XResClientIdSpec spec = { window, XRES_CLIENT_ID_PID_MASK };
    XResClientIdValue *values = NULL;
    long count = 0;
    pid_t pid = -1;
    if (XResQueryClientIds(display, 1, &spec, &count, &values) == Success) {
        for (long index = 0; index < count; index++) {
            pid_t candidate = XResGetClientPid(&values[index]);
            if (candidate > 0) {
                if (pid > 0 && pid != candidate) { pid = -1; break; }
                pid = candidate;
            }
        }
    }
    if (values) XResClientIdsDestroy(count, values);
    return pid;
}

static int closable(Display *display, Window window, Atom deletion)
{
    Atom *protocols = NULL;
    int count = 0, found = 0;
    if (XGetWMProtocols(display, window, &protocols, &count))
        for (int index = 0; index < count; index++)
            if (protocols[index] == deletion) found = 1;
    if (protocols) XFree(protocols);
    return found;
}

static void json_string(const char *text)
{
    putchar('"');
    for (const unsigned char *p = (const unsigned char *)(text ? text : ""); *p; p++) {
        if (*p == '"' || *p == '\\') printf("\\%c", *p);
        else if (*p < 32 || *p >= 127) printf("\\u%04x", *p);
        else putchar(*p);
    }
    putchar('"');
}

static int read_name(Display *display, Window window, char **name)
{
    Atom type = None;
    int format = 0;
    unsigned long count = 0, remaining = 0;
    unsigned char *value = NULL;
    *name = NULL;
    int status = XGetWindowProperty(display, window, XA_WM_NAME, 0, MAX_NAME_BYTES / 4,
                                   False, XA_STRING, &type, &format, &count, &remaining, &value);
    if (status != Success) { if (value) XFree(value); return 0; }
    // Match XFetchName's STRING/8-bit semantics, with a fixed property bound.
    if (type != XA_STRING || format != 8) { if (value) XFree(value); return 1; }
    if (remaining || count > MAX_NAME_BYTES) { if (value) XFree(value); return 0; }
    *name = (char *)value; // XGetWindowProperty supplies an extra terminating zero.
    return 1;
}

static int enumerate(Display *display, Window window, pid_t pid, Atom deletion, unsigned depth)
{
    if (++visited > MAX_VISITS || depth > 64) return 0;
    XWindowAttributes attributes;
    if (!XGetWindowAttributes(display, window, &attributes)) return 0;
    Window root, actual_parent, *children = NULL;
    unsigned count = 0;
    if (!XQueryTree(display, window, &root, &actual_parent, &children, &count)) {
        if (children) XFree(children);
        return 0;
    }
    int result = 1;
    if (owner_pid(display, window) == pid) {
        char *name = NULL;
        if (record_count == MAX_VISITS || !read_name(display, window, &name)) {
            if (name) XFree(name);
            result = 0;
        } else {
            WindowRecord *record = &records[record_count++];
            record->window = window; record->parent = actual_parent;
            record->map_state = attributes.map_state;
            record->x = attributes.x; record->y = attributes.y;
            record->width = attributes.width; record->height = attributes.height;
            record->can_close = closable(display, window, deletion);
            record->name = name;
        }
    }
    for (unsigned index = 0; result && index < count; index++)
        if (!enumerate(display, children[index], pid, deletion, depth + 1)) { result = 0; break; }
    if (children) XFree(children);
    return result;
}

static unsigned long positive(const char *text)
{
    char *end = NULL;
    errno = 0;
    unsigned long value = strtoul(text, &end, 0);
    if (errno || !text[0] || text[0] == '-' || !end || *end || value <= 1) return 0;
    return value;
}

int main(int argc, char **argv)
{
    if (argc < 3 || (strcmp(argv[1], "inventory") && strcmp(argv[1], "close"))) return 2;
    unsigned long value = positive(argv[2]);
    if (!value || value > INT_MAX ||
        (!strcmp(argv[1], "inventory") && argc != 3) || (!strcmp(argv[1], "close") && argc != 4)) return 2;
    pid_t pid = (pid_t)value;
    Display *display = XOpenDisplay(NULL);
    if (!display) { fprintf(stderr, "No authenticated X11 display.\n"); return 3; }
    XSetErrorHandler(on_error);
    int major = 0, minor = 0;
    if (!XResQueryVersion(display, &major, &minor) || major < 1 || (major == 1 && minor < 2)) {
        fprintf(stderr, "XRes 1.2 local-client PID ownership is required; no title/property fallback.\n");
        XCloseDisplay(display); return 4;
    }
    Atom deletion = XInternAtom(display, "WM_DELETE_WINDOW", False);
    int result = 0;
    if (!strcmp(argv[1], "inventory")) {
        // One server-side tree, including XRes credentials and actual parents.
        // Only requests on THIS connection run while grabbed. Never wait for
        // application IPC, sleep or serialize to a possibly blocked pipe here.
        XGrabServer(display);
        XSync(display, False);
        result = enumerate(display, XDefaultRootWindow(display), pid, deletion, 0) ? 0 : 5;
        XUngrabServer(display);
        XSync(display, False);
        XCloseDisplay(display);
        display = NULL;
        printf("{\"schemaVersion\":1,\"ownership\":\"XRes-local-client-pid\",\"pid\":%ld,"
               "\"xresVersion\":[%d,%d],\"windows\":[", (long)pid, major, minor);
        for (unsigned index = 0; index < record_count; index++) {
            WindowRecord *record = &records[index];
            printf("%s{\"xid\":%lu,\"parent\":%lu,\"ownerPid\":%ld,\"mapState\":%d,"
                   "\"x\":%d,\"y\":%d,\"width\":%d,\"height\":%d,\"canClose\":%s,\"name\":",
                   index ? "," : "", record->window, record->parent, (long)pid, record->map_state,
                   record->x, record->y, record->width, record->height, record->can_close ? "true" : "false");
            json_string(record->name);
            printf("}");
            if (record->name) XFree(record->name);
        }
        printf("],\"visited\":%u,\"complete\":%s}\n", visited, result ? "false" : "true");
    } else {
        Window window = positive(argv[3]);
        XWindowAttributes attributes;
        if (!window || owner_pid(display, window) != pid ||
            !XGetWindowAttributes(display, window, &attributes) || attributes.map_state != IsViewable ||
            !closable(display, window, deletion)) {
            fprintf(stderr, "Normal close refused: window is not a mapped, WM_DELETE-capable resource of the exact owned PID.\n");
            result = 6;
        } else {
            XEvent event;
            memset(&event, 0, sizeof(event));
            event.xclient.type = ClientMessage;
            event.xclient.display = display;
            event.xclient.window = window;
            event.xclient.message_type = XInternAtom(display, "WM_PROTOCOLS", False);
            event.xclient.format = 32;
            event.xclient.data.l[0] = (long)deletion;
            event.xclient.data.l[1] = CurrentTime;
            xerror = 0;
            int sent = XSendEvent(display, window, False, NoEventMask, &event);
            XSync(display, False);
            if (!sent || xerror) result = 7;
            printf("{\"pid\":%ld,\"xid\":%lu,\"protocol\":\"WM_DELETE_WINDOW\",\"sent\":%s,\"xerror\":%d}\n",
                   (long)pid, window, sent ? "true" : "false", xerror);
        }
    }
    if (display) XCloseDisplay(display);
    return result;
}
