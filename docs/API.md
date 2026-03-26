# FairgroundAPI Documentation

Welcome to the documentation for the FairgroundAPI. This API allows you to remotely control and monitor rides in the game via a local WebSocket connection.

## 🔌 Connection

**Endpoint URL:** `ws://127.0.0.1:8765/api` _(The port is configurable in the BepInEx config)_

The API uses standard WebSockets. You can connect to it using Javascript in a browser, Python, C#, or any other language that supports WebSockets.

## 🛠️ Built-in Developer Reference

Before writing your code, we **highly recommend** opening the included Web Dashboard!

Simply double-click `web/index.html` in your browser while connected to a ride in-game. Navigate to the **"Developer API Reference"** tab. It acts as a live, self-documenting registry. It automatically discovers every button, switch, and status light on the specific ride you're controlling and gives you:

- The exact API `name` needed to interact with the component.
- The minimum / maximum constraints or available string values.
- **Ready-to-use JSON payloads** perfectly formatted for each individual interaction.
- A live event console to observe incoming traffic.

This makes developing for specific rides easy!

## 📥 Incoming Commands (Web → Game)

Send these JSON objects to the server to control the ride.

### 1. Set Button State

Presses or releases a physical button on the control panel.

```json
{
  "action": "setButton",
  "name": "Button_Start",
  "value": true
}
```

- `value` (boolean): `true` for pressed, `false` for released.

### 2. Set Switch State

Turns a rotary switch to a specific position.

```json
{
  "action": "setSwitch",
  "name": "Switch_Mode",
  "value": 2
}
```

- `value` (integer): The position to snap the switch to (starts at `0`).

### 3. Set Potentiometer

Turns a continuous dial (potentiometer).

```json
{
  "action": "setPotentiometer",
  "name": "Pot_Speed",
  "value": 0.5
}
```

- `value` (float): The actual value within the potentiometer's limits (e.g. between `0` and `270`).

### 4. Set Joystick

Moves a 2-Axis Joystick.

```json
{
  "action": "setJoystick",
  "name": "Joy_MainArm",
  "x": 0.0,
  "y": 30.0
}
```

- `x`, `y` (float): The axis positions, usually between `-30` and `30`.

### 5. Toggle Stop Button

Toggles a stop button. Each call alternates between pressed and released.

```json
{
  "action": "toggleStopButton",
  "name": "Button_Emergency"
}
```

### 6. Toggle Multy Toggle

Toggles a multi-position toggle switch. Each call flips the state between on and off.

```json
{
  "action": "toggleMultyToggle",
  "name": "LED_All_Off_Multy_Toggle"
}
```

### 7. Press Preset Button

Triggers a preset activation button on a screen.

```json
{
  "action": "pressPreset",
  "name": "UI_Presets_Button_1"
}
```

### 8. Set Dropdown

Changes the selected option of a dropdown menu.

```json
{
  "action": "setDropdown",
  "name": "LED_Color_Dropdown",
  "value": 1
}
```

- `value` (integer): The index of the selected option, starting at `0`.

### 9. Set Slider

Changes the value of a UI slider.

```json
{
  "action": "setSlider",
  "name": "UI_MovingHead_Program_Speed_Sllider",
  "value": 5.5
}
```

- `value` (float): The actual value within the slider's limits.

### 10. Request Full State

Forces the server to broadcast the complete state of the control panel again.

```json
{
  "action": "requestFullState"
}
```

## 📤 Outgoing Messages (Game → Web)

The server sends these JSON objects to all connected clients.

### 1. Full State (`fullState`)

Sent automatically when you connect, or when you take control/lose control of a ride. Contains the physical limits of all elements.

```json
{
  "type": "fullState",
  "active": true,
  "lights": [{ "name": "StatusLight_1", "color": "Green", "mode": "Strobe" }],
  "buttons": [{ "name": "Button_Start" }],
  "switches": [{ "name": "Switch_Mode", "maxState": 3 }],
  "potentiometers": [
    { "name": "Pot_Speed", "min": 0, "max": 270, "current": 110 }
  ],
  "joysticks": [
    {
      "name": "Joy_MainArm",
      "minX": 0.0,
      "maxX": 0.0,
      "minY": -30.0,
      "maxY": 30.0,
      "currentX": 0.0,
      "currentY": 0.0
    }
  ],
  "stopButtons": [{ "name": "Stop_Button_1", "isDown": false }],
  "multyToggles": [{ "name": "LED_All_Off_Multy_Toggle", "value": 0 }],
  "presetButtons": [{ "name": "UI_Presets_Button_1" }],
  "dropdowns": [
    {
      "name": "LED_Color_Dropdown",
      "options": ["0 - Automatic Slow", "1 - White"]
    }
  ],
  "sliders": [
    {
      "name": "UI_MovingHead_Program_Speed_Sllider",
      "min": 0.0,
      "max": 10.0,
      "current": 1.0
    }
  ]
}
```

### 2. Light Update (`lightUpdate`)

Sent whenever a light changes its color, turns on, turns off, or starts blinking.

```json
{
  "type": "lightUpdate",
  "name": "StatusLight_1",
  "color": "Green",
  "mode": "Off"
}
```

- `mode`: `On`, `Off`, `Strobe`, `Strobe_Fast` or `Strobe_Slow`.

### 3. Session Status (`session`)

Sent when the local player loses control of the ride panel (e.g. by stepping away).

```json
{
  "type": "session",
  "active": false
}
```

## 💬 Support & Feedback

If you have any questions, encounter bugs, or need help implementing the API, feel free to reach out.

`Discord`: @invalidluca
