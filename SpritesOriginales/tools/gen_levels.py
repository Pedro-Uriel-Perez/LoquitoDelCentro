"""Genera los mapas de texto de las 4 mazmorras a partir de secciones reutilizables.
Leyenda: # sólido | B muro falso (se rompe con la botella) | P jugador | C checkpoint | H panalito
D duende | E esbirro alado | A ángel | K coleccionable | G compuerta del jefe | J jefe | N antena
[ y ] esquinas (inf-izq / sup-der) del interior de la arena del jefe. y=0 es la fila de abajo."""
import os
H = 40
OUT = os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "Levels")

class Map:
    def __init__(self, w): self.w = w; self.g = [[" "] * w for _ in range(H)]
    def rect(self, x0, y0, x1, y1, ch="#"):
        for y in range(y0, y1 + 1):
            for x in range(x0, x1 + 1):
                if 0 <= x < self.w and 0 <= y < H: self.g[y][x] = ch
    def put(self, x, y, ch): self.g[y][x] = ch
    def text(self): return "\n".join("".join(self.g[y]) for y in range(H - 1, -1, -1)) + "\n"

FLOOR = 3  # superficie del piso

def start(m, x):
    m.put(x + 3, FLOOR, "P"); m.put(x + 7, FLOOR, "C")
    m.rect(x, 26, x + 13, H - 3)                       # techo bajo de la sala inicial
    return x + 14

def platforms(m, x, panalito=False, variant=0):
    w = 30
    if variant == 0:
        m.rect(x + 3, 6, x + 8, 7); m.rect(x + 12, 9, x + 17, 10); m.rect(x + 21, 6, x + 26, 7)
        m.put(x + 6, FLOOR, "D"); m.put(x + 18, FLOOR, "D"); m.put(x + 15, 16, "E")
        if panalito:  # flotando sobre la plataforma media: solo se alcanza con doble salto
            m.put(x + 14, 20, "H"); m.put(x + 9, 17, "E"); m.put(x + 14, 11, "D")
    else:
        m.rect(x + 2, 8, x + 6, 9); m.rect(x + 10, 5, x + 15, 6); m.rect(x + 19, 9, x + 24, 10); m.rect(x + 26, 14, x + 28, 15)
        m.put(x + 12, 7, "D"); m.put(x + 20, FLOOR, "D"); m.put(x + 8, 15, "E")
        if panalito:
            m.put(x + 27, 16, "H"); m.put(x + 22, 18, "E"); m.put(x + 22, 11, "D")
    return x + w

def pits(m, x, count=2):
    cx = x
    for i in range(count):
        m.rect(cx + 5, 0, cx + 8, 2, " ")              # hoyo de 4 de ancho
        m.put(cx + 3, 11, "E") if i % 2 == 0 else m.put(cx + 2, FLOOR, "D")   # el esbirro nace sobre piso y cruza el hoyo volando
        cx += 10
    return cx + 3

def climb(m, x, panalito=True, angel=True):
    """Pozo vertical para subir, pasillo alto (con ángel) y caída de regreso al piso."""
    m.rect(x, 9, x + 3, H - 3)                          # pared izquierda del pozo (entrada abajo)
    sx = x + 4                                          # interior del pozo: sx..sx+18
    for i, (a, b, s) in enumerate([(2, 6, 8), (12, 17, 12), (4, 8, 16), (11, 16, 20), (6, 10, 24)]):
        m.rect(sx + a, s - 2, sx + b, s - 1)
    m.rect(sx + 12, 25, sx + 18, 26)                    # escalón hacia el pasillo
    if panalito:
        m.rect(sx, 20, sx + 1, 20); m.put(sx, 21, "H")      # cornisa en la pared, se salta desde la plataforma de 16
    m.put(sx + 9, 18, "E"); m.put(sx + 8, 29, "E")
    cx = sx + 19                                        # pasillo alto: piso sup. en 27
    m.rect(cx, FLOOR, cx + 3, 26)                       # pared derecha del pozo
    m.rect(cx, 24, cx + 34, 26)                         # piso del pasillo
    m.rect(cx + 4, 34, cx + 40, H - 3)                  # techo del pasillo
    m.put(cx + 9, 27, "D"); m.put(cx + 17, 27, "D")
    if angel: m.put(cx + 27, 27, "A")
    # hueco de 4 para caer al salón de abajo (x cx+35..cx+38) y salón inferior
    m.put(cx + 15, 12, "E"); m.put(cx + 24, FLOOR, "D"); m.put(cx + 28, FLOOR, "D")
    m.rect(cx + 39, 24, cx + 41, 26)                    # borde después del hueco
    m.rect(cx + 39, 27, cx + 41, H - 3)                 # pared que cierra el pasillo
    return cx + 42

def secret_tower(m, x):
    """Torre con un cuarto secreto: el muro izquierdo es falso (B) y adentro está el coleccionable."""
    m.rect(x, 6, x + 2, 7)                              # escalón para subir la torre
    t = x + 6
    m.rect(t, FLOOR, t + 9, 10)                         # torre sólida (superficie 11)
    m.rect(t, FLOOR, t, 7, "B"); m.rect(t + 1, FLOOR, t + 1, 7, "B")
    m.rect(t + 2, FLOOR, t + 7, 7, " ")                 # cuarto
    m.put(t + 5, FLOOR, "K")
    return t + 14

def boss_arena(m, x, w=40):
    m.put(x + 6, FLOOR, "C"); m.put(x + 1, 12, "E")
    g = x + 12
    m.rect(g, 8, g + 1, H - 3)                          # pared con la entrada
    m.rect(g, FLOOR, g + 1, 7, "G")                     # compuerta (abierta al inicio)
    a0, a1 = g + 2, g + 2 + w - 1                       # interior de la arena
    m.rect(a0, 22, a1 + 2, H - 3)                       # techo de la arena
    m.rect(a0 + 1, 7, a0 + 6, 8); m.rect(a0 + 1, 13, a0 + 5, 14)        # plataformas izquierda
    m.rect(a1 - 5, 7, a1, 8); m.rect(a1 - 4, 13, a1, 14)                # plataformas derecha
    m.rect(a0 + w // 2 - 3, 17, a0 + w // 2 + 3, 18)                     # plataforma alta central
    m.put(a0 + w // 2, FLOOR, "J"); m.put(a1 - 2, FLOOR, "N")
    m.put(a0, FLOOR, "["); m.put(a1, 21, "]")
    return a1 + 1

def build(sections, name):
    # primer pase para medir el ancho
    w = 2
    probe = Map(2000)
    for fn, kw in sections: w = fn(probe, w, **kw)
    m = Map(w + 2)
    m.rect(0, 0, m.w - 1, 2); m.rect(0, 0, 1, H - 1); m.rect(m.w - 2, 0, m.w - 1, H - 1); m.rect(0, H - 2, m.w - 1, H - 1)
    x = 2
    for fn, kw in sections: x = fn(m, x, **kw)
    open(os.path.join(OUT, name + ".txt"), "w", encoding="utf8", newline="\n").write(m.text())
    return m.w

def build_nivel0():
    """El centro: Martha (M) entrega los objetos. Muro falso (B) que se rompe con la botella,
    escalón alto que pide doble salto y la puerta (X) hacia la Mazmorra 1."""
    m = Map(80)
    m.rect(0, 0, m.w - 1, 2); m.rect(0, 0, 1, H - 1); m.rect(m.w - 2, 0, m.w - 1, H - 1)
    m.rect(0, 22, m.w - 1, H - 1)                       # techo
    m.put(4, FLOOR, "P"); m.put(9, FLOOR, "C"); m.put(16, FLOOR, "M")
    m.rect(34, FLOOR, 35, 21, "B")                      # muro falso: se rompe con la botella
    m.rect(48, FLOOR, 77, 10)                           # zona alta: 8 de alto, pide doble salto
    m.put(62, 15, "E")                                  # esbirro para practicar el cartón
    m.put(72, 11, "X")                                  # puerta a la Mazmorra 1
    rows = m.text().splitlines()[-24:]                  # el centro solo ocupa 24 filas de alto
    rows[0] = "#" * m.w
    open(os.path.join(OUT, "Nivel0.txt"), "w", encoding="utf8", newline="\n").write("\n".join(rows) + "\n")
    return m.w

if __name__ == "__main__":
    print(build_nivel0())
    print(build([(start, {}), (platforms, dict(panalito=False)), (pits, dict(count=1)), (climb, dict(panalito=True)), (secret_tower, {}), (boss_arena, {})], "Mazmorra1"))
    print(build([(start, {}), (secret_tower, {}), (pits, dict(count=3)), (platforms, dict(panalito=True, variant=1)), (climb, dict(panalito=False)), (boss_arena, {})], "Mazmorra2"))
    print(build([(start, {}), (climb, dict(panalito=True)), (platforms, dict(variant=1)), (pits, dict(count=2)), (secret_tower, {}), (platforms, dict(panalito=True)), (boss_arena, {})], "Mazmorra3"))
    print(build([(start, {}), (platforms, dict(variant=0)), (climb, dict(panalito=False)), (pits, dict(count=2)), (climb, dict(panalito=True)), (platforms, dict(variant=1)), (secret_tower, {}), (boss_arena, dict(w=44))], "Mazmorra4"))
