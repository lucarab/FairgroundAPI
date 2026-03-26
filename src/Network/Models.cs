using System;

namespace FairgroundAPI.Network
{
    /// <summary>
    /// Base class for all outgoing WebSocket messages sent from the plugin to the web dashboard.
    /// </summary>
    [Serializable]
    public class WsMessage
    {
        public string type;
    }

    /// <summary>
    /// Message broadcasted when the local player gains or loses control of a ride session.
    /// </summary>
    [Serializable]
    public class SessionMessage : WsMessage
    {
        public bool active;
    }

    /// <summary>
    /// Message containing the full state and component layout of the currently controlled ride.
    /// </summary>
    [Serializable]
    public class FullStateMessage : WsMessage
    {
        public bool active;
        public LightData[] lights;
        public ButtonData[] buttons;
        public SwitchData[] switches;
        public PotentiometerData[] potentiometers;
        public JoystickData[] joysticks;
        public StopButtonData[] stopButtons;
        public MultyToggleData[] multyToggles;
        public DropdownData[] dropdowns;
        public SliderData[] sliders;
        public PresetButtonData[] presetButtons;
    }

    /// <summary>
    /// Message broadcasted when a status light changes its color, state, or blinking mode.
    /// </summary>
    [Serializable]
    public class LightUpdateMessage : WsMessage
    {
        public string name;
        public string color;
        public string mode;
    }

    /// <summary>
    /// Message broadcasted when a multy toggle successfully changes its position or state.
    /// </summary>
    [Serializable]
    public class MultyToggleUpdateMessage : WsMessage
    {
        public string name;
        public int value;
    }

    /// <summary>
    /// Data representation of a light component for JSON serialization.
    /// </summary>
    [Serializable]
    public class LightData
    {
        public string name;
        public string color;
        public string mode;
    }

    /// <summary>
    /// Data representation of a physical panel button.
    /// </summary>
    [Serializable]
    public class ButtonData
    {
        public string name;
    }

    /// <summary>
    /// Data representation of a rotary switch, including its maximum allowed position index.
    /// </summary>
    [Serializable]
    public class SwitchData
    {
        public string name;
        public int maxState;
    }

    /// <summary>
    /// Data representation of a potentiometer, including its minimum and maximum rotation values.
    /// </summary>
    [Serializable]
    public class PotentiometerData
    {
        public string name;
        public float min;
        public float max;
        public float current;
    }

    /// <summary>
    /// Data representation of a 2-axis joystick, including its allowed constraints for X and Y axes.
    /// </summary>
    [Serializable]
    public class JoystickData
    {
        public string name;
        public float minX;
        public float maxX;
        public float minY;
        public float maxY;
        public float currentX;
        public float currentY;
    }

    /// <summary>
    /// Data representation of an emergency or operational stop button.
    /// </summary>
    [Serializable]
    public class StopButtonData
    {
        public string name;
    }

    /// <summary>
    /// Data representation of a multi-toggle component.
    /// </summary>
    [Serializable]
    public class MultyToggleData
    {
        public string name;
        public int value;
    }

    /// <summary>
    /// Data representation of an on-screen dropdown menu, including its options and current index.
    /// </summary>
    [Serializable]
    public class DropdownData
    {
        public string name;
        public int selectedIndex;
        public string[] options;
    }

    /// <summary>
    /// Message broadcasted when a dropdown changes its selected option.
    /// </summary>
    [Serializable]
    public class DropdownUpdateMessage : WsMessage
    {
        public string name;
        public int selectedIndex;
    }

    /// <summary>
    /// Data representation of an on-screen slider, including its defined limits.
    /// </summary>
    [Serializable]
    public class SliderData
    {
        public string name;
        public float min;
        public float max;
        public float current;
    }

    /// <summary>
    /// Message broadcasted when a slider's value is modified.
    /// </summary>
    [Serializable]
    public class SliderUpdateMessage : WsMessage
    {
        public string name;
        public float value;
    }

    /// <summary>
    /// Data representation of an on-screen preset activation button.
    /// </summary>
    [Serializable]
    public class PresetButtonData
    {
        public string name;
    }

    /// <summary>
    /// Base class for all incoming WebSocket commands received from the web dashboard.
    /// </summary>
    [Serializable]
    public class IncomingCommand
    {
        public string action;
        public string name;
    }

    /// <summary>
    /// Command payload for interactions accepting a boolean value (e.g., buttons).
    /// </summary>
    [Serializable]
    public class BoolCommand : IncomingCommand
    {
        public bool value;
    }

    /// <summary>
    /// Command payload for interactions accepting an integer value (e.g., switches, dropdowns).
    /// </summary>
    [Serializable]
    public class IntCommand : IncomingCommand
    {
        public int value;
    }

    /// <summary>
    /// Command payload for interactions accepting a float value (e.g., potentiometers, sliders).
    /// </summary>
    [Serializable]
    public class FloatCommand : IncomingCommand
    {
        public float value;
    }

    /// <summary>
    /// Command payload for interactions accepting X and Y float coordinates (e.g., joysticks).
    /// </summary>
    [Serializable]
    public class Vector2Command : IncomingCommand
    {
        public float x;
        public float y;
    }
}
