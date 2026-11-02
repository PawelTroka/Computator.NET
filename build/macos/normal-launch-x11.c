// External acceptance tool, never linked into or shipped with the application.
// XRes local-client credentials establish PID ownership; titles and _NET_WM_PID
// are not an ownership oracle (Mono does not publish _NET_WM_PID).
#include <sys/types.h>
#include <X11/Xlib.h>
#include <X11/Xutil.h>
#include <X11/extensions/XRes.h>
#include <errno.h>
#include <limits.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

static int xerror;
static unsigned visited;
static int first = 1;
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

static int enumerate(Display *display, Window window, Window parent, pid_t pid, Atom deletion, unsigned depth)
{
    if (++visited > 8192 || depth > 64) return 0;
    XWindowAttributes attributes;
    if (!XGetWindowAttributes(display, window, &attributes)) return 1; // a vanished splash is not a match
    if (owner_pid(display, window) == pid) {
        char *name = NULL;
        XFetchName(display, window, &name);
        printf("%s{\"xid\":%lu,\"parent\":%lu,\"ownerPid\":%ld,\"mapState\":%d,"
               "\"x\":%d,\"y\":%d,\"width\":%d,\"height\":%d,\"canClose\":%s,\"name\":",
               first ? "" : ",", window, parent, (long)pid, attributes.map_state,
               attributes.x, attributes.y, attributes.width, attributes.height, closable(display, window, deletion) ? "true" : "false");
        json_string(name);
        printf("}");
        if (name) XFree(name);
        first = 0;
    }
    Window root, actual_parent, *children = NULL;
    unsigned count = 0;
    if (!XQueryTree(display, window, &root, &actual_parent, &children, &count)) return 1;
    int result = 1;
    for (unsigned index = 0; index < count; index++)
        if (!enumerate(display, children[index], window, pid, deletion, depth + 1)) { result = 0; break; }
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
        printf("{\"schemaVersion\":1,\"ownership\":\"XRes-local-client-pid\",\"pid\":%ld,"
               "\"xresVersion\":[%d,%d],\"windows\":[", (long)pid, major, minor);
        result = enumerate(display, XDefaultRootWindow(display), None, pid, deletion, 0) ? 0 : 5;
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
    XCloseDisplay(display);
    return result;
}
