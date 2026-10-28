"""Exporta el fondo del menú, los botones (normal / seleccionado) y los objetos nuevos en alta calidad.
Fuentes en SpritesOriginales/: menu_fondo.jpg, botones_menu.png, objetos_v3.png (copiadas de Recursos_Loquito)."""
import os, sys
import numpy as np
from PIL import Image
sys.path.insert(0, os.path.dirname(__file__))
from export_frames import downscale, OUT, ROOT

UI = os.path.join(OUT, "UI")
# El menú se carga en tiempo de ejecución (Resources.Load), por eso va en Assets/Resources/Menu
MENU = os.path.join(ROOT, "..", "Assets", "Resources", "Menu")

# Botones de texto: (x0, x1) por columna; fila normal arriba, fila seleccionada abajo
TEXT_COLS = {"jugar": (44, 532), "creditos": (546, 994), "salir": (1006, 1493)}
TEXT_ROWS = {"normal": (55, 266), "rojo": (266, 476)}
ICON_COLS = {"config": (155, 404), "volumen": (493, 747), "mas": (823, 1064), "menos": (1135, 1374)}
ICON_ROWS = {"normal": (494, 737), "rojo": (737, 982)}

# Objetos nuevos (coordenadas en objetos_v3.png). Las monedas se ignoran.
OBJECTS = {"bolsa": (37, 115, 700, 808), "carton": (792, 108, 1311, 768), "lata_coca": (1477, 288, 1817, 613),
           "botella": (2009, 173, 2238, 808), "caja_madera": (43, 1082, 826, 1535), "lata_frijoles": (875, 1119, 1157, 1514),
           "calcetas": (1266, 1022, 1864, 1630)}
# Alto en pixeles del objeto que aparece dentro del nivel (16 PPU)
WORLD_HEIGHT = {"bolsa": 34, "lata_frijoles": 24, "lata_coca": 20, "caja_madera": 24}


def clean_button(img):
    """Quita el resplandor/fondo semitransparente y recorta al contenido."""
    from scipy import ndimage as ndi
    arr = np.array(img)
    arr[arr[:, :, 3] < 140] = 0
    arr[:, :, 3] = np.where(arr[:, :, 3] > 0, 255, 0)
    # solo la pieza principal (quita picos sueltos del botón de la fila vecina)
    lab, n = ndi.label(arr[:, :, 3] > 0)
    if n > 1:
        sizes = ndi.sum(np.ones_like(lab), lab, range(1, n + 1))
        arr[lab != 1 + int(np.argmax(sizes))] = 0
    out = Image.fromarray(arr, "RGBA")
    return out.crop(out.getbbox())


def fit(img, max_side):
    s = min(1.0, max_side / max(img.size))
    return img if s >= 1 else img.resize((round(img.width * s), round(img.height * s)), Image.LANCZOS)


if __name__ == "__main__":
    os.makedirs(MENU, exist_ok=True)
    # Fondo del menú
    bg = Image.open(os.path.join(ROOT, "menu_fondo.jpg")).convert("RGB")
    bg.save(os.path.join(MENU, "menu_fondo.png")); print("fondo", bg.size)

    sheet = Image.open(os.path.join(ROOT, "botones_menu.png")).convert("RGBA")
    for cols, rows in ((TEXT_COLS, TEXT_ROWS), (ICON_COLS, ICON_ROWS)):
        for name, (x0, x1) in cols.items():
            for state, (y0, y1) in rows.items():
                b = clean_button(sheet.crop((x0, y0, x1, y1)))
                b.save(os.path.join(MENU, f"boton_{name}_{state}.png")); print(f"boton_{name}_{state}", b.size)

    objs = Image.open(os.path.join(ROOT, "objetos_v3.png")).convert("RGBA")
    for name, box in OBJECTS.items():
        piece = objs.crop(box); piece = piece.crop(piece.getbbox())
        big = fit(piece, 480)
        ui_name = ("item_" + name) if name in ("carton", "botella", "calcetas") else ("coleccionable_" + name)
        big.save(os.path.join(UI, ui_name + ".png"))
        line = f"{ui_name} {big.size}"
        if name in WORLD_HEIGHT:
            small = downscale(piece, WORLD_HEIGHT[name] / piece.height)
            small.save(os.path.join(OUT, "Objetos", "coleccionable_" + name + ".png")); line += f" mundo {small.size}"
        print(line)
