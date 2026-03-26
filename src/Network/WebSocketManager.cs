using System.Text.Json;
using BepInEx.Logging;
using FairgroundAPI.Core;
using FairgroundAPI.Managers;
using FairgroundAPI.Utilities;
using TMPro;
using UnityEngine.UI;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace FairgroundAPI.Network
{
    /// <summary>
    /// WebSocket endpoint that routes incoming messages to the main thread
    /// and sends the full state to newly connected clients.
    /// </summary>
    public class ApiService : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            string data = e.Data;
            Components.MainLoopComponent.EnqueueOnMainThread(
                () => WebSocketManager.HandleIncomingMessage(data));
        }

        protected override void OnOpen()
        {
            FairgroundPlugin.Log.LogInfo("[WebSocket] Client connected.");
            Components.MainLoopComponent.EnqueueOnMainThread(
                () => WebSocketManager.SendFullState());
        }

        protected override void OnClose(CloseEventArgs e)
        {
            FairgroundPlugin.Log.LogInfo("[WebSocket] Client disconnected.");
        }
    }

    /// <summary>
    /// Manages the WebSocket server lifecycle, handles incoming commands from the
    /// web dashboard, and broadcasts game state updates to all connected clients.
    /// </summary>
    public static class WebSocketManager
    {
        private static WebSocketServer _server;
        private static ManualLogSource Log => FairgroundPlugin.Log;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            IncludeFields = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>Starts the WebSocket server on the configured port.</summary>
        public static void Start()
        {
            try
            {
                int port = FairgroundPlugin.ConfigPort.Value;
                _server = new WebSocketServer($"ws://127.0.0.1:{port}");
                _server.AddWebSocketService<ApiService>("/api");
                _server.Start();
                Log.LogInfo($"[WebSocket] Server started on ws://127.0.0.1:{port}/api");
            }
            catch (System.Exception ex)
            {
                Log.LogError($"[WebSocket] Failed to start server: {ex.Message}");
            }
        }

        /// <summary>Stops the WebSocket server if it is currently running.</summary>
        public static void Stop()
        {
            if (_server != null && _server.IsListening)
            {
                _server.Stop();
                Log.LogInfo("[WebSocket] Server stopped.");
            }
        }

        /// <summary>Sends a JSON string to all connected WebSocket clients.</summary>
        public static void Broadcast(string json)
        {
            if (_server == null || !_server.IsListening) return;
            _server.WebSocketServices["/api"]?.Sessions?.Broadcast(json);
        }

        private static string ToJson<T>(T obj) => JsonSerializer.Serialize(obj, JsonOptions);
        private static T FromJson<T>(string json) => JsonSerializer.Deserialize<T>(json, JsonOptions);

        /// <summary>Broadcasts the complete state of all tracked components to all clients.</summary>
        public static void SendFullState()
        {
            if (!SessionManager.HasActiveSession)
            {
                Broadcast(ToJson(new SessionMessage { type = "session", active = false }));
                return;
            }

            var msg = new FullStateMessage
            {
                type = "fullState",
                active = true,
                lights = BuildLightData(),
                buttons = BuildButtonData(),
                switches = BuildSwitchData(),
                potentiometers = BuildPotentiometerData(),
                joysticks = BuildJoystickData(),
                stopButtons = BuildStopButtonData(),
                multyToggles = BuildMultyToggleData(),
                dropdowns = BuildDropdownData(),
                sliders = BuildSliderData(),
                presetButtons = BuildPresetButtonData()
            };

            Broadcast(ToJson(msg));
        }

        /// <summary>Broadcasts a single light's state change to all clients.</summary>
        public static void BroadcastLightUpdate(string name, string color, string mode)
        {
            var msg = new LightUpdateMessage
            {
                type = "lightUpdate",
                name = name,
                color = color,
                mode = mode
            };

            Broadcast(ToJson(msg));
        }

        /// <summary>Notifies all clients that the current session has ended.</summary>
        public static void BroadcastSessionLost()
        {
            Broadcast(ToJson(new SessionMessage { type = "session", active = false }));
        }

        /// <summary>Broadcasts a multy toggle state change to all clients.</summary>
        public static void BroadcastMultyToggleUpdate(string name, int value)
        {
            var msg = new MultyToggleUpdateMessage
            {
                type = "multyToggleUpdate",
                name = name,
                value = value
            };

            Broadcast(ToJson(msg));
        }

        /// <summary>Broadcasts a dropdown value change to all clients.</summary>
        public static void BroadcastDropdownUpdate(string name, int selectedIndex)
        {
            var msg = new DropdownUpdateMessage
            {
                type = "dropdownUpdate",
                name = name,
                selectedIndex = selectedIndex
            };

            Broadcast(ToJson(msg));
        }

        /// <summary>Broadcasts a slider value change to all clients.</summary>
        public static void BroadcastSliderUpdate(string name, float value)
        {
            var msg = new SliderUpdateMessage
            {
                type = "sliderUpdate",
                name = name,
                value = value
            };

            Broadcast(ToJson(msg));
        }

        /// <summary>
        /// Parses an incoming JSON command and routes it to the appropriate InteractionManager method.
        /// </summary>
        public static void HandleIncomingMessage(string data)
        {
            try
            {
                using var doc = JsonDocument.Parse(data);
                if (!doc.RootElement.TryGetProperty("action", out var actionProp))
                {
                    Log.LogWarning("[WebSocket] Message missing 'action' field.");
                    return;
                }

                string action = actionProp.GetString();

                switch (action)
                {
                    case "setButton":
                        var btnCmd = FromJson<BoolCommand>(data);
                        InteractionManager.SetButtonState(btnCmd.name, btnCmd.value);
                        break;

                    case "setSwitch":
                        var swCmd = FromJson<IntCommand>(data);
                        InteractionManager.SetSwitchState(swCmd.name, swCmd.value);
                        break;

                    case "setPotentiometer":
                        var potCmd = FromJson<FloatCommand>(data);
                        InteractionManager.SetPotentiometerValue(potCmd.name, potCmd.value);
                        break;

                    case "setJoystick":
                        var joyCmd = FromJson<Vector2Command>(data);
                        InteractionManager.SetJoystickValue(joyCmd.name, joyCmd.x, joyCmd.y);
                        break;

                    case "requestFullState":
                        SendFullState();
                        break;

                    case "toggleStopButton":
                        var sbCmd = FromJson<IncomingCommand>(data);
                        InteractionManager.ToggleStopButton(sbCmd.name);
                        break;

                    case "toggleMultyToggle":
                        var mtCmd = FromJson<IncomingCommand>(data);
                        InteractionManager.ToggleMultyToggle(mtCmd.name);
                        break;

                    case "setDropdown":
                        var ddCmd = FromJson<IntCommand>(data);
                        InteractionManager.SetDropdownValue(ddCmd.name, ddCmd.value);
                        break;

                    case "setSlider":
                        var sliderCmd = FromJson<FloatCommand>(data);
                        InteractionManager.SetSliderValue(sliderCmd.name, sliderCmd.value);
                        break;

                    case "pressPreset":
                        var presetCmd = FromJson<IncomingCommand>(data);
                        InteractionManager.PressPresetButton(presetCmd.name);
                        break;

                    default:
                        Log.LogWarning($"[WebSocket] Unknown action: {action}");
                        break;
                }
            }
            catch (System.Exception ex)
            {
                Log.LogError($"[WebSocket] Error processing command: {ex.Message}");
            }
        }

        private static LightData[] BuildLightData()
        {
            var list = new List<LightData>();
            foreach (var kvp in SessionManager.TrackedLights)
            {
                var light = kvp.Value;
                if (light.WasCollected) continue;

                list.Add(new LightData
                {
                    name = light.gameObject.name,
                    color = MaterialHelper.ExtractColorName(light),
                    mode = light.Light_Mode.ToString()
                });
            }
            return list.ToArray();
        }

        private static ButtonData[] BuildButtonData()
        {
            var list = new List<ButtonData>();
            foreach (var kvp in SessionManager.TrackedButtons)
            {
                if (kvp.Value.WasCollected) continue;
                list.Add(new ButtonData { name = kvp.Key });
            }
            return list.ToArray();
        }

        private static SwitchData[] BuildSwitchData()
        {
            var list = new List<SwitchData>();
            foreach (var kvp in SessionManager.TrackedSwitches)
            {
                if (kvp.Value.WasCollected) continue;
                list.Add(new SwitchData
                {
                    name = kvp.Key,
                    maxState = kvp.Value.Max_State
                });
            }
            return list.ToArray();
        }

        private static PotentiometerData[] BuildPotentiometerData()
        {
            var list = new List<PotentiometerData>();
            foreach (var kvp in SessionManager.TrackedPotentiometers)
            {
                if (kvp.Value.WasCollected) continue;
                list.Add(new PotentiometerData
                {
                    name = kvp.Key,
                    min = kvp.Value.Min_Rot,
                    max = kvp.Value.Max_Rot,
                    current = kvp.Value.c_Rot
                });
            }
            return list.ToArray();
        }

        private static JoystickData[] BuildJoystickData()
        {
            var list = new List<JoystickData>();
            foreach (var kvp in SessionManager.TrackedJoysticks)
            {
                var j = kvp.Value;
                if (j.WasCollected) continue;

                list.Add(new JoystickData
                {
                    name = kvp.Key,
                    minX = j.min_Rot_X,
                    maxX = j.max_Rot_X,
                    minY = j.min_Rot_Y,
                    maxY = j.max_Rot_Y,
                    currentX = j.c_Rot_X,
                    currentY = j.c_Rot_Y
                });
            }
            return list.ToArray();
        }

        private static StopButtonData[] BuildStopButtonData()
        {
            var list = new List<StopButtonData>();
            foreach (var kvp in SessionManager.TrackedStopButtons)
            {
                var sb = kvp.Value;
                if (sb.WasCollected) continue;

                list.Add(new StopButtonData
                {
                    name = kvp.Key
                });
            }
            return list.ToArray();
        }

        private static MultyToggleData[] BuildMultyToggleData()
        {
            var list = new List<MultyToggleData>();
            foreach (var kvp in SessionManager.TrackedMultyToggles)
            {
                var sync = kvp.Value;
                if (sync == null || sync.WasCollected) continue;

                list.Add(new MultyToggleData
                {
                    name = kvp.Key,
                    value = sync.Value
                });
            }
            return list.ToArray();
        }

        private static DropdownData[] BuildDropdownData()
        {
            var list = new List<DropdownData>();
            foreach (var kvp in SessionManager.TrackedDropdowns)
            {
                var sync = kvp.Value;
                if (sync == null || sync.WasCollected || sync.Dropdown == null) continue;

                var dd = sync.Dropdown;
                var optionNames = new List<string>();
                try
                {
                    var options = dd.options;
                    if (options != null)
                    {
                        // IL2CPP: options is Il2CppSystem.Collections.Generic.List<TMP_Dropdown.OptionData>
                        // Access the internal _items array
                        var items = options._items;
                        int count = options.Count;
                        for (int i = 0; i < count; i++)
                        {
                            var opt = items[i];
                            optionNames.Add(opt?.text ?? "");
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Log.LogWarning($"[WebSocket] Failed to read dropdown options for '{kvp.Key}': {ex.Message}");
                }

                list.Add(new DropdownData
                {
                    name = kvp.Key,
                    selectedIndex = sync.Value,
                    options = optionNames.ToArray()
                });
            }
            return list.ToArray();
        }

        private static SliderData[] BuildSliderData()
        {
            var list = new List<SliderData>();
            foreach (var kvp in SessionManager.TrackedSliders)
            {
                var sync = kvp.Value;
                if (sync == null || sync.WasCollected || sync.Slider == null) continue;

                list.Add(new SliderData
                {
                    name = kvp.Key,
                    min = sync.Slider.minValue,
                    max = sync.Slider.maxValue,
                    current = sync.Value
                });
            }
            return list.ToArray();
        }

        private static PresetButtonData[] BuildPresetButtonData()
        {
            var list = new List<PresetButtonData>();
            foreach (var kvp in SessionManager.TrackedPresetButtons)
            {
                var btn = kvp.Value;
                if (btn == null || btn.WasCollected) continue;
                list.Add(new PresetButtonData { name = kvp.Key });
            }
            return list.ToArray();
        }
    }
}
