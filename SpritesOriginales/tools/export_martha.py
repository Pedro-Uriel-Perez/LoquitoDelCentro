"""Exporta a Martha la Loca (roncar / hablar), la nube de diálogo y los objetos que entrega."""
import os, sys
from PIL import Image
sys.path.insert(0, os.path.dirname(__file__))
from export_frames import loquito_scale, OUT, ROOT
from export_extra import save_rect_frames, save_piece

def frames_left_aligned(starts, y0, y1, scale, name, max_w=200):
    """Recorta cada cuadro desde el borde izquierdo del cuerpo sin invadir el siguiente y los alinea a la izquierda."""
    from export_frames import downscale
    img = Image.open(os.path.join(ROOT, "martha.png")).convert("RGBA")
    crops = []
    for i, x in enumerate(starts):
        nxt = starts[i + 1] if i + 1 < len(starts) else img.width
        crops.append(downscale(img.crop((x, y0, min(x + max_w, nxt - 1), y1)), scale))
    W = max(c.width for c in crops); W += W % 2; H = max(c.height for c in crops)
    d = os.path.join(OUT, "NPC", "Martha", name); os.makedirs(d, exist_ok=True)
    for f in os.listdir(d):
        if f.endswith(".png"): os.remove(os.path.join(d, f))
    for i, c in enumerate(crops):
        canvas = Image.new("RGBA", (W, H), (0, 0, 0, 0)); canvas.alpha_composite(c, (0, H - c.height))
        canvas.save(os.path.join(d, f"f-{i + 1:02d}.png"))
    return [W, H]

if __name__ == "__main__":
    s, _ = loquito_scale()
    print("roncar", frames_left_aligned([153, 378, 580, 785, 992, 1189, 1380], 226, 400, s, "roncar"))
    print("hablar", frames_left_aligned([153, 373, 581, 783, 986, 1182], 548, 719, s, "hablar"))
    ui = os.path.join(OUT, "UI"); os.makedirs(ui, exist_ok=True)
    Image.open(os.path.join(ROOT, "martha.png")).convert("RGBA").crop((638, 768, 886, 954)).save(os.path.join(ui, "nube_dialogo.png"))
    for idx, name in [(47, "item_carton"), (53, "item_botella"), (45, "item_calcetas")]:
        print(name, save_piece("loquito_extra.png", "extra_objs.json", idx, 1.0, os.path.join(ui, name + ".png")))
