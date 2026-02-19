# Pause Menu Setup

## 1. Popup (Canvas_Popup_PauseMenu)

- Add **Animator** to the root GameObject (Canvas_Popup_PauseMenu) and assign **Animations/Canvas_Popup_PauseMenu** controller.
- Add **PauseMenuController** to the same root:
  - **Animator**: assign the Animator above (or leave empty to use GetComponent).
  - **Close Button**: assign Pause_CloseButton (X).
  - **Continue Button**: assign ContinueButton.
  - **Quit Button**: assign QuitButton.
- Remove any existing OnClick on the Close button that calls `SetActive` on the popup (so only our Close runs).

## 2. Pause Button

- Add **PauseButtonOpener** to the PauseButton. Optionally assign **Pause Popup**; if empty, uses `PauseMenuController.Instance`.

## 3. Toggle Buttons (Music / Sound / Vibration)

- On **MusicButton**, **SoundButton**, **VibrationButton** add **PauseMenuToggleButton**:
  - **Type**: Music / Sound Effects / Vibration.
  - **Overlay Image**: assign the child "TurnOffIcon" (shown when option is OFF).
  - **Target Graphic**: optional; button image for pressed color when OFF.

## 4. Game Settings

- Create an empty GameObject (e.g. "GameSettings") and add **GameSettings**. This persists Music/Sound/Vibration in PlayerPrefs.

## 5. Background Music (optional)

- If you have a background music AudioSource, add **BackgroundMusic** to that GameObject. It will mute when Music is OFF in the pause menu.

## Behaviour

- **Pause button** → `IsOpen = true` → opening animation, time scale 0.
- **Close (X) or Continue** → `IsOpen = false` → closing animation, time scale 1.
- **Quit** → exit play mode (Editor) or quit application (build).
- **Music/Sound/Vibration** toggles: overlay visible when OFF, button uses pressed color; state saved in PlayerPrefs. SFX and rocket sound respect Sound Effects setting; vibration respects Vibration setting.
