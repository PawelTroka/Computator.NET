// The exact Mono6.14.1 overloads all carry a NULL terminator. Inspect only slots
// that exist in every applicable signature, then tail-call the matching fixed
// C prototype without changing registers or the caller's stack arguments.
.text
.p2align 2
.globl _ComputatorMonoXCreateIC
_ComputatorMonoXCreateIC:
    cbz x5, _ComputatorMonoXCreateIC2
    b _ComputatorMonoXCreateIC3

.p2align 2
.globl _ComputatorMonoXVaCreateNestedList
_ComputatorMonoXVaCreateNestedList:
    cbz x3, _ComputatorMonoXVaCreateNestedList1
    cbz x5, _ComputatorMonoXVaCreateNestedList2
    b _ComputatorMonoXVaCreateNestedList4
