# Инструкция по созданию SpriteOutline Shader Graph

Если вы хотите использовать Shader Graph версию вместо обычного шейдера, выполните следующие шаги:

## Создание Shader Graph

1. В Unity Editor: **Create → Shader Graph → 2D → Sprite Unlit Graph**
2. Назовите его `SpriteOutline`
3. Откройте граф для редактирования

## Настройка Properties (Blackboard)

Добавьте следующие свойства в Blackboard:

1. **MainTex** (Texture2D)
   - Reference: `_MainTex`
   - Default: White

2. **OutlineColor** (Color)
   - Reference: `_OutlineColor`
   - Default: (1, 1, 1, 1) - белый

3. **OutlineThickness** (Float)
   - Reference: `_OutlineThickness`
   - Default: 1.0
   - Range: 0..5

4. **AlphaThreshold** (Float)
   - Reference: `_AlphaThreshold`
   - Default: 0.01
   - Range: 0..0.2

## Построение графа

### Основные узлы:

1. **UV Node** → подключить к `uv0`

2. **Sample Texture 2D** (MainTex)
   - Texture: MainTex property
   - UV: uv0
   - Выходы: RGBA (BaseColor, BaseAlpha)

3. **Texel Size Node** (из MainTex)
   - Подключить MainTex property
   - Split на X и Y компоненты

4. **Multiply** для вычисления оффсетов:
   - OffsetX = OutlineThickness × TexelSize.X
   - OffsetY = OutlineThickness × TexelSize.Y

5. **Vector2** для создания оффсетов:
   - offE = (OffsetX, 0)
   - offW = (-OffsetX, 0) - использовать Negate
   - offN = (0, OffsetY)
   - offS = (0, -OffsetY) - использовать Negate

6. **Add** для UV соседей:
   - uvE = uv0 + offE
   - uvW = uv0 + offW
   - uvN = uv0 + offN
   - uvS = uv0 + offS

7. **Sample Texture 2D** для каждого соседа (4 направления):
   - Использовать те же настройки, но разные UV
   - Извлечь альфу (A канал) из каждого

8. **Maximum** узлы:
   - m1 = max(aE, aW)
   - m2 = max(aN, aS)
   - NeighborMax = max(m1, m2)

9. **Step** узлы для масок:
   - InsideMask = step(AlphaThreshold, BaseAlpha)
   - NeighborMask = step(AlphaThreshold, NeighborMax)

10. **One Minus** для OutsideMask:
    - OutsideMask = 1 - InsideMask

11. **Multiply** для OutlineMask:
    - OutlineMask = OutsideMask × NeighborMask

12. **Lerp** для финального цвета:
    - FinalColor = lerp(BaseColor, OutlineColor.rgb, OutlineMask)
    - FinalAlpha = lerp(BaseAlpha, OutlineColor.a, OutlineMask)

13. **Split** OutlineColor для получения альфы

14. Подключить к **Sprite Unlit Master**:
    - Base Color = FinalColor
    - Alpha = FinalAlpha

## Опционально: диагональные соседи

Для более плавного контура добавьте 4 диагональных направления (NE, NW, SE, SW) и включите их в NeighborMax через дополнительные Maximum узлы.

## Использование

После создания Shader Graph:
1. Создайте Material на основе этого Shader Graph
2. Настройте параметры в Material Inspector
3. ElementController автоматически найдет и использует шейдер по имени "Universal Render Pipeline/2D/SpriteOutline"
