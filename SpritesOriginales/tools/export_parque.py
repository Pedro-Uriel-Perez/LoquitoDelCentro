"""Exporta los assets del jardín: carteles, decoración, tiles de pasto/tierra, fondo y la nueva cama/fogata."""
import os, sys, json
import numpy as np
from PIL import Image
from scipy import ndimage as ndi
sys.path.insert(0, os.path.dirname(__file__))
from export_frames import downscale, OUT, ROOT
from export_extra import save_rect_frames

P = os.path.join(OUT, "Parque")
DECO_SCALE = 0.75

# Carteles (se dejan a resolución original; en Unity usan 32 PPU para que el texto se lea)
SIGNS = {
    "cartel_mover": (7, 4, 479, 181),
    "cartel_saltar": (485, 34, 614, 163),
    "cartel_doble_salto": (614, 34, 757, 163),
    "cartel_atacar": (757, 34, 873, 163),
    "cartel_bloquear": (873, 34, 1002, 163),
    "cartel_interactuar": (1002, 34, 1122, 163),
}
# Piezas por índice de parque_objs.json (componentes separados)
PIECES = {10: "banca_1", 11: "banca_2", 17: "banca_3", 16: "banca_4", 3: "farol_1", 4: "farol_2", 5: "farol_3", 9: "farol_doble",
          21: "bote_basura", 6: "maceta_1", 7: "maceta_larga", 15: "maceta_2", 36: "maceta_3", 40: "fuente", 48: "arco_flores",
          72: "arco_seto", 61: "reja", 77: "mesa_picnic", 81: "carrito", 42: "tablero", 56: "bebedero_aves"}
# Piezas unidas en la hoja: recorte por rectángulo
RECTS = {"arbol_1": (6, 181, 103, 287), "arbol_2": (105, 181, 193, 287), "arbol_rosa": (195, 181, 278, 287),
         "arbol_naranja": (280, 181, 363, 287), "pino_1": (366, 181, 425, 287), "cipres": (427, 181, 451, 287),
         "pino_2": (452, 181, 504, 287), "kiosco": (1000, 308, 1122, 476)}
# Tiles (se recorta el interior de cada cuadro para evitar el borde oscuro)
TILES = {"pasto": (15, 513, 62, 560), "tierra": (73, 513, 117, 560), "tierra_oscura": (130, 456, 175, 502)}


def crop_alpha(img, box):
    return img.crop(box)


if __name__ == "__main__":
    img = Image.open(os.path.join(ROOT, "parque_tileset.png")).convert("RGBA")
    rep = {}
    for d in ("Carteles", "Decoracion", "Tiles", "Fondos"):
        os.makedirs(os.path.join(P, d), exist_ok=True)
    for name, box in SIGNS.items():
        im = img.crop(box)
        if name == "cartel_mover": im = downscale(im, 0.8)
        im.save(os.path.join(P, "Carteles", name + ".png")); rep[name] = im.size

    a = np.array(img)[:, :, 3] > 100
    lab, _ = ndi.label(ndi.binary_dilation(a, iterations=2))
    objs = {o["i"]: o for o in json.load(open(os.path.join(ROOT, "tools", "parque_objs.json")))}
    for idx, name in PIECES.items():
        o = objs[idx]; sl = (slice(o["y0"], o["y1"]), slice(o["x0"], o["x1"]))
        arr = np.array(img)[sl].copy(); arr[~((lab[sl] == o["lab"]) & a[sl])] = 0
        im = downscale(Image.fromarray(arr, "RGBA"), DECO_SCALE)
        im.save(os.path.join(P, "Decoracion", name + ".png")); rep[name] = im.size
    for name, box in RECTS.items():
        piece = np.array(img.crop(box))
        if name == "kiosco":  # solo el componente más grande (quita restos de las piezas vecinas)
            l2, n2 = ndi.label(ndi.binary_dilation(piece[:, :, 3] > 100, iterations=1))
            sizes = ndi.sum(np.ones_like(l2), l2, range(1, n2 + 1)); keep = 1 + int(np.argmax(sizes))
            piece[l2 != keep] = 0
        im = downscale(Image.fromarray(piece, "RGBA"), DECO_SCALE)
        im.save(os.path.join(P, "Decoracion", name + ".png")); rep[name] = im.size

    for name, box in TILES.items():
        t = img.crop(box).convert("RGBA").resize((16, 16), Image.BOX)
        t.putalpha(255)
        t.save(os.path.join(P, "Tiles", name + ".png")); rep[name] = t.size

    # Fondo del parque completo; en Unity la banqueta queda alineada con el piso del nivel
    bg = Image.open(os.path.join(ROOT, "fondo_parque.png")).convert("RGBA")   # completa: incluye la tierra de abajo
    bg.save(os.path.join(P, "Fondos", "fondo_parque.png")); rep["fondo"] = bg.size

    # Nueva cama y fogata (reemplazan a las anteriores)
    rep["cama"] = save_rect_frames("objetos_v2.png", [(30, 528, 186, 602), (197, 528, 348, 602), (361, 528, 515, 602), (527, 528, 679, 602), (691, 528, 844, 602)], 0.33, "Objetos", "cama")
    rep["fogata"] = save_rect_frames("objetos_v2.png", [(888, 462, 1000, 612), (1002, 462, 1104, 612), (1106, 462, 1208, 612), (1210, 462, 1310, 612), (1312, 462, 1412, 612), (1414, 462, 1515, 612)], 0.33, "Objetos", "fogata")
    print(json.dumps(rep))
