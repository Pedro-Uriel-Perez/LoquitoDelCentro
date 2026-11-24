// Genera docs/manuales/Manual_de_Usuario.docx  (node docs/manuales/manual_usuario.js)
const fs = require("fs");
const path = require("path");
const { Document, Packer } = require("docx");
const c = require("./comun");
const { p, h1, h2, h3, vinetas, pasos, recuadro, figura, filaFiguras, tabla } = c;

const I = n => `docs/manuales/img/${n}`;
const R = n => `docs/img/${n}`;

const cuerpo = [
  ...c.portada("Manual de Usuario", "Loquito del Centro — Guía para jugar"),
  ...c.indice(),

  // 3 ─────────────────────────────────────────────
  h1("1. Introducción"),
  p("Bienvenido a **Loquito del Centro**, un plataformero de acción en 2D con estética pixel art inspirado en los clásicos góticos tipo Castlevania. Este manual explica cómo jugar: los menús, los controles, las mecánicas, los niveles, los enemigos y los objetos, además de cómo resolver los problemas más comunes durante una partida."),
  p("El juego está pensado para jugadores de **15 a 25 años**, principalmente estudiantes mexicanos que disfrutan del humor de internet y de los retos de los plataformeros clásicos. Mezcla una estética de aventura épica con un protagonista absurdo armado con basura, en escenarios inspirados en San Felipe, Guanajuato."),
  ...recuadro("nota", "Para instalar el juego en la computadora (navegador) o en Android, consulte el **Manual de Instalación**. Este manual supone que el juego ya está funcionando."),

  // 4 ─────────────────────────────────────────────
  h1("2. Requisitos previos"),
  p("Antes de jugar, asegúrese de contar con lo siguiente:"),
  ...vinetas([
    "El juego **abierto** en el navegador (versión web) o **instalado** en Android (consulte el Manual de Instalación).",
    "Uno de estos métodos de control: **teclado**, **control** (Xbox, PlayStation o genérico) o **pantalla táctil** en Android.",
    "Bocinas o audífonos para disfrutar la música y los efectos (opcional, pero recomendado).",
    "Unos **30 a 60 minutos** para completar la aventura principal.",
  ]),
  ...recuadro("consejo", "En la versión web, **haga clic** una vez dentro del juego antes de empezar: así recibe el teclado y comienza la música."),

  // 5 ─────────────────────────────────────────────
  h1("3. Descripción general"),
  h2("3.1 Historia"),
  p("Unas **antenas** que instaló **La Señora** tienen a San Felipe bajo su señal. El **Loquito**, un vagabundo del centro al que nadie tomaba en serio, es inmune porque no tiene celular. Despierta en el jardín, recibe de **Martha la Loca** lo necesario para defenderse y baja a los templos a **derribar cada antena**. En cada templo lo espera un jefe que la protege."),
  h2("3.2 Objetivo"),
  ...vinetas([
    "Atravesar **El Jardín** y las **tres mazmorras**.",
    "Vencer al **jefe** de cada mazmorra y **derribar su antena**.",
    "Encontrar los **4 coleccionables** escondidos en cuartos secretos para desbloquear el **final secreto**.",
  ]),
  h2("3.3 Elementos de la pantalla (HUD)"),
  ...figura(I("juego_hud.png"), "Pantalla de juego con el HUD y los botones táctiles."),
  ...tabla(["Elemento", "Ubicación", "Qué indica"], [
    ["Cara del Loquito", "Arriba a la izquierda", "El personaje que controla."],
    ["Panalitos", "Junto a la cara", "Su vida: empieza con 5. Cada golpe recibido quita uno."],
    ["Cartón 3/3", "Debajo de los panalitos", "Golpes que puede bloquear el cartón antes de romperse."],
    ["Engrane", "Arriba a la derecha", "Abre el menú de configuración y pausa el juego."],
    ["Barra del jefe", "Abajo al centro", "Vida restante del jefe (solo en la sala del jefe)."],
    ["Botones táctiles", "Esquinas inferiores", "Controles en pantalla (solo en Android)."],
  ], [2300, 2500, 4560]),

  // 6 ─────────────────────────────────────────────
  h1("4. Guía de uso"),
  h2("4.1 Menú principal"),
  ...figura(I("android_05_menu.png"), "Menú principal."),
  ...tabla(["Botón", "Acción"], [
    ["Jugar", "Inicia una partida nueva desde El Jardín."],
    ["Créditos", "Muestra el equipo de desarrollo, la universidad y los recursos externos."],
    ["Salir", "Cierra el juego (no aparece en la versión web, porque el navegador no permite cerrar la página)."],
    ["Engrane", "Abre la configuración de sonido."],
  ], [2400, 6960]),
  p("**Seleccione** un botón con el mouse o el dedo, o **muévase** entre ellos con las flechas o el control y **presione** Enter / A para confirmar. El botón seleccionado se muestra en **rojo**."),
  ...recuadro("nota", "Después de ver el final secreto aparece en el menú principal la opción **Música del menú**, que permite elegir entre la canción original y la del final secreto. La elección se guarda."),

  h2("4.2 Configuración y pausa"),
  p("**Abra** la configuración con el **engrane**, con **Esc** o **Tab**, con **Start** en el control, o con el botón **Atrás** en Android. Dentro de un nivel el juego queda **en pausa** mientras el menú está abierto."),
  ...figura(I("juego_config.png"), "Menú de configuración."),
  ...vinetas([
    "**Silencio:** apaga todo el sonido. El ícono se tacha mientras está activo.",
    "**Música / Efectos:** **toque** – o + para bajar o subir el volumen (10 niveles cada uno).",
    "**Jugar:** reanuda la partida (en el menú principal, inicia una nueva).",
    "**Créditos:** muestra los créditos; **presione Volver** para regresar.",
    "**Salir:** dentro de un nivel regresa al menú principal; en el menú principal cierra el juego.",
  ]),

  h2("4.3 Controles"),
  ...tabla(["Acción", "Teclado", "Control", "Táctil (Android)"], [
    ["Moverse", "A / D o ← / →", "Stick izquierdo o cruceta", "Botones ◀ ▶"],
    ["Saltar / doble salto", "Espacio", "A / Cruz", "Botón grande ▲"],
    ["Atacar con la botella", "J", "X / Cuadrado", "Botón de la botella"],
    ["Cubrirse con el cartón", "K (mantener)", "Gatillo derecho (mantener)", "Botón del cartón (mantener)"],
    ["Interactuar", "E", "Y / Triángulo", "Botón E"],
    ["Configuración / pausa", "Esc o Tab", "Start", "Engrane o botón Atrás"],
  ], [2500, 2000, 2400, 2460]),
  ...recuadro("consejo", "En Android, **use** el pulgar izquierdo para moverse y el derecho para saltar y atacar. Puede mantener presionado ◀ o ▶ mientras toca Saltar."),

  h2("4.4 Mecánicas del juego"),
  h3("Vida y checkpoints"),
  ...vinetas([
    "El Loquito tiene **5 panalitos** de vida. Cada golpe quita uno y deja un momento de invulnerabilidad (el personaje parpadea).",
    "**Recoja** los **panalitos** tirados en el mapa para recuperar vida.",
    "**Toque** la **cama de cartón con fogata** (checkpoint) para guardar su punto de reaparición.",
    "Si pierde todos los panalitos, reaparece en el último checkpoint con la vida completa.",
  ]),
  h3("Los tres objetos del Loquito"),
  p("En El Jardín, **acérquese** a **Martha la Loca** y **presione Interactuar** para hablar con ella. Para avanzar el diálogo, **presione Interactuar** otra vez. Durante la plática le entrega tres objetos; cada uno es una habilidad:"),
  ...figura(I("juego_martha.png"), "Diálogo con Martha la Loca en El Jardín."),
  ...filaFiguras([[R("objetos/botella.png"), "Botella de chela"], [R("objetos/carton.png"), "Cartón"], [R("objetos/calcetas.png"), "Calcetas del Verde"]],
                 "Objetos que entrega Martha la Loca.", 110),
  ...tabla(["Objeto", "Cómo se usa", "Detalles"], [
    ["Botella de chela", "Presione Atacar", "Combo de **tres golpes** seguidos; el tercero empuja al enemigo. En el aire hace una **patada aérea**."],
    ["Cartón", "Mantenga presionado Cubrirse", "Bloquea golpes y proyectiles de frente. Mientras se cubre camina a la mitad de velocidad. **Aguanta 3 golpes** y se recupera solo tras **8 segundos**."],
    ["Calcetas del Verde", "Presione Saltar en el aire", "Permiten el **doble salto**. Se recarga al tocar el suelo."],
  ], [2300, 2600, 4460]),
  h3("Secretos, jefes y antenas"),
  ...vinetas([
    "Algunas paredes son **muros falsos**: **golpéelas** con la botella para abrir un **cuarto secreto** con un coleccionable.",
    "Al entrar a la **sala del jefe**, la compuerta se cierra. Si pierde, la compuerta se abre y la pelea se reinicia.",
    "Tras vencer al jefe, **acérquese** a la **antena** y **presione Interactuar** para derribarla.",
    "**Cruce** la **puerta de salida** e **interactúe** con ella para pasar al siguiente nivel.",
  ]),

  h2("4.5 Niveles"),
  ...tabla(["Nivel", "Descripción"], [
    ["El Jardín (tutorial)", "Carteles que enseñan cada control y el encuentro con Martha la Loca."],
    ["Mazmorra 1 — Templo del Reloj", "Primera mazmorra. Jefe: **El Campanero del Reloj**."],
    ["Mazmorra 2 — Templo del Pozo", "Plataformas que se atraviesan desde abajo. Jefe: **El Dragón Borracho**."],
    ["Mazmorra 3 — Templo de la Campana Rota", "El mapa más grande. Jefa final: **La Señora**."],
  ], [3300, 6060]),
  ...filaFiguras([[R("capturas/jardin_inicio.png"), "El Jardín"], [R("capturas/mazmorra1.png"), "Mazmorra 1"]], "Inicio de El Jardín y de la Mazmorra 1.", 150),
  ...filaFiguras([[R("capturas/mazmorra2.png"), "Mazmorra 2"], [R("capturas/mazmorra3.png"), "Mazmorra 3"]], "Inicio de la Mazmorra 2 y de la Mazmorra 3.", 150),
  ...recuadro("nota", "En la Mazmorra 2 y la Mazmorra 3 los enemigos **reaparecen** cuando el Loquito pierde todos sus panalitos."),

  h2("4.6 Enemigos y jefes"),
  h3("Enemigos"),
  ...tabla(["Enemigo", "Comportamiento", "Cómo vencerlo"], [
    ["Duende", "Corre de un lado a otro y salta de repente.", "Espere a que aterrice y golpéelo con el combo."],
    ["Esbirro alado", "Vuela patrullando y lanza bolas de chocolate.", "Bloquee los proyectiles con el cartón y alcáncelo con un salto."],
    ["Ángel blanco", "Estatua inmortal que lanza un espadazo si se acerca.", "No se puede destruir: espere a que ataque y pase durante su descanso."],
  ], [2000, 3700, 3660]),
  h3("Jefes"),
  ...filaFiguras([[R("personajes/campanero.png"), "El Campanero"], [R("personajes/dragon.png"), "El Dragón Borracho"], [R("personajes/senora.png"), "La Señora"]],
                 "Los tres jefes del juego.", 120),
  ...tabla(["Jefe", "Ataques", "Estrategia"], [
    ["El Campanero del Reloj (Mazmorra 1)", "Lejos: lanza su chancla. Cerca: golpe doble de garrote. Antes de atacar brilla y tiembla.", "Cuando **se cansa** se queda quieto unos segundos: aproveche para atacar."],
    ["El Dragón Borracho (Mazmorra 2)", "Lejos: tragos y 3 bolas de fuego, o un ave que persigue 3 segundos. Cerca: rasguño o embestida con onda expansiva.", "Salte las ondas expansivas y ataque mientras bebe de su copa."],
    ["La Señora (Mazmorra 3)", "Patada giratoria si salta cerca, golpe con onda o embestida de cerca, y dos aves que persiguen de lejos.", "Evite saltar junto a ella; ataque en su descanso después de cada ataque. Tiene 20 puntos de vida."],
  ], [2500, 3800, 3060]),
  ...figura(R("capturas/jefe_senora.png"), "Sala de La Señora, la jefa final.", 480),

  h2("4.7 Objetos y coleccionables"),
  ...filaFiguras([[R("objetos/bolsa.png"), "Bolsa de vagabundo"], [R("objetos/lata_frijoles.png"), "Lata de frijoles"],
                  [R("objetos/lata_coca.png"), "Lata de coca"], [R("objetos/caja_madera.png"), "Cajita de madera"]],
                 "Los cuatro coleccionables (uno por nivel).", 80),
  ...tabla(["Objeto", "Dónde está", "Para qué sirve"], [
    ["Panalito", "Tirado en los niveles", "Recupera un panalito de vida."],
    ["Checkpoint (cama y fogata)", "En puntos clave de cada nivel", "Guarda el punto de reaparición."],
    ["Bolsa de vagabundo", "Cuarto secreto de El Jardín", "Coleccionable 1 de 4."],
    ["Lata de frijoles", "Cuarto secreto de la Mazmorra 1", "Coleccionable 2 de 4."],
    ["Lata de coca", "Cuarto secreto de la Mazmorra 2", "Coleccionable 3 de 4."],
    ["Cajita de madera", "Cuarto secreto de la Mazmorra 3", "Coleccionable 4 de 4."],
  ], [3000, 3200, 3160]),
  p("Al tocar un coleccionable aparece una ventana con su imagen y descripción. **Presione Recoger** para guardarlo."),

  h2("4.8 Final del juego"),
  ...vinetas([
    "Al derribar la última antena aparece la **pantalla de victoria** con los coleccionables obtenidos.",
    "Con los **4 coleccionables** se muestra el **final secreto**, y en el menú principal se desbloquea la canción del final secreto.",
    "**Presione Volver al inicio** para regresar al menú principal.",
  ]),

  // 7 ─────────────────────────────────────────────
  h1("5. Mantenimiento y actualización"),
  ...vinetas([
    "**Progreso:** la partida empieza siempre desde El Jardín. El juego guarda los **ajustes de sonido** y si ya se vio el **final secreto**.",
    "**Web:** las actualizaciones aparecen al recargar la página del juego (Ctrl + F5).",
    "**Android:** descargue el APK nuevo e instálelo encima del anterior; los ajustes se conservan.",
    "Consulte el **Manual de Instalación** (sección 7) para más detalles sobre actualizar y desinstalar.",
  ]),
  ...recuadro("advertencia", "Borrar los datos del navegador o desinstalar la app en Android reinicia los ajustes y vuelve a bloquear la canción del final secreto."),

  // 8 ─────────────────────────────────────────────
  h1("6. Solución de problemas"),
  ...tabla(["Problema", "Solución"], [
    ["El personaje no se mueve", "Web: haga clic dentro del juego. Revise que el menú de configuración esté cerrado."],
    ["No puedo atacar, cubrirme ni hacer doble salto", "Hable primero con Martha la Loca en El Jardín (presione Interactuar junto a ella); ella entrega los objetos."],
    ["No hay sonido", "Haga clic o toque el juego una vez. Revise que Silencio esté desactivado y que los volúmenes no estén en cero."],
    ["Un jefe me derrota siempre", "Observe su aviso (destello o pose) antes de cada ataque y bloquee con el cartón. Recoja panalitos antes de entrar."],
    ["No encuentro un coleccionable", "Golpee con la botella las paredes sospechosas: los muros falsos se rompen."],
    ["La antena no se rompe", "Primero venza al jefe; después acérquese y presione Interactuar."],
    ["Esc saca de pantalla completa (web)", "Use Tab para abrir la configuración."],
    ["Los botones táctiles tapan algo", "Gire el celular en horizontal; los botones quedan en las esquinas inferiores."],
  ], [3300, 6060]),

  // 9 ─────────────────────────────────────────────
  h1("7. Glosario"),
  ...tabla(["Término", "Definición"], [
    ["APK", "Archivo de instalación de aplicaciones para Android."],
    ["Checkpoint", "Punto de control donde el personaje reaparece al perder toda su vida."],
    ["Coleccionable", "Objeto escondido; reunir los 4 desbloquea el final secreto."],
    ["Combo", "Serie de golpes seguidos; el tercero de la botella empuja al enemigo."],
    ["Doble salto", "Segundo salto en el aire que permiten las Calcetas del Verde."],
    ["HUD", "Información fija en pantalla: vida, cartón, engrane y barra del jefe."],
    ["Invulnerabilidad", "Momento después de recibir un golpe en el que no se recibe más daño."],
    ["Jefe", "Enemigo principal que protege la antena de cada mazmorra."],
    ["Muro falso", "Pared que se rompe de un botellazo y esconde un cuarto secreto."],
    ["Onda expansiva", "Ataque que avanza por el suelo; se esquiva saltando."],
    ["Panalito", "Unidad de vida del Loquito; también el objeto que la recupera."],
    ["Pixel art", "Estilo gráfico hecho con píxeles visibles, como en los juegos clásicos."],
    ["WebGL", "Tecnología que permite jugar en el navegador sin instalar nada."],
  ], [2300, 7060]),

  // 10 ────────────────────────────────────────────
  h1("8. Apéndices"),
  h2("Apéndice A. Personajes"),
  ...filaFiguras([[R("personajes/loquito.png"), "El Loquito"], [R("personajes/martha.png"), "Martha la Loca"], [R("personajes/duende.png"), "Duende"],
                  [R("personajes/esbirro.png"), "Esbirro alado"], [R("personajes/angel.png"), "Ángel blanco"]],
                 "Protagonista, aliada y enemigos comunes.", 90),
  h2("Apéndice B. Recorrido de la aventura"),
  ...tabla(["Orden", "Nivel", "Qué hacer para avanzar"], [
    ["1", "El Jardín", "Hablar con Martha, aprender los controles y cruzar la salida."],
    ["2", "Mazmorra 1", "Vencer al Campanero, derribar la antena y salir."],
    ["3", "Mazmorra 2", "Vencer al Dragón Borracho, derribar la antena y salir."],
    ["4", "Mazmorra 3", "Vencer a La Señora y derribar la última antena."],
    ["5", "Victoria", "Ver el final (o el final secreto con los 4 coleccionables)."],
  ], [1000, 2200, 6160]),
  h2("Apéndice C. Créditos"),
  ...vinetas([
    "**Equipo:** Pedro Uriel Pérez Monzón y Miguel Ángel Álvarez Ibarra.",
    "**Universidad Tecnológica del Norte de Guanajuato**, Ingeniería en Desarrollo y Gestión de Software, asignatura Creación de Videojuegos. Profesor: Gabriel Barrón Rodríguez.",
    "**Recursos externos:** GothicVania Church Pack, de Luis Zuno (ansimuz).",
    "Hecho con Unity 6.",
  ]),

  // 11 ────────────────────────────────────────────
  ...c.contacto("9. "),
];

const doc = new Document({
  creator: "Pedro Uriel Pérez Monzón y Miguel Ángel Álvarez Ibarra",
  title: "Manual de Usuario — Loquito del Centro",
  styles: c.estilos, numbering: c.numeracion,
  sections: [c.seccion(cuerpo, "Manual de Usuario")],
});
Packer.toBuffer(doc).then(buf => {
  const out = path.join(__dirname, "Manual_de_Usuario.docx");
  fs.writeFileSync(out, buf);
  console.log("Listo:", out);
});
