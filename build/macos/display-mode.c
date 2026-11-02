// Ephemeral CI display setup only. Never called by the application launcher.
#include <CoreGraphics/CoreGraphics.h>
#include <CoreFoundation/CoreFoundation.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

int main(int argc, char **argv)
{
    if (!getenv("GITHUB_ACTIONS") || strcmp(getenv("GITHUB_ACTIONS"), "true")) return 2;
    CGDirectDisplayID display = CGMainDisplayID();
    if (argc == 2 && strcmp(argv[1], "verify") == 0) {
        CGRect bounds = CGDisplayBounds(display);
        printf("Fresh process observes actual native display bounds %.0f x %.0f\n", bounds.size.width, bounds.size.height);
        return bounds.size.width >= 1440 && bounds.size.height >= 1000 ? 0 : 8;
    }
    if (argc != 3) return 2;
    CGDisplayModeRef current = CGDisplayCopyDisplayMode(display);
    if (!current) return 3;
    const void *key = kCGDisplayShowDuplicateLowResolutionModes;
    const void *value = kCFBooleanTrue;
    CFDictionaryRef options = CFDictionaryCreate(NULL, &key, &value, 1, &kCFTypeDictionaryKeyCallBacks, &kCFTypeDictionaryValueCallBacks);
    CFArrayRef modes = CGDisplayCopyAllDisplayModes(display, options);
    CFRelease(options);
    if (!modes) { CGDisplayModeRelease(current); return 4; }
    unsigned restore_id = 0;
    int restoring = strcmp(argv[1], "restore") == 0;
    if (restoring) {
        FILE *state = fopen(argv[2], "r");
        if (!state || fscanf(state, "%u", &restore_id) != 1) { if (state) fclose(state); CFRelease(modes); CGDisplayModeRelease(current); return 5; }
        fclose(state);
    } else if (strcmp(argv[1], "prepare") == 0) {
        FILE *state = fopen(argv[2], "w");
        if (!state) { CFRelease(modes); CGDisplayModeRelease(current); return 6; }
        fprintf(state, "%u\n", CGDisplayModeGetIODisplayModeID(current));
        fclose(state);
    } else { CFRelease(modes); CGDisplayModeRelease(current); return 2; }
    CGDisplayModeRef selected = NULL;
    size_t selected_area = (size_t)-1;
    printf("Original native display: %zu x %zu, mode %u\n", CGDisplayModeGetWidth(current), CGDisplayModeGetHeight(current), CGDisplayModeGetIODisplayModeID(current));
    for (CFIndex i = 0; i < CFArrayGetCount(modes); i++) {
        CGDisplayModeRef mode = (CGDisplayModeRef)CFArrayGetValueAtIndex(modes, i);
        size_t width = CGDisplayModeGetWidth(mode), height = CGDisplayModeGetHeight(mode);
        unsigned identity = CGDisplayModeGetIODisplayModeID(mode);
        printf("Supported native display mode %u: %zu x %zu (%zu x %zu pixels)\n", identity, width, height, CGDisplayModeGetPixelWidth(mode), CGDisplayModeGetPixelHeight(mode));
        if ((restoring && identity == restore_id) || (!restoring && width >= 1440 && height >= 1000 && width * height < selected_area)) {
            selected = mode; selected_area = width * height;
        }
    }
    CGDisplayConfigRef configuration = NULL;
    CGError error = selected ? CGBeginDisplayConfiguration(&configuration) : kCGErrorFailure;
    if (error == kCGErrorSuccess) {
        error = CGConfigureDisplayWithDisplayMode(configuration, display, selected, NULL);
        if (error == kCGErrorSuccess)
            // SetDisplayMode alone is app-scoped and reverted when this helper
            // exits on ARM hosted Macs. Keep the owned CI setting for the login
            // session, then explicitly restore it in stop-display.sh.
            error = CGCompleteDisplayConfiguration(configuration, kCGConfigureForSession);
        else CGCancelDisplayConfiguration(configuration);
    }
    CGRect actual = CGDisplayBounds(display);
    printf("Native display selection result %d; actual bounds %.0f x %.0f\n", (int)error, actual.size.width, actual.size.height);
    CFRelease(modes); CGDisplayModeRelease(current);
    return error == kCGErrorSuccess ? 0 : 7;
}
