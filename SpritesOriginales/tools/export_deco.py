"""Extrae objetos de decoración de la hoja de la iglesia a Assets/Sprites/Propios/Decoracion."""
import os, sys, json
import numpy as np
from PIL import Image
from scipy import ndimage as ndi
sys.path.insert(0, os.path.dirname(__file__))
from export_frames import downscale

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT = os.path.join(ROOT, "..", "Assets", "Sprites", "Propios", "Decoracion")
SCALE = 0.67
# índice (de deco_indices.png) -> nombre
PICK = {5: "vitral_grande", 2: "vitral_roseton", 10: "vitral_alto", 6: "vitral_chico_1", 16: "vitral_chico_2", 17: "vitral_chico_3",
        3: "puerta_1", 19: "puerta_2", 24: "puerta_3", 23: "estandarte_rojo", 22: "estandarte_morado", 25: "estandarte_azul", 77: "estandarte_azul_grande",
        72: "banca_1", 73: "banca_2", 74: "banca_3", 89: "banca_larga", 70: "pulpito", 69: "estatua_cruz", 75: "estatua_monje", 84: "estatua_virgen",
        54: "pilar_1", 55: "pilar_2", 79: "candil", 56: "candelabro_pared", 59: "vela_pared", 62: "candelabro_doble", 81: "antorcha",
        117: "altar", 88: "librero", 87: "librero_alto", 53: "cuadro_1", 60: "cuadro_2", 91: "atril", 120: "pila_bautismal", 124: "jarron", 125: "barril"}

if __name__ == "__main__":
    img = Image.open(os.path.join(ROOT, "decoracion_iglesia.png")).convert("RGBA")
    a = np.array(img)[:, :, 3] > 100
    lab, _ = ndi.label(ndi.binary_dilation(a, iterations=2))
    objs = {o["i"]: o for o in json.load(open(os.path.join(ROOT, "tools", "deco_objs.json")))}
    os.makedirs(OUT, exist_ok=True)
    sizes = {}
    for idx, name in PICK.items():
        o = objs[idx]
        sl = (slice(o["y0"], o["y1"]), slice(o["x0"], o["x1"]))
        arr = np.array(img)[sl].copy()
        arr[~((lab[sl] == o["lab"]) & a[sl])] = 0
        small = downscale(Image.fromarray(arr, "RGBA"), SCALE)
        small.save(os.path.join(OUT, name + ".png"))
        sizes[name] = [small.width, small.height]
    print(json.dumps(sizes))
