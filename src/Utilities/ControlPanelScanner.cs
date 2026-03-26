using UnityEngine;
using System.Collections.Generic;

namespace FairgroundAPI.Utilities
{
    /// <summary>
    /// Discovers all control panel components (lights, buttons, switches, potentiometers, joysticks)
    /// on a ride by traversing the transform hierarchy from the rights controller up to the ride root.
    /// </summary>
    public static class ControlPanelScanner
    {
        /// <summary>
        /// Clears all tracked component dictionaries and repopulates them
        /// by scanning the ride's transform hierarchy.
        /// </summary>
        public static void ScanAndPopulate(Ride_Rights_Controller rightsController)
        {
            Managers.SessionManager.TrackedLights.Clear();
            Managers.SessionManager.TrackedButtons.Clear();
            Managers.SessionManager.TrackedSwitches.Clear();
            Managers.SessionManager.TrackedPotentiometers.Clear();
            Managers.SessionManager.TrackedJoysticks.Clear();
            Managers.SessionManager.TrackedStopButtons.Clear();
            Managers.SessionManager.TrackedMultyToggles.Clear();

            Transform root = FindRideRoot(rightsController.transform);

            ScanLights(root, Managers.SessionManager.TrackedLights);
            ScanComponents(root, Managers.SessionManager.TrackedButtons);
            ScanComponents(root, Managers.SessionManager.TrackedSwitches);
            ScanComponents(root, Managers.SessionManager.TrackedPotentiometers);
            ScanComponents(root, Managers.SessionManager.TrackedJoysticks);
            ScanComponents(root, Managers.SessionManager.TrackedStopButtons);
            ScanComponents(root, Managers.SessionManager.TrackedMultyToggles);
        }

        /// <summary>
        /// Walks up the transform hierarchy until reaching the ride root container.
        /// </summary>
        private static Transform FindRideRoot(Transform current)
        {
            while (current.parent != null && !current.parent.name.Contains("---Ride---"))
            {
                current = current.parent;
            }
            return current;
        }

        private static void ScanLights(Transform root, Dictionary<int, State_Light> dict)
        {
            State_Light[] lights = root.GetComponentsInChildren<State_Light>(true);
            foreach (var light in lights)
            {
                string name = light.gameObject.name;
                if (name.StartsWith("Button_") || name.StartsWith("StatusLight_") || name.StartsWith(" StatusLight_"))
                {
                    dict[light.GetInstanceID()] = light;
                }
            }
        }

        private static void ScanComponents<T>(Transform root, Dictionary<string, T> dict) where T : Component
        {
            T[] components = root.GetComponentsInChildren<T>(true);
            foreach (var component in components)
            {
                dict.TryAdd(component.gameObject.name, component);
            }
        }
    }
}
