"""Exporta jefe (campanero), objetos (panalito, checkpoint, antena) y extras del Loquito (idle, UI, coleccionables)."""
import os, sys, json
import numpy as np
from PIL import Image
from scipy import ndimage as ndi
sys.path.insert(0, os.path.dirname(__file__))
from segment import components, frames_seeds
from export_frames import downscale, export_set, loquito_scale, OUT, ROOT

def save_rect_frames(sheet, rects, scale, out_dir, name):
    """Recorta rectángulos fijos (x0,y0,x1,y1) y los alinea por la base del rectángulo."""
    img = Image.open(os.path.join(ROOT, sheet)).convert("RGBA")
    smalls = [downscale(img.crop(r), scale) for r in rects]
    W = max(s.width for s in smalls); H = max(s.height for s in smalls)
    W += W % 2
    d = os.path.join(OUT, out_dir, name); os.makedirs(d, exist_ok=True)
    for f in os.listdir(d):
        if f.endswith(".png"): os.remove(os.path.join(d, f))
    for i, s in enumerate(smalls):
        c = Image.new("RGBA", (W, H), (0, 0, 0, 0)); c.alpha_composite(s, ((W - s.width) // 2, H - s.height))
        c.save(os.path.join(d, f"f-{i + 1:02d}.png"))
    return [W, H]

def save_piece(sheet, objs_json, idx, scale, path, dil=3):
    img = Image.open(os.path.join(ROOT, sheet)).convert("RGBA")
    a = np.array(img)[:, :, 3] > 100
    lab, _ = ndi.label(ndi.binary_dilation(a, iterations=dil))
    o = {o["i"]: o for o in json.load(open(os.path.join(ROOT, "tools", objs_json)))}[idx]
    sl = (slice(o["y0"], o["y1"]), slice(o["x0"], o["x1"]))
    arr = np.array(img)[sl].copy(); arr[~((lab[sl] == o["lab"]) & a[sl])] = 0
    out = Image.fromarray(arr, "RGBA")
    if scale != 1: out = downscale(out, scale)
    os.makedirs(os.path.dirname(path), exist_ok=True); out.save(path)
    return [out.width, out.height]

if __name__ == "__main__":
    s, _ = loquito_scale()
    rep = {}
    # Jefe: el campanero (1.3x el tamaño de los personajes)
    rep["Campanero"] = export_set("campanero.png", 165, [("quieto", 40, 280), ("ataque", 280, 590), ("chancla", 600, 830), ("derrotado", 840, 1010)],
                                  "Enemigos/Campanero", s * 1.3, only_seed_anims=("chancla",))
    img = Image.open(os.path.join(ROOT, "campanero.png")).convert("RGBA")
    lab, comps = components(img)
    ch = max([c for c in comps if 600 <= c['cy'] < 830 and 1090 < c['cx'] < 1170], key=lambda c: c['area'])
    arr = np.array(img)[ch['y0']:ch['y1'], ch['x0']:ch['x1']].copy(); arr[lab[ch['y0']:ch['y1'], ch['x0']:ch['x1']] != ch['id']] = 0
    downscale(Image.fromarray(arr, "RGBA"), s * 1.3).save(os.path.join(OUT, "Enemigos", "Campanero", "chancla_proyectil.png"))
    # Objetos
    O = "Objetos"
    rep["panalito_gira"] = save_rect_frames("objetos.png", [(30, 115, 125, 312), (170, 115, 270, 312), (325, 115, 416, 312), (456, 115, 547, 312), (586, 115, 677, 312)], 0.13, O, "panalito_gira")
    rep["panalito_recoger"] = save_rect_frames("objetos.png", [(795, 95, 885, 335), (935, 95, 1035, 335)], 0.13, O, "panalito_recoger")
    rep["panalito_desaparecer"] = save_rect_frames("objetos.png", [(1100, 140, 1235, 345), (1240, 140, 1360, 345), (1380, 140, 1510, 345)], 0.13, O, "panalito_desaparecer")
    rep["cama"] = save_rect_frames("objetos.png", [(680, 490, 842, 633)], 0.33, O, "cama")
    rep["fogata"] = save_rect_frames("objetos.png", [(885, 470, 1000, 630), (1000, 470, 1102, 630), (1102, 470, 1203, 630), (1203, 470, 1304, 630), (1304, 470, 1410, 630), (1410, 470, 1515, 630)], 0.33, O, "fogata")
    rep["antena_activa"] = save_rect_frames("objetos.png", [(35, 750, 160, 970), (173, 750, 298, 970), (335, 750, 460, 970), (466, 750, 600, 970), (614, 750, 739, 970)], 0.3, O, "antena_activa")
    rep["antena_rota"] = save_rect_frames("objetos.png", [(784, 760, 904, 972), (927, 760, 1047, 972), (1073, 760, 1193, 972), (1209, 760, 1329, 972), (1358, 760, 1478, 972)], 0.3, O, "antena_rota")
    # Loquito: idle nuevo (7 cuadros) de la hoja extra
    rep["idle"] = export_set("loquito_extra.png", 400, [("idle", 432, 590)], "Loquito", s)
    # el idle de esta hoja mira a la izquierda; se refleja para que mire a la derecha como las demás animaciones
    d = os.path.join(OUT, "Loquito", "idle")
    for f in os.listdir(d):
        if f.endswith(".png"): Image.open(os.path.join(d, f)).transpose(Image.FLIP_LEFT_RIGHT).save(os.path.join(d, f))
    # UI y coleccionables
    E = "loquito_extra.png"; J = "extra_objs.json"; UI = os.path.join(OUT, "UI")
    rep["retrato"] = save_piece(E, J, 3, 0.5, os.path.join(UI, "retrato.png"))
    for i, n in [(0, "cara_frente"), (2, "cara_lado"), (1, "cara_tres_cuartos")]:
        rep[n] = save_piece(E, J, i, 0.4, os.path.join(UI, n + ".png"))
    rep["sentado"] = save_piece(E, J, 34, 1.0, os.path.join(UI, "sentado_caja.png"))
    for i, n in [(49, "bolsa"), (58, "lata_frijoles"), (52, "lata_coca"), (57, "caja_madera")]:
        rep[n + "_grande"] = save_piece(E, J, i, 1.0, os.path.join(UI, "coleccionable_" + n + ".png"))
        rep[n + "_mundo"] = save_piece(E, J, i, 0.22 if n != "lata_frijoles" and n != "lata_coca" else 0.35, os.path.join(OUT, O, "coleccionable_" + n + ".png"))
    print(json.dumps(rep))
