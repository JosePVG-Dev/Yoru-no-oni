# Spec: UI Polish & Fixes — Text Legibility + Panel Colors

**Date**: 2026-05-16
**Status**: Pending screenshots from user
**Context**: Continuación de `2026-05-16-menu-and-hud-ui-design.md`. El UI fue implementado pero tiene problemas de legibilidad y colores incorrectos.

---

## Issues Identified (via screenshot)

### 1. Texto borroso / ilegible
- La fuente LegacyRuntime a tamaños 16-42 se ve blurry en el HUD
- El texto no tiene contorno negro, se funde con el fondo oscuro
- "Tiempo" (gris `#4A4A6A`) es casi invisible sobre el fondo púrpura

### 2. Paneles con color incorrecto
- Los paneles VidaPanel y EnergiaPanel se ven dorados sólidos, no oscuros semitransparentes
- Sospecha: el componente `Outline` con `effectDistance=(3,0)` está creando un borde tan grueso que llena todo el panel

### 3. Menú principal (screenshot evaluado)
- "Salir" (gris) casi invisible sobre fondo oscuro — necesita outline negro urgente
- "Yoru no Oni" título legible pero se funde con el fondo — necesita outline
- "Jugar" mejor contraste pero necesita outline para consistencia
- Panel borde dorado se ve bien

---

## Fixes a Aplicar

### Fix 1: Agregar Outline negro a TODOS los textos

Aplicar `Outline` component con `effectColor=black 0.8 alpha, effectDistance=(1,-1)` a:
- Todos los corazones (Heart1-5)
- VidaLabel
- EnergiaLabel
- WaveText
- WaveStatus
- TimeText
- TimeLabel

### Fix 2: Aumentar tamaños de fuente

| Elemento | Actual | Nuevo |
|----------|--------|-------|
| WaveText | 36 | 42 |
| TimeText | 42 | 48 |
| EnergiaLabel | 16 | 18 |
| VidaLabel | 14 | 18 |
| WaveStatus | 16 | 18 |
| TimeLabel | 16 | 18 |

### Fix 3: Corregir paneles

En vez de usar `Outline` para simular bordes (que llena el panel), crear un `Image` hijo ligeramente más grande como borde:
- Crear `VidaPanel_Border` como hijo de VidaPanel, color dorado `#D4A017` 90% alpha, 2px más grande en cada lado
- Crear `EnergiaPanel_Border` como hijo de EnergiaPanel, mismo approach
- Remover el Outline de los paneles

### Fix 4: Cambiar color de TimeLabel

`#B8A9C9` (púrpura claro) en vez de `#4A4A6A` para mejor contraste.

---

## Contexto para el agente

### Proyecto
- **Path**: `F:\Yoru no Oni\My project`
- **Unity**: 6000.3.11f1, URP 2D
- **Font**: LegacyRuntime (NO TextMeshPro)
- **Compiler**: CodeDom (C# 6, NO Roslyn)
- **MCP**: Unity MCP tools disponibles

### Escenas a modificar
- `Assets/Scenes/Menu.unity` — Canvas con Title, ButtonPanel (hijos: PlayButton, QuitButton)
- `Assets/Scenes/Game.unity` — HUD_Canvas con VidaPanel, EnergiaPanel, CenterHUD

### Paleta de colores
| Color | Hex | Unity (0-1) |
|-------|-----|-------------|
| Gold | #FFD700 | (1, 0.843, 0) |
| Gold dark | #D4A017 | (0.831, 0.627, 0.09) |
| Magenta | #C71585 | (0.78, 0.082, 0.522) |
| Cyan | #4A90D9 | (0.29, 0.565, 0.851) |
| Dark bg | #1A0A2E | (0.102, 0.039, 0.18) |
| Purple light | #B8A9C9 | (0.722, 0.663, 0.788) |
| Black outline | #000000 | (0, 0, 0, 0.8) |

### Archivos de referencia
- `docs/superpowers/specs/2026-05-16-menu-and-hud-ui-design.md` — spec original
- `docs/superpowers/plans/2026-05-16-menu-and-hud-ui-implementation.md` — plan de implementación

---

## Screenshots pendientes

- [ ] Screenshot del menú principal (provisto por el usuario)
- [ ] Evaluar legibilidad del título y botones del menú
- [ ] Aplicar mismos fixes de Outline si es necesario
