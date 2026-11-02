// Test-only readback from an actually mapped X11 drawable. This reads server
// pixels through XGetImage; it does not ask WinForms to redraw into a bitmap.
#include <X11/Xlib.h>
#include <X11/Xutil.h>
#include <X11/Xatom.h>
#include <stdint.h>
#include <stddef.h>
#include <stdio.h>
#include <CoreGraphics/CoreGraphics.h>
#include <CoreFoundation/CoreFoundation.h>

#define API __attribute__((visibility("default")))

// XQuartz publishes the real native window number on its framed ancestor.
// Read geometry only here; pixels come from one full compositor screenshot,
// never the redirected XGetImage backing store or a reconstructed window image.
API int ComputatorMacDesktopGeometry(unsigned long anchor, double *data, size_t count, const char *report_path)
{
    if (!data || count < 26 || !report_path) return -1;
    Display *display = XOpenDisplay(NULL);
    if (!display) return -2;
    FILE *report = fopen(report_path, "w");
    if (!report) { XCloseDisplay(display); return -3; }
    Atom property = XInternAtom(display, "_NATIVE_WINDOW_ID", True);
    Window native = anchor;
    unsigned long native_id = 0;
    int status = -4;
    fprintf(report, "{\"ancestorChain\":[");
    for (int depth = 0; property != None && native != None && depth < 64; depth++) {
        fprintf(report, "%s%lu", depth ? "," : "", native);
        Atom actual_type = None;
        int actual_format = 0;
        unsigned long items = 0, remaining = 0;
        unsigned char *value = NULL;
        if (XGetWindowProperty(display, native, property, 0, 1, False, AnyPropertyType,
                               &actual_type, &actual_format, &items, &remaining, &value) == Success &&
            actual_type == XA_INTEGER && actual_format == 32 && items == 1 && remaining == 0 && value) {
            native_id = *(unsigned long *)value;
        }
        if (value) XFree(value);
        if (native_id) break;
        Window root, parent, *children = NULL;
        unsigned child_count = 0;
        if (!XQueryTree(display, native, &root, &parent, &children, &child_count)) break;
        if (children) XFree(children);
        if (parent == native || parent == None) break;
        native = parent;
    }
    fprintf(report, "],\"nativeWindowId\":%lu,\"nativeAncestor\":%lu", native_id, native);
    if (!native_id) goto done;
    XWindowAttributes attributes;
    if (!XGetWindowAttributes(display, native, &attributes) || attributes.map_state != IsViewable) { status = -5; goto done; }
    int offset_x, offset_y, root_x, root_y;
    Window child;
    if (!XTranslateCoordinates(display, anchor, native, 0, 0, &offset_x, &offset_y, &child) ||
        !XTranslateCoordinates(display, native, XDefaultRootWindow(display), 0, 0, &root_x, &root_y, &child)) { status = -6; goto done; }
    CFArrayRef windows = CGWindowListCopyWindowInfo(kCGWindowListOptionAll, kCGNullWindowID);
    if (!windows) { status = -7; goto done; }
    status = -8;
    for (CFIndex index = 0; index < CFArrayGetCount(windows); index++) {
        CFDictionaryRef window = (CFDictionaryRef)CFArrayGetValueAtIndex(windows, index);
        int number = 0, owner = 0;
        CFNumberRef number_value = CFDictionaryGetValue(window, kCGWindowNumber);
        if (!number_value || !CFNumberGetValue(number_value, kCFNumberIntType, &number) || (unsigned)number != native_id) continue;
        CGRect bounds;
        CFDictionaryRef bounds_value = CFDictionaryGetValue(window, kCGWindowBounds);
        CFBooleanRef visible = CFDictionaryGetValue(window, kCGWindowIsOnscreen);
        CFNumberRef owner_value = CFDictionaryGetValue(window, kCGWindowOwnerPID);
        if (!bounds_value || !CGRectMakeWithDictionaryRepresentation(bounds_value, &bounds) ||
            !visible || !CFBooleanGetValue(visible) || !owner_value ||
            !CFNumberGetValue(owner_value, kCFNumberIntType, &owner)) { status = -9; break; }
        CGDirectDisplayID main = CGMainDisplayID();
        CGRect screen = CGDisplayBounds(main);
        data[0] = native_id; data[1] = native; data[2] = root_x; data[3] = root_y;
        data[4] = attributes.width; data[5] = attributes.height; data[6] = attributes.border_width;
        data[7] = offset_x; data[8] = offset_y;
        data[9] = bounds.origin.x; data[10] = bounds.origin.y; data[11] = bounds.size.width; data[12] = bounds.size.height;
        data[13] = screen.origin.x; data[14] = screen.origin.y; data[15] = screen.size.width; data[16] = screen.size.height;
        data[17] = main; data[18] = CGDisplayPixelsWide(main); data[19] = CGDisplayPixelsHigh(main);
        data[20] = 1; data[21] = owner;
        data[22] = XDisplayWidth(display, XDefaultScreen(display)); data[23] = XDisplayHeight(display, XDefaultScreen(display));
        data[24] = attributes.map_state; data[25] = anchor;
        fprintf(report, ",\"geometry\":[");
        for (size_t field = 0; field < 26; field++) fprintf(report, "%s%.17g", field ? "," : "", data[field]);
        fprintf(report, "]");
        status = 0;
        break;
    }
    CFRelease(windows);
done:
    fprintf(report, ",\"status\":%d}\n", status);
    if (fclose(report) != 0) status = -10;
    XCloseDisplay(display);
    return status;
}
API int ComputatorMacDrawableInfo(unsigned long window, int *info, size_t count)
{
    if (!info || count < 8) return -1;
    Display *display = XOpenDisplay(NULL);
    if (!display) return -2;
    XWindowAttributes attributes;
    int result = XGetWindowAttributes(display, window, &attributes);
    if (result) {
        info[0] = attributes.width; info[1] = attributes.height;
        info[2] = attributes.map_state; info[3] = attributes.depth;
        info[4] = attributes.x; info[5] = attributes.y;
        Window child;
        if (!XTranslateCoordinates(display, window, XDefaultRootWindow(display), 0, 0,
                                   &info[6], &info[7], &child)) result = 0;
    }
    XCloseDisplay(display);
    return result ? 0 : -3;
}

static unsigned channel(unsigned long pixel, unsigned long mask)
{
    if (!mask) return 0;
    unsigned shift = 0;
    while ((mask & 1) == 0) { mask >>= 1; shift++; }
    return (unsigned)(((pixel >> shift) & mask) * 255 / mask);
}

API int ComputatorMacDrawableArgb(unsigned long window, uint32_t *pixels, size_t capacity, int *info, const char *ppm_path)
{
    Display *display = XOpenDisplay(NULL);
    if (!display) return -1;
    XWindowAttributes attributes;
    if (!XGetWindowAttributes(display, window, &attributes) || attributes.map_state != IsViewable ||
        attributes.width <= 0 || attributes.height <= 0 || attributes.width > 8192 || attributes.height > 8192 ||
        (size_t)attributes.width * attributes.height > capacity) { XCloseDisplay(display); return -2; }
    XSync(display, False);
    XImage *image = XGetImage(display, window, 0, 0, (unsigned)attributes.width, (unsigned)attributes.height, AllPlanes, ZPixmap);
    if (!image) { XCloseDisplay(display); return -3; }
    info[0] = image->width; info[1] = image->height;
    info[2] = image->bytes_per_line; info[3] = image->bits_per_pixel;
    if (image->width <= 0 || image->height <= 0 || (size_t)image->width * image->height > capacity ||
        image->bytes_per_line < (image->width * image->bits_per_pixel + 7) / 8) {
        XDestroyImage(image); XCloseDisplay(display); return -4;
    }
    FILE *ppm = ppm_path ? fopen(ppm_path, "wb") : NULL;
    if (ppm_path && !ppm) { XDestroyImage(image); XCloseDisplay(display); return -5; }
    if (ppm) fprintf(ppm, "P6\n%d %d\n255\n", image->width, image->height);
    for (int y = 0; y < image->height; y++) {
        for (int x = 0; x < image->width; x++) {
            unsigned long pixel = XGetPixel(image, x, y);
            unsigned char rgb[3] = { (unsigned char)channel(pixel, image->red_mask),
                (unsigned char)channel(pixel, image->green_mask), (unsigned char)channel(pixel, image->blue_mask) };
            if (ppm) fwrite(rgb, 1, 3, ppm);
            pixels[(size_t)y * image->width + x] = UINT32_C(0xff000000) |
                (uint32_t)rgb[0] << 16 | (uint32_t)rgb[1] << 8 | rgb[2];
        }
    }
    int write_failed = ppm && ferror(ppm);
    if (ppm) fclose(ppm);
    XDestroyImage(image);
    XCloseDisplay(display);
    return write_failed ? -6 : 0;
}
