load LU.hdl,
output-file LU.out,
compare-to LU.cmp,
output-list a%B1.16.1 b%B1.16.1 negateA%B3.1.3 negateB%B3.1.3 sel%B1.2.1 out%B1.16.1;

set a %B0000111100001111,
set b %B0011110000111100;

// and
set negateA 0,
set negateB 0,
set sel 0,
eval,
output;

set negateA 1,
set negateB 0,
set sel 0,
eval,
output;

set negateA 0,
set negateB 1,
set sel 0,
eval,
output;

set negateA 1,
set negateB 1,
set sel 0,
eval,
output;

// or
set negateA 0,
set negateB 0,
set sel 1,
eval,
output;

set negateA 1,
set negateB 0,
set sel 1,
eval,
output;

set negateA 0,
set negateB 1,
set sel 1,
eval,
output;

set negateA 1,
set negateB 1,
set sel 1,
eval,
output;

// nand
set negateA 0,
set negateB 0,
set sel 2,
eval,
output;

set negateA 1,
set negateB 0,
set sel 2,
eval,
output;

set negateA 0,
set negateB 1,
set sel 2,
eval,
output;

set negateA 1,
set negateB 1,
set sel 2,
eval,
output;

// nor
set negateA 0,
set negateB 0,
set sel 3,
eval,
output;

set negateA 1,
set negateB 0,
set sel 3,
eval,
output;

set negateA 0,
set negateB 1,
set sel 3,
eval,
output;

set negateA 1,
set negateB 1,
set sel 3,
eval,
output;