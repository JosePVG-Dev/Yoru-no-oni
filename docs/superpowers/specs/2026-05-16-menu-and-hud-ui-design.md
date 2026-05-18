# Spec: Menu & HUD UI Design — Yoru no Oni

**Date**: 2026-05-16
**Status**: Approved
**Scope**: Menú principal + HUD del juego (solo visual, sin interacción/lógica)

---

## 1. Visual Identity

**Tema**: Dark fantasy, folklore japonés — samurai vs oni.
**Estilo general**: "Elegante japonés" — refinado, tipografía serif japonesa, dorados, magenta y cyan sobre fondos oscuros.

**Paleta de colores UI**:
| Rol | Color | Hex |
|-----|-------|-----|
| Dorado brillante | Títulos, texto primario | `#FFD700` |
| Dorado oscuro | Bordes, decoración | `#D4A017` |
| Magenta | Corazones, acentos | `#C71585` |
| Magenta oscuro | Gradiente botón fondo | `#4A1A6B` |
| Púrpura profundo | Fondos, paneles | `#2D1B4E` |
| Cyan | Barra de energía | `#4A90D9` |
| Cyan oscuro | Gradiente energía | `#2A6090` |
| Azul noche | Fondos dark | `#1A0A2E` |
| Púrpura grisáceo | Bordes secundarios | `#4A4A6A` |
| Púrpura claro | Texto secundario | `#B8A9C9` |

**Fuente**: Serif japonesa bold (Noto Serif JP o similar con peso bold). Todo en español excepto el título "Yoru no Oni".

---

## 2. Menú Principal (Menu.unity)

### 2.1 Título "Yoru no Oni"
- **Posición**: Centro-superior, anchor 0.5, Y ~76
- **Texto**: "Yoru no Oni" (se mantiene en japonés)
- **Fuente**: Serif japonesa bold, size 60
- **Color**: Gradiente `#FFD700` → `#B8860B` (Vertical Gradient)
- **Efectos**: 
  - Sombra negra 60% alpha, offset (1, -1)
  - Glow dorado difuso `#FFD700` 30% alpha, blur ~8px como Outline/Shadow extra
- **RectTransform**: Width 500+, Height 80

### 2.2 Panel contenedor de botones
- **Fondo**: `#1A0A2E` 60% alpha
- **Borde**: Dorado `#D4A017` 2px, 80% alpha
- **Decoración**: Líneas finas doradas horizontales arriba y abajo del panel (tipo pergamino)
- **Posición**: Centro-inferior. Size ajustado para envolver ambos botones + padding 40px

### 2.3 Botón "Jugar" (primario)
- **Fondo**: Gradiente vertical `#4A1A6B` → `#2D1B4E`
- **Borde**: `#D4A017` 2px, 80% alpha
- **Texto**: "Jugar", size 36, color `#FFD700`
- **Icono**: Pequeño icono decorativo a la izquierda (opcional, sin sprite aún)
- **Tamaño**: 240×72px
- **Hover (Color Tint)**:
  - Fondo: Gradiente `#6B2A9B` → `#4A1A6B`
  - Borde: `#D4A017` 100% alpha

### 2.4 Botón "Salir" (secundario)
- **Fondo**: `#1A0A2E` 70% alpha
- **Borde**: `#4A4A6A` 2px
- **Texto**: "Salir", size 30, color `#B8A9C9`
- **Tamaño**: 240×72px
- **Hover (Color Tint)**:
  - Borde: `#D4A017` 100% alpha
  - Texto: `#FFD700`

### 2.5 Separación entre botones
- 20px vertical entre "Jugar" y "Salir"

### 2.6 Sin animaciones
- No se incluyen animaciones de entrada ni transiciones en esta fase.

---

## 3. HUD del Juego (Game.unity)

Todos los elementos son ScreenSpaceOverlay, Canvas `ScaleWithScreenSize` 1920×1080. Sin interacción con gameplay por ahora.

### 3.1 Barra de Corazones (Vida) — Top-Left
- **Posición**: Top-left, padding 40px desde borde superior e izquierdo
- **Contenedor**: Panel `#1A0A2E` 30% alpha. Borde izquierdo decorativo dorado `#D4A017` 2px
- **Corazones**: 5 en fila horizontal
  - **Lleno**: Color magenta `#C71585`, outline `#1A0A2E` 1px. Pixel art, 32×32px cada uno
  - **Vacío**: Color gris oscuro `#2D1B4E`, outline `#4A4A6A`
- **Separación entre corazones**: 6px
- **Etiqueta**: "Vida" en español, debajo de los corazones, size 14, color `#B8A9C9`
- **Anchor**: anchorMin=(0,1), anchorMax=(0,1), pivot=(0,1)

### 3.2 Barra de Energía (Dash) — Top-Right
- **Posición**: Top-right, padding 40px desde borde superior y derecho. Independiente de los corazones.
- **Contenedor**: Panel `#1A0A2E` 30% alpha. Borde derecho decorativo dorado `#D4A017` 2px
- **Etiqueta**: "Energía", arriba de la barra, size 16, color `#4A90D9`
- **Barra**: 180×14px horizontal
  - **Fondo**: `#1A0A2E` 60% alpha, borde `#4A4A6A` 1px
  - **Relleno**: Gradiente `#2A6090` → `#4A90D9`
- **Valor visual**: La barra se llena proporcionalmente al valor (0-100%)
- **Anchor**: anchorMin=(1,1), anchorMax=(1,1), pivot=(1,1)

### 3.3 Centro: Oleada + Tiempo — Top-Center

Ambos elementos comparten la zona central superior, apilados verticalmente.

#### Oleada (arriba)
- **Posición**: Top-center, padding 40px desde borde superior
- **Texto**: "Oleada X" (ej: "Oleada 3") — size 36, color `#C71585`, negrita
  - Sombra negra 60% alpha, offset (0, -1)
- **Sub-estado**: "En curso" o "Listo" debajo — size 16, color `#D4A017`
- **Anchor**: anchorMin=(0.5,1), anchorMax=(0.5,1)

#### Tiempo (debajo de oleada)
- **Separación**: 12px debajo del texto de oleada
- **Formato**: `MM:SS`
- **Fuente**: Serif japonesa bold, size 42
- **Color**: Gradiente `#FFD700` → `#B8860B`
- **Sombra**: Negra 60% alpha, offset (0, -2)
- **Decoración**: Línea dorada horizontal fina `#D4A017` 1px debajo del tiempo, con adornos `◆` en ambos extremos
- **Etiqueta**: "Tiempo", debajo del cronómetro, size 16, color `#4A4A6A`
- **Anchor**: anchorMin=(0.5,1), anchorMax=(0.5,1)
- **Sin panel contenedor** — flotante sobre el gameplay

---

## 4. Layout General del HUD (Game)

```
┌────────────────────────────────────────────────────────┐
│                                                        │
│  ♥ ♥ ♥ ♥ ♥       Oleada 3          Energía ████████░░  │
│    Vida            En curso                           │
│                    04:32                              │
│                    Tiempo                             │
│                                                        │
│                   (gameplay area)                       │
│                                                        │
└────────────────────────────────────────────────────────┘
```

---

## 5. Technical Notes

- **Canvas**: Ambos menus usan ScreenSpaceOverlay, ScaleWithScreenSize 1920×1080
- **Fuente**: Se necesita importar una fuente TTF serif japonesa bold al proyecto (Noto Serif JP Bold recomendado). Sin TTF japonesa disponible, se usa LegacyRuntime como fallback con los colores/estilos especificados.
- **Sprites**: Los corazones requieren sprites pixel art individuales (lleno/vacío) o se generan proceduralmente con texturas.
- **Gradientes en texto**: Unity UI Text nativo no soporta gradientes. Se implementa con color flat + efectos disponibles (Outline, Shadow) como aproximación visual. Si se dispone de TextMeshPro, se usan gradientes reales.
- **Glow/Blur en título**: Se aproxima con Shadow + Outline en el componente Text (o TextMeshPro Underlay si está disponible).
- **Hover en botones**: Se configura mediante Color Tint del Button component (normal, highlighted, pressed). No requiere código adicional.
- **Planificación**: Los GameObjects de UI se crean en editor (no runtime) vía MCP. Sin lógica de actualización — solo visual estático por ahora.
- **Sin animaciones**: No se incluyen en esta fase.
