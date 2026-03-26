using BepInEx.Logging;
using FairgroundAPI.Core;
using FairgroundAPI.Utilities;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FairgroundAPI.Managers
{
    /// <summary>
    /// Translates incoming WebSocket commands into game actions by
    /// looking up tracked components and invoking their resolved methods.
    /// </summary>
    public static class InteractionManager
    {
        private static ManualLogSource Log => FairgroundPlugin.Log;

        /// <summary>Presses or releases a panel button by name.</summary>
        public static void SetButtonState(string buttonName, bool isPressed)
        {
            if (!EnsureReady()) return;

            if (!SessionManager.TrackedButtons.TryGetValue(buttonName, out Panel_Button button))
            {
                Log.LogWarning($"Button '{buttonName}' not found on this console.");
                return;
            }

            if (button.WasCollected) return;

            MethodResolver.ApplyBoolValue(button, isPressed);
            Log.LogDebug($"[Command] Button {(isPressed ? "PRESSED" : "RELEASED")} -> {buttonName}");
        }

        /// <summary>Sets a rotary switch to the specified position.</summary>
        public static void SetSwitchState(string switchName, int state)
        {
            if (!EnsureReady()) return;

            if (!SessionManager.TrackedSwitches.TryGetValue(switchName, out Rot_Switch rotSwitch))
            {
                Log.LogWarning($"Switch '{switchName}' not found on this console.");
                return;
            }

            if (state < 0 || state > rotSwitch.Max_State)
            {
                Log.LogWarning($"State {state} for Switch '{switchName}' is invalid. Allowed: 0–{rotSwitch.Max_State}.");
                return;
            }

            if (rotSwitch.WasCollected) return;

            MethodResolver.ApplyIntValue(rotSwitch, state);
            Log.LogDebug($"[Command] Switch SET state {state} -> {switchName}");
        }

        /// <summary>Sets a potentiometer to the specified value within its allowed range.</summary>
        public static void SetPotentiometerValue(string name, float value)
        {
            if (!EnsureReady()) return;

            if (!SessionManager.TrackedPotentiometers.TryGetValue(name, out Potentiometer potentiometer))
            {
                Log.LogWarning($"Potentiometer '{name}' not found on this console.");
                return;
            }

            if (value < potentiometer.Min_Rot || value > potentiometer.Max_Rot)
            {
                Log.LogWarning($"Value {value} for Potentiometer '{name}' out of range. Allowed: {potentiometer.Min_Rot}–{potentiometer.Max_Rot}.");
                return;
            }

            if (potentiometer.WasCollected) return;

            MethodResolver.ApplyFloatValue(potentiometer, value);
            Log.LogDebug($"[Command] Potentiometer SET to {value} -> {name}");
        }

        /// <summary>Sets a joystick to the specified X/Y coordinates, clamped to its range.</summary>
        public static void SetJoystickValue(string name, float x, float y)
        {
            if (!EnsureReady()) return;

            if (!SessionManager.TrackedJoysticks.TryGetValue(name, out Joystick joystick))
            {
                Log.LogWarning($"Joystick '{name}' not found on this console.");
                return;
            }

            float clampedX = Math.Clamp(x, joystick.min_Rot_X, joystick.max_Rot_X);
            float clampedY = Math.Clamp(y, joystick.min_Rot_Y, joystick.max_Rot_Y);

            if (joystick.WasCollected) return;

            MethodResolver.ApplyVector2Value(joystick, clampedX, clampedY);
            Log.LogDebug($"[Command] Joystick SET to ({clampedX:F1}, {clampedY:F1}) -> {name}");
        }

        /// <summary>Toggles a stop button – pressing it down or releasing it back up.</summary>
        public static void ToggleStopButton(string name)
        {
            if (!EnsureReady()) return;

            if (!SessionManager.TrackedStopButtons.TryGetValue(name, out Stop_Button stopButton))
            {
                Log.LogWarning($"StopButton '{name}' not found on this console.");
                return;
            }

            if (stopButton.WasCollected) return;

            MethodResolver.InvokeStopButtonToggle(stopButton);
            Log.LogDebug($"[Command] StopButton TOGGLED -> {name} (Is_Down: {stopButton.Is_Down})");
        }

        /// <summary>Toggles a multy toggle.</summary>
        public static void ToggleMultyToggle(string name)
        {
            if (!EnsureReady()) return;

            if (!SessionManager.TrackedMultyToggles.TryGetValue(name, out Multy_Toggle_Sync sync))
            {
                Log.LogWarning($"MultyToggle '{name}' not found on this console.");
                return;
            }

            if (sync == null || sync.WasCollected || sync.Multy_Toggle == null) return;

            MethodResolver.InvokeMultyToggleState(sync.Multy_Toggle);
            Log.LogDebug($"[Command] MultyToggle TOGGLED -> {name} (Value: {sync.Value})");
        }

        /// <summary>Sets a dropdown to the specified index.</summary>
        public static void SetDropdownValue(string name, int index)
        {
            if (!EnsureReady()) return;

            if (!SessionManager.TrackedDropdowns.TryGetValue(name, out Dropdown_Sync sync))
            {
                Log.LogWarning($"Dropdown '{name}' not found on this console.");
                return;
            }

            if (sync == null || sync.WasCollected || sync.Dropdown == null) return;

            int optionCount = 0;
            try { optionCount = sync.Dropdown.options?.Count ?? 0; } catch { }

            if (index < 0 || index >= optionCount)
            {
                Log.LogWarning($"Index {index} for Dropdown '{name}' out of range. Allowed: 0–{optionCount - 1}.");
                return;
            }

            sync.ger(index);
            Log.LogDebug($"[Command] Dropdown SET to {index} -> {name}");
        }

        /// <summary>Sets a slider to the specified float value.</summary>
        public static void SetSliderValue(string name, float value)
        {
            if (!EnsureReady()) return;

            if (!SessionManager.TrackedSliders.TryGetValue(name, out Slider_Sync sync))
            {
                Log.LogWarning($"Slider '{name}' not found on this console.");
                return;
            }

            if (sync == null || sync.WasCollected || sync.Slider == null) return;

            if (value < sync.Slider.minValue || value > sync.Slider.maxValue)
            {
                Log.LogWarning($"Value {value} for Slider '{name}' out of range. Allowed: {sync.Slider.minValue}–{sync.Slider.maxValue}.");
                return;
            }

            sync.gee(value);
            Log.LogDebug($"[Command] Slider SET to {value} -> {name}");
        }

        /// <summary>Presses a preset button via Button_Sync.OnPointerDown.</summary>
        public static void PressPresetButton(string name)
        {
            if (!EnsureReady()) return;

            if (!SessionManager.TrackedPresetButtons.TryGetValue(name, out Button_Sync btnSync))
            {
                Log.LogWarning($"PresetButton '{name}' not found on this console.");
                return;
            }

            if (btnSync == null || btnSync.WasCollected) return;

            btnSync.OnPointerDown(new PointerEventData(null));
            Log.LogDebug($"[Command] PresetButton PRESSED -> {name}");
        }

        private static bool EnsureReady()
        {
            if (!SessionManager.HasActiveSession)
            {
                Log.LogWarning("No active session. Command ignored.");
                return false;
            }
            if (!MethodResolver.IsResolved)
            {
                Log.LogError("MethodResolver not ready. Command ignored.");
                return false;
            }
            return true;
        }
    }
}
