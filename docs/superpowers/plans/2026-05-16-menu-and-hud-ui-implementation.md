# Menu & HUD UI Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Rediseñar el menú principal y crear el HUD del juego con estilo "Elegante japonés" — dark fantasy, tipografía serif japonesa, dorados, magenta y cyan sobre fondos oscuros.

**Architecture:** Se modifican GameObjects existentes en Menu.unity (título y botones) y se crean nuevos GameObjects de UI en Game.unity (Canvas + HUD). Todo es visual estático sin lógica de actualización. Se usa Unity UI nativo (no TextMeshPro, no UI Toolkit). Fuente: LegacyRuntime como fallback. Paleta: gold `#FFD700`, magenta `#C71585`, cyan `#4A90D9`, dark `#1A0A2E`.

**Tech Stack:** Unity 6 LTS, URP 2D, C# (CodeDom compiler), Unity MCP, manage_gameobject, manage_components, execute_code, manage_texture

---

### Task 1: Rediseñar Título del Menú

**Files:**
- Modify: `Assets/Scenes/Menu.unity` (GameObject "Title")
- Reader: `docs/superpowers/specs/2026-05-16-menu-and-hud-ui-design.md`

- [ ] **Step 1: Cargar escena Menu y cambiar color del título a dorado**

```
manage_scene: action=load, path="Assets/Scenes/Menu.unity"
```

Luego ejecutar código:
```
execute_code:
  code: var title = GameObject.Find("Title"); var txt = title.GetComponent<UnityEngine.UI.Text>(); txt.color = new Color(1f, 0.843f, 0f); return "Title color set to " + txt.color;
```

- [ ] **Step 2: Agregar Shadow al título (sombra negra)**

```
manage_components:
  action: add
  target: Title
  component_type: Shadow
  search_method: by_name
```

Luego configurar el Shadow:
```
execute_code:
  code: var sh = GameObject.Find("Title").GetComponent<UnityEngine.UI.Shadow>(); sh.effectColor = new Color(0,0,0,0.6f); sh.effectDistance = new Vector2(1,-1); return "Shadow configured";
```

- [ ] **Step 3: Agregar Outline al título (efecto glow dorado)**

```
manage_components:
  action: add
  target: Title
  component_type: Outline
  search_method: by_name
```

Luego configurar Outline:
```
execute_code:
  code: var ol = GameObject.Find("Title").GetComponent<UnityEngine.UI.Outline>(); ol.effectColor = new Color(1f, 0.843f, 0f, 0.3f); ol.effectDistance = new Vector2(2, -2); return "Outline configured";
```

- [ ] **Step 4: Ajustar tamaño del título a 500×80**

```
execute_code:
  code: var rt = GameObject.Find("Title").transform as RectTransform; rt.sizeDelta = new Vector2(500, 80); return "Title size set to " + rt.sizeDelta;
```

- [ ] **Step 5: Screenshot de verificación**

```
manage_camera: action=screenshot, include_image=true
```

Verificar que el título se ve dorado con sombra negra y glow dorado difuso.

- [ ] **Step 6: Guardar escena**

```
manage_scene: action=save
```

---

### Task 2: Crear Panel Contenedor de Botones

**Files:**
- Modify: `Assets/Scenes/Menu.unity` (nuevo GameObject "ButtonPanel" hijo de Canvas)

- [ ] **Step 1: Crear GameObject ButtonPanel como hijo de Canvas**

```
manage_gameobject:
  action: create
  name: ButtonPanel
  parent: Canvas
  components_to_add: ["Image"]
```

- [ ] **Step 2: Configurar RectTransform del panel**

```
execute_code:
  code: var panel = GameObject.Find("ButtonPanel"); var rt = panel.transform as RectTransform; rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f); rt.pivot = new Vector2(0.5f, 0.5f); rt.anchoredPosition = new Vector2(0, -170); rt.sizeDelta = new Vector2(320, 200); return "Panel rect set";
```

- [ ] **Step 3: Configurar color de fondo del panel**

```
execute_code:
  code: var img = GameObject.Find("ButtonPanel").GetComponent<UnityEngine.UI.Image>(); img.color = new Color(0.102f, 0.039f, 0.18f, 0.6f); return "Panel color set to " + img.color;
```

- [ ] **Step 4: Agregar Outline dorado al panel (efecto borde)**

```
manage_components:
  action: add
  target: ButtonPanel
  component_type: Outline
  search_method: by_name
```

```
execute_code:
  code: var ol = GameObject.Find("ButtonPanel").GetComponent<UnityEngine.UI.Outline>(); ol.effectColor = new Color(0.831f, 0.627f, 0.09f, 0.8f); ol.effectDistance = new Vector2(2, -2); return "Outline configured";
```

- [ ] **Step 5: Guardar escena**

```
manage_scene: action=save
```

---

### Task 3: Reparentear y Rediseñar Botones del Menú

**Files:**
- Modify: `Assets/Scenes/Menu.unity` (GameObjects "PlayButton", "QuitButton")

- [ ] **Step 1: Reparentear PlayButton y QuitButton a ButtonPanel**

Primero desvincularlos del Canvas poniéndolos bajo ButtonPanel. Usamos `execute_code` porque manage_gameobject move puede fallar:

```
execute_code:
  code: var play = GameObject.Find("PlayButton"); var quit = GameObject.Find("QuitButton"); var panel = GameObject.Find("ButtonPanel"); play.transform.SetParent(panel.transform); quit.transform.SetParent(panel.transform); return "Buttons reparented";
```

- [ ] **Step 2: Posicionar PlayButton dentro del panel**

```
execute_code:
  code: var play = GameObject.Find("PlayButton"); var rt = play.transform as RectTransform; rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f); rt.pivot = new Vector2(0.5f, 0.5f); rt.anchoredPosition = new Vector2(0, 45); rt.sizeDelta = new Vector2(240, 72); return "PlayButton positioned";
```

- [ ] **Step 3: Posicionar QuitButton dentro del panel (20px debajo)**

```
execute_code:
  code: var quit = GameObject.Find("QuitButton"); var rt = quit.transform as RectTransform; rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f); rt.pivot = new Vector2(0.5f, 0.5f); rt.anchoredPosition = new Vector2(0, -45); rt.sizeDelta = new Vector2(240, 72); return "QuitButton positioned";
```

- [ ] **Step 4: Estilizar PlayButton — color de fondo, texto y tint**

```
execute_code:
  code: var play = GameObject.Find("PlayButton"); var img = play.GetComponent<UnityEngine.UI.Image>(); img.color = new Color(0.29f, 0.102f, 0.42f); var txt = play.GetComponentInChildren<UnityEngine.UI.Text>(); txt.color = new Color(1f, 0.843f, 0f); txt.fontSize = 36; txt.text = "Jugar"; var btn = play.GetComponent<UnityEngine.UI.Button>(); var cb = btn.colors; cb.normalColor = new Color(0.29f, 0.102f, 0.42f); cb.highlightedColor = new Color(0.42f, 0.165f, 0.61f); cb.pressedColor = new Color(0.177f, 0.106f, 0.306f); cb.colorMultiplier = 1f; btn.colors = cb; return "PlayButton styled";
```

- [ ] **Step 5: Estilizar QuitButton — color de fondo, texto y tint**

```
execute_code:
  code: var quit = GameObject.Find("QuitButton"); var img = quit.GetComponent<UnityEngine.UI.Image>(); img.color = new Color(0.102f, 0.039f, 0.18f, 0.7f); var txt = quit.GetComponentInChildren<UnityEngine.UI.Text>(); txt.color = new Color(0.722f, 0.663f, 0.788f); txt.fontSize = 30; txt.text = "Salir"; var btn = quit.GetComponent<UnityEngine.UI.Button>(); var cb = btn.colors; cb.normalColor = new Color(0.102f, 0.039f, 0.18f, 0.7f); cb.highlightedColor = new Color(0.831f, 0.627f, 0.09f); cb.pressedColor = new Color(0.177f, 0.106f, 0.306f); cb.colorMultiplier = 1f; btn.colors = cb; return "QuitButton styled";
```

- [ ] **Step 6: Agregar Outline dorado a PlayButton y QuitButton**

```
manage_components:
  action: add
  target: PlayButton
  component_type: Outline
  search_method: by_name
```

```
execute_code:
  code: var ol = GameObject.Find("PlayButton").GetComponent<UnityEngine.UI.Outline>(); ol.effectColor = new Color(0.831f, 0.627f, 0.09f, 0.8f); ol.effectDistance = new Vector2(2, -2); return "PlayButton border set";
```

```
manage_components:
  action: add
  target: QuitButton
  component_type: Outline
  search_method: by_name
```

```
execute_code:
  code: var ol = GameObject.Find("QuitButton").GetComponent<UnityEngine.UI.Outline>(); ol.effectColor = new Color(0.29f, 0.29f, 0.416f); ol.effectDistance = new Vector2(2, -2); return "QuitButton border set";
```

- [ ] **Step 7: Screenshot y guardar escena**

```
manage_camera: action=screenshot, include_image=true
manage_scene: action=save
```

Verificar: panel oscuro con borde dorado, botón Jugar magenta con borde dorado y texto dorado, botón Salir oscuro con borde gris púrpura y texto púrpura claro.

---

### Task 4: Crear Canvas del HUD en Game.unity

**Files:**
- Modify: `Assets/Scenes/Game.unity` (nuevo Canvas para HUD)

- [ ] **Step 1: Cargar escena Game**

```
manage_scene: action=load, path="Assets/Scenes/Game.unity"
```

- [ ] **Step 2: Crear Canvas para HUD**

```
manage_gameobject:
  action: create
  name: HUD_Canvas
  components_to_add: ["Canvas", "CanvasScaler", "GraphicRaycaster"]
```

- [ ] **Step 3: Configurar Canvas como ScreenSpaceOverlay**

```
execute_code:
  code: var canvas = GameObject.Find("HUD_Canvas").GetComponent<UnityEngine.Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; var scaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>(); scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920, 1080); return "HUD Canvas configured: " + canvas.renderMode + " " + scaler.referenceResolution;
```

- [ ] **Step 4: Verificar que el Canvas existe en la jerarquía**

```
manage_scene: action=get_hierarchy
```

- [ ] **Step 5: Guardar escena**

```
manage_scene: action=save
```

---

### Task 5: Crear Barra de Corazones (Vida) — Top-Left

**Files:**
- Modify: `Assets/Scenes/Game.unity` (nuevos GameObjects bajo HUD_Canvas)

- [ ] **Step 1: Crear panel contenedor "VidaPanel"**

```
manage_gameobject:
  action: create
  name: VidaPanel
  parent: HUD_Canvas
  components_to_add: ["Image"]
  position: [-40, 40, 0]
```

- [ ] **Step 2: Configurar RectTransform del panel (anclado top-left)**

```
execute_code:
  code: var panel = GameObject.Find("VidaPanel"); var rt = panel.transform as RectTransform; rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(0, 1); rt.pivot = new Vector2(0, 1); rt.anchoredPosition = new Vector2(40, -40); rt.sizeDelta = new Vector2(220, 90); return "VidaPanel anchored top-left";
```

- [ ] **Step 3: Configurar color de fondo del panel**

```
execute_code:
  code: var img = GameObject.Find("VidaPanel").GetComponent<UnityEngine.UI.Image>(); img.color = new Color(0.102f, 0.039f, 0.18f, 0.3f); return "Panel color set";
```

- [ ] **Step 4: Agregar Outline dorado al panel (borde izquierdo decorativo)**

```
manage_components:
  action: add
  target: VidaPanel
  component_type: Outline
  search_method: by_name
```

```
execute_code:
  code: var ol = GameObject.Find("VidaPanel").GetComponent<UnityEngine.UI.Outline>(); ol.effectColor = new Color(0.831f, 0.627f, 0.09f, 0.9f); ol.effectDistance = new Vector2(3, 0); return "Outline set — left gold border";
```

- [ ] **Step 5: Crear 5 corazones como hijos del panel**

Cada corazón es un GameObject con Text "♥" (magenta, outline negro). Creamos Heart1:

```
execute_code:
  code: var h = GameObject.Find("Heart1"); var rt = h.transform as RectTransform; rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(0, 1); rt.pivot = new Vector2(0, 1); rt.anchoredPosition = new Vector2(12, -10); rt.sizeDelta = new Vector2(36, 36); var txt = h.GetComponent<UnityEngine.UI.Text>(); txt.text = "♥"; txt.fontSize = 28; txt.color = new Color(0.78f, 0.082f, 0.522f); txt.alignment = TextAnchor.MiddleCenter; return "Heart1 created";
```

Ahora los otros 4 corazones en fila (separación 6px → posición X: 48, 84, 120, 156):

```
manage_gameobject:
  action: create
  name: Heart2
  parent: VidaPanel
  components_to_add: ["Text"]
```

```
manage_gameobject:
  action: create
  name: Heart3
  parent: VidaPanel
  components_to_add: ["Text"]
```

```
manage_gameobject:
  action: create
  name: Heart4
  parent: VidaPanel
  components_to_add: ["Text"]
```

```
manage_gameobject:
  action: create
  name: Heart5
  parent: VidaPanel
  components_to_add: ["Text"]
```

Configurar todos de una vez con execute_code:

```
execute_code:
  code: for(int i=2;i<=5;i++) { var h = GameObject.Find("Heart"+i); var rt = h.transform as RectTransform; rt.anchorMin = new Vector2(0,1); rt.anchorMax = new Vector2(0,1); rt.pivot = new Vector2(0,1); rt.anchoredPosition = new Vector2(12 + (i-1)*42, -10); rt.sizeDelta = new Vector2(36,36); var txt = h.GetComponent<UnityEngine.UI.Text>(); txt.text = "♥"; txt.fontSize = 28; txt.color = new Color(0.78f, 0.082f, 0.522f); txt.alignment = TextAnchor.MiddleCenter; } return "All hearts configured";
```

- [ ] **Step 6: Agregar Outline a cada corazón (borde negro)**

```
execute_code:
  code: for(int i=1;i<=5;i++) { var h = GameObject.Find("Heart"+i); h.AddComponent<UnityEngine.UI.Outline>(); var ol = h.GetComponent<UnityEngine.UI.Outline>(); ol.effectColor = new Color(0.102f, 0.039f, 0.18f, 1f); ol.effectDistance = new Vector2(1, -1); } return "Heart outlines added";
```

- [ ] **Step 7: Crear etiqueta "Vida" debajo de los corazones**

```
manage_gameobject:
  action: create
  name: VidaLabel
  parent: VidaPanel
  components_to_add: ["Text"]
```

```
execute_code:
  code: var lbl = GameObject.Find("VidaLabel"); var rt = lbl.transform as RectTransform; rt.anchorMin = new Vector2(0,1); rt.anchorMax = new Vector2(0,1); rt.pivot = new Vector2(0,1); rt.anchoredPosition = new Vector2(12, -50); rt.sizeDelta = new Vector2(200, 24); var txt = lbl.GetComponent<UnityEngine.UI.Text>(); txt.text = "Vida"; txt.fontSize = 14; txt.color = new Color(0.722f, 0.663f, 0.788f); txt.alignment = TextAnchor.MiddleLeft; return "Vida label set";
```

- [ ] **Step 8: Guardar escena**

```
manage_scene: action=save
```

---

### Task 6: Crear Barra de Energía (Dash) — Top-Right

**Files:**
- Modify: `Assets/Scenes/Game.unity` (nuevos GameObjects bajo HUD_Canvas)

- [ ] **Step 1: Crear panel contenedor "EnergiaPanel"**

```
manage_gameobject:
  action: create
  name: EnergiaPanel
  parent: HUD_Canvas
  components_to_add: ["Image"]
```

- [ ] **Step 2: Configurar RectTransform (anclado top-right)**

```
execute_code:
  code: var panel = GameObject.Find("EnergiaPanel"); var rt = panel.transform as RectTransform; rt.anchorMin = new Vector2(1, 1); rt.anchorMax = new Vector2(1, 1); rt.pivot = new Vector2(1, 1); rt.anchoredPosition = new Vector2(-40, -40); rt.sizeDelta = new Vector2(220, 90); return "EnergiaPanel anchored top-right";
```

- [ ] **Step 3: Configurar color y borde del panel**

```
execute_code:
  code: var img = GameObject.Find("EnergiaPanel").GetComponent<UnityEngine.UI.Image>(); img.color = new Color(0.102f, 0.039f, 0.18f, 0.3f); return "Panel color set";
```

```
manage_components:
  action: add
  target: EnergiaPanel
  component_type: Outline
  search_method: by_name
```

```
execute_code:
  code: var ol = GameObject.Find("EnergiaPanel").GetComponent<UnityEngine.UI.Outline>(); ol.effectColor = new Color(0.831f, 0.627f, 0.09f, 0.9f); ol.effectDistance = new Vector2(-3, 0); return "Right gold border set";
```

- [ ] **Step 4: Crear etiqueta "Energía"**

```
manage_gameobject:
  action: create
  name: EnergiaLabel
  parent: EnergiaPanel
  components_to_add: ["Text"]
```

```
execute_code:
  code: var lbl = GameObject.Find("EnergiaLabel"); var rt = lbl.transform as RectTransform; rt.anchorMin = new Vector2(0.5f, 1); rt.anchorMax = new Vector2(0.5f, 1); rt.pivot = new Vector2(0.5f, 1); rt.anchoredPosition = new Vector2(0, -10); rt.sizeDelta = new Vector2(200, 24); var txt = lbl.GetComponent<UnityEngine.UI.Text>(); txt.text = "Energía"; txt.fontSize = 16; txt.color = new Color(0.29f, 0.565f, 0.851f); txt.alignment = TextAnchor.MiddleCenter; return "Energia label set";
```

- [ ] **Step 5: Crear barra de energía (fondo)**

```
manage_gameobject:
  action: create
  name: EnergyBar_BG
  parent: EnergiaPanel
  components_to_add: ["Image"]
```

```
execute_code:
  code: var bg = GameObject.Find("EnergyBar_BG"); var rt = bg.transform as RectTransform; rt.anchorMin = new Vector2(0.5f, 1); rt.anchorMax = new Vector2(0.5f, 1); rt.pivot = new Vector2(0.5f, 1); rt.anchoredPosition = new Vector2(0, -40); rt.sizeDelta = new Vector2(180, 14); var img = bg.GetComponent<UnityEngine.UI.Image>(); img.color = new Color(0.102f, 0.039f, 0.18f, 0.6f); return "Energy bar background set";
```

- [ ] **Step 6: Crear barra de energía (relleno)**

```
manage_gameobject:
  action: create
  name: EnergyBar_Fill
  parent: EnergyBar_BG
  components_to_add: ["Image"]
```

```
execute_code:
  code: var fill = GameObject.Find("EnergyBar_Fill"); var rt = fill.transform as RectTransform; rt.anchorMin = new Vector2(0, 0); rt.anchorMax = new Vector2(1, 1); rt.pivot = new Vector2(0, 0.5f); rt.anchoredPosition = new Vector2(0, 0); rt.sizeDelta = new Vector2(0, 0); var img = fill.GetComponent<UnityEngine.UI.Image>(); img.color = new Color(0.29f, 0.565f, 0.851f); img.type = UnityEngine.UI.Image.Type.Filled; img.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal; img.fillOrigin = 0; img.fillAmount = 0.8f; return "Energy bar fill set at 80%";
```

- [ ] **Step 7: Agregar borde a la barra de fondo (Outline)**

```
manage_components:
  action: add
  target: EnergyBar_BG
  component_type: Outline
  search_method: by_name
```

```
execute_code:
  code: var ol = GameObject.Find("EnergyBar_BG").GetComponent<UnityEngine.UI.Outline>(); ol.effectColor = new Color(0.29f, 0.29f, 0.416f); ol.effectDistance = new Vector2(1, -1); return "Bar outline added";
```

- [ ] **Step 8: Guardar escena**

```
manage_scene: action=save
```

---

### Task 7: Crear HUD Central — Oleada + Tiempo

**Files:**
- Modify: `Assets/Scenes/Game.unity` (nuevos GameObjects bajo HUD_Canvas)

- [ ] **Step 1: Crear contenedor "CenterHUD"**

```
manage_gameobject:
  action: create
  name: CenterHUD
  parent: HUD_Canvas
  components_to_add: ["Image"]
```

**Nota:** El CenterHUD no debe tener panel visible (sin color). Lo configuramos transparente y sin Outline.

```
execute_code:
  code: var ch = GameObject.Find("CenterHUD"); var rt = ch.transform as RectTransform; rt.anchorMin = new Vector2(0.5f, 1); rt.anchorMax = new Vector2(0.5f, 1); rt.pivot = new Vector2(0.5f, 1); rt.anchoredPosition = new Vector2(0, -40); rt.sizeDelta = new Vector2(300, 130); var img = ch.GetComponent<UnityEngine.UI.Image>(); img.color = new Color(0,0,0,0); return "CenterHUD positioned transparent";
```

- [ ] **Step 2: Crear texto "Oleada X" (parte superior del centro)**

```
manage_gameobject:
  action: create
  name: WaveText
  parent: CenterHUD
  components_to_add: ["Text"]
```

```
execute_code:
  code: var w = GameObject.Find("WaveText"); var rt = w.transform as RectTransform; rt.anchorMin = new Vector2(0.5f, 1); rt.anchorMax = new Vector2(0.5f, 1); rt.pivot = new Vector2(0.5f, 1); rt.anchoredPosition = new Vector2(0, 0); rt.sizeDelta = new Vector2(280, 44); var txt = w.GetComponent<UnityEngine.UI.Text>(); txt.text = "Oleada 1"; txt.fontSize = 36; txt.color = new Color(0.78f, 0.082f, 0.522f); txt.alignment = TextAnchor.MiddleCenter; txt.fontStyle = FontStyle.Bold; return "WaveText set";
```

- [ ] **Step 3: Agregar sombra a "Oleada X"**

```
manage_components:
  action: add
  target: WaveText
  component_type: Shadow
  search_method: by_name
```

```
execute_code:
  code: var sh = GameObject.Find("WaveText").GetComponent<UnityEngine.UI.Shadow>(); sh.effectColor = new Color(0,0,0,0.6f); sh.effectDistance = new Vector2(0,-1); return "Wave shadow set";
```

- [ ] **Step 4: Crear sub-texto "En curso" debajo de oleada**

```
manage_gameobject:
  action: create
  name: WaveStatus
  parent: CenterHUD
  components_to_add: ["Text"]
```

```
execute_code:
  code: var ws = GameObject.Find("WaveStatus"); var rt = ws.transform as RectTransform; rt.anchorMin = new Vector2(0.5f, 1); rt.anchorMax = new Vector2(0.5f, 1); rt.pivot = new Vector2(0.5f, 1); rt.anchoredPosition = new Vector2(0, -40); rt.sizeDelta = new Vector2(200, 20); var txt = ws.GetComponent<UnityEngine.UI.Text>(); txt.text = "En curso"; txt.fontSize = 16; txt.color = new Color(0.831f, 0.627f, 0.09f); txt.alignment = TextAnchor.MiddleCenter; return "WaveStatus set";
```

- [ ] **Step 5: Crear texto de tiempo "00:00"**

```
manage_gameobject:
  action: create
  name: TimeText
  parent: CenterHUD
  components_to_add: ["Text"]
```

```
execute_code:
  code: var t = GameObject.Find("TimeText"); var rt = t.transform as RectTransform; rt.anchorMin = new Vector2(0.5f, 1); rt.anchorMax = new Vector2(0.5f, 1); rt.pivot = new Vector2(0.5f, 1); rt.anchoredPosition = new Vector2(0, -72); rt.sizeDelta = new Vector2(280, 50); var txt = t.GetComponent<UnityEngine.UI.Text>(); txt.text = "00:00"; txt.fontSize = 42; txt.color = new Color(1f, 0.843f, 0f); txt.alignment = TextAnchor.MiddleCenter; txt.fontStyle = FontStyle.Bold; return "TimeText set";
```

- [ ] **Step 6: Agregar sombra al texto de tiempo**

```
manage_components:
  action: add
  target: TimeText
  component_type: Shadow
  search_method: by_name
```

```
execute_code:
  code: var sh = GameObject.Find("TimeText").GetComponent<UnityEngine.UI.Shadow>(); sh.effectColor = new Color(0,0,0,0.6f); sh.effectDistance = new Vector2(0,-2); return "Time shadow set";
```

- [ ] **Step 7: Crear sub-etiqueta "Tiempo" debajo del cronómetro**

```
manage_gameobject:
  action: create
  name: TimeLabel
  parent: CenterHUD
  components_to_add: ["Text"]
```

```
execute_code:
  code: var tl = GameObject.Find("TimeLabel"); var rt = tl.transform as RectTransform; rt.anchorMin = new Vector2(0.5f, 1); rt.anchorMax = new Vector2(0.5f, 1); rt.pivot = new Vector2(0.5f, 1); rt.anchoredPosition = new Vector2(0, -120); rt.sizeDelta = new Vector2(200, 20); var txt = tl.GetComponent<UnityEngine.UI.Text>(); txt.text = "Tiempo"; txt.fontSize = 16; txt.color = new Color(0.29f, 0.29f, 0.416f); txt.alignment = TextAnchor.MiddleCenter; return "TimeLabel set";
```

- [ ] **Step 8: Guardar escena**

```
manage_scene: action=save
```

---

### Task 8: Verificación Final — Screenshots de Ambos Menús

**Files:**
- Verify: `Assets/Scenes/Menu.unity`, `Assets/Scenes/Game.unity`

- [ ] **Step 1: Cargar escena Menu y tomar screenshot**

```
manage_scene: action=load, path="Assets/Scenes/Menu.unity"
manage_camera: action=screenshot, include_image=true, max_resolution=1280
```

Verificar visualmente:
- Título dorado con sombra negra y glow
- Panel oscuro con borde dorado alrededor de los botones
- Botón Jugar magenta con borde dorado, texto "Jugar" dorado
- Botón Salir oscuro con borde gris púrpura, texto "Salir" púrpura claro

- [ ] **Step 2: Cargar escena Game y tomar screenshot**

```
manage_scene: action=load, path="Assets/Scenes/Game.unity"
manage_camera: action=screenshot, include_image=true, max_resolution=1280
```

Verificar visualmente:
- Top-left: 5 corazones magenta "♥" con outline negro + "Vida" debajo, panel con borde dorado izquierdo
- Top-center: "Oleada 1" magenta bold con sombra + "En curso" dorado + "00:00" dorado bold con sombra + "Tiempo" gris
- Top-right: "Energía" cyan + barra cyan (80% llena) con fondo oscuro, panel con borde dorado derecho

- [ ] **Step 3: Verificar que el Canvas de juego no tiene EventSystem (es display-only)**

```
manage_scene: action=get_hierarchy
```

Confirmar que no hay EventSystem en Game.unity (no necesario, solo display).

- [ ] **Step 4: Compilar y verificar que no hay errores**

```
manage_editor: action=stop  (si está en play mode)
refresh_unity: mode=force, scope=all, compile=request, wait_for_ready=true
```

```
read_console: types=["error"]
```

Debe estar vacío (sin errores).

---

### Task 9: Commit Final

- [ ] **Step 1: Verificar estado de git**

```bash
git status
```

- [ ] **Step 2: Hacer commit**

```bash
git add Assets/Scenes/Menu.unity Assets/Scenes/Game.unity Assets/Sprites/UI docs/superpowers/specs docs/superpowers/plans
git commit -m "feat: rediseño menu elegante japones + HUD gameplay"
```

---

## Appendix: Referencia de Colores

| Color | Hex | Unity Color (0-1) |
|-------|-----|-------------------|
| Gold bright | `#FFD700` | `(1, 0.843, 0)` |
| Gold dark | `#D4A017` | `(0.831, 0.627, 0.09)` |
| Magenta | `#C71585` | `(0.78, 0.082, 0.522)` |
| Magenta dark (button) | `#4A1A6B` | `(0.29, 0.102, 0.42)` |
| Purple deep | `#2D1B4E` | `(0.177, 0.106, 0.306)` |
| Cyan | `#4A90D9` | `(0.29, 0.565, 0.851)` |
| Cyan dark | `#2A6090` | `(0.165, 0.376, 0.565)` |
| Night blue | `#1A0A2E` | `(0.102, 0.039, 0.18)` |
| Purple grey | `#4A4A6A` | `(0.29, 0.29, 0.416)` |
| Purple light | `#B8A9C9` | `(0.722, 0.663, 0.788)` |
| Magenta hover | `#6B2A9B` | `(0.42, 0.165, 0.61)` |
