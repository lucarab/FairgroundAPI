using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

namespace FairgroundAPI.Utilities
{
    /// <summary>
    /// Discovers all control panel components (lights, buttons, switches, potentiometers, joysticks, stop buttons, multy toggles)
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
            Managers.SessionManager.TrackedDropdowns.Clear();

            Transform root = FindRideRoot(rightsController.transform);

            ScanLights(root, Managers.SessionManager.TrackedLights);
            ScanComponents(root, Managers.SessionManager.TrackedButtons);
            ScanComponents(root, Managers.SessionManager.TrackedSwitches);
            ScanComponents(root, Managers.SessionManager.TrackedPotentiometers);
            ScanComponents(root, Managers.SessionManager.TrackedJoysticks);
            ScanComponents(root, Managers.SessionManager.TrackedStopButtons);
            ScanComponents(root, Managers.SessionManager.TrackedMultyToggles);
            ScanComponents(root, Managers.SessionManager.TrackedDropdowns);
            ScanComponents(root, Managers.SessionManager.TrackedSliders);
            ScanComponents(root, Managers.SessionManager.TrackedPresetButtons);
        }

        /// <summary>
        /// Walks up the transform hierarchy until reaching the individual ride root (e.g. _StarLight_)
        /// under the "---Ride---" container.
        /// </summary>
        private static Transform FindRideRoot(Transform current)
        {
            Transform temp = current;
            while (temp.parent != null && !temp.parent.name.Contains("---Ride---"))
            {
                temp = temp.parent;
            }

            // If we found the ---Ride--- container, temp is the individual ride (e.g. _StarLight_)
            if (temp.parent != null && temp.parent.name.Contains("---Ride---"))
            {
                return temp;
            }

            // Fallback: If not found, don't return the scene root. Just return the controller's immediate transform.
            return current;
        }

        /// <summary>
        /// Checks if any parent in the hierarchy contains "UI" in its name.
        /// Useful for filtering out on-screen UI representations of controls.
        /// </summary>
        private static bool HasUIParent(Transform current, Transform stopTarget)
        {
            if (current == null) return false;
            current = current.parent;
            while (current != null && current != stopTarget && current.parent != stopTarget)
            {
                if (current.name.Contains("UI")) return true;
                current = current.parent;
            }
            return false;
        }

        private static void ScanLights(Transform root, Dictionary<string, State_Light> dict)
        {
            State_Light[] lights = root.GetComponentsInChildren<State_Light>(true);
            foreach (var light in lights)
            {
                if (HasUIParent(light.transform, root))
                {
                    continue;
                }

                string name = light.gameObject.name;
                if (name.StartsWith("Button_") || name.StartsWith("StatusLight_") || name.StartsWith(" StatusLight_"))
                {
                    dict.TryAdd(name, light);
                }
            }
        }

        private static void ScanComponents<T>(Transform root, Dictionary<string, T> dict) where T : Component
        {
            string typeName = typeof(T).Name;

            T[] components = root.GetComponentsInChildren<T>(true);
            foreach (var component in components)
            {
                string goName = component.gameObject.name;

                if (!goName.Contains("MovingHead") && HasUIParent(component.transform, root))
                {
                    continue;
                }

                if (goName == "Speed_Sllider" || goName == "Time_Sllider")
                {
                    continue;
                }

                if (typeName == "Multy_Toggle_Sync" && !goName.EndsWith("Multy_Toggle"))
                {
                    continue;
                }

                if (typeName == "Button_Sync" && (!goName.Contains("Preset") || goName.Contains("Save")))
                {
                    continue;
                }

                dict.TryAdd(goName, component);
            }
        }
    }
}
