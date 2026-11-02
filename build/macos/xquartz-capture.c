// Test-only readback from an actually mapped X11 drawable. This reads server
// pixels through XGetImage; it does not ask WinForms to redraw into a bitmap.
#include <X11/Xlib.h>
#include <X11/Xutil.h>
#include <stdint.h>
#include <stddef.h>
#include <stdio.h>

#define API __attribute__((visibility("default")))
API int ComputatorMacDrawableInfo(unsigned long window, int *info, size_t count)
{
    if (!info || count < 6) return -1;
    Display *display = XOpenDisplay(NULL);
    if (!display) return -2;
    XWindowAttributes attributes;
    int result = XGetWindowAttributes(display, window, &attributes);
    if (result) {
        info[0] = attributes.width; info[1] = attributes.height;
        info[2] = attributes.map_state; info[3] = attributes.depth;
        info[4] = attributes.x; info[5] = attributes.y;
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
