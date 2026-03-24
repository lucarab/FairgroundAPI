using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using FairgroundAPI.Components;
using FairgroundAPI.Network;
using FairgroundAPI.Utilities;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

namespace FairgroundAPI.Core
{
    /// <summary>
    /// Main entry point for the FairgroundAPI plugin.
    /// Initializes Harmony patches, resolves obfuscated methods, and starts the WebSocket server.
    /// </summary>
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class FairgroundPlugin : BasePlugin
    {
        public const string PLUGIN_GUID = "com.invalidluca.fairground.api";
        public const string PLUGIN_NAME = "FairgroundAPI";
        public const string PLUGIN_VERSION = "1.0.0";

        public static FairgroundPlugin Instance { get; private set; }
        internal new static ManualLogSource Log;

        public override void Load()
        {
            Instance = this;
            Log = base.Log;

            Log.LogInfo($"{PLUGIN_NAME} v{PLUGIN_VERSION} initiated.");

            if (!ApplyHarmonyPatches()) return;

            MethodResolver.ResolveAll();

            ClassInjector.RegisterTypeInIl2Cpp<MainLoopComponent>();

            var loopObj = new GameObject("FairgroundAPI_MainLoop");
            GameObject.DontDestroyOnLoad(loopObj);
            loopObj.AddComponent<MainLoopComponent>();

            WebSocketManager.Start();

            Log.LogInfo($"{PLUGIN_NAME} loaded successfully!");
        }

        public override bool Unload()
        {
            WebSocketManager.Stop();
            Log.LogInfo($"{PLUGIN_NAME} unloaded.");
            return base.Unload();
        }

        private bool ApplyHarmonyPatches()
        {
            try
            {
                new Harmony(PLUGIN_GUID).PatchAll();
                return true;
            }
            catch (Exception e)
            {
                Log.LogError($"Critical error during patching: {e.Message}");
                return false;
            }
        }
    }
}
