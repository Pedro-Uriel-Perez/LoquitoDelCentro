"""Genera las imágenes del README en docs/img a partir de los sprites del juego.
Las capturas de los niveles las toma Unity (menú Loquito > Capturas para el README)."""
import os
from PIL import Image, ImageEnhance, ImageFilter

HERE = os.path.dirname(os.path.abspath(__file__))
PROJECT = os.path.normpath(os.path.join(HERE, "..", ".."))
ORIG = os.path.join(PROJECT, "SpritesOriginales")
SPR = os.path.join(PROJECT, "Assets", "Sprites", "Propios")
OUT = os.path.join(PROJECT, "docs", "img")


def crop_alpha(img):
    box = img.getchannel("A").point(lambda a: 255 if a > 8 else 0).getbbox()
    return img.crop(box) if box else img


def pixel(path, scale=3):
    """Sprite de pixel art agrandado sin suavizar."""
    img = crop_alpha(Image.open(os.path.join(SPR, path)).convert("RGBA"))
    return img.resize((img.width * scale, img.height * scale), Image.NEAREST)


def smooth(path, height):
    """Imagen en alta calidad (popups) a una altura fija."""
    img = crop_alpha(Image.open(os.path.join(SPR, path)).convert("RGBA"))
    return img.resize((round(img.width * height / img.height), height), Image.LANCZOS)


def save(img, name):
    path = os.path.join(OUT, name)
    os.makedirs(os.path.dirname(path), exist_ok=True)
    img.save(path)
    print(name, img.size)


def cover():
    """Portada 630x500: fondo del menú oscurecido con el logo encima."""
    w, h = 630, 500
    bg = Image.open(os.path.join(ORIG, "menu_fondo.jpg")).convert("RGB")
    s = max(w / bg.width, h / bg.height)
    bg = bg.resize((round(bg.width * s), round(bg.height * s)), Image.LANCZOS)
    x = (bg.width - w) // 2
    bg = ImageEnhance.Brightness(bg.crop((x, 0, x + w, h))).enhance(0.8).convert("RGBA")
    logo = crop_alpha(Image.open(os.path.join(ORIG, "logo.png")).convert("RGBA"))
    lw = 520
    logo = logo.resize((lw, round(logo.height * lw / logo.width)), Image.LANCZOS)
    shadow = Image.new("RGBA", logo.size, (0, 0, 0, 0))
    shadow.putalpha(logo.getchannel("A").point(lambda a: int(a * 0.7)))
    shadow = shadow.filter(ImageFilter.GaussianBlur(10))
    pos = ((w - lw) // 2, (h - logo.height) // 2 - 20)
    bg.alpha_composite(shadow, (pos[0], pos[1] + 8))
    bg.alpha_composite(logo, pos)
    return bg.convert("RGB")


if __name__ == "__main__":
    save(cover(), "portada.png")
    logo = crop_alpha(Image.open(os.path.join(ORIG, "logo.png")).convert("RGBA"))
    save(logo.resize((640, round(logo.height * 640 / logo.width)), Image.LANCZOS), "logo.png")

    personajes = {
        "loquito": ("Loquito/idle/f-01.png", 3),
        "martha": ("NPC/Martha/hablar/f-01.png", 3),
        "duende": ("Enemigos/Duende/correr/f-01.png", 3),
        "esbirro": ("Enemigos/Esbirro/vuelo/f-01.png", 3),
        "angel": ("Enemigos/Angel/ataque/f-01.png", 3),
        "campanero": ("Enemigos/Campanero/quieto/f-01.png", 2),
        "dragon": ("Enemigos/Dragon/quieto/f-01.png", 2),
        "senora": ("Enemigos/Senora/quieto/f-01.png", 2),
    }
    for name, (path, scale) in personajes.items():
        save(pixel(path, scale), f"personajes/{name}.png")

    for name in ("carton", "botella", "calcetas"):
        save(smooth(f"UI/item_{name}.png", 140), f"objetos/{name}.png")
    for name in ("bolsa", "lata_frijoles", "lata_coca", "caja_madera"):
        save(smooth(f"UI/coleccionable_{name}.png", 140), f"objetos/{name}.png")
    save(pixel("Objetos/panalito_gira/f-01.png", 4), "objetos/panalito.png")
    save(pixel("Objetos/cama/f-01.png", 3), "objetos/checkpoint.png")
    save(pixel("Objetos/antena_activa/f-01.png", 3), "objetos/antena.png")
