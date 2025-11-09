// RAM[4] = (RAM[0] * 3 + (RAM[1] | RAM[2])) & !RAM[3] + 11

@1
D = M
@2
D = D | M

@0
D = D + M
D = D + M
D = D + M

@3
D = !D
D = D | M
D = !D

@11
D = D + A

@4
M = D