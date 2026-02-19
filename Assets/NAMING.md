# Правила нейминга проекта SuperBanana

Единая структура имён для предсказуемости и читаемости.

## Общий принцип: **PascalCase**

- Без подчёркиваний в именах.
- Каждое слово с заглавной буквы: `LevelFinishOpening`, `CanvasLevelFinish`.

---

## Код (C#)

| Элемент        | Правило     | Примеры                    |
|----------------|-------------|----------------------------|
| Классы         | PascalCase  | `GameManager`, `ShelfController` |
| Методы/свойства| PascalCase  | `NotifyLevelComplete`, `ComboFillAmount` |
| Приватные поля | _camelCase  | `_chainListeners`, `_comboTimer` |
| Константы      | PascalCase  | `LevelFinishCanvasName`    |
| Enum           | PascalCase  | `ElementType.Banana`, `ElementType.CoffeeCup` |

---

## Ассеты Unity

| Тип           | Правило     | Примеры |
|---------------|-------------|---------|
| Сцены         | PascalCase  | `DemoScene.unity` |
| Префабы       | PascalCase или Entity_Type для элементов | `Chain.prefab`, `Element_CoffeeCup.prefab`, `CanvasLevelFinish.prefab` |
| Анимации      | PascalCase  | `ChainBreaking.anim`, `LevelFinishOpening.anim` |
| Контроллеры   | PascalCase  | `CanvasLevelFinish.controller` |
| Спрайты/UI    | PascalCase, при необходимости префикс категории | `UI_Mascot_Icon.png`, `CoffeeCup_Item3.png` |
| Материалы     | PascalCase  | `Star_Particle` → предпочтительно `StarParticle` |
| VFX           | PascalCase  | `RocketExplosionVFX.prefab` |

---

## Имена GameObject в сценах/префабах

- **PascalCase**: корневые канвасы и ключевые объекты — `CanvasLevelFinish`, `CanvasPopupPauseMenu`, `Mascot`.
- Для элементов геймплея допускается префикс типа: `Element_Banana`, `Element_CoffeeCup` (соответствует enum/префабам).

---

## Исправленные опечатки (уже внесены)

- `CoffieCup` → **CoffeeCup**
- `ChainBraking` → **ChainBreaking** (разрыв цепи, не торможение)
- `Maskot` → **Mascot**

---

## Рекомендации при добавлении новых ассетов

1. Канвасы: `Canvas` + назначение в PascalCase, например `CanvasLevelFinish`, `CanvasPopupPauseMenu`.
2. Клипы анимации: сущность + состояние, например `LevelFinishOpening`, `PauseMenuClosed`.
3. Не использовать подчёркивания в середине имени, кроме устоявшихся префиксов (`UI_`, `Element_`) при необходимости.
