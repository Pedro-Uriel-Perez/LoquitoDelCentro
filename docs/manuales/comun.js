// Piezas compartidas por los dos manuales (portada, estilos, recuadros, figuras y tablas).
const fs = require("fs");
const path = require("path");
const {
  Paragraph, TextRun, ImageRun, Table, TableRow, TableCell, WidthType, ShadingType, BorderStyle,
  AlignmentType, HeadingLevel, PageBreak, TableOfContents, Footer, Header, PageNumber, LevelFormat,
  TabStopType,
} = require("docx");

const ROOT = path.resolve(__dirname, "..", "..");
const IMG = (...p) => path.join(ROOT, ...p);
const FONT = "Arial";
const ANCHO = 9360;            // ancho útil en DXA (carta con márgenes de 1")
const COLOR = { morado: "3B2F63", rosa: "C2185B", gris: "5A5A6E", claro: "F3F1F8" };

const estilos = {
  default: { document: { run: { font: FONT, size: 22 }, paragraph: { spacing: { after: 120, line: 288 } } } },
  paragraphStyles: [
    { id: "Title", name: "Title", basedOn: "Normal", run: { font: FONT, size: 52, bold: true, color: COLOR.morado },
      paragraph: { alignment: AlignmentType.CENTER, spacing: { before: 0, after: 200 }, keepNext: true } },
    { id: "Heading1", name: "Heading 1", basedOn: "Normal", next: "Normal", quickFormat: true,
      run: { font: FONT, size: 32, bold: true, color: COLOR.morado },
      paragraph: { spacing: { before: 360, after: 160 }, keepNext: true, outlineLevel: 0 } },
    { id: "Heading2", name: "Heading 2", basedOn: "Normal", next: "Normal", quickFormat: true,
      run: { font: FONT, size: 26, bold: true, color: COLOR.rosa },
      paragraph: { spacing: { before: 240, after: 120 }, keepNext: true, outlineLevel: 1 } },
    { id: "Heading3", name: "Heading 3", basedOn: "Normal", next: "Normal", quickFormat: true,
      run: { font: FONT, size: 23, bold: true, color: COLOR.gris },
      paragraph: { spacing: { before: 200, after: 80 }, keepNext: true, outlineLevel: 2 } },
    { id: "Pie", name: "Pie de figura", basedOn: "Normal",
      run: { font: FONT, size: 18, italics: true, color: COLOR.gris },
      paragraph: { alignment: AlignmentType.CENTER, spacing: { before: 60, after: 240 } } },
  ],
};

const numeracion = {
  config: [
    { reference: "vinetas", levels: [
      { level: 0, format: LevelFormat.BULLET, text: "•", alignment: AlignmentType.LEFT,
        style: { paragraph: { indent: { left: 540, hanging: 270 } } } },
      { level: 1, format: LevelFormat.BULLET, text: "–", alignment: AlignmentType.LEFT,
        style: { paragraph: { indent: { left: 1080, hanging: 270 } } } } ] },
    ...Array.from({ length: 30 }, (_, i) => ({ reference: `pasos${i}`, levels: [
      { level: 0, format: LevelFormat.DECIMAL, text: "%1.", alignment: AlignmentType.LEFT,
        style: { paragraph: { indent: { left: 540, hanging: 360 } } } } ] })),
  ],
};

// Texto con **negritas** marcadas así dentro de la cadena
function runs(texto, extra = {}) {
  return texto.split(/(\*\*[^*]+\*\*)/).filter(Boolean).map(t =>
    t.startsWith("**") ? new TextRun({ text: t.slice(2, -2), bold: true, ...extra }) : new TextRun({ text: t, ...extra }));
}

const p = (texto, opts = {}) => new Paragraph({ children: runs(texto), ...opts });
const h1 = t => new Paragraph({ heading: HeadingLevel.HEADING_1, children: [new TextRun(t)] });
const h2 = t => new Paragraph({ heading: HeadingLevel.HEADING_2, children: [new TextRun(t)] });
const h3 = t => new Paragraph({ heading: HeadingLevel.HEADING_3, children: [new TextRun(t)] });
const salto = () => new Paragraph({ children: [new PageBreak()] });
const vinetas = items => items.map(t => new Paragraph({ numbering: { reference: "vinetas", level: 0 }, children: runs(t) }));

let listaPasos = 0;
// Pasos numerados (cada lista empieza en 1)
function pasos(items) {
  const ref = `pasos${listaPasos++}`;
  return items.map(t => new Paragraph({ numbering: { reference: ref, level: 0 }, children: runs(t) }));
}

// Recuadro de aviso: tipo = "advertencia" | "nota" | "consejo"
function recuadro(tipo, texto) {
  const t = {
    advertencia: { icono: "⚠  ADVERTENCIA", borde: "D97706", fondo: "FFF4E0" },
    nota: { icono: "ℹ  NOTA", borde: "2563EB", fondo: "EAF1FF" },
    consejo: { icono: "✔  CONSEJO", borde: "16A34A", fondo: "EAF8EE" },
  }[tipo];
  const lineas = Array.isArray(texto) ? texto : [texto];
  return [new Table({
    width: { size: ANCHO, type: WidthType.DXA }, columnWidths: [ANCHO],
    rows: [new TableRow({ cantSplit: true, children: [new TableCell({
      width: { size: ANCHO, type: WidthType.DXA },
      shading: { type: ShadingType.CLEAR, fill: t.fondo, color: "auto" },
      margins: { top: 100, bottom: 100, left: 180, right: 180 },
      borders: { left: { style: BorderStyle.SINGLE, size: 36, color: t.borde },
                 top: { style: BorderStyle.NONE }, bottom: { style: BorderStyle.NONE }, right: { style: BorderStyle.NONE } },
      children: [new Paragraph({ spacing: { after: 60 }, children: [new TextRun({ text: t.icono, bold: true, color: t.borde })] }),
                 ...lineas.map(l => new Paragraph({ spacing: { after: 40 }, children: runs(l) }))],
    })] })],
  }), new Paragraph({ spacing: { after: 60 }, children: [] })];
}

// Tamaño de un PNG o JPG leyendo su cabecera
function medidas(file) {
  const b = fs.readFileSync(file);
  if (b[0] === 0x89) return { w: b.readUInt32BE(16), h: b.readUInt32BE(20), type: "png" };
  let i = 2;
  while (i < b.length) {
    const marker = b[i + 1], len = b.readUInt16BE(i + 2);
    if (marker >= 0xC0 && marker <= 0xC3) return { h: b.readUInt16BE(i + 5), w: b.readUInt16BE(i + 7), type: "jpg" };
    i += 2 + len;
  }
  throw new Error("No se pudo leer el tamaño de " + file);
}

let numFigura = 0;
// Figura centrada con pie numerado. anchoPx = ancho en el documento (a 96 ppp); altoMax limita las verticales.
function figura(rel, pie, anchoPx = 520, altoMax = 420) {
  const file = IMG(rel);
  numFigura++;
  if (!fs.existsSync(file)) {
    console.warn("  FALTA imagen:", rel);
    return [p(`[Figura ${numFigura} pendiente: ${rel}]`, { alignment: AlignmentType.CENTER }),
            new Paragraph({ style: "Pie", children: [new TextRun(`Figura ${numFigura}. ${pie}`)] })];
  }
  const m = medidas(file);
  let w = anchoPx, h = Math.round(m.h * anchoPx / m.w);
  if (h > altoMax) { h = altoMax; w = Math.round(m.w * altoMax / m.h); }
  return [
    new Paragraph({ alignment: AlignmentType.CENTER, keepNext: true, spacing: { before: 120, after: 0 },
      children: [new ImageRun({ type: m.type, data: fs.readFileSync(file), transformation: { width: w, height: h },
                                altText: { title: pie, description: pie, name: path.basename(rel) } })] }),
    new Paragraph({ style: "Pie", children: [new TextRun(`Figura ${numFigura}. ${pie}`)] }),
  ];
}

// Varias imágenes chicas en una fila (por ejemplo, capturas verticales del celular)
function filaFiguras(items, pie, altoPx = 300) {
  numFigura++;
  const n = items.length, col = Math.floor(ANCHO / n);
  const celdas = items.map(([rel, etiqueta]) => {
    const file = IMG(rel);
    const hijos = [];
    if (fs.existsSync(file)) {
      const m = medidas(file);
      let h = altoPx, w = Math.round(m.w * altoPx / m.h);
      const maxW = Math.floor(col / 15) - 10;
      if (w > maxW) { w = maxW; h = Math.round(m.h * maxW / m.w); }
      hijos.push(new Paragraph({ alignment: AlignmentType.CENTER, children: [new ImageRun({ type: m.type, data: fs.readFileSync(file),
        transformation: { width: w, height: h }, altText: { title: etiqueta, description: etiqueta, name: path.basename(rel) } })] }));
    } else {
      console.warn("  FALTA imagen:", rel);
      hijos.push(p(`[pendiente: ${path.basename(rel)}]`, { alignment: AlignmentType.CENTER }));
    }
    hijos.push(new Paragraph({ alignment: AlignmentType.CENTER, children: [new TextRun({ text: etiqueta, size: 18, bold: true, color: COLOR.gris })] }));
    return new TableCell({ width: { size: col, type: WidthType.DXA }, children: hijos,
      borders: { top: { style: BorderStyle.NONE }, bottom: { style: BorderStyle.NONE }, left: { style: BorderStyle.NONE }, right: { style: BorderStyle.NONE } } });
  });
  const sin = { style: BorderStyle.NONE, size: 0, color: "FFFFFF" };
  return [new Table({ width: { size: col * n, type: WidthType.DXA }, columnWidths: items.map(() => col),
                      borders: { top: sin, bottom: sin, left: sin, right: sin, insideHorizontal: sin, insideVertical: sin },
                      alignment: AlignmentType.CENTER, rows: [new TableRow({ cantSplit: true, children: celdas })] }),
          new Paragraph({ style: "Pie", children: [new TextRun(`Figura ${numFigura}. ${pie}`)] })];
}

// Tabla con encabezado sombreado. anchos en DXA (deben sumar ANCHO).
function tabla(encabezados, filas, anchos) {
  const borde = { style: BorderStyle.SINGLE, size: 4, color: "B9B4CC" };
  const bordes = { top: borde, bottom: borde, left: borde, right: borde };
  const celda = (texto, i, enc) => new TableCell({
    width: { size: anchos[i], type: WidthType.DXA }, borders: bordes,
    margins: { top: 60, bottom: 60, left: 100, right: 100 },
    shading: enc ? { type: ShadingType.CLEAR, fill: COLOR.morado, color: "auto" } : undefined,
    children: String(texto).split("\n").map(l => new Paragraph({ spacing: { after: 20 },
      children: enc ? [new TextRun({ text: l, bold: true, color: "FFFFFF" })] : runs(l) })),
  });
  return [new Table({
    width: { size: anchos.reduce((a, b) => a + b, 0), type: WidthType.DXA }, columnWidths: anchos,
    rows: [new TableRow({ tableHeader: true, cantSplit: true, children: encabezados.map((t, i) => celda(t, i, true)) }),
           ...filas.map(f => new TableRow({ cantSplit: true, children: f.map((t, i) => celda(t, i, false)) }))],
  }), new Paragraph({ spacing: { after: 60 }, children: [] })];
}

function portada(tipoManual, subtitulo) {
  const logo = IMG("docs", "img", "logo.png");
  const m = medidas(logo);
  const linea = (t, opts = {}) => new Paragraph({ alignment: AlignmentType.CENTER, spacing: { after: 60 },
                                                 children: [new TextRun({ text: t, ...opts })] });
  return [
    new Paragraph({ alignment: AlignmentType.CENTER, spacing: { before: 400, after: 300 }, children: [new ImageRun({ type: "png",
      data: fs.readFileSync(logo), transformation: { width: 380, height: Math.round(m.h * 380 / m.w) },
      altText: { title: "Logo", description: "Logo de Loquito del Centro", name: "logo" } })] }),
    new Paragraph({ style: "Title", children: [new TextRun(tipoManual)] }),
    linea(subtitulo, { size: 26, color: COLOR.gris }),
    linea("Versión 1.0 · Noviembre de 2026", { size: 22, color: COLOR.gris }),
    new Paragraph({ spacing: { before: 700 }, children: [] }),
    linea("Universidad Tecnológica del Norte de Guanajuato", { bold: true, size: 24 }),
    linea("Ingeniería en Desarrollo y Gestión de Software", { size: 22 }),
    linea("Asignatura: Creación de Videojuegos", { size: 22 }),
    linea("Profesor: Gabriel Barrón Rodríguez", { size: 22 }),
    new Paragraph({ spacing: { before: 300 }, children: [] }),
    linea("Equipo de desarrollo", { bold: true, size: 22, color: COLOR.morado }),
    linea("Pedro Uriel Pérez Monzón", { size: 22 }),
    linea("Miguel Ángel Álvarez Ibarra", { size: 22 }),
    new Paragraph({ spacing: { before: 300 }, children: [] }),
    linea("Dolores Hidalgo C.I.N., Guanajuato · Noviembre de 2026", { size: 20, color: COLOR.gris }),
    salto(),
  ];
}

function indice() {
  return [
    new Paragraph({ style: "Title", alignment: AlignmentType.LEFT, children: [new TextRun({ text: "Índice", size: 36 })] }),
    new TableOfContents("Índice", { hyperlink: true, headingStyleRange: "1-2" }),
    salto(),
  ];
}

function contacto(numero = "", conReporte = true) {
  return [
    h1(`${numero}Datos de contacto`),
    p("Para dudas, reportes de errores o sugerencias, comuníquese con el equipo de desarrollo:"),
    ...tabla(["Integrante", "Correo electrónico"], [
      ["Pedro Uriel Pérez Monzón", "pedrourielperezmonzon@gmail.com"],
      ["Miguel Ángel Álvarez Ibarra", "miguelangelalvarezibarrautng@gmail.com"],
    ], [4200, 5160]),
    ...tabla(["Recurso", "Dirección"], [
      ["Página del juego (itch.io)", "https://pedrourielperezmonzon.itch.io/loquitodelcentro"],
      ["Repositorio del proyecto (GitHub)", "https://github.com/Pedro-Uriel-Perez/LoquitoDelCentro"],
    ], [4200, 5160]),
    ...(conReporte ? [p("Al reportar un error, incluya: dispositivo o navegador, versión de Android (si aplica), qué estaba haciendo y, de ser posible, una captura de pantalla.")] : []),
  ];
}

function seccion(children, titulo) {
  return {
    properties: { titlePage: true, page: { size: { width: 12240, height: 15840 },
      margin: { top: 1440, bottom: 1440, left: 1440, right: 1440 } } },
    headers: {
      first: new Header({ children: [] }),
      default: new Header({ children: [new Paragraph({ alignment: AlignmentType.RIGHT,
        children: [new TextRun({ text: `Loquito del Centro · ${titulo}`, size: 16, color: COLOR.gris })] })] }),
    },
    footers: {
      first: new Footer({ children: [] }),
      default: new Footer({ children: [new Paragraph({ alignment: AlignmentType.CENTER,
        children: [new TextRun({ children: ["Página ", PageNumber.CURRENT, " de ", PageNumber.TOTAL_PAGES], size: 16, color: COLOR.gris })] })] }),
    },
    children,
  };
}

module.exports = { estilos, numeracion, p, h1, h2, h3, salto, vinetas, pasos, recuadro, figura, filaFiguras, tabla,
                   portada, indice, contacto, seccion, runs, ANCHO, COLOR };
