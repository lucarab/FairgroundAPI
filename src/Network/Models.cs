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

    [Serializable]
    public class SessionMessage : WsMessage
    {
        public bool active;
    }

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
    }

    [Serializable]
    public class LightUpdateMessage : WsMessage
    {
        public int id;
        public string name;
        public string color;
        public string mode;
    }

    /// <summary>
    /// Data representation of a light component for JSON serialization.
    /// </summary>
    [Serializable]
    public class LightData
    {
        public int id;
        public string name;
        public string color;
        public string mode;
    }

    [Serializable]
    public class ButtonData
    {
        public string name;
    }

    [Serializable]
    public class SwitchData
    {
        public string name;
        public int maxState;
    }

    [Serializable]
    public class PotentiometerData
    {
        public string name;
        public float min;
        public float max;
        public float current;
    }

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

    [Serializable]
    public class StopButtonData
    {
        public string name;
        public bool isDown;
    }

    [Serializable]
    public class MultyToggleData
    {
        public string name;
        public int value;
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

    [Serializable]
    public class BoolCommand : IncomingCommand
    {
        public bool value;
    }

    [Serializable]
    public class IntCommand : IncomingCommand
    {
        public int value;
    }

    [Serializable]
    public class FloatCommand : IncomingCommand
    {
        public float value;
    }

    [Serializable]
    public class Vector2Command : IncomingCommand
    {
        public float x;
        public float y;
    }
}
