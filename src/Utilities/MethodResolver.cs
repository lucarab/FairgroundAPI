using System.Reflection;
using BepInEx.Logging;
using FairgroundAPI.Core;
using UnityEngine;

namespace FairgroundAPI.Utilities
{
    /// <summary>
    /// Resolves obfuscated game methods at runtime using heuristic reflection.
    /// Finds the ApplyValue method and its record type on Panel_Button, then caches
    /// the resolved method per component type for efficient repeated invocation.
    /// </summary>
    public static class MethodResolver
    {
        private static ManualLogSource Log => FairgroundPlugin.Log;

        private static string _applyMethodName;
        private static Type _recordType;
        private static FieldInfo _fieldBool, _fieldInt, _fieldFloat, _fieldVector2, _fieldKind;
        private static object _kindBool, _kindInt, _kindFloat, _kindVector2;
        private static readonly Dictionary<Type, MethodInfo> _applyCache = new();

        private static MethodInfo _stopButtonToggleMethod;

        public static bool IsResolved { get; private set; }

        /// <summary>
        /// Performs all heuristic method resolutions at plugin startup.
        /// </summary>
        public static bool ResolveAll()
        {
            Log.LogInfo("[Resolver] Starting obfuscated method resolution...");

            bool applyOk = ResolveApplyValue();
            bool stopBtnOk = ResolveStopButtonToggle();

            IsResolved = applyOk && stopBtnOk;

            Log.LogInfo(IsResolved
                ? "[Resolver] All methods resolved successfully."
                : "[Resolver] FAILED to resolve some methods. Check logs.");

            return IsResolved;
        }

        /// <summary>
        /// Searches Panel_Button for a void method that accepts a single struct parameter
        /// containing boolValue, intValue, floatValue, vector2Value, and kind fields.
        /// </summary>
        private static bool ResolveApplyValue()
        {
            foreach (var method in typeof(Panel_Button).GetMethods(BindingFlags.Instance | BindingFlags.Public))
            {
                if (method.ReturnType != typeof(void)) continue;
                var parms = method.GetParameters();
                if (parms.Length != 1) continue;

                var pt = parms[0].ParameterType;
                if (pt.IsPrimitive || pt == typeof(string) || !pt.IsValueType || pt.IsEnum) continue;

                var bf = pt.GetField("boolValue");
                var inf = pt.GetField("intValue");
                var ff = pt.GetField("floatValue");
                var v2f = pt.GetField("vector2Value");
                var kf = pt.GetField("kind");
                if (bf == null || inf == null || ff == null || kf == null) continue;

                _applyMethodName = method.Name;
                _recordType = pt;
                _fieldBool = bf; _fieldInt = inf; _fieldFloat = ff; _fieldVector2 = v2f; _fieldKind = kf;

                var kindType = kf.FieldType;
                try
                {
                    _kindBool = Enum.Parse(kindType, "Bool");
                    _kindInt = Enum.Parse(kindType, "Int");
                    _kindFloat = Enum.Parse(kindType, "Float");
                    _kindVector2 = Enum.Parse(kindType, "Vector2");
                }
                catch
                {
                    var vals = Enum.GetValues(kindType);
                    if (vals.Length < 4) { Log.LogError("[Resolver] RecordKind enum has < 4 values!"); return false; }
                    _kindBool = vals.GetValue(0);
                    _kindInt = vals.GetValue(1);
                    _kindFloat = vals.GetValue(2);
                    _kindVector2 = vals.GetValue(3);
                    Log.LogWarning("[Resolver] RecordKind enum names obfuscated — resolved by ordinal.");
                }

                Log.LogDebug($"[Resolver] ApplyValue -> '{_applyMethodName}' | RecordType: '{pt.Name}' | KindType: '{kindType.Name}'");
                if (_fieldVector2 != null)
                    Log.LogDebug("[Resolver] Vector2 support available.");
                else
                    Log.LogWarning("[Resolver] vector2Value field not found — Joystick support disabled.");

                return true;
            }

            Log.LogError("[Resolver] Could not find ApplyValue method on Panel_Button!");
            return false;
        }

        /// <summary>Applies a boolean value (press/release) to a component.</summary>
        public static void ApplyBoolValue(object component, bool value)
        {
            var record = CreateRecord();
            _fieldKind.SetValue(record, _kindBool);
            _fieldBool.SetValue(record, value);
            InvokeApply(component, record);
        }

        /// <summary>Applies an integer value (switch position) to a component.</summary>
        public static void ApplyIntValue(object component, int value)
        {
            var record = CreateRecord();
            _fieldKind.SetValue(record, _kindInt);
            _fieldInt.SetValue(record, value);
            InvokeApply(component, record);
        }

        /// <summary>Applies a float value (potentiometer rotation) to a component.</summary>
        public static void ApplyFloatValue(object component, float value)
        {
            var record = CreateRecord();
            _fieldKind.SetValue(record, _kindFloat);
            _fieldFloat.SetValue(record, value);
            InvokeApply(component, record);
        }

        /// <summary>Applies a Vector2 value (joystick position) to a component.</summary>
        public static void ApplyVector2Value(object component, float x, float y)
        {
            if (_fieldVector2 == null)
            {
                Log.LogError("[Resolver] Cannot apply Vector2 — field not resolved.");
                return;
            }

            var record = CreateRecord();
            _fieldKind.SetValue(record, _kindVector2);
            _fieldVector2.SetValue(record, new Vector2(x, y));
            InvokeApply(component, record);
        }

        private static object CreateRecord() => Activator.CreateInstance(_recordType);

        /// <summary>
        /// Invokes the cached ApplyValue method on the given component.
        /// Resolves and caches the method per type on first call.
        /// </summary>
        private static void InvokeApply(object component, object record)
        {
            var type = component.GetType();
            if (!_applyCache.TryGetValue(type, out var method))
            {
                method = type.GetMethod(_applyMethodName);
                _applyCache[type] = method;
            }

            if (method == null)
            {
                Log.LogWarning($"[Resolver] ApplyValue method '{_applyMethodName}' not found on type '{type.Name}'.");
                return;
            }

            method.Invoke(component, new[] { record });
        }

        /// <summary>
        /// Resolves the toggle method on Stop_Button by its known obfuscated name.
        /// The method "ljq" is a void, parameterless method that toggles the button state.
        /// </summary>
        private static bool ResolveStopButtonToggle()
        {
            const string toggleMethodName = "ljq";

            _stopButtonToggleMethod = typeof(Stop_Button)
                .GetMethod(toggleMethodName, BindingFlags.Instance | BindingFlags.Public);

            if (_stopButtonToggleMethod != null)
            {
                Log.LogDebug($"[Resolver] StopButton toggle -> '{toggleMethodName}'");
                return true;
            }

            Log.LogWarning($"[Resolver] Could not find method '{toggleMethodName}' on Stop_Button. StopButton support disabled.");
            return false;
        }

        /// <summary>Invokes the resolved toggle method on a Stop_Button instance.</summary>
        public static void InvokeStopButtonToggle(Stop_Button stopButton)
        {
            if (_stopButtonToggleMethod == null)
            {
                Log.LogError("[Resolver] StopButton toggle method not resolved.");
                return;
            }

            _stopButtonToggleMethod.Invoke(stopButton, null);
        }
    }
}
