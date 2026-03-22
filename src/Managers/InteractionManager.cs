using BepInEx.Logging;
using FairgroundAPI.Core;
using FairgroundAPI.Utilities;

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
