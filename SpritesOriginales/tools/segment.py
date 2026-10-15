"""Segmenta hojas de sprites en cuadros: componentes conectados asignados a bandas (filas) y agrupados por huecos en X."""
import numpy as np
from PIL import Image
from scipy import ndimage as ndi

def components(img, alpha_thr=100, min_area=20):
    a = np.array(img)[:, :, 3] > alpha_thr
    lab, n = ndi.label(a)
    out = []
    for i, sl in enumerate(ndi.find_objects(lab)):
        m = lab[sl] == i + 1
        area = int(m.sum())
        if area < min_area: continue
        ys, xs = np.nonzero(m)
        out.append(dict(id=i + 1, y0=sl[0].start, y1=sl[0].stop, x0=sl[1].start, x1=sl[1].stop,
                        cy=sl[0].start + ys.mean(), cx=sl[1].start + xs.mean(), area=area))
    return lab, out

def frames_in_band(comps, y0, y1, xmin, gap=10):
    cs = [c for c in comps if y0 <= c['cy'] < y1 and c['cx'] >= xmin]
    cs.sort(key=lambda c: c['x0'])
    groups = []
    for c in cs:
        if groups and c['x0'] <= groups[-1]['x1'] + gap:
            g = groups[-1]; g['c'].append(c); g['x1'] = max(g['x1'], c['x1'])
        else:
            groups.append(dict(x0=c['x0'], x1=c['x1'], c=[c]))
    return groups

def frames_main(comps, y0, y1, xmin, main_area=800, gap=6):
    """Agrupa solo componentes grandes (cuerpos); los pequeños se agregan si caen dentro del cuadro."""
    band = [c for c in comps if y0 <= c['cy'] < y1 and c['cx'] >= xmin]
    mains = sorted([c for c in band if c['area'] >= main_area], key=lambda c: c['x0'])
    groups = []
    for c in mains:
        if groups and c['x0'] <= groups[-1]['x1'] + gap:
            g = groups[-1]; g['c'].append(c); g['x1'] = max(g['x1'], c['x1'])
        else:
            groups.append(dict(x0=c['x0'], x1=c['x1'], c=[c]))
    for g in groups:
        g['y0'] = min(c['y0'] for c in g['c']); g['y1'] = max(c['y1'] for c in g['c'])
    for c in band:
        if c['area'] >= main_area: continue
        for g in groups:
            if g['x0'] <= c['cx'] <= g['x1'] and g['y0'] - 10 <= c['cy'] <= g['y1'] + 10:
                g['c'].append(c); break
    return groups

def frames_seeds(comps, y0, y1, xmin, seed_area=4000, pad=20):
    """Cada componente grande es un cuadro; los demás se asignan al cuadro que los contiene (o al más cercano)."""
    band = [c for c in comps if y0 <= c['cy'] < y1 and c['cx'] >= xmin]
    seeds = sorted([c for c in band if c['area'] >= seed_area], key=lambda c: c['cx'])
    groups = [dict(c=[s], x0=s['x0'], x1=s['x1'], y0=s['y0'], y1=s['y1'], cx=s['cx'], cy=s['cy']) for s in seeds]
    for c in band:
        if c['area'] >= seed_area: continue
        best = None
        for g in groups:
            inside = g['x0'] - pad <= c['cx'] <= g['x1'] + pad and g['y0'] - pad <= c['cy'] <= g['y1'] + pad
            if not inside: continue
            d = abs(c['cx'] - g['cx']) + abs(c['cy'] - g['cy'])
            if best is None or d < best[0]: best = (d, g)
        if best: best[1]['c'].append(c)
    return groups
