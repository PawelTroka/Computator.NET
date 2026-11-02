// Fixed-arity bridge for the exact Mono6.14.1 X11Keyboard imports. Apple ARM64
// places C variadic arguments on the stack; Mono declares these as fixed calls.
// XCreateIC/NestedList overloads are selected by mono-xim-arm64.S before entering
// C, so every C function receives exactly the argument list it declares.
#define _DARWIN_C_SOURCE 1
#include <X11/Xlib.h>
#include <stdint.h>
#include <stdatomic.h>
#include <stdlib.h>
#include <string.h>

#define API __attribute__((visibility("default")))
static atomic_uint calls[8];
API int ComputatorMonoXimAbiVersion(void) { return 1; }
API unsigned ComputatorMonoXimCallCount(int operation)
{
    return operation >= 0 && operation < 8 ? atomic_load(&calls[operation]) : 0;
}
static char *copy_error(char *error) { return error ? strdup(error) : NULL; }

API char *ComputatorMonoXGetIMValues(XIM im, const char *name, void *value, void *terminator)
{
    atomic_fetch_add(&calls[0], 1);
    if (terminator) return strdup("Invalid Mono XGetIMValues terminator");
    return copy_error(XGetIMValues(im, name, value, (char *)NULL));
}

API char *ComputatorMonoXGetICValues(XIC ic, const char *name, int32_t *value, void *terminator)
{
    atomic_fetch_add(&calls[1], 1);
    if (terminator) return strdup("Invalid Mono XGetICValues terminator");
    // Mono's EventMask is int32, while Xlib writes an LP64 unsigned long.
    unsigned long mask = 0;
    char *error = XGetICValues(ic, name, &mask, (char *)NULL);
    if (!error && value) *value = (int32_t)mask;
    return copy_error(error);
}

API void ComputatorMonoXSetICValues(XIC ic, const char *name, void *value, void *terminator)
{
    atomic_fetch_add(&calls[2], 1);
    if (!terminator) (void)XSetICValues(ic, name, value, (char *)NULL);
}

API XIC ComputatorMonoXCreateIC2(XIM im, const char *name, uint32_t style,
                                const char *name2, void *value2, void *terminator)
{
    atomic_fetch_add(&calls[3], 1);
    return terminator ? NULL : XCreateIC(im, name, (unsigned long)style, name2, value2, (char *)NULL);
}

API XIC ComputatorMonoXCreateIC3(XIM im, const char *name, uint32_t style,
                                const char *name2, void *value2, const char *name3,
                                void *value3, void *terminator)
{
    atomic_fetch_add(&calls[4], 1);
    return terminator ? NULL : XCreateIC(im, name, (unsigned long)style, name2, value2, name3, value3, (char *)NULL);
}

API XVaNestedList ComputatorMonoXVaCreateNestedList1(int dummy, const char *name0,
                                                   void *value0, void *terminator)
{
    atomic_fetch_add(&calls[5], 1);
    return terminator ? NULL : XVaCreateNestedList(dummy, name0, value0, (char *)NULL);
}

API XVaNestedList ComputatorMonoXVaCreateNestedList2(int dummy, const char *name0,
                                                   void *value0, const char *name1,
                                                   void *value1, void *terminator)
{
    atomic_fetch_add(&calls[6], 1);
    return terminator ? NULL : XVaCreateNestedList(dummy, name0, value0, name1, value1, (char *)NULL);
}

API XVaNestedList ComputatorMonoXVaCreateNestedList4(int dummy, const char *name0,
                                                   void *value0, const char *name1,
                                                   void *value1, const char *name2,
                                                   void *value2, const char *name3,
                                                   void *value3, void *terminator)
{
    atomic_fetch_add(&calls[7], 1);
    return terminator ? NULL : XVaCreateNestedList(dummy, name0, value0, name1, value1,
                                                name2, value2, name3, value3, (char *)NULL);
}
