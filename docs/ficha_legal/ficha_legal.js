// Genera docs/ficha_legal/Ficha_Legal_y_de_Negocio.docx  (node docs/ficha_legal/ficha_legal.js)
// Actividad 3: propiedad intelectual (IMPI / INDAUTOR), licencias de terceros, uso de IA y Modelo Canvas.
const fs = require("fs");
const path = require("path");
const {
  Document, Packer, Paragraph, TextRun, Table, TableRow, TableCell, WidthType, ShadingType, BorderStyle,
  AlignmentType, PageOrientation, Header, Footer, PageNumber, VerticalAlign,
} = require("docx");
const c = require("../manuales/comun");
const { p, h1, h2, h3, vinetas, pasos, recuadro, tabla, COLOR } = c;

const FECHA_CONSULTA = "24 de noviembre de 2026";

// ───────────────────────── Parte 1 (vertical) ─────────────────────────
const parte1 = [
  ...c.portada("Ficha Legal y de Negocio", "Loquito del Centro — Propiedad intelectual, licencias, uso de IA y Modelo Canvas"),
  ...c.indice(),

  h1("1. Introducción"),
  p("Esta ficha identifica qué elementos de **Loquito del Centro** pueden protegerse legalmente en México, qué recursos de terceros utiliza el proyecto y bajo qué licencias, cómo se empleó la inteligencia artificial durante su desarrollo y cuál es el modelo de negocio propuesto para el videojuego."),
  ...tabla(["Dato", "Descripción"], [
    ["Proyecto", "Loquito del Centro, plataformero de acción 2D en pixel art"],
    ["Titulares", "Pedro Uriel Pérez Monzón y Miguel Ángel Álvarez Ibarra (alumnos de la UTNG)"],
    ["Plataformas", "Navegador (WebGL en itch.io) y Android (APK)"],
    ["Estado", "Prototipo funcional, versión 1.0, distribuido gratis"],
    ["Público objetivo", "Jugadores de 15 a 25 años, principalmente estudiantes mexicanos"],
  ], [2400, 6960]),
  ...recuadro("nota", `Las tarifas y requisitos citados se consultaron el ${FECHA_CONSULTA} en las fuentes de la sección 5. Las tarifas del IMPI y del INDAUTOR se actualizan cada año, por lo que deben confirmarse en el sitio oficial antes de cualquier pago.`),

  // 2 ────────────────────────────────────────────
  h1("2. Protección de la propiedad intelectual"),
  p("En México la propiedad intelectual se divide en dos ramas: la **propiedad industrial**, a cargo del **IMPI** (marcas, patentes, diseños), y los **derechos de autor**, a cargo del **INDAUTOR** (obras literarias, artísticas, musicales y programas de cómputo). Un videojuego toca ambas."),

  h2("2.1 Estrategia de propiedad industrial (IMPI)"),
  p("Se identificaron los siguientes elementos distintivos del videojuego y la figura de protección que les corresponde:"),
  ...tabla(["Elemento", "Figura ante el IMPI", "Alcance comercial"], [
    ["Nombre comercial **Loquito del Centro**", "Marca nominativa o mixta", "Es el nombre con el que el juego se busca y se vende en itch.io y Google Play. Registrarlo evita que un tercero lo use o lo registre primero."],
    ["Logotipo (emblema gótico con el nombre)", "Marca mixta (nombre + diseño)", "Identifica visualmente el juego en tiendas, portada, ícono y posible mercancía. Se recomienda registrarlo junto con el nombre en una sola marca mixta."],
    ["Personajes distintivos: **El Loquito**, **Martha la Loca**, **La Señora**", "Marca figurativa o mixta (opcional, a futuro)", "Útil solo si se planea mercancía (playeras, figuras, stickers). En el corto plazo se protegen mejor como personajes ante el INDAUTOR (sección 2.2)."],
    ["Mecánica \"tres objetos, tres verbos\" (botella, cartón y calcetas) y controles táctiles", "No patentable", "El artículo 47 de la LFPPI excluye los programas de computación de las invenciones patentables, y una mecánica de juego no es una solución técnica nueva. Se protege como parte del código (derechos de autor) y manteniendo en privado lo que no se publique."],
  ], [2700, 2300, 4360]),
  h3("Clases de la Clasificación de Niza recomendadas"),
  ...tabla(["Clase", "Productos o servicios", "Prioridad"], [
    ["9", "Software de videojuegos descargable; aplicaciones móviles descargables", "Alta: es el producto principal"],
    ["41", "Servicios de entretenimiento; suministro de juegos en línea (versión web)", "Alta: cubre la versión que se juega en el navegador"],
    ["28 / 25", "Juguetes y figuras / prendas de vestir", "Baja: solo si se lanza mercancía"],
  ], [1100, 5260, 3000]),
  ...recuadro("consejo", "Antes de solicitar la marca, **busque** el nombre en **MARCanet** (marcanet.impi.gob.mx) para confirmar que no exista una marca igual o parecida en las clases 9 y 41."),

  h2("2.2 Protección de derechos de autor (INDAUTOR)"),
  p("Los derechos de autor protegen las obras originales desde el momento en que se crean; el registro ante el INDAUTOR no los otorga, pero sirve como **prueba de autoría y fecha**. La ley mexicana protege solo obras de **creación humana**: en 2025 la Suprema Corte (Amparo Directo 6/2025) confirmó la negativa del INDAUTOR a registrar una obra generada con IA. Las obras **híbridas**, con aportación humana demostrable, sí pueden registrarse. Por eso la tabla distingue el origen de cada obra:"),
  ...tabla(["Obra del proyecto", "Tipo de obra", "Origen", "¿Registrable?"], [
    ["Código fuente del juego (scripts C#, constructor de niveles, herramientas de build)", "Programa de cómputo", "Equipo, con apoyo de IA (Claude y Gemini) en escritura y corrección; diseño, integración y pruebas humanas", "Sí, como obra del equipo; conviene documentar la aportación humana (historial de GitHub)."],
    ["Historia, guion, diálogos y documento de diseño (GDD)", "Obra literaria", "Equipo", "Sí."],
    ["Diseño de niveles (mapas de texto del jardín y las mazmorras)", "Parte del programa de cómputo", "Equipo (Miguel Ángel)", "Sí, dentro del registro del código."],
    ["Logo, fondo del menú, botones y objetos", "Obra artística / gráfica", "Equipo, con aplicaciones de diseño", "Sí."],
    ["Sprites del Loquito, Martha, enemigos y jefes", "Obra artística", "Generados con ChatGPT a partir de ideas y descripciones del equipo; recortados y ajustados por el equipo", "La imagen generada por sí sola, no. El concepto, nombre y caracterización de los personajes sí son aportación humana."],
    ["Música y efectos de sonido", "Obra musical / fonograma", "Generados con Gemini y BandLab SongStarter; seleccionados e integrados por el equipo", "No, mientras no tengan aportación humana sustancial."],
    ["Manuales de Instalación y de Usuario, README y esta ficha", "Obra literaria", "Equipo, con apoyo de Claude en la redacción; revisados y validados por el equipo", "Sí, como obra híbrida."],
  ], [2600, 1600, 2860, 2300]),
  ...recuadro("advertencia", ["Los **sprites** y el **audio** generados con IA no quedan protegidos por derechos de autor y, en el caso de BandLab SongStarter, la plataforma indica que el usuario no es dueño del track generado aunque pueda usarlo sin pagar regalías.", "**Recomendación antes de vender el juego:** redibujar o editar a mano los sprites finales y componer o reelaborar la música y los efectos, documentando el proceso, para que sean obras con autoría humana registrable."]),
  h3("Personajes: reserva de derechos"),
  p("Además del registro de obra, el INDAUTOR otorga la **reserva de derechos al uso exclusivo** de **personajes ficticios** (trámite INDAUTOR-02-001-A, artículo 173 de la Ley Federal del Derecho de Autor). Protege el nombre y la caracterización del personaje (rasgos, personalidad, forma de actuar), por lo que aplica a El Loquito y a Martha la Loca, cuya caracterización es creación del equipo. Es renovable."),

  h2("2.3 Plan de registro integral"),
  p("Pasos que seguirá el equipo para formalizar la protección. Ambos integrantes figurarán como **cotitulares**."),
  h3("Etapa 0. Preparación (antes de cualquier trámite)"),
  ...pasos([
    "**Firmar** un acuerdo simple de cotitularidad entre Pedro Uriel y Miguel Ángel: porcentajes, quién decide sobre ventas y qué pasa si alguien deja el proyecto.",
    "**Consultar** el reglamento de la UTNG sobre la propiedad intelectual de proyectos académicos, para confirmar si la universidad tiene algún derecho sobre el juego.",
    "**Tramitar** la e.firma del SAT de cada titular: la piden el portal Marca en Línea del IMPI y los trámites en línea.",
    "**Reunir** las evidencias de autoría: historial de commits en GitHub, documento de diseño, bocetos y archivos editables.",
  ]),
  h3("Etapa 1. Derechos de autor ante el INDAUTOR (corto plazo)"),
  ...pasos([
    "**Preparar** cada obra por separado: (1) código fuente, (2) guion e historia, (3) arte gráfico propio (logo, menú, botones, objetos), (4) manuales.",
    "**Llenar** la solicitud del trámite **INDAUTOR-01-001** (registro de obra literaria o artística), seleccionando la rama correspondiente; para el código, la rama **Programa de cómputo**.",
    "**Pagar** los derechos: **$367.00 M.N. por obra** (Ley Federal de Derechos, art. 184, tarifa 2026). Las cuatro obras suman **$1,468.00**.",
    "**Presentar** la solicitud con el ejemplar de cada obra en medio digital. Antes, **verificar** si el registro en línea (INDARELIN) está disponible o si debe hacerse en ventanilla o por correo.",
    "**Recibir** el certificado de registro y guardarlo junto con las evidencias de autoría.",
    "(Opcional) **Solicitar** la reserva de derechos de los personajes El Loquito y Martha la Loca (INDAUTOR-02-001-A) y **anotar** la fecha de renovación.",
  ]),
  h3("Etapa 2. Marca ante el IMPI (mediano plazo, antes de cobrar por el juego)"),
  ...pasos([
    "**Buscar** anterioridades del nombre y del logo en **MARCanet** (marcanet.impi.gob.mx) en las clases 9 y 41.",
    "**Definir** la marca como **mixta** (nombre + logotipo) y la lista de productos y servicios de cada clase.",
    "**Pagar** los derechos de solicitud por clase: aproximadamente **$2,813.77 con IVA** por clase en línea (**$3,126.41** en ventanilla). Para las clases 9 y 41 en línea: **≈ $5,627.54**.",
    "**Presentar** la solicitud en **Marca en Línea** con la e.firma y dar seguimiento al examen de forma y de fondo; **responder** cualquier requerimiento o citatorio del IMPI dentro del plazo indicado.",
    "Al obtener el título (vigencia de **10 años**, renovable), **presentar** la **declaración de uso** al tercer año y **agendar** la renovación.",
  ]),
  h3("Cronograma y costo estimado"),
  ...tabla(["Etapa", "Cuándo", "Costo estimado"], [
    ["0. Preparación", "Al cerrar el cuatrimestre (diciembre 2026)", "$0"],
    ["1. INDAUTOR (4 obras)", "Enero a marzo 2027", "$1,468.00"],
    ["Reemplazo de sprites y audio generados con IA", "Antes de la versión comercial", "Tiempo del equipo"],
    ["2. IMPI marca mixta (clases 9 y 41)", "Antes de publicar la versión de pago", "≈ $5,627.54"],
    ["Total aproximado", "", "≈ $7,095.54 M.N."],
  ], [4200, 2960, 2200]),

  h2("2.4 Auditoría de licencias de terceros"),
  p("Inventario de herramientas, motores, bibliotecas y recursos externos usados en el proyecto, con el tipo de licencia y si permite la distribución comercial del juego:"),
  ...tabla(["Recurso", "Uso en el proyecto", "Licencia / términos", "¿Distribución comercial?"], [
    ["Unity 6 (6000.3.24f1), plan Personal", "Motor del juego", "Licencia de software de Unity; plan Personal gratuito para ingresos y financiamiento menores a 200,000 USD en 12 meses", "Sí, mientras no se supere el límite (después se requiere Unity Pro)."],
    ["Paquetes de Unity: URP 17.3, Input System 1.20, uGUI 2.0, 2D Tilemap Extras", "Gráficos 2D, controles, interfaz", "Unity Companion License (verificado en el archivo LICENSE de cada paquete)", "Sí, usados dentro de un proyecto de Unity."],
    ["Paquete 2D Tilemap", "Mapas de bloques", "Unity Package Distribution License (archivo LICENSE del paquete)", "Sí, dentro de Unity."],
    ["GothicVania Church Pack, de Luis Zuno (ansimuz)", "Fondos, bloques y decoración de las mazmorras", "Licencia del autor en itch.io: uso personal y comercial, se puede modificar; no se pueden redistribuir los recursos de pago como recursos; la música del paquete requiere crédito", "Sí. Se da crédito en los créditos del juego y en el README; no se usó la música del paquete."],
    ["ChatGPT (OpenAI)", "Generación de los sprites de personajes", "Términos de uso de OpenAI: el usuario es dueño del resultado en la medida que lo permita la ley; puede haber resultados similares para otros usuarios", "Sí, pero sin protección de derechos de autor en México (sección 2.2)."],
    ["Gemini (Google)", "Parte del audio y apoyo en correcciones de código", "Términos de Google: Google no reclama la propiedad del contenido generado y puede generar contenido similar para otros", "Sí, con la misma limitación de derechos de autor."],
    ["BandLab SongStarter", "Bases instrumentales para la música", "Uso libre de regalías, pero el usuario no es dueño del track generado (según la ayuda de BandLab)", "Con riesgo: se recomienda reemplazar o reelaborar antes de vender."],
    ["Claude (Anthropic), en Claude Code", "Apoyo en código, depuración y documentación", "Términos de Anthropic: cede al usuario los derechos que tenga sobre los resultados", "Sí."],
    ["AI Game Developer (Unity MCP, Ivan Murzak) y sus bibliotecas en Assets/Plugins/NuGet", "Herramienta de desarrollo que conecta el editor con el asistente", "Apache License 2.0 (archivo LICENSE del paquete)", "Sí, aunque no es necesaria en el juego: se recomienda quitarla de las versiones comerciales."],
    ["Python + Pillow, Node.js + docx, Microsoft Word", "Herramientas internas para recortar sprites y generar documentos", "Licencias de código abierto (Python, Pillow, docx) y licencia de Microsoft 365", "No se distribuyen con el juego."],
    ["itch.io y GitHub", "Publicación del juego y del código", "Términos de cada plataforma; itch.io permite elegir su comisión (10 % por defecto)", "Sí."],
  ], [2300, 2100, 2960, 2000]),

  // 3 ────────────────────────────────────────────
  h1("3. Del plagio y uso de inteligencia artificial"),
  p("Conforme a la definición de la UNAM (2018), el plagio es presentar como propio el trabajo ajeno sin citar la fuente. Para evitarlo, el equipo declara de forma explícita todas las herramientas de IA utilizadas y da crédito a los recursos de terceros."),
  h2("3.1 Declaración del uso de IA"),
  ...tabla(["Herramienta", "Quién la usó", "Apartado del proyecto", "Propósito"], [
    ["ChatGPT (OpenAI)", "Pedro Uriel", "Arte de personajes", "Generar los sprites del Loquito, Martha la Loca, enemigos y jefes a partir de las ideas, descripciones y bocetos del equipo. El equipo los seleccionó, recortó y ajustó."],
    ["Gemini (Google)", "Pedro Uriel", "Audio", "Generar parte de la música y los efectos de sonido."],
    ["BandLab SongStarter", "Pedro Uriel", "Audio", "Generar bases instrumentales editables para la música."],
    ["Gemini (Google)", "Miguel Ángel", "Programación", "Apoyo en correcciones y dudas al programar a El Dragón Borracho, La Señora y la Mazmorra 3."],
    ["Claude (Anthropic), en Claude Code", "Ambos", "Programación, publicación y documentación", "Asistente de desarrollo: apoyo en la escritura y corrección de código, depuración de errores, sugerencias de diseño técnico, guías paso a paso para publicar en itch.io y compilar para Android, y apoyo en la redacción del README, los manuales y esta ficha."],
  ], [2000, 1500, 2100, 3760]),
  p("**Sin uso de IA:** la idea y la historia del juego, el documento de diseño, los nombres y la caracterización de los personajes, el logotipo, el fondo del menú, los botones y los objetos (hechos con aplicaciones de diseño), el diseño de los niveles, las decisiones de diseño y las pruebas del juego."),
  h2("3.2 Verificación y criterio humano"),
  p("El contenido técnico y jurídico generado o redactado con apoyo de IA se revisó así:"),
  ...vinetas([
    "**Datos legales:** cada tarifa, artículo de ley y criterio se contrastó con fuentes oficiales o especializadas mexicanas (IMPI, INDAUTOR, Gobierno de México, Diario Oficial). Las fuentes de la sección 5 se consultaron el " + FECHA_CONSULTA + " y el equipo abrió cada enlace para comprobar que funciona y dice lo citado.",
    "**Contexto mexicano:** se usaron la LFPPI, la Ley Federal del Derecho de Autor, la Ley Federal de Derechos y el criterio de la Suprema Corte de 2025 sobre obras hechas con IA, no legislación de otros países.",
    "**Licencias:** las de los paquetes de Unity se leyeron directamente en los archivos LICENSE incluidos en el proyecto; las de los recursos externos, en las páginas oficiales de cada proveedor.",
    "**Código:** todo el código sugerido por IA se compiló y se probó en el editor, en el navegador (itch.io) y en un celular Samsung Galaxy A51 con Android 13. Los errores encontrados (por ejemplo, botones táctiles que no detectaban el dedo) se diagnosticaron y corrigieron antes de publicar.",
    "**Documentación:** los manuales se validaron siguiendo sus propios pasos, con capturas reales tomadas durante la instalación.",
  ]),
  ...recuadro("nota", "Las cifras marcadas como aproximadas provienen de guías especializadas que citan la tarifa oficial; el equipo las confirmará en el portal del IMPI y del INDAUTOR al momento de pagar."),
];

// ───────────────────────── Parte 2 (horizontal): Modelo Canvas ─────────────────────────
const L_ANCHO = 15840 - 2 * 1080;       // carta horizontal con márgenes de 0.75"
const col = Math.floor(L_ANCHO / 5);

function bloque(titulo, lineas, opts = {}) {
  const borde = { style: BorderStyle.SINGLE, size: 6, color: "B9B4CC" };
  return new TableCell({
    width: { size: col * (opts.span || 1), type: WidthType.DXA },
    columnSpan: opts.span, rowSpan: opts.rows, verticalAlign: VerticalAlign.TOP,
    borders: { top: borde, bottom: borde, left: borde, right: borde },
    shading: opts.fondo ? { type: ShadingType.CLEAR, fill: opts.fondo, color: "auto" } : undefined,
    margins: { top: 80, bottom: 80, left: 100, right: 100 },
    children: [
      new Paragraph({ spacing: { after: 60 }, children: [new TextRun({ text: titulo, bold: true, size: 22, color: COLOR.morado })] }),
      ...lineas.map(l => new Paragraph({ spacing: { after: 50, line: 252 }, children: c.runs("• " + l, { size: 18 }) })),
    ],
  });
}

const canvas = new Table({
  width: { size: col * 5, type: WidthType.DXA }, columnWidths: [col, col, col, col, col],
  rows: [
    new TableRow({ height: { value: 3000, rule: "atLeast" }, children: [
      bloque("Socios clave", ["Unity Technologies (motor, plan Personal gratuito)", "itch.io (tienda y hospedaje web)", "Google Play (tienda Android, a futuro)", "Luis Zuno / ansimuz (GothicVania, licencia comercial)", "UTNG y profesor asesor", "Creadores de contenido y comunidad de Sanfe"], { rows: 2 }),
      bloque("Actividades clave", ["Desarrollo y corrección del juego", "Diseño de niveles y jefes nuevos", "Publicación de actualizaciones web y APK", "Registro ante INDAUTOR e IMPI", "Promoción en redes y comunidad"]),
      bloque("Propuesta de valor", ["El único plataformero ambientado en San Felipe, Gto.: humor local y referencias reconocibles", "Estética de aventura gótica épica con un protagonista absurdo armado con basura", "\"Tres objetos, tres verbos\": botella, cartón y calcetas = atacar, bloquear y doble salto", "Gratis, sin anuncios, se juega en el navegador o en Android con controles táctiles", "Partidas cortas (30-60 min) con jefes retadores y final secreto"], { rows: 2, fondo: "F3F1F8" }),
      bloque("Relación con jugadores", ["Comentarios y devlog en la página de itch.io", "Actualizaciones con nuevas mazmorras", "Soporte por correo (en los manuales)", "Final secreto y coleccionables para rejugar"]),
      bloque("Segmentos de clientes", ["Jugadores de 15 a 25 años, estudiantes mexicanos", "Conocen el humor de internet y los plataformeros clásicos (Castlevania)", "Juegan en celular Android o en PC desde el navegador, en sesiones cortas", "Habitantes y exalumnos de San Felipe y Guanajuato (referencias locales)"], { rows: 2 }),
    ] }),
    new TableRow({ height: { value: 2100, rule: "atLeast" }, children: [
      bloque("Recursos clave", ["Equipo de 2 desarrolladores", "Unity 6, código fuente y herramientas de build", "Arte, audio y documento de diseño", "Repositorio en GitHub", "Computadora y celular Android de prueba"]),
      bloque("Canales", ["itch.io: versión web gratis y APK descargable", "Google Play (versión completa a futuro)", "Redes sociales y grupos de la región", "Presentación en la UTNG y ferias de videojuegos"]),
    ] }),
    new TableRow({ height: { value: 1700, rule: "atLeast" }, children: [
      bloque("Estructura de costos", ["Motor Unity Personal: $0 (hasta 200,000 USD de ingresos)", "Hospedaje y tienda en itch.io: $0 (comisión configurable, 10 % por defecto)", "Cuenta de desarrollador de Google Play: 25 USD, pago único", "INDAUTOR: $1,468 (4 obras) · IMPI: ≈ $5,627.54 (marca en clases 9 y 41)", "Reemplazo de arte y audio generados con IA: tiempo del equipo", "Equipo: tiempo de los 2 integrantes; equipo de cómputo propio"], { span: 3 }),
      bloque("Fuentes de ingresos", ["Hoy: gratis, con donaciones voluntarias en itch.io (\"$0 or donate\")", "Futuro: versión completa con las 5 mazmorras por compra única de $49 MXN en itch.io y Google Play", "Sin anuncios ni microtransacciones (respeta al público de 15+ y la experiencia)"], { span: 2 }),
    ] }),
  ],
});

const parte2 = [
  new Paragraph({ heading: "Heading1", children: [new TextRun("4. Modelo Canvas")] }),
  p("Lienzo del modelo de negocio del videojuego. Los nueve bloques se explican y justifican en la sección 4.1."),
  canvas,
];

// ───────────────────────── Parte 3 (vertical): justificación y fuentes ─────────────────────────
const parte3 = [
  h2("4.1 Justificación de los bloques"),
  h3("Propuesta de valor y segmento de clientes"),
  p("El público definido desde el diseño (15 a 25 años, estudiantes mexicanos) valora la dificultad honesta y el humor local. Ningún otro plataformero está ambientado en San Felipe: esa identidad, más la mezcla de estética gótica épica con un protagonista absurdo, es lo que lo distingue. Como ese público juega sobre todo en el celular y en sesiones cortas, el juego se compiló para Android con controles táctiles y para navegador, sin instalación."),
  h3("Canales y relación con los jugadores"),
  p("Los canales coinciden con las plataformas compiladas: **itch.io** aloja la versión web y el APK sin costo ni proceso de aprobación; **Google Play** se reserva para la versión completa, cuando se cuente con arte y audio propios. Para atraer jugadores se usarán las redes y grupos de la región y la presentación en la UTNG; para retenerlos, actualizaciones con nuevas mazmorras, el final secreto y la sección de comentarios de itch.io."),
  h3("Fuentes de ingresos"),
  p("Se eligió un esquema **viable y ético** para un prototipo: hoy el juego es gratis y acepta **donaciones voluntarias**; a futuro, una **compra única de $49 MXN** por la versión completa. Se descartan los anuncios y las microtransacciones porque interrumpen un juego corto basado en la habilidad y porque el público incluye menores de 18 años. La clasificación de edad se obtendrá con el cuestionario IARC al publicar en Google Play; por la violencia caricaturesca y el humor, se espera una clasificación para adolescentes."),
  h3("Recursos, actividades y socios clave"),
  p("El juego depende de Unity (gratuito con el plan Personal), de itch.io y Google Play como tiendas, y del paquete GothicVania, cuya licencia permite el uso comercial con modificaciones. El equipo de dos personas mantiene el código en GitHub y prueba en un celular Android real."),
  h3("Estructura de costos y coherencia"),
  p("Los costos son bajos y coherentes con los ingresos esperados: con un precio de $49 MXN, la marca y los registros (≈ $7,100) se recuperan con unas 150 ventas antes de comisiones e impuestos. Los costos se escalonan: primero los registros baratos del INDAUTOR; la marca, la cuenta de Google Play y el reemplazo del arte y audio generados con IA solo se pagan cuando se lance la versión de pago. Así el modelo no genera deudas mientras el juego sea un prototipo gratuito."),

  // 5 ────────────────────────────────────────────
  h1("5. Fuentes consultadas"),
  p(`Fuentes consultadas el ${FECHA_CONSULTA}:`),
  ...vinetas([
    "INDAUTOR. Registro de obra literaria o artística (trámite INDAUTOR-01-001): tramitesindautor.cultura.gob.mx/tramites-mas-informacion/01/indautor-01-001.html",
    "Gobierno de México, Secretaría de Cultura. \"INDAUTOR reconoce la decisión de la Suprema Corte de Justicia de la Nación sobre el registro de obras creadas con IA\" (2025): gob.mx/cultura/prensa",
    "El Financiero (30 de agosto de 2025). \"Indautor y SCJN rechazan registro de obras generadas con IA\": elfinanciero.com.mx",
    "IMPI. Búsqueda de marcas MARCanet: marcanet.impi.gob.mx",
    "Rivenza (2026). \"Cuánto cuesta registrar una marca en el IMPI 2026\" y \"Registro de obra ante INDAUTOR: costo y plazos 2026\": rivenza.com.mx/blog",
    "Ley Federal de Protección a la Propiedad Industrial, artículo 47 (DOF, 1 de julio de 2020): mley.mx/LFPPI/articulo/47",
    "Unity Technologies. Planes y precios: unity.com/products · Unity Companion License: unity.com/legal/licenses/unity-companion-license",
    "Luis Zuno (ansimuz). GothicVania Church Pack, licencia: ansimuz.itch.io/gothicvania-church-pack",
    "OpenAI. Terms of Use: openai.com/policies/row-terms-of-use",
    "Google. Gemini API Additional Terms of Service: ai.google.dev/gemini-api/terms",
    "BandLab. Music ownership on BandLab: help.bandlab.com",
    "Anthropic. Consumer Terms of Service: anthropic.com/legal/consumer-terms",
    "Universidad Nacional Autónoma de México (2018). Definición de plagio, citada en el instrumento de evaluación.",
  ]),
  ...c.contacto("6. ", false).slice(0, -1),
  // párrafo final mínimo: Word necesita uno después de la última tabla y uno normal crea una hoja en blanco
  new Paragraph({ spacing: { before: 0, after: 0, line: 20, lineRule: "exact" }, children: [new TextRun({ text: "", size: 2 })] }),
];

function seccionVertical(children, first = false) {
  const s = c.seccion(children, "Ficha Legal y de Negocio");
  if (!first) s.properties.titlePage = false;
  return s;
}

const doc = new Document({
  creator: "Pedro Uriel Pérez Monzón y Miguel Ángel Álvarez Ibarra",
  title: "Ficha Legal y de Negocio — Loquito del Centro",
  styles: c.estilos, numbering: c.numeracion,
  sections: [
    seccionVertical(parte1, true),
    {
      properties: { page: { size: { width: 12240, height: 15840, orientation: PageOrientation.LANDSCAPE },
                            margin: { top: 1080, bottom: 1080, left: 1080, right: 1080 } } },
      headers: { default: new Header({ children: [new Paragraph({ alignment: AlignmentType.RIGHT,
        children: [new TextRun({ text: "Loquito del Centro · Ficha Legal y de Negocio", size: 16, color: COLOR.gris })] })] }) },
      footers: { default: new Footer({ children: [new Paragraph({ alignment: AlignmentType.CENTER,
        children: [new TextRun({ children: ["Página ", PageNumber.CURRENT, " de ", PageNumber.TOTAL_PAGES], size: 16, color: COLOR.gris })] })] }) },
      children: parte2,
    },
    seccionVertical(parte3),
  ],
});
Packer.toBuffer(doc).then(buf => {
  const out = path.join(__dirname, "Ficha_Legal_y_de_Negocio.docx");
  fs.writeFileSync(out, buf);
  console.log("Listo:", out);
});
