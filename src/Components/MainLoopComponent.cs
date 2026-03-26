using System.Collections.Concurrent;
using FairgroundAPI.Managers;
using FairgroundAPI.Network;
using FairgroundAPI.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FairgroundAPI.Components
{
    /// <summary>
    /// Persistent MonoBehaviour that runs on the Unity main thread.
    /// Processes queued actions from WebSocket threads and polls light states for changes.
    /// </summary>
    public class MainLoopComponent : MonoBehaviour
    {
        public static MainLoopComponent Instance { get; private set; }
        public MainLoopComponent(System.IntPtr ptr) : base(ptr) { }

        private readonly Dictionary<string, string> _lastKnownModes = new();
        private readonly Dictionary<string, int> _lastKnownMultyToggleStates = new();
        private readonly Dictionary<string, int> _lastKnownDropdownValues = new();
        private readonly Dictionary<string, float> _lastKnownSliderValues = new();
        private float _timer;

        private static readonly ConcurrentQueue<System.Action> _mainThreadQueue = new();

        /// <summary>
        /// Schedules an action to be executed on the Unity main thread during the next Update cycle.
        /// </summary>
        public static void EnqueueOnMainThread(System.Action action)
        {
            _mainThreadQueue.Enqueue(action);
        }

        private void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// Resets cached light modes. Called when a session is cleared.
        /// </summary>
        public void ClearCache()
        {
            _lastKnownModes.Clear();
            _lastKnownMultyToggleStates.Clear();
            _lastKnownDropdownValues.Clear();
            _lastKnownSliderValues.Clear();
        }

        private void Update()
        {
            ProcessQueue();

            if (!SessionManager.HasActiveSession) return;

            _timer += Time.deltaTime;
            if (_timer < Core.FairgroundPlugin.ConfigPollRate.Value) return;
            _timer = 0f;

            PollLights();
            PollMultyToggles();
            PollDropdowns();
            PollSliders();
        }

        private void ProcessQueue()
        {
            while (_mainThreadQueue.TryDequeue(out var action))
            {
                try { action.Invoke(); }
                catch (System.Exception ex)
                {
                    Core.FairgroundPlugin.Log.LogError($"[MainThread] Queued action failed: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Checks all tracked lights for mode changes and broadcasts updates to connected clients.
        /// </summary>
        private void PollLights()
        {
            foreach (var kvp in SessionManager.TrackedLights)
            {
                State_Light light = kvp.Value;
                if (light.WasCollected) continue;

                string currentMode = light.Light_Mode.ToString();

                if (!_lastKnownModes.TryGetValue(kvp.Key, out string lastMode))
                {
                    _lastKnownModes[kvp.Key] = currentMode;
                }
                else if (lastMode != currentMode)
                {
                    _lastKnownModes[kvp.Key] = currentMode;
                    string color = MaterialHelper.ExtractColorName(light);
                    WebSocketManager.BroadcastLightUpdate(light.gameObject.name, color, currentMode);
                }
            }
        }

        /// <summary>
        /// Checks all tracked multy toggles for state changes and broadcasts updates to connected clients.
        /// </summary>
        private void PollMultyToggles()
        {
            foreach (var kvp in SessionManager.TrackedMultyToggles)
            {
                Multy_Toggle_Sync sync = kvp.Value;
                if (sync == null || sync.WasCollected) continue;

                int currentState = sync.Value;

                if (!_lastKnownMultyToggleStates.TryGetValue(kvp.Key, out int lastState))
                {
                    _lastKnownMultyToggleStates[kvp.Key] = currentState;
                }
                else if (lastState != currentState)
                {
                    _lastKnownMultyToggleStates[kvp.Key] = currentState;
                    WebSocketManager.BroadcastMultyToggleUpdate(kvp.Key, currentState);
                }
            }
        }

        /// <summary>
        /// Checks all tracked dropdowns for value changes and broadcasts updates to connected clients.
        /// </summary>
        private void PollDropdowns()
        {
            foreach (var kvp in SessionManager.TrackedDropdowns)
            {
                Dropdown_Sync sync = kvp.Value;
                if (sync == null || sync.WasCollected) continue;

                int currentValue = sync.Value;

                if (!_lastKnownDropdownValues.TryGetValue(kvp.Key, out int lastValue))
                {
                    _lastKnownDropdownValues[kvp.Key] = currentValue;
                }
                else if (lastValue != currentValue)
                {
                    _lastKnownDropdownValues[kvp.Key] = currentValue;
                    WebSocketManager.BroadcastDropdownUpdate(kvp.Key, currentValue);
                }
            }
        }

        /// <summary>
        /// Checks all tracked sliders for value changes and broadcasts updates to connected clients.
        /// </summary>
        private void PollSliders()
        {
            foreach (var kvp in SessionManager.TrackedSliders)
            {
                Slider_Sync slider = kvp.Value;
                if (slider == null || slider.WasCollected) continue;

                float currentValue = slider.Value;

                if (!_lastKnownSliderValues.TryGetValue(kvp.Key, out float lastValue))
                {
                    _lastKnownSliderValues[kvp.Key] = currentValue;
                }
                else if (lastValue != currentValue)
                {
                    _lastKnownSliderValues[kvp.Key] = currentValue;
                    WebSocketManager.BroadcastSliderUpdate(kvp.Key, currentValue);
                }
            }
        }
    }
}
