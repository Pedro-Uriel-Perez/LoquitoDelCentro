// Genera docs/propuesta_valor/Propuesta_de_Valor.docx (Actividad 4: máximo una cuartilla)
// node docs/propuesta_valor/propuesta_valor.js
const fs = require("fs");
const path = require("path");
const { Document, Packer, Paragraph, TextRun, AlignmentType, BorderStyle } = require("docx");
const c = require("../manuales/comun");
const { tabla, COLOR } = c;

const t = (texto, extra = {}) => new Paragraph({ spacing: { after: 90, line: 264 }, alignment: AlignmentType.JUSTIFIED, children: c.runs(texto), ...extra });
const sub = texto => new Paragraph({ spacing: { before: 110, after: 50 }, keepNext: true,
  children: [new TextRun({ text: texto, bold: true, size: 23, color: COLOR.rosa })] });

const cuerpo = [
  new Paragraph({ spacing: { after: 0 }, children: [new TextRun({ text: "Propuesta de valor — Loquito del Centro", bold: true, size: 32, color: COLOR.morado })] }),
  new Paragraph({ spacing: { after: 140 }, border: { bottom: { style: BorderStyle.SINGLE, size: 8, color: COLOR.morado, space: 4 } },
    children: [new TextRun({ text: "Pedro Uriel Pérez Monzón y Miguel Ángel Álvarez Ibarra · UTNG, Creación de Videojuegos · Noviembre de 2026", size: 17, color: COLOR.gris })] }),

  t("**Para** estudiantes mexicanos de 15 a 25 años que juegan en ratos cortos y disfrutan el humor de internet y los plataformeros clásicos, **Loquito del Centro** es un plataformero de acción en pixel art que convierte a San Felipe, Guanajuato, en un castillo gótico. **A diferencia de** otros juegos del género, ambientados en mundos de fantasía ajenos, su héroe es un vagabundo del centro armado con basura."),

  sub("Por qué elegirlo frente a otros plataformeros"),
  ...c.vinetas([
    "**Identidad local única:** ningún otro plataformero ocurre en San Felipe; el jugador reconoce lugares y humor de su región.",
    "**Tres objetos, tres verbos:** la botella ataca, el cartón bloquea y las calcetas dan doble salto; se aprenden en minutos.",
    "**Reto justo:** jefes que avisan antes de atacar, checkpoints frecuentes y un final secreto que invita a rejugar.",
    "**Sin barreras:** gratis en el navegador sin instalar nada, o en Android con controles táctiles.",
  ]),

  sub("Atributos cualitativos y cuantitativos"),
  ...tabla(["Cualitativos (experiencia)", "Cuantitativos (acceso y rendimiento)"], [
    ["Estética de aventura gótica con un protagonista absurdo: contraste que genera humor", "Precio: **$0** hoy (donación voluntaria); versión completa a **$49 MXN**, pago único"],
    ["Música y efectos para cada nivel y jefe; voz \"balbuceo\" en los diálogos", "Descarga ligera: **72 MB** el APK y **65 MB** la versión web"],
    ["Pixel art nítido y controles táctiles que se iluminan al presionarlos", "Funciona en un celular de gama media de 2020 (Samsung Galaxy A51, Android 13)"],
    ["Diálogos y personajes con humor local que generan cercanía", "Partida completa de **30 a 60 minutos**; sin anuncios ni compras dentro del juego"],
  ], [4680, 4680]),

  sub("Alineación con el segmento"),
  t("El público definido en el documento de diseño juega en sesiones cortas, sobre todo en el celular, y valora la dificultad honesta y las referencias locales. El juego responde con niveles breves, una dificultad que premia la práctica y una versión web que se prueba en segundos desde un enlace."),

  sub("Base para la sostenibilidad"),
  t("Modelo escalonado y sin deudas: hoy el prototipo es gratuito para crecer la comunidad en itch.io y validar el interés; después, la **versión completa con cinco mazmorras** se venderá a **$49 MXN** en itch.io y Google Play, con arte y audio propios y marca registrada. Unas 150 ventas cubren los registros ante el IMPI y el INDAUTOR, y el formato puede crecer con capítulos en otros municipios de Guanajuato."),
];

const doc = new Document({
  creator: "Pedro Uriel Pérez Monzón y Miguel Ángel Álvarez Ibarra",
  title: "Propuesta de Valor — Loquito del Centro",
  styles: c.estilos, numbering: c.numeracion,
  sections: [{ properties: { page: { size: { width: 12240, height: 15840 }, margin: { top: 1134, bottom: 1134, left: 1247, right: 1247 } } },
               children: cuerpo }],
});
Packer.toBuffer(doc).then(buf => {
  const out = path.join(__dirname, "Propuesta_de_Valor.docx");
  fs.writeFileSync(out, buf);
  console.log("Listo:", out);
});
