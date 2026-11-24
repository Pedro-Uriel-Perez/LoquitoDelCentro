// Genera docs/manuales/Manual_de_Instalacion.docx  (node docs/manuales/manual_instalacion.js)
const fs = require("fs");
const path = require("path");
const { Document, Packer } = require("docx");
const c = require("./comun");
const { p, h1, h2, h3, vinetas, pasos, recuadro, figura, filaFiguras, tabla } = c;

const I = n => `docs/manuales/img/${n}`;

const cuerpo = [
  ...c.portada("Manual de Instalación", "Loquito del Centro — Plataformero 2D de acción"),
  ...c.indice(),

  // 3 ─────────────────────────────────────────────
  h1("1. Introducción"),
  p("**Loquito del Centro** es un videojuego de plataformas y acción en 2D con estética pixel art, inspirado en los clásicos góticos tipo Castlevania y ambientado en San Felipe, Guanajuato. El jugador controla al Loquito, un vagabundo del centro que recorre un jardín y tres mazmorras para derribar las antenas de La Señora."),
  p("Este manual explica cómo instalar y poner en marcha el juego en las dos plataformas en las que se distribuye:"),
  ...vinetas([
    "**Versión web (WebGL):** se juega directamente en el navegador desde la página del juego en itch.io, sin instalar nada.",
    "**Versión Android (.apk):** se instala en un celular o tableta con Android y se juega con controles táctiles en pantalla.",
  ]),
  p("También describe la configuración posterior, cómo verificar que la instalación funciona, la solución de los problemas más comunes y cómo actualizar el juego. El anexo A explica cómo abrir el proyecto fuente en Unity, dirigido a desarrolladores."),
  h3("Público al que va dirigido"),
  p("Jugadores sin conocimientos técnicos que quieran instalar el juego, y docentes o evaluadores que necesiten comprobar su funcionamiento. No se requiere experiencia previa con Unity ni con herramientas de desarrollo."),
  ...recuadro("nota", "Las capturas de este manual se tomaron con la versión 1.0 del juego. La apariencia de los menús de Android puede variar ligeramente según la marca del celular y la versión del sistema."),

  // 4 ─────────────────────────────────────────────
  h1("2. Requisitos previos"),
  h2("2.1 Versión web (navegador)"),
  ...tabla(["Elemento", "Requisito mínimo", "Recomendado"], [
    ["Sistema operativo", "Windows 10, macOS 11 o Linux con escritorio", "Windows 10/11"],
    ["Navegador", "Google Chrome, Microsoft Edge, Firefox u Opera actualizados, con WebGL 2.0", "Google Chrome o Microsoft Edge"],
    ["Procesador y memoria", "Doble núcleo, 4 GB de RAM", "Cuatro núcleos, 8 GB de RAM"],
    ["Gráficos", "Tarjeta compatible con WebGL 2.0 (integradas desde 2015)", "Aceleración por hardware activada en el navegador"],
    ["Conexión a internet", "Para la carga inicial (~65 MB)", "Banda ancha de 10 Mbps o más"],
    ["Pantalla", "960 × 540 píxeles", "1920 × 1080 en pantalla completa"],
    ["Controles", "Teclado", "Teclado o control (Xbox, PlayStation o genérico)"],
    ["Audio", "Bocinas o audífonos (opcional)", "Audífonos"],
  ], [2200, 4160, 3000]),
  h2("2.2 Versión Android"),
  ...tabla(["Elemento", "Requisito mínimo", "Recomendado"], [
    ["Sistema operativo", "Android 7.1 (API 25) o superior", "Android 10 o superior"],
    ["Procesador", "ARM de 64 bits (ARM64), presente en casi todos los celulares desde 2017", "ARM64 de los últimos 5 años"],
    ["Memoria RAM", "3 GB", "4 GB o más"],
    ["Almacenamiento libre", "250 MB", "500 MB"],
    ["Pantalla", "Táctil, 5 pulgadas", "6 pulgadas o más, en horizontal"],
    ["Permisos", "Permitir la instalación desde el navegador o el administrador de archivos", "—"],
  ], [2200, 4160, 3000]),
  ...recuadro("advertencia", "El archivo .apk no proviene de Google Play, por lo que Android pedirá autorizar la instalación de apps de origen desconocido. Descargue el archivo **solo desde la página oficial del juego** (sección 9, Datos de contacto)."),

  // 5 ─────────────────────────────────────────────
  h1("3. Procedimiento de instalación"),
  h2("3.1 Versión web en itch.io"),
  p("La versión web no se instala: se ejecuta dentro del navegador. Siga estos pasos:"),
  ...pasos([
    "**Abra** su navegador y **escriba** la dirección: **pedrourielperezmonzon.itch.io/loquitodelcentro**",
    "**Presione** el botón **Run game** que aparece en el recuadro del juego.",
    "**Espere** a que se complete la barra de carga. La primera vez descarga cerca de 65 MB; las siguientes veces carga más rápido.",
    "Cuando aparezca el menú principal, **haga clic** una vez dentro del juego para que reciba el teclado y comience la música.",
    "(Opcional) **Presione** el botón de pantalla completa de la esquina inferior derecha del recuadro.",
  ]),
  ...figura(I("web_pagina.png"), "Página del juego en itch.io con el botón Run game."),
  ...figura(I("web_carga.png"), "Pantalla de carga con el logo y la barra de progreso."),
  ...figura(I("web_menu.png"), "Menú principal del juego ejecutándose en el navegador."),
  ...recuadro("consejo", "En pantalla completa, la tecla **Esc** del navegador sale de la pantalla completa. Para abrir la configuración del juego use la tecla **Tab**."),

  h2("3.2 Versión Android (.apk)"),
  h3("Paso 1. Descargar el archivo"),
  ...pasos([
    "**Abra** el navegador del celular (por ejemplo, Chrome) y **entre** a **pedrourielperezmonzon.itch.io/loquitodelcentro**",
    "**Desplácese** hasta la sección **Download** y **toque** el archivo **LoquitoDelCentro.apk**.",
    "Si el navegador advierte que el archivo puede ser dañino, **toque Descargar de todas formas**. El archivo es seguro: se compiló directamente desde el proyecto del equipo.",
  ]),
  ...figura(I("web_descarga.png"), "Sección Download de la página del juego con el archivo para Android.", 480),
  h3("Paso 2. Autorizar la instalación"),
  ...pasos([
    "Cuando termine la descarga, **toque Abrir** en la notificación, o **abra** la app **Mis archivos** (o **Archivos**) y **entre** a **Descargas**.",
    "**Toque** el archivo **LoquitoDelCentro.apk**.",
    "Aparece el aviso **\"Por tu seguridad, el teléfono no tiene permitido actualmente instalar apps desconocidas de esta fuente\"**. **Toque Configuración**.",
    "En la lista **Instalar aplicaciones desconocidas**, **active** el interruptor de la app desde la que abrió el archivo (en la figura, **Mis archivos**). Enseguida aparece la ventana **¿Deseas instalar esta app?**.",
  ]),
  ...filaFiguras([[I("android_01_archivo.png"), "1. Archivo en Descargas"], [I("android_02_aviso.png"), "2. Aviso de seguridad"],
                  [I("android_02_permiso.png"), "3. Permiso activado"]],
                 "Ubicar el APK y autorizar la instalación de apps de origen desconocido (Samsung Galaxy A51, Android 13).", 330),
  h3("Paso 3. Instalar y abrir"),
  ...pasos([
    "En la ventana **¿Deseas instalar esta app?**, **toque Instalar**.",
    "Google Play Protect muestra **Se recomienda analizar la app**. **Toque Analizar app**: Google revisa el juego y, al terminar, indica **Esta app parece segura**.",
    "**Toque Instalar** para continuar.",
    "Al aparecer **Se instaló la app**, **toque Abrir**. A partir de ahora encontrará el ícono de **Loquito del Centro** en su lista de aplicaciones.",
    "**Gire** el celular en horizontal: el juego funciona solo en esa orientación.",
  ]),
  ...filaFiguras([[I("android_03_playprotect.png"), "4. Analizar app"], [I("android_03_segura.png"), "5. App segura"],
                  [I("android_04_listo.png"), "6. App instalada"]],
                 "Análisis de Google Play Protect e instalación terminada.", 330),
  ...recuadro("nota", "Si prefiere no enviar la app a Google, en el aviso de Play Protect **toque Más detalles** y luego **Instalar sin analizar**. El resultado es el mismo; analizarla es la opción más segura."),
  ...figura(I("android_05_menu.png"), "Menú principal del juego en el celular.", 520),
  ...recuadro("advertencia", "Después de instalar el juego, puede volver a desactivar el permiso **Permitir de esta fuente** en **Ajustes > Apps > Acceso especial > Instalar apps desconocidas**. Se recomienda hacerlo para mantener la seguridad del dispositivo."),

  // 6 ─────────────────────────────────────────────
  h1("4. Configuración posterior"),
  p("El juego funciona sin configuración adicional. Los siguientes ajustes son opcionales y se guardan automáticamente, aunque se cierre el juego."),
  h2("4.1 Menú de configuración"),
  p("**Abra** la configuración con el **engrane** de la esquina superior derecha, con la tecla **Esc** o **Tab**, con el botón **Start** del control, o con el botón **Atrás** en Android. Dentro de un nivel, abrir este menú **pausa el juego**."),
  ...tabla(["Opción", "Qué hace"], [
    ["Silencio", "Apaga por completo música y efectos. El ícono se tacha mientras está activo."],
    ["Música  (–  +)", "Baja o sube el volumen de la música en 10 niveles."],
    ["Efectos  (–  +)", "Baja o sube el volumen de los efectos de sonido en 10 niveles."],
    ["Jugar", "En el menú principal inicia una partida; dentro de un nivel reanuda el juego."],
    ["Créditos", "Muestra los integrantes del equipo y los recursos externos."],
    ["Salir", "Dentro de un nivel regresa al menú principal. En el menú principal cierra el juego (no aparece en la versión web)."],
  ], [2400, 6960]),
  ...figura(I("juego_config.png"), "Menú de configuración con volumen, silencio y botones."),
  h2("4.2 Ajustes recomendados por plataforma"),
  ...vinetas([
    "**Web:** active la **pantalla completa** para ver el juego más grande. Si el juego se ve lento, cierre otras pestañas y confirme que la **aceleración por hardware** del navegador esté activada.",
    "**Android:** **suba** el volumen multimedia del celular, **active** el modo **No molestar** para evitar notificaciones durante el juego y **desactive** el ahorro de batería extremo, que puede reducir el rendimiento.",
    "**Control (PC):** conecte el control **antes** de abrir el juego; se detecta solo, sin configuración.",
  ]),

  // 7 ─────────────────────────────────────────────
  h1("5. Verificación de instalación"),
  p("Compruebe que la instalación es correcta con la siguiente lista. Si algún punto falla, consulte la sección 6, Solución de problemas."),
  ...tabla(["#", "Prueba", "Resultado esperado", "✔"], [
    ["1", "Abrir el juego", "Aparece la pantalla de carga y después el menú principal con el logo.", "☐"],
    ["2", "Sonido", "Tras el primer clic o toque se escucha la música del menú.", "☐"],
    ["3", "Botón Jugar", "Carga el nivel El Jardín con el Loquito y el HUD (cara, panalitos y cartón).", "☐"],
    ["4", "Controles", "El personaje camina, salta y ataca con el teclado, el control o los botones táctiles.", "☐"],
    ["5", "Configuración", "El engrane abre el menú y el juego se pausa; Jugar lo reanuda.", "☐"],
    ["6", "Volumen guardado", "Al cerrar y volver a abrir el juego se conservan los volúmenes elegidos.", "☐"],
    ["7", "Orientación (Android)", "El juego se muestra en horizontal y gira entre las dos posiciones acostadas.", "☐"],
  ], [500, 2200, 5860, 800]),
  ...figura(I("juego_hud.png"), "Juego en funcionamiento: HUD arriba y botones táctiles abajo (Android)."),

  // 8 ─────────────────────────────────────────────
  h1("6. Solución de problemas"),
  h2("6.1 Versión web"),
  ...tabla(["Problema", "Causa probable", "Solución"], [
    ["La barra de carga no avanza o la pantalla se queda negra", "Conexión lenta o navegador sin WebGL 2.0", "Espere un minuto. Si no carga, actualice el navegador o use Chrome/Edge y recargue con Ctrl + F5."],
    ["Mensaje de error de WebGL", "Aceleración por hardware desactivada", "En Chrome: Configuración > Sistema > activar \"Usar aceleración de gráficos\" y reiniciar el navegador."],
    ["No se escucha nada", "El navegador bloquea el audio hasta la primera interacción, o el silencio está activo", "Haga clic dentro del juego. Revise el botón Silencio en la configuración y el volumen de la pestaña."],
    ["El teclado no responde", "El juego no tiene el foco", "Haga clic una vez dentro del recuadro del juego."],
    ["Esc saca de la pantalla completa", "Comportamiento del navegador", "Use Tab para abrir la configuración."],
    ["El juego se ve pequeño", "Modo incrustado en la página", "Presione el botón de pantalla completa del recuadro."],
  ], [2700, 2900, 3760]),
  h2("6.2 Versión Android"),
  ...tabla(["Problema", "Causa probable", "Solución"], [
    ["\"No se instaló la app\"", "Poco espacio, o una versión anterior firmada de otra forma", "Libere espacio. Si ya tenía el juego, desinstálelo y vuelva a instalar el APK."],
    ["\"Tu teléfono no puede instalar apps desconocidas\"", "Falta el permiso de la fuente", "Siga el paso 2 de la sección 3.2 (Permitir de esta fuente)."],
    ["Google Play Protect muestra un aviso", "La app no proviene de Google Play", "Toque Analizar app y después Instalar, o Más detalles > Instalar sin analizar."],
    ["\"El análisis del paquete falló\"", "Descarga incompleta o Android anterior a 7.1", "Descargue de nuevo el archivo y verifique la versión de Android."],
    ["El juego va lento o se calienta", "Ahorro de batería o muchas apps abiertas", "Cierre las demás apps y desactive el ahorro de batería extremo."],
    ["No aparecen los botones táctiles", "Se está en el menú principal", "Los botones aparecen solo dentro de los niveles; toque Jugar."],
  ], [2700, 2900, 3760]),

  // 9 ─────────────────────────────────────────────
  h1("7. Mantenimiento y actualización"),
  h2("7.1 Actualizar el juego"),
  ...vinetas([
    "**Web:** las actualizaciones se publican en la misma página. **Recargue** la página (Ctrl + F5) para obtener la versión más reciente.",
    "**Android:** **descargue** el APK nuevo desde la página del juego e **instálelo** encima del anterior; Android lo detecta como actualización y conserva los ajustes guardados.",
  ]),
  ...recuadro("advertencia", "Si al actualizar en Android aparece **\"La app no se instaló porque el paquete entra en conflicto\"**, **desinstale** primero la versión anterior y después instale la nueva. Al desinstalar se borran los ajustes guardados (volúmenes y final secreto desbloqueado)."),
  h2("7.2 Datos guardados"),
  p("El juego guarda en el dispositivo los volúmenes, el estado de silencio y si ya se vio el final secreto. En la versión web estos datos se guardan en el navegador: si borra los datos del sitio o usa una ventana de incógnito, se reinician."),
  h2("7.3 Desinstalar"),
  ...vinetas([
    "**Web:** no requiere desinstalación. Para borrar los datos guardados, elimine los datos del sitio itch.io (o de html-classic.itch.zone) en la configuración del navegador.",
    "**Android:** **mantenga presionado** el ícono de Loquito del Centro y **toque Desinstalar**, o vaya a **Ajustes > Apps > Loquito del Centro > Desinstalar**.",
  ]),

  // 10 ────────────────────────────────────────────
  h1("8. Anexos"),
  h2("Anexo A. Instalación del proyecto para desarrolladores"),
  p("Este anexo es para quien quiera abrir, modificar o compilar el código fuente."),
  ...pasos([
    "**Instale** Unity Hub y el editor **Unity 6000.3.24f1** con los módulos **WebGL Build Support** y **Android Build Support** (incluyendo OpenJDK y Android SDK & NDK Tools).",
    "**Clone** el repositorio: **git clone https://github.com/Pedro-Uriel-Perez/LoquitoDelCentro.git**",
    "En Unity Hub, **seleccione Add > Add project from disk** y **elija** la carpeta clonada.",
    "**Abra** la escena **Assets/Scenes/Inicio.unity** y **presione Play**.",
    "Para generar los ejecutables, **use** el menú **Loquito > Build web (itch)** o **Loquito > Build Android (APK)**. Los archivos quedan en la carpeta **Builds/**.",
  ]),
  ...recuadro("advertencia", "La primera apertura del proyecto tarda varios minutos mientras Unity importa los recursos. No cierre el editor durante ese proceso."),
  h2("Anexo B. Especificaciones técnicas"),
  ...tabla(["Concepto", "Valor"], [
    ["Motor", "Unity 6 (6000.3.24f1), Universal Render Pipeline 2D"],
    ["Entrada", "Input System: teclado, control y botones táctiles en pantalla"],
    ["Versión web", "WebGL con compresión Gzip; resolución base 960 × 540, escalable a pantalla completa"],
    ["Versión Android", "APK con IL2CPP para ARM64 (64 bits); identificador com.utng.loquitodelcentro; orientación horizontal"],
    ["Resolución del juego", "384 × 216 píxeles escalados (Pixel Perfect Camera, 16 píxeles por unidad)"],
    ["Escenas", "Inicio, El Jardín, Mazmorra 1, Mazmorra 2, Mazmorra 3 y Victoria"],
  ], [2600, 6760]),
  h2("Anexo C. Estructura del paquete web"),
  p("El archivo ZIP publicado en itch.io contiene **index.html** en la raíz, la carpeta **Build/** (datos, código WebAssembly y cargador) y la carpeta **TemplateData/** (logo y estilo de la pantalla de carga)."),

  // 11 ────────────────────────────────────────────
  ...c.contacto("9. "),
];

const doc = new Document({
  creator: "Pedro Uriel Pérez Monzón y Miguel Ángel Álvarez Ibarra",
  title: "Manual de Instalación — Loquito del Centro",
  styles: c.estilos, numbering: c.numeracion,
  sections: [c.seccion(cuerpo, "Manual de Instalación")],
});
Packer.toBuffer(doc).then(buf => {
  const out = path.join(__dirname, "Manual_de_Instalacion.docx");
  fs.writeFileSync(out, buf);
  console.log("Listo:", out);
});
