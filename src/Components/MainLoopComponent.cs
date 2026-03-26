using System.Collections.Concurrent;
using FairgroundAPI.Managers;
using FairgroundAPI.Network;
using FairgroundAPI.Utilities;
using UnityEngine;

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

        private readonly Dictionary<int, string> _lastKnownModes = new();
        private const float PollInterval = 0.25f;
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
        }

        private void Update()
        {
            ProcessQueue();
            PollLights();
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
        /// Checks all tracked lights for mode changes at a fixed interval
        /// and broadcasts updates to connected clients.
        /// </summary>
        private void PollLights()
        {
            if (!SessionManager.HasActiveSession) return;

            _timer += Time.deltaTime;
            if (_timer < PollInterval) return;
            _timer = 0f;

            var snapshot = new List<KeyValuePair<int, State_Light>>(SessionManager.TrackedLights);

            foreach (var kvp in snapshot)
            {
                State_Light light = kvp.Value;
                if (light.WasCollected) continue;

                string currentMode = light.Light_Mode.ToString();

                if (!_lastKnownModes.TryGetValue(kvp.Key, out string lastMode))
                {
                    _lastKnownModes[kvp.Key] = currentMode;
                    BroadcastLight(kvp.Key, light, currentMode);
                }
                else if (lastMode != currentMode)
                {
                    _lastKnownModes[kvp.Key] = currentMode;
                    BroadcastLight(kvp.Key, light, currentMode);
                }
            }
        }

        private void BroadcastLight(int id, State_Light light, string mode)
        {
            string color = MaterialHelper.ExtractColorName(light);
            WebSocketManager.BroadcastLightUpdate(id, light.gameObject.name, color, mode);
        }
    }
}
