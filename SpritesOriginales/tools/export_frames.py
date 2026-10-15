"""Exporta los cuadros de las hojas de personajes a Assets/Sprites/Propios a escala de 16 PPU."""
import os, sys, json
import numpy as np
from PIL import Image
sys.path.insert(0, os.path.dirname(__file__))
from segment import components, frames_seeds

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT = os.path.join(ROOT, "..", "Assets", "Sprites", "Propios")
LOQUITO_HEIGHT_PX = 46  # alto objetivo del Loquito en el juego (≈2.9 unidades a 16 PPU)

def downscale(rgba, scale):
    w, h = rgba.size
    nw, nh = max(1, round(w * scale)), max(1, round(h * scale))
    a = np.asarray(rgba).astype(np.float32) / 255.0
    pre = a.copy(); pre[..., :3] *= pre[..., 3:4]
    small = np.stack([np.asarray(Image.fromarray((pre[..., i] * 255).astype(np.uint8)).resize((nw, nh), Image.BOX)).astype(np.float32) / 255 for i in range(4)], -1)
    alpha = small[..., 3]
    rgb = np.where(alpha[..., None] > 1e-4, small[..., :3] / np.maximum(alpha[..., None], 1e-4), 0)
    out_a = (alpha > 0.5).astype(np.float32)  # alfa binaria: bordes nítidos de pixel art
    out = np.concatenate([np.clip(rgb, 0, 1), out_a[..., None]], -1)
    return Image.fromarray((out * 255).astype(np.uint8), "RGBA")

def extract(img, lab, group, only_seed=False):
    ids = [group['c'][0]['id']] if only_seed else [c['id'] for c in group['c']]
    x0 = min(c['x0'] for c in group['c'] if c['id'] in ids); x1 = max(c['x1'] for c in group['c'] if c['id'] in ids)
    y0 = min(c['y0'] for c in group['c'] if c['id'] in ids); y1 = max(c['y1'] for c in group['c'] if c['id'] in ids)
    arr = np.array(img)[y0:y1, x0:x1].copy()
    mask = np.isin(lab[y0:y1, x0:x1], ids)
    arr[~mask] = 0
    return Image.fromarray(arr, "RGBA"), (x0, y0)

def feet_anchor(lab, seed):
    m = lab[seed['y0']:seed['y1'], seed['x0']:seed['x1']] == seed['id']
    h = m.shape[0]; bottom = m[int(h * 0.88):]
    xs = np.nonzero(bottom)[1]
    return seed['x0'] + (xs.mean() if len(xs) else m.shape[1] / 2), seed['y1']

def export_set(sheet, xmin, anims, out_dir, scale, anchor="feet", only_seed_anims=()):
    img = Image.open(os.path.join(ROOT, sheet)).convert("RGBA")
    lab, comps = components(img)
    frames = {}
    for name, y0, y1 in anims:
        gs = frames_seeds(comps, y0, y1, xmin)
        lst = []
        for g in gs:
            crop, (ox, oy) = extract(img, lab, g, only_seed=name in only_seed_anims)
            seed = g['c'][0]
            ax, ay = feet_anchor(lab, seed) if anchor == "feet" else (seed['cx'], seed['cy'])
            small = downscale(crop, scale)
            lst.append((small, (ax - ox) * scale, (ay - oy) * scale))
        frames[name] = lst
    # lienzo común por personaje: ancla en (W/2, abajo) o en el centro
    left = max(ax for fr in frames.values() for _, ax, _ in fr)
    right = max(im.width - ax for fr in frames.values() for im, ax, _ in fr)
    up = max(ay for fr in frames.values() for _, _, ay in fr)
    down = max(im.height - ay for fr in frames.values() for im, _, ay in fr)
    half = int(np.ceil(max(left, right))) + 1
    W = half * 2
    if anchor == "feet":
        H = int(np.ceil(up)) + 1; base_y = H
    else:
        hv = int(np.ceil(max(up, down))) + 1; H = hv * 2; base_y = hv
    info = {}
    for name, lst in frames.items():
        d = os.path.join(OUT, out_dir, name); os.makedirs(d, exist_ok=True)
        for f in os.listdir(d):
            if f.endswith(".png"): os.remove(os.path.join(d, f))
        for i, (im, ax, ay) in enumerate(lst):
            canvas = Image.new("RGBA", (W, H), (0, 0, 0, 0))
            canvas.alpha_composite(im, (int(round(half - ax)), int(round(base_y - ay))))
            canvas.save(os.path.join(d, f"f-{i + 1:02d}.png"))
        info[name] = len(lst)
    return dict(size=[W, H], frames=info)

def loquito_scale():
    img = Image.open(os.path.join(ROOT, "loquito.png")).convert("RGBA")
    lab, comps = components(img)
    gs = frames_seeds(comps, 0, 150, 205)
    hs = [g['c'][0]['y1'] - g['c'][0]['y0'] for g in gs]
    return LOQUITO_HEIGHT_PX / float(np.median(hs)), float(np.median(hs))

if __name__ == "__main__":
    s, h = loquito_scale()
    report = {"escala": s, "alto_caminar_original": h}
    report["Loquito"] = export_set("loquito.png", 205, [("caminar", 0, 150), ("correr", 150, 300), ("saltar", 300, 445), ("agacharse", 445, 555),
                                   ("atacar", 555, 690), ("defender", 690, 820), ("victoria", 820, 1024)], "Loquito", s, only_seed_anims=("correr",))
    report["Esbirro"] = export_set("enemigos.png", 165, [("vuelo", 85, 235), ("ataque", 250, 400)], "Enemigos/Esbirro", s, anchor="center", only_seed_anims=("ataque",))
    report["Duende"] = export_set("enemigos.png", 165, [("correr", 475, 610), ("saltar", 605, 770)], "Enemigos/Duende", s)
    report["Angel"] = export_set("enemigos.png", 165, [("ataque", 815, 990)], "Enemigos/Angel", s * 1.2)
    # bola de chocolate: el componente pequeño más grande y redondo de la fila de ataque del esbirro
    img = Image.open(os.path.join(ROOT, "enemigos.png")).convert("RGBA")
    lab, comps = components(img)
    cands = [c for c in comps if 250 <= c['cy'] < 400 and 150 < c['area'] < 4000 and 0.75 < (c['x1'] - c['x0']) / max(1, c['y1'] - c['y0']) < 1.33]
    ball = max(cands, key=lambda c: c['area'])
    crop = np.array(img)[ball['y0']:ball['y1'], ball['x0']:ball['x1']].copy(); crop[lab[ball['y0']:ball['y1'], ball['x0']:ball['x1']] != ball['id']] = 0
    small = downscale(Image.fromarray(crop, "RGBA"), s * 1.3)
    d = os.path.join(OUT, "Enemigos", "Esbirro"); small.save(os.path.join(d, "bola_chocolate.png"))
    report["bola"] = [small.width, small.height]
    print(json.dumps(report, indent=1))
