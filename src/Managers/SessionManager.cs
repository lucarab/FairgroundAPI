using FairgroundAPI.Core;
using FairgroundAPI.Network;
using FairgroundAPI.Utilities;
using TMPro;
using UnityEngine.UI;

namespace FairgroundAPI.Managers
{
    /// <summary>
    /// Manages the current ride session, including tracking all discovered control panel
    /// components (lights, buttons, switches, potentiometers, joysticks, stop buttons, multy toggles, dropdowns) and handling
    /// ownership changes when the local player gains or loses control of a ride.
    /// </summary>
    public static class SessionManager
    {
        public static int ActiveRightsControllerId { get; private set; }
        public static Dictionary<string, State_Light> TrackedLights { get; } = new();
        public static Dictionary<string, Panel_Button> TrackedButtons { get; } = new();
        public static Dictionary<string, Rot_Switch> TrackedSwitches { get; } = new();
        public static Dictionary<string, Potentiometer> TrackedPotentiometers { get; } = new();
        public static Dictionary<string, Joystick> TrackedJoysticks { get; } = new();
        public static Dictionary<string, Stop_Button> TrackedStopButtons { get; } = new();
        public static Dictionary<string, Multy_Toggle_Sync> TrackedMultyToggles { get; } = new();
        public static Dictionary<string, Dropdown_Sync> TrackedDropdowns { get; } = new();
        public static Dictionary<string, Slider_Sync> TrackedSliders { get; } = new();
        public static Dictionary<string, Button_Sync> TrackedPresetButtons { get; } = new();

        public static bool HasActiveSession => ActiveRightsControllerId != 0;

        /// <summary>
        /// Handles a change in ride control ownership. Starts a new session if the local
        /// player acquires control, or clears the session if they lose it.
        /// </summary>
        public static void ProcessRightsChange(Ride_Rights_Controller rightsController, bool isLocalPlayer)
        {
            int controllerId = rightsController.GetInstanceID();

            if (isLocalPlayer && ActiveRightsControllerId == controllerId) return;

            if (!isLocalPlayer)
            {
                if (ActiveRightsControllerId == controllerId)
                {
                    FairgroundPlugin.Log.LogDebug("Lost rights to the active controller.");
                    ClearSession();
                    WebSocketManager.BroadcastSessionLost();
                }
                return;
            }

            if (HasActiveSession)
            {
                FairgroundPlugin.Log.LogDebug("Switching controller. Clearing previous session.");
                ClearSession();
            }

            ActiveRightsControllerId = controllerId;
            FairgroundPlugin.Log.LogDebug("Acquired rights to a new controller. Scanning components...");

            ControlPanelScanner.ScanAndPopulate(rightsController);

            LogScannedComponents();
            WebSocketManager.SendFullState();
        }

        private static void LogScannedComponents()
        {
            FairgroundPlugin.Log.LogDebug(
                $"Scan complete: {TrackedLights.Count} Lights, {TrackedButtons.Count} Buttons, " +
                $"{TrackedSwitches.Count} Switches, {TrackedPotentiometers.Count} Potentiometers, " +
                $"{TrackedJoysticks.Count} Joysticks, {TrackedStopButtons.Count} StopButtons, " +
                $"{TrackedMultyToggles.Count} MultyToggles, {TrackedDropdowns.Count} Dropdowns, " +
                $"{TrackedSliders.Count} Sliders, {TrackedPresetButtons.Count} PresetButtons."
            );
        }

        /// <summary>
        /// Clears all tracked components and resets the session state.
        /// </summary>
        public static void ClearSession()
        {
            ActiveRightsControllerId = 0;
            TrackedLights.Clear();
            TrackedButtons.Clear();
            TrackedSwitches.Clear();
            TrackedPotentiometers.Clear();
            TrackedJoysticks.Clear();
            TrackedStopButtons.Clear();
            TrackedMultyToggles.Clear();
            TrackedDropdowns.Clear();
            TrackedSliders.Clear();
            TrackedPresetButtons.Clear();
            Components.MainLoopComponent.Instance?.ClearCache();
            FairgroundPlugin.Log.LogDebug("Session cleared.");
        }
    }
}
