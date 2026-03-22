using FairgroundAPI.Core;
using FairgroundAPI.Network;
using FairgroundAPI.Utilities;

namespace FairgroundAPI.Managers
{
    /// <summary>
    /// Manages the current ride session, including tracking all discovered control panel
    /// components (lights, buttons, switches, potentiometers, joysticks) and handling
    /// ownership changes when the local player gains or loses control of a ride.
    /// </summary>
    public static class SessionManager
    {
        public static int ActiveRightsControllerId { get; private set; }
        public static Dictionary<int, State_Light> TrackedLights { get; } = new();
        public static Dictionary<string, Panel_Button> TrackedButtons { get; } = new();
        public static Dictionary<string, Rot_Switch> TrackedSwitches { get; } = new();
        public static Dictionary<string, Potentiometer> TrackedPotentiometers { get; } = new();
        public static Dictionary<string, Joystick> TrackedJoysticks { get; } = new();

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
                    FairgroundPlugin.Log.LogInfo("Lost rights to the active controller.");
                    ClearSession();
                    WebSocketManager.BroadcastSessionLost();
                }
                return;
            }

            if (HasActiveSession)
            {
                FairgroundPlugin.Log.LogInfo("Switching controller. Clearing previous session.");
                ClearSession();
            }

            ActiveRightsControllerId = controllerId;
            FairgroundPlugin.Log.LogInfo("Acquired rights to a new controller. Scanning components...");

            ControlPanelScanner.ScanAndPopulate(rightsController);

            LogScannedComponents();
            WebSocketManager.SendFullState();
        }

        private static void LogScannedComponents()
        {
            FairgroundPlugin.Log.LogInfo(
                $"Scan complete: {TrackedLights.Count} Lights, {TrackedButtons.Count} Buttons, " +
                $"{TrackedSwitches.Count} Switches, {TrackedPotentiometers.Count} Potentiometers, " +
                $"{TrackedJoysticks.Count} Joysticks."
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
            Components.MainLoopComponent.Instance?.ClearCache();
            FairgroundPlugin.Log.LogInfo("Session cleared.");
        }
    }
}
