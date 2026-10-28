using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

/// <summary>
/// Construye las escenas del juego a partir de los mapas de texto de Assets/Levels.
/// Menú: Loquito > Construir todo. Editar un .txt y volver a construir regenera la escena.
///
/// Leyenda de los mapas (la primera línea es la fila de arriba):
///   #  bloque sólido            B  muro falso (se rompe con la botella)
///   P  inicio del jugador       C  checkpoint (cama + fogata)
///   H  panalito                 K  coleccionable del nivel
///   D  duende                   E  esbirro alado            A  ángel blanco
///   G  compuerta del jefe       J  jefe (campanero)         N  antena
///   [ ]  esquinas inferior-izquierda y superior-derecha del interior de la arena
///   M  Martha la Loca (jardín)    X  puerta de salida del nivel
///   Q  duende inmóvil (jardín)
///   1..6  carteles del jardín: 1 mover, 2 saltar, 3 doble salto, 4 atacar, 5 bloquear, 6 interactuar
/// </summary>
public static class LoquitoLevelBuilder
{
    const string Propios = "Assets/Sprites/Propios/";
    const string Tiles = "Assets/Sprites/Tiles/";
    const string Prefabs = "Assets/Prefabs/";
    const float CamHalfW = 12f, CamHalfH = 6.75f;

    class LevelData
    {
        public int number, collectibleId; public string name, theme = "templo", bossName, itemKey, itemName, itemDesc;
        public float telegraph, pause; public int chanclas;
        public bool IsPark => theme == "parque";
    }

    static readonly LevelData Jardin = new LevelData { number = 0, name = "El Jardín", theme = "parque", collectibleId = 1, itemKey = "bolsa",
        itemName = "Bolsa de vagabundo", itemDesc = "Como la del Chavo del 8." };

    static string SceneName(LevelData d) => d.number == 0 ? "Jardin" : $"Mazmorra{d.number}";

    static readonly LevelData[] Levels =
    {
        new LevelData { number = 1, collectibleId = 2, name = "Templo del Reloj", bossName = "El Campanero del Reloj", itemKey = "lata_frijoles",
            itemName = "Lata de frijoles", itemDesc = "Mejor esto a un suaperro.", telegraph = 0.5f, pause = 1.0f, chanclas = 1 },
        new LevelData { number = 2, collectibleId = 3, name = "Templo del Pozo", bossName = "El Campanero del Pozo", itemKey = "lata_coca",
            itemName = "Lata de coca", itemDesc = "Ya junté 3 kilos de lata, puedo comprarme una de verdad.", telegraph = 0.45f, pause = 0.85f, chanclas = 2 },
        new LevelData { number = 3, collectibleId = 4, name = "Templo de la Campana Rota", bossName = "El Campanero de la Campana Rota", itemKey = "caja_madera",
            itemName = "Cajita de madera", itemDesc = "¡¡¡Mi trono!!!", telegraph = 0.4f, pause = 0.75f, chanclas = 2 },
    };

    static readonly Color SkyColor = new Color32(35, 137, 225, 255);
    static readonly Color DungeonColor = new Color32(39, 38, 56, 255);

    // ======================================================================
    [MenuItem("Loquito/Construir todo")]
    public static void BuildAll()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        ConfigureSprites();
        ConfigureAudio();
        ConfigureTiles();
        UpdateLoquito();
        BuildPrefabs();
        BuildLevel(Jardin);
        foreach (var l in Levels) BuildLevel(l);
        BuildTitleScene();
        BuildVictoryScene();
        DeleteOldScenes();
        UpdateBuildSettings();
        EditorSceneManager.OpenScene("Assets/Scenes/Inicio.unity");
        Debug.Log("[Loquito] Escenas construidas: Inicio, Jardin, Mazmorra1-3, Victoria.");
    }

    [MenuItem("Loquito/Construir solo mazmorras")]
    public static void BuildLevelsOnly()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        ConfigureTiles();
        BuildLevel(Jardin);
        foreach (var l in Levels) BuildLevel(l);
    }

    /// <summary>Escenas que ya no forman parte del juego (el juego son 6 escenas).</summary>
    static void DeleteOldScenes()
    {
        foreach (var n in new[] { "Nivel0", "Mazmorra4", "NivelPrueba", "SampleScene" })
            AssetDatabase.DeleteAsset($"Assets/Scenes/{n}.unity");
    }

    /// <summary>
    /// Los bloques de las mazmorras usan colisión de cuadro completo. Con la forma del sprite, las esquinas
    /// redondeadas de cada tile dejaban muescas en el piso donde los duendes se quedaban atorados.
    /// </summary>
    static void ConfigureTiles()
    {
        foreach (var (c, r) in new[] { (1, 7), (2, 7), (4, 7), (5, 7), (1, 8), (2, 8), (4, 8), (5, 8), (1, 5) })
        {
            var t = AssetDatabase.LoadAssetAtPath<Tile>($"{Tiles}tile_c{c}_r{r}.asset");
            if (t == null || t.colliderType == Tile.ColliderType.Grid) continue;
            t.colliderType = Tile.ColliderType.Grid;
            EditorUtility.SetDirty(t);
        }
        if (!AssetDatabase.IsValidFolder(Tiles + "Parque")) AssetDatabase.CreateFolder(Tiles.TrimEnd('/'), "Parque");
        foreach (var n in new[] { "pasto", "tierra", "tierra_oscura" })
        {
            string path = $"{Tiles}Parque/{n}.asset";
            var t = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (t == null) { t = ScriptableObject.CreateInstance<Tile>(); AssetDatabase.CreateAsset(t, path); }
            t.sprite = S($"Parque/Tiles/{n}.png");
            t.colliderType = Tile.ColliderType.Grid;
            EditorUtility.SetDirty(t);
        }
        AssetDatabase.SaveAssets();
    }

    // ======================================================================
    // Sprites y prefabs
    // ======================================================================

    /// <summary>Música en streaming (no se carga completa en memoria); efectos descomprimidos para que suenen al instante.</summary>
    static void ConfigureAudio()
    {
        foreach (var g in AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/Resources/Audio" }))
        {
            string p = AssetDatabase.GUIDToAssetPath(g);
            var imp = AssetImporter.GetAtPath(p) as AudioImporter;
            if (imp == null) continue;
            bool isMusic = p.Contains("/Musica/");
            var s = imp.defaultSampleSettings;
            var load = isMusic ? AudioClipLoadType.Streaming : AudioClipLoadType.DecompressOnLoad;
            if (s.loadType == load) continue;
            s.loadType = load;
            s.compressionFormat = AudioCompressionFormat.Vorbis;
            s.quality = isMusic ? 0.7f : 0.9f;
            imp.defaultSampleSettings = s;
            imp.SaveAndReimport();
        }
    }

    static void ConfigureSprites()
    {
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (var g in AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Sprites/Propios", "Assets/Resources/Menu" }))
            {
                string p = AssetDatabase.GUIDToAssetPath(g);
                var imp = AssetImporter.GetAtPath(p) as TextureImporter;
                if (imp == null) continue;
                bool ui = p.Contains("/Propios/UI/") || p.Contains("/Resources/Menu/");
                bool center = ui || p.Contains("/Esbirro/") || p.Contains("chancla_proyectil") || p.Contains("/Parque/Tiles/");
                int ppu = p.Contains("/Parque/Carteles/") || p.Contains("/Parque/Fondos/") ? 32 : 16;
                var filter = ui ? FilterMode.Bilinear : FilterMode.Point;   // la interfaz se agranda: filtro suave
                bool changed = imp.textureType != TextureImporterType.Sprite || imp.spritePixelsPerUnit != ppu || imp.filterMode != filter
                               || imp.textureCompression != TextureImporterCompression.Uncompressed;
                var s = new TextureImporterSettings(); imp.ReadTextureSettings(s);
                int align = (int)(center ? SpriteAlignment.Center : SpriteAlignment.BottomCenter);
                changed |= s.spriteAlignment != align;
                if (!changed) continue;
                imp.textureType = TextureImporterType.Sprite; imp.spriteImportMode = SpriteImportMode.Single; imp.spritePixelsPerUnit = ppu;
                imp.maxTextureSize = 4096;
                imp.filterMode = filter; imp.textureCompression = TextureImporterCompression.Uncompressed; imp.mipmapEnabled = false;
                imp.ReadTextureSettings(s); s.spriteAlignment = align; s.spriteMeshType = SpriteMeshType.FullRect; imp.SetTextureSettings(s);
                imp.SaveAndReimport();
            }
        }
        finally { AssetDatabase.StopAssetEditing(); }
    }

    static void UpdateLoquito()
    {
        // Idle nuevo (7 cuadros). Se reescribe la curva del clip existente para no romper la referencia del Animator.
        var idle = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Animations/Loquito/Idle.anim");
        if (idle != null) SetClipFrames(idle, Frames("Loquito/idle"), 7f, true);

        string lp = Prefabs + "Loquito.prefab";
        var root = PrefabUtility.LoadPrefabContents(lp);
        var dbg = root.GetComponent("DebugHUD");
        if (dbg != null) Object.DestroyImmediate(dbg);
        PrefabUtility.SaveAsPrefabAsset(root, lp);
        PrefabUtility.UnloadPrefabContents(root);
    }

    static void BuildPrefabs()
    {
        int jug = 1 << LayerMask.NameToLayer("Jugador"), suelo = 1 << LayerMask.NameToLayer("Suelo");
        int ene = LayerMask.NameToLayer("Enemigo");

        // Panalito
        var pan = new GameObject("Panalito");
        var psr = pan.AddComponent<SpriteRenderer>(); psr.sprite = Frames("Objetos/panalito_gira")[0]; psr.sortingOrder = 8;
        var pcol = pan.AddComponent<CircleCollider2D>(); pcol.isTrigger = true; pcol.radius = 0.7f; pcol.offset = new Vector2(0, 0.8f);
        var panim = pan.AddComponent<SpriteAnim>();
        SetClips(panim, "gira", ("gira", Frames("Objetos/panalito_gira"), 8f, true), ("recoger", Frames("Objetos/panalito_recoger"), 6f, false),
                 ("desaparecer", Frames("Objetos/panalito_desaparecer"), 10f, false));
        Set(pan.AddComponent<PanalitoPickup>(), "anim", panim);
        SavePrefab(pan, "Panalito");

        // Checkpoint: cama + fogata
        var cp = new GameObject("Checkpoint");
        var cpc = cp.AddComponent<BoxCollider2D>(); cpc.isTrigger = true; cpc.size = new Vector2(4f, 3f); cpc.offset = new Vector2(0, 1.5f);
        cp.AddComponent<Checkpoint>();
        var cama = Child(cp, "Cama", new Vector3(-1.5f, 0, 0)); var csr = cama.AddComponent<SpriteRenderer>(); csr.sprite = Frames("Objetos/cama")[0]; csr.sortingOrder = -3;
        SetClips(cama.AddComponent<SpriteAnim>(), "cama", ("cama", Frames("Objetos/cama"), 4f, true));
        var fog = Child(cp, "Fogata", new Vector3(1.3f, 0, 0)); var fsr = fog.AddComponent<SpriteRenderer>(); fsr.sprite = Frames("Objetos/fogata")[0]; fsr.sortingOrder = -2;
        SetClips(fog.AddComponent<SpriteAnim>(), "fuego", ("fuego", Frames("Objetos/fogata"), 10f, true));
        SavePrefab(cp, "Checkpoint");

        // Antena
        var ant = new GameObject("Antena");
        var asr = ant.AddComponent<SpriteRenderer>(); asr.sprite = Frames("Objetos/antena_activa")[0]; asr.sortingOrder = -2;
        var aanim = ant.AddComponent<SpriteAnim>();
        SetClips(aanim, "activa", ("activa", Frames("Objetos/antena_activa"), 8f, true), ("rota", Frames("Objetos/antena_rota"), 8f, true));
        Set(ant.AddComponent<Antenna>(), "anim", aanim);
        SavePrefab(ant, "Antena");

        // Chancla
        var ch = new GameObject("Chancla");
        var chsr = ch.AddComponent<SpriteRenderer>(); chsr.sprite = S("Enemigos/Campanero/chancla_proyectil.png"); chsr.sortingOrder = 12;
        var proj = ch.AddComponent<Projectile>();
        Set(proj, "speed", 6f); Set(proj, "radius", 0.45f); Set(proj, "spinSpeed", 720f); Set(proj, "lifetime", 5f);
        Set(proj, "playerLayer", jug); Set(proj, "groundLayer", suelo);
        var chanclaPrefab = SavePrefab(ch, "Chancla");

        // Campanero
        var boss = new GameObject("Campanero"); boss.layer = ene;
        var brb = boss.AddComponent<Rigidbody2D>(); brb.bodyType = RigidbodyType2D.Kinematic;
        var bcol = boss.AddComponent<BoxCollider2D>(); bcol.size = new Vector2(2.6f, 4.6f); bcol.offset = new Vector2(0, 2.3f);
        var bvis = Child(boss, "Sprite", Vector3.zero); bvis.layer = ene;
        var bsr = bvis.AddComponent<SpriteRenderer>(); bsr.sprite = Frames("Enemigos/Campanero/quieto")[0]; bsr.sortingOrder = 6;
        var tp = Child(boss, "PuntoChancla", new Vector3(1.8f, 1.6f, 0));
        var bh = boss.AddComponent<Health>(); Set(bh, "maxHealth", 15); Set(bh, "destroyOnDeath", false); Set(bh, "hitStunTime", 0f); Set(bh, "spriteRenderer", bsr);
        var bc = boss.AddComponent<BossCampanero>();
        Set(bc, "spriteRenderer", bsr);
        Set(bc, "idleFrames", Frames("Enemigos/Campanero/quieto")); Set(bc, "clubFrames", Frames("Enemigos/Campanero/ataque"));
        Set(bc, "throwFrames", Frames("Enemigos/Campanero/chancla")); Set(bc, "defeatedFrames", Frames("Enemigos/Campanero/derrotado"));
        Set(bc, "playerLayer", jug); Set(bc, "chanclaPrefab", chanclaPrefab.GetComponent<Projectile>()); Set(bc, "throwPoint", tp.transform);
        var bcd = boss.AddComponent<ContactDamage>(); Set(bcd, "body", bcol); Set(bcd, "playerLayer", jug);
        SavePrefab(boss, "Campanero");

        // Duende: sin fricción para que no se pegue a paredes ni bordes
        var pm = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>("Assets/Materials/SinFriccion.physicsMaterial2D");
        EditPrefab("Duende", root => root.GetComponent<Rigidbody2D>().sharedMaterial = pm);
        // Esbirro: vuela a altura fija sobre el suelo (alcanzable saltando)
        EditPrefab("EsbirroAlado", root =>
        {
            Set(root.GetComponent<FlyingEnemy>(), "groundLayer", suelo);
            root.GetComponent<CircleCollider2D>().radius = 1f;
        });

        // Duende inmóvil: copia del duende sin patrulla ni saltos (sí hace daño al tocarlo)
        {
            var root = PrefabUtility.LoadPrefabContents(Prefabs + "Duende.prefab");
            foreach (var c in new Component[] { root.GetComponent<EnemyPatrol>(), root.GetComponent<RandomJumper>() }) if (c != null) Object.DestroyImmediate(c);
            var vis = root.transform.Find("Sprite");
            var an = vis.GetComponent<Animator>(); if (an != null) Object.DestroyImmediate(an);
            vis.GetComponent<SpriteRenderer>().sprite = Frames("Enemigos/Duende/saltar")[0];
            root.name = "DuendeQuieto";
            PrefabUtility.SaveAsPrefabAsset(root, Prefabs + "DuendeQuieto.prefab");
            PrefabUtility.UnloadPrefabContents(root);
        }

        // Martha la Loca
        var martha = new GameObject("MarthaLaLoca");
        var msr = martha.AddComponent<SpriteRenderer>(); msr.sprite = Frames("NPC/Martha/roncar")[0]; msr.sortingOrder = -1;
        var manim = martha.AddComponent<SpriteAnim>();
        SetClips(manim, "roncar", ("roncar", Frames("NPC/Martha/roncar"), 5f, true), ("hablar", Frames("NPC/Martha/hablar"), 6f, true));
        var anchor = Child(martha, "Nube", new Vector3(-0.6f, 2.8f, 0));
        var npc = martha.AddComponent<MarthaNPC>();
        Set(npc, "anim", manim); Set(npc, "bubbleAnchor", anchor.transform);
        SetStrings(npc, "lines", MarthaLines);
        SetGifts(npc,
            (6, "Cartón", "Te protege de los golpes de frente.\nMantén K (gatillo derecho) para cubrirte. Aguanta 3 golpes y se recupera solo.", S("UI/item_carton.png")),
            (7, "Botella de chela", "Rómpela y tendrás un arma.\nPresiona J (botón X) para atacar. Tres golpes seguidos hacen un combo.", S("UI/item_botella.png")),
            (8, "Calcetas del Verde", "Calcetas de la suerte.\nPresiona Espacio otra vez en el aire para hacer doble salto.", S("UI/item_calcetas.png")));
        SavePrefab(martha, "MarthaLaLoca");
    }

    static readonly string[] MarthaLines =
    {
        "¿Eres tú? ¿Eres tú? ¡Loquito del Centro!",
        "¡Sí eres tú! Qué alegría verte :D",
        "Nuestro amado pueblo ha sido atacado por La Señora.",
        "Ese malvado ser atacó a los sacerdotes de nuestras iglesias.",
        "Implantó chips de control 5G y antenas en cada una de ellas.",
        "¿Puedes ir a quitarlas? Son un fastidio... Pillophon dejó de funcionar desde que llegaron.",
        "Para ayudarte te daré mis mayores tesoros.",
        "Eso te ayudará a protegerte. Toma esto también.",
        "Rómpela y tendrás un arma... y falta lo mejor.",
        "Mis calcetas de la suerte: póntelas y llegarás alto.",
        "Confío en ti, Loquito. Hazlo por mí, ¿ok?",
    };

    static void EditPrefab(string name, System.Action<GameObject> edit)
    {
        string path = Prefabs + name + ".prefab";
        var root = PrefabUtility.LoadPrefabContents(path);
        edit(root);
        PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);
    }

    // ======================================================================
    // Mazmorras
    // ======================================================================

    static void BuildLevel(LevelData data)
    {
        string mapPath = $"Assets/Levels/{SceneName(data)}.txt";
        var rows = File.ReadAllLines(mapPath).Where(r => r.Length > 0).ToList();
        int H = rows.Count, W = rows.Max(r => r.Length);
        char At(int x, int y) => (x < 0 || y < 0 || x >= W || y >= H || x >= rows[H - 1 - y].Length) ? '#' : rows[H - 1 - y][x];
        bool Solid(int x, int y) { char c = At(x, y); return c == '#' || c == 'B'; }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        int sueloLayer = LayerMask.NameToLayer("Suelo"), ene = LayerMask.NameToLayer("Enemigo");

        // ---- Tilemaps ----
        var grid = new GameObject("Grid"); grid.AddComponent<Grid>();
        var ground = Child(grid, "Suelo", Vector3.zero); ground.layer = sueloLayer;
        var map = ground.AddComponent<Tilemap>(); ground.AddComponent<TilemapRenderer>().sortingOrder = 0;
        var grb = ground.AddComponent<Rigidbody2D>(); grb.bodyType = RigidbodyType2D.Static;
        var tcol = ground.AddComponent<TilemapCollider2D>(); tcol.compositeOperation = Collider2D.CompositeOperation.Merge;
        ground.AddComponent<CompositeCollider2D>().geometryType = CompositeCollider2D.GeometryType.Polygons;

        TileBase TileFor(int x, int y)
        {
            if (data.IsPark)
            {
                if (!Solid(x, y + 1)) return ParkTile("pasto");
                return Solid(x, y + 2) && Solid(x, y + 3) ? ParkTile("tierra_oscura") : ParkTile("tierra");
            }
            int pair = ((x / 2) * 7 + (y / 2) * 3) % 2 == 0 ? 1 : 4;   // alterna los dos estilos de bloque
            int c = pair + (x % 2);
            if (!Solid(x, y + 1)) return T(c, 7);                       // cara superior
            bool edge = !Solid(x - 1, y) || !Solid(x + 1, y) || !Solid(x, y - 1) || !Solid(x, y + 2);
            return edge ? T(c, 8) : T(1, 5);                            // borde o relleno oscuro
        }

        var entities = new List<(char c, int x, int y)>();
        var breakable = new List<Vector2Int>();
        var gateCells = new List<Vector2Int>();
        Vector2Int arenaMin = Vector2Int.zero, arenaMax = Vector2Int.zero;
        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
            {
                char c = At(x, y);
                switch (c)
                {
                    case '#': map.SetTile(new Vector3Int(x, y, 0), TileFor(x, y)); break;
                    case 'B': breakable.Add(new Vector2Int(x, y)); break;
                    case 'G': gateCells.Add(new Vector2Int(x, y)); break;
                    case '[': arenaMin = new Vector2Int(x, y); break;
                    case ']': arenaMax = new Vector2Int(x, y); break;
                    case ' ': break;
                    default: entities.Add((c, x, y)); break;
                }
            }

        // ---- Muro falso (cuarto secreto) ----
        if (breakable.Count > 0)
        {
            var wall = Child(grid, "MuroFalso", Vector3.zero); wall.layer = sueloLayer;
            var wmap = wall.AddComponent<Tilemap>(); wall.AddComponent<TilemapRenderer>().sortingOrder = 0;
            foreach (var b in breakable) wmap.SetTile(new Vector3Int(b.x, b.y, 0), TileFor(b.x, b.y));
            var wrb = wall.AddComponent<Rigidbody2D>(); wrb.bodyType = RigidbodyType2D.Static;
            wall.AddComponent<TilemapCollider2D>();
            var hit = Child(wall, "Golpeable", Vector3.zero); hit.layer = ene;
            var hb = hit.AddComponent<BoxCollider2D>();
            int bx0 = breakable.Min(b => b.x), bx1 = breakable.Max(b => b.x), by0 = breakable.Min(b => b.y), by1 = breakable.Max(b => b.y);
            hb.offset = new Vector2((bx0 + bx1 + 1) / 2f, (by0 + by1 + 1) / 2f);
            hb.size = new Vector2(bx1 - bx0 + 1, by1 - by0 + 1);
            Set(wall.AddComponent<Health>(), "maxHealth", 1);
            Set(wall.AddComponent<SecretWall>(), "message", "¡Un cuarto secreto!");

            // Cubierta: tapa el interior del cuarto para que el coleccionable no se vea desde afuera.
            // Es hija del muro falso, así que desaparece cuando el muro se rompe. No tiene colisión.
            var cover = Child(wall, "Cubierta", Vector3.zero); cover.layer = 0;
            var cmap = cover.AddComponent<Tilemap>(); cover.AddComponent<TilemapRenderer>().sortingOrder = 9;
            foreach (int row in breakable.Select(b => b.y).Distinct())
                for (int x = bx1 + 1; x < W && !Solid(x, row); x++)
                    cmap.SetTile(new Vector3Int(x, row, 0), data.IsPark ? ParkTile("tierra") : T(1 + (x % 2), 8));   // relleno liso, sin "superficie"
        }

        // ---- Entidades ----
        GameObject Spawn(string prefab, float x, float y, Transform parent)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + prefab + ".prefab"), scene);
            go.transform.position = new Vector3(x, y, 0);
            if (parent != null) go.transform.SetParent(parent, true);
            return go;
        }
        var enemies = new GameObject("Enemigos").transform;
        var objects = new GameObject("Objetos").transform;
        GameObject player = null; BossCampanero boss = null; Antenna antenna = null;
        foreach (var (c, x, y) in entities)
        {
            float cx = x + 0.5f;
            switch (c)
            {
                case 'P': player = Spawn("Loquito", cx, y + 0.05f, null); break;
                case 'C': Spawn("Checkpoint", cx, y, objects); break;
                case 'H': Spawn("Panalito", cx, y + 0.2f, objects); break;
                case 'D': Spawn("Duende", cx, y + 0.1f, enemies); break;
                case 'E': Spawn("EsbirroAlado", cx, y + 0.5f, enemies); break;
                case 'A': Spawn("AngelBlanco", cx, y, enemies); break;
                case 'J':
                    var bgo = Spawn("Campanero", cx, y, enemies);
                    boss = bgo.GetComponent<BossCampanero>();
                    Set(boss, "bossName", data.bossName); Set(boss, "telegraphTime", data.telegraph); Set(boss, "pauseBetweenAttacks", data.pause);
                    Set(boss, "chanclasPerThrow", data.chanclas); Set(boss, "aimAtPlayer", true);
                    break;
                case 'N':
                    antenna = Spawn("Antena", cx, y, objects).GetComponent<Antenna>();
                    if (data.IsPark)
                    {
                        Set(antenna, "prompt", "E / Y: Derribar la torre de radio");
                        Set(antenna, "destroyedMessage", "¡Torre de radio derribada!");
                    }
                    break;
                case 'Q':
                    var dq = Spawn("DuendeQuieto", cx, y + 0.1f, enemies);
                    dq.transform.localScale = new Vector3(-1f, 1f, 1f);   // mira hacia donde llega el jugador
                    break;
                case '1': case '2': case '3': case '4': case '5': case '6': CreateSign(c, cx, y, objects); break;
                case 'K': CreateCollectible(data, cx, y + 0.3f, objects); break;
                case 'M': Spawn("MarthaLaLoca", cx, y, objects); break;
                case 'X': CreateExit(cx, y, objects); break;
            }
        }
        if (antenna != null && boss != null) Set(antenna, "requiredBoss", boss);

        // ---- Arena del jefe ----
        if (gateCells.Count > 0 && boss != null)
        {
            int gx0 = gateCells.Min(g => g.x), gx1 = gateCells.Max(g => g.x), gy0 = gateCells.Min(g => g.y), gy1 = gateCells.Max(g => g.y);
            var gate = new GameObject("Compuerta"); gate.layer = sueloLayer;
            gate.transform.position = new Vector3((gx0 + gx1 + 1) / 2f, (gy0 + gy1 + 1) / 2f, 0);
            var gsr = gate.AddComponent<SpriteRenderer>(); gsr.sprite = SpriteOfTile(T(4, 8)); gsr.drawMode = SpriteDrawMode.Tiled;
            gsr.size = new Vector2(gx1 - gx0 + 1, gy1 - gy0 + 1); gsr.sortingOrder = 1; gsr.color = new Color(0.75f, 0.7f, 0.8f);
            var gcol = gate.AddComponent<BoxCollider2D>(); gcol.size = gsr.size;
            var gr = gate.AddComponent<Rigidbody2D>(); gr.bodyType = RigidbodyType2D.Kinematic;

            var arena = new GameObject("ArenaJefe");
            float ax0 = gx1 + 3, ax1 = arenaMax.x + 1, ay0 = arenaMin.y, ay1 = arenaMax.y + 1;
            arena.transform.position = new Vector3((ax0 + ax1) / 2f, (ay0 + ay1) / 2f, 0);
            var acol = arena.AddComponent<BoxCollider2D>(); acol.isTrigger = true; acol.size = new Vector2(ax1 - ax0, ay1 - ay0);
            var ba = arena.AddComponent<BossArena>();
            Set(ba, "boss", boss); Set(ba, "gate", gate.transform); Set(ba, "gateOpenHeight", (float)(gy1 - gy0 + 2));
            // Área visible durante la pelea: la arena con sus muros, piso y techo
            Set(ba, "cameraMin", new Vector2(arenaMin.x - 2f, 0f));
            Set(ba, "cameraMax", new Vector2(arenaMax.x + 3f, arenaMax.y + 3f));
        }

        // ---- Jugador y cámara ----
        if (player != null)
        {
            var ph = player.GetComponent<PlayerHealth>();
            Set(ph, "killHeight", -6f);
        }
        var cam = CreateCamera(player != null ? player.transform : null, Vector2.zero, new Vector2(W, H));
        if (data.IsPark) cam.backgroundColor = SkyColor;
        Vector3 camStart = cam.transform.position;

        // ---- Fondos con parallax y decoración ----
        if (data.IsPark)
        {
            CreateParkBackground(camStart, W);
            // Vacío oscuro solo dentro de los hoyos del piso (columnas sin bloque en la fila de abajo)
            var holes = new GameObject("Vacio").transform;
            var square = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Cuadrado.png");   // 1x1 unidad, blanco
            for (int x = 0; x < W; x++)
            {
                if (Solid(x, 0)) continue;
                int x1 = x; while (x1 + 1 < W && !Solid(x1 + 1, 0)) x1++;
                var hole = new GameObject("Hoyo"); hole.transform.SetParent(holes, false);
                var hsr = hole.AddComponent<SpriteRenderer>(); hsr.sprite = square;
                hsr.color = new Color32(28, 20, 24, 255); hsr.sortingOrder = -15;
                hole.transform.position = new Vector3((x + x1 + 1) / 2f, 2.6f - 10f, 0);
                hole.transform.localScale = new Vector3(x1 - x + 1, 20f, 1);
                x = x1;
            }
        }
        else CreateParallax(camStart, W);
        Decorate(data, W, H, At, Solid, entities);

        // ---- Datos del nivel y HUD ----
        var info = new GameObject("Nivel").AddComponent<LevelInfo>();
        Set(info, "levelNumber", data.number); Set(info, "levelName", data.name);
        CreateHUD();

        // La geometría fusionada del CompositeCollider se guarda con la escena: hay que generarla
        // antes de guardar o el piso queda sin colisión.
        map.CompressBounds();
        tcol.ProcessTilemapChanges();
        var composite = ground.GetComponent<CompositeCollider2D>();
        composite.GenerateGeometry();
        if (composite.pathCount == 0)
        {
            composite.geometryType = CompositeCollider2D.GeometryType.Outlines;
            composite.GenerateGeometry();
        }
        if (composite.pathCount == 0) Debug.LogError($"[Loquito] {SceneName(data)}: el piso quedó sin colisión.");
        else Debug.Log($"[Loquito] {SceneName(data)}: colisión del piso con {composite.pathCount} contornos.");
        EditorSceneManager.SaveScene(scene, $"Assets/Scenes/{SceneName(data)}.unity");
    }

    static void CreateCollectible(LevelData data, float x, float y, Transform parent)
    {
        var go = new GameObject("Coleccionable_" + data.itemKey);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0);
        var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = S($"Objetos/coleccionable_{data.itemKey}.png"); sr.sortingOrder = 8;
        var col = go.AddComponent<CircleCollider2D>(); col.isTrigger = true; col.radius = 0.8f; col.offset = new Vector2(0, 0.7f);
        var c = go.AddComponent<Collectible>();
        Set(c, "id", data.collectibleId); Set(c, "displayName", data.itemName); Set(c, "description", data.itemDesc);
        Set(c, "bigSprite", S($"UI/coleccionable_{data.itemKey}.png"));
    }

    static readonly Dictionary<char, string> SignNames = new Dictionary<char, string>
    {
        { '1', "cartel_mover" }, { '2', "cartel_saltar" }, { '3', "cartel_doble_salto" },
        { '4', "cartel_atacar" }, { '5', "cartel_bloquear" }, { '6', "cartel_interactuar" },
    };

    static void CreateSign(char c, float x, float y, Transform parent)
    {
        var go = new GameObject(SignNames[c]);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0);
        var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = S($"Parque/Carteles/{SignNames[c]}.png"); sr.sortingOrder = -3;
    }

    static TileBase ParkTile(string name)
    {
        var t = AssetDatabase.LoadAssetAtPath<TileBase>($"{Tiles}Parque/{name}.asset");
        if (t == null) throw new System.Exception("Falta el tile del parque " + name);
        return t;
    }

    /// <summary>Fondo del jardín: la parte de arriba de la ilustración del parque, con parallax.</summary>
    static void CreateParkBackground(Vector3 camStart, float levelWidth)
    {
        var fondos = new GameObject("Fondos");
        var layer = Child(fondos, "FondoParque", Vector3.zero);
        // Fijo en vertical (factor 0): la banqueta y el pasto del dibujo quedan a la altura del piso del nivel
        // y la tierra de la ilustración cubre lo que hay debajo. En horizontal se mueve con parallax.
        Set(layer.AddComponent<Parallax>(), "factor", new Vector2(0.8f, 0f));
        var sp = S("Parque/Fondos/fondo_parque.png");
        const float grassFromBottom = 162f / 32f;   // el pasto del dibujo está a 162 px del borde inferior (32 PPU)
        float w = sp.bounds.size.x, baseY = 3f - grassFromBottom;
        for (float x = camStart.x - 40f; x < camStart.x + 0.2f * levelWidth + 60f; x += w)
        {
            var g = Child(layer, "Parque", new Vector3(x + w / 2f, baseY, 0));
            var sr = g.AddComponent<SpriteRenderer>(); sr.sprite = sp; sr.sortingOrder = -20;
        }
    }

    static void CreateExit(float x, float y, Transform parent)
    {
        var door = new GameObject("Salida");
        door.transform.SetParent(parent, false);
        door.transform.position = new Vector3(x, y, 0);
        var sr = door.AddComponent<SpriteRenderer>(); sr.sprite = S("Decoracion/puerta_1.png"); sr.sortingOrder = -3;
        door.AddComponent<LevelExit>();
    }

    static Camera CreateCamera(Transform target, Vector2 min, Vector2 max)
    {
        var camGo = new GameObject("Main Camera"); camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true; cam.orthographicSize = CamHalfH;
        cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color32(39, 38, 56, 255);
        camGo.AddComponent<AudioListener>();
        var ppcType = System.Type.GetType("UnityEngine.Rendering.Universal.PixelPerfectCamera, Unity.RenderPipelines.Universal.2D.Runtime");
        var ppc = camGo.AddComponent(ppcType);
        ppcType.GetProperty("assetsPPU").SetValue(ppc, 16);
        ppcType.GetProperty("refResolutionX").SetValue(ppc, 384);
        ppcType.GetProperty("refResolutionY").SetValue(ppc, 216);
        Vector3 p = new Vector3(0, 0, -10);
        if (target != null)
        {
            var follow = camGo.AddComponent<CameraFollow>();
            Set(follow, "target", target); Set(follow, "offset", new Vector2(0, 2f)); Set(follow, "useBounds", true);
            Set(follow, "areaMin", min); Set(follow, "areaMax", max);
            p = new Vector3(Mathf.Clamp(target.position.x, min.x + CamHalfW, max.x - CamHalfW),
                            Mathf.Clamp(target.position.y + 2f, min.y + CamHalfH, max.y - CamHalfH), -10f);
        }
        camGo.transform.position = p;
        return cam;
    }

    static void CreateParallax(Vector3 camStart, float levelWidth)
    {
        var fondos = new GameObject("Fondos");
        var bgs = AssetDatabase.LoadAllAssetRepresentationsAtPath("Assets/Sprites/GothicVania/ENVIRONMENT/backgrounds.png").OfType<Sprite>().ToDictionary(s => s.name);
        string[] order = { "fondo_ventana", "fondo_pilar", "fondo_antorcha", "fondo_altar", "fondo_gargola", "fondo_pilar", "fondo_antorcha" };
        float baseY = camStart.y - CamHalfH - 0.5f;

        var far = Child(fondos, "FondoLejano", Vector3.zero);
        Set(far.AddComponent<Parallax>(), "factor", new Vector2(0.7f, 0.85f));
        float x = camStart.x - CamHalfW - 6f, end = camStart.x + 0.3f * levelWidth + 20f; int k = 0;
        while (x < end)
        {
            var sp = bgs[order[k++ % order.Length]];
            var g = Child(far, sp.name, new Vector3(x + sp.bounds.size.x / 2f, baseY, 0));
            var sr = g.AddComponent<SpriteRenderer>(); sr.sprite = sp; sr.sortingOrder = -20;
            x += sp.bounds.size.x;
        }

        var colSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/GothicVania/ENVIRONMENT/column_sinfondo.png");
        var near = Child(fondos, "Columnas", Vector3.zero);
        Set(near.AddComponent<Parallax>(), "factor", new Vector2(0.35f, 0.85f));
        for (float cx = camStart.x - CamHalfW - 4f; cx < camStart.x + 0.65f * levelWidth + 20f; cx += 17f)
        {
            var g = Child(near, "Columna", new Vector3(cx, baseY + 0.5f, 0));
            var sr = g.AddComponent<SpriteRenderer>(); sr.sprite = colSprite; sr.sortingOrder = -10; sr.color = new Color(0.75f, 0.75f, 0.85f);
        }
    }

    delegate char CellFn(int x, int y);
    delegate bool SolidFn(int x, int y);

    static void Decorate(LevelData data, int W, int H, CellFn at, SolidFn solid, List<(char c, int x, int y)> entities)
    {
        var rnd = new System.Random(data.number * 101 + 7);
        var parent = new GameObject("Decoracion").transform;
        bool park = data.IsPark;
        string folder = park ? "Parque/Decoracion" : "Decoracion";
        string[] tall = park
            ? new[] { "arbol_1", "arbol_2", "arbol_rosa", "arbol_naranja", "pino_1", "pino_2", "kiosco", "farol_doble", "arco_flores", "farol_1" }
            : new[] { "estatua_cruz", "estatua_monje", "estatua_virgen", "vitral_grande", "vitral_roseton", "vitral_alto" };
        string[] medium = park
            ? new[] { "banca_1", "banca_2", "banca_3", "banca_4", "fuente", "maceta_larga", "maceta_2", "arco_seto", "tablero", "mesa_picnic", "carrito", "reja", "cipres" }
            : new[] { "banca_1", "banca_2", "banca_3", "pulpito", "altar", "librero", "pila_bautismal", "estandarte_rojo", "estandarte_morado", "estandarte_azul", "cuadro_1", "cuadro_2", "vitral_chico_1", "vitral_chico_2" };
        string[] small = park
            ? new[] { "maceta_1", "maceta_3", "bebedero_aves", "bote_basura" }
            : new[] { "barril", "jarron", "atril", "candelabro_doble" };
        var hanging = new HashSet<string> { "estandarte_rojo", "estandarte_morado", "estandarte_azul", "cuadro_1", "cuadro_2", "vitral_chico_1", "vitral_chico_2" };

        int Headroom(int x, int y) { int h = 0; while (y + h < H && at(x, y + h) == ' ') h++; return h; }
        // El cartel grande (1) ocupa más espacio
        bool NearEntity(int x, int y) => entities.Any(e => Mathf.Abs(e.x - x) < (e.c == '1' ? 8 : 4) && Mathf.Abs(e.y - y) < 5);

        int next = 6;
        for (int y = 1; y < H - 1; y++)
        {
            int lastX = -100;
            for (int x = 2; x < W - 2; x++)
            {
                if (at(x, y) != ' ' || !solid(x, y - 1)) continue;          // casilla vacía sobre suelo
                if (x - lastX < next || NearEntity(x, y)) continue;
                if (!solid(x - 1, y - 1) || !solid(x + 1, y - 1)) continue; // no en bordes
                int head = Headroom(x, y);
                if (head < 3) continue;
                string[] pool = head >= 9 ? (park && rnd.NextDouble() < 0.5 ? medium : tall) : head >= 5 ? medium : small;
                string name = pool[rnd.Next(pool.Length)];
                var sprite = S($"{folder}/{name}.png");
                float h = sprite.bounds.size.y;
                float yOff = hanging.Contains(name) ? Mathf.Min(2.5f, head - h - 0.5f) : (name.StartsWith("vitral") ? 1.2f : 0f);
                if (h + yOff > head - 0.3f) continue;
                var g = new GameObject(name); g.transform.SetParent(parent, false);
                g.transform.position = new Vector3(x + 0.5f, y + Mathf.Max(0f, yOff), 0);
                var sr = g.AddComponent<SpriteRenderer>(); sr.sprite = sprite; sr.sortingOrder = -4;
                lastX = x;
                next = 6 + rnd.Next(7);
            }
        }
    }

    static void CreateHUD()
    {
        var hud = new GameObject("HUD").AddComponent<HUD>();
        Set(hud, "portrait", S("UI/cara_frente.png"));
        Set(hud, "heartSprite", Frames("Objetos/panalito_gira")[0]);
        Set(hud, "heartVanishFrames", Frames("Objetos/panalito_desaparecer"));
        Set(hud, "dialogueBubble", S("UI/nube_dialogo.png"));
    }

    // ======================================================================
    // Inicio y Victoria
    // ======================================================================

    static void BuildTitleScene()
    {
        // La pantalla de inicio es solo interfaz: la ilustración del menú cubre toda la pantalla
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateCamera(null, Vector2.zero, Vector2.zero);
        new GameObject("Menu").AddComponent<TitleMenu>();
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Inicio.unity");
    }

    static void BuildVictoryScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateCamera(null, Vector2.zero, Vector2.zero);
        var v = new GameObject("Victoria").AddComponent<VictoryScreen>();
        Set(v, "victoryFrames", Frames("Loquito/victoria"));
        Set(v, "sittingSprite", S("UI/sentado_caja.png"));
        Set(v, "beansSprite", S("UI/coleccionable_lata_frijoles.png"));
        Set(v, "colaSprite", S("UI/coleccionable_lata_coca.png"));
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Victoria.unity");
    }

    static void UpdateBuildSettings()
    {
        // Las 6 escenas del juego, en orden
        var scenes = new List<string> { "Assets/Scenes/Inicio.unity", "Assets/Scenes/Jardin.unity" };
        for (int i = 1; i <= Levels.Length; i++) scenes.Add($"Assets/Scenes/Mazmorra{i}.unity");
        scenes.Add("Assets/Scenes/Victoria.unity");
        EditorBuildSettings.scenes = scenes.Where(File.Exists).Select(s => new EditorBuildSettingsScene(s, true)).ToArray();
    }

    // ======================================================================
    // Utilidades
    // ======================================================================

    static Sprite S(string path)
    {
        var s = AssetDatabase.LoadAssetAtPath<Sprite>(Propios + path);
        if (s == null) throw new System.Exception("Falta el sprite " + Propios + path);
        return s;
    }

    static Sprite[] Frames(string dir) =>
        AssetDatabase.FindAssets("t:Sprite", new[] { Propios + dir }).Select(AssetDatabase.GUIDToAssetPath).Distinct()
            .Where(p => Path.GetDirectoryName(p).Replace('\\', '/') == (Propios + dir).TrimEnd('/'))
            .OrderBy(p => p).Select(AssetDatabase.LoadAssetAtPath<Sprite>).ToArray();

    static TileBase T(int c, int r)
    {
        var t = AssetDatabase.LoadAssetAtPath<TileBase>($"{Tiles}tile_c{c}_r{r}.asset");
        if (t == null) throw new System.Exception($"Falta el tile c{c} r{r}");
        return t;
    }

    static Sprite SpriteOfTile(TileBase t) => (t as Tile)?.sprite;

    static GameObject Child(GameObject parent, string name, Vector3 local)
    {
        var g = new GameObject(name);
        g.transform.SetParent(parent.transform, false);
        g.transform.localPosition = local;
        g.layer = parent.layer;
        return g;
    }

    static GameObject SavePrefab(GameObject go, string name)
    {
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, Prefabs + name + ".prefab");
        Object.DestroyImmediate(go);
        return prefab;
    }

    static void SetClipFrames(AnimationClip clip, Sprite[] frames, float fps, bool loop)
    {
        clip.frameRate = fps;
        var b = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        var keys = new ObjectReferenceKeyframe[frames.Length + 1];
        for (int i = 0; i < frames.Length; i++) keys[i] = new ObjectReferenceKeyframe { time = i / fps, value = frames[i] };
        keys[frames.Length] = new ObjectReferenceKeyframe { time = frames.Length / fps, value = frames[frames.Length - 1] };
        AnimationUtility.SetObjectReferenceCurve(clip, b, keys);
        var st = AnimationUtility.GetAnimationClipSettings(clip); st.loopTime = loop; AnimationUtility.SetAnimationClipSettings(clip, st);
        EditorUtility.SetDirty(clip);
    }

    static void SetClips(SpriteAnim anim, string start, params (string name, Sprite[] frames, float fps, bool loop)[] clips)
    {
        var so = new SerializedObject(anim);
        var list = so.FindProperty("clips");
        list.arraySize = clips.Length;
        for (int i = 0; i < clips.Length; i++)
        {
            var e = list.GetArrayElementAtIndex(i);
            e.FindPropertyRelative("name").stringValue = clips[i].name;
            e.FindPropertyRelative("fps").floatValue = clips[i].fps;
            e.FindPropertyRelative("loop").boolValue = clips[i].loop;
            var fr = e.FindPropertyRelative("frames");
            fr.arraySize = clips[i].frames.Length;
            for (int k = 0; k < clips[i].frames.Length; k++) fr.GetArrayElementAtIndex(k).objectReferenceValue = clips[i].frames[k];
        }
        so.FindProperty("playOnStart").stringValue = start;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void SetStrings(Object o, string prop, string[] values)
    {
        var so = new SerializedObject(o);
        var p = so.FindProperty(prop);
        p.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++) p.GetArrayElementAtIndex(i).stringValue = values[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void SetGifts(Object o, params (int afterLine, string title, string description, Sprite sprite)[] gifts)
    {
        var so = new SerializedObject(o);
        var p = so.FindProperty("gifts");
        p.arraySize = gifts.Length;
        for (int i = 0; i < gifts.Length; i++)
        {
            var e = p.GetArrayElementAtIndex(i);
            e.FindPropertyRelative("afterLine").intValue = gifts[i].afterLine;
            e.FindPropertyRelative("title").stringValue = gifts[i].title;
            e.FindPropertyRelative("description").stringValue = gifts[i].description;
            e.FindPropertyRelative("sprite").objectReferenceValue = gifts[i].sprite;
        }
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void Set(Object o, string prop, object v)
    {
        var so = new SerializedObject(o);
        var p = so.FindProperty(prop);
        if (p == null) throw new System.Exception($"No existe '{prop}' en {o.GetType().Name}");
        switch (v)
        {
            case Sprite[] arr:
                p.arraySize = arr.Length;
                for (int i = 0; i < arr.Length; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = arr[i];
                break;
            case Object obj: p.objectReferenceValue = obj; break;
            case int i: p.intValue = i; break;
            case float f: p.floatValue = f; break;
            case bool b: p.boolValue = b; break;
            case string s: p.stringValue = s; break;
            case Vector2 v2: p.vector2Value = v2; break;
        }
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
