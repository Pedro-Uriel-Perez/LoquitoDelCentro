"""Reescala en alta calidad (x4, Lanczos) las imágenes grandes de la interfaz: coleccionables y objetos de Martha.
Correr después de export_extra.py y export_martha.py (ellos las guardan a tamaño original).
En Unity estas imágenes usan filtro Bilinear para que no se vean pixeladas al agrandarse."""
import os, sys
from PIL import Image
sys.path.insert(0, os.path.dirname(__file__))
from export_extra import save_piece
from export_frames import OUT

UI = os.path.join(OUT, "UI")
PIECES = [("loquito_extra.png", 49, "coleccionable_bolsa"), ("loquito_extra.png", 58, "coleccionable_lata_frijoles"),
          ("loquito_extra.png", 52, "coleccionable_lata_coca"), ("loquito_extra.png", 57, "coleccionable_caja_madera"),
          ("loquito_extra.png", 47, "item_carton"), ("loquito_extra.png", 53, "item_botella"), ("loquito_extra.png", 45, "item_calcetas")]

if __name__ == "__main__":
    for sheet, idx, name in PIECES:
        path = os.path.join(UI, name + ".png")
        save_piece(sheet, "extra_objs.json", idx, 1.0, path)
        im = Image.open(path).convert("RGBA")
        big = im.resize((im.width * 4, im.height * 4), Image.LANCZOS)
        big.save(path)
        print(name, im.size, "->", big.size)
