// The exact Mono6.14.1 overloads all carry a NULL terminator. Inspect only slots
// that exist in every applicable signature, then tail-call the matching fixed
// C prototype without changing registers or the caller's stack arguments.
.text
.p2align 2
.globl _ComputatorMonoXCreateIC
_ComputatorMonoXCreateIC:
    cbz x5, Lcreate_two
    b _ComputatorMonoXCreateIC3
Lcreate_two:
    b _ComputatorMonoXCreateIC2

.p2align 2
.globl _ComputatorMonoXVaCreateNestedList
_ComputatorMonoXVaCreateNestedList:
    cbz x3, Lnested_one
    cbz x5, Lnested_two
    b _ComputatorMonoXVaCreateNestedList4
Lnested_one:
    b _ComputatorMonoXVaCreateNestedList1
Lnested_two:
    b _ComputatorMonoXVaCreateNestedList2
