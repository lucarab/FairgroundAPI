# Fairground Control Panel API Documentation

Welcome to the documentation for the Fairground Control Panel API. This API allows you to remotely control and monitor rides in the game via a local WebSocket connection.

---

## 🔌 Connection

**Endpoint URL:** `ws://127.0.0.1:8765/api`

The API uses standard WebSockets. You can connect to it using Javascript in a browser, Python, C#, or any other language that supports WebSockets.

---

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

### 5. Request Full State

Forces the server to broadcast the complete state of the control panel again.

```json
{
  "action": "requestFullState"
}
```

---

## 📤 Outgoing Messages (Game → Web)

The server sends these JSON objects to all connected clients.

### 1. Full State (`fullState`)

Sent automatically when you connect, or when you take control/lose control of a ride. Contains the physical limits of all elements.

```json
{
  "type": "fullState",
  "active": true,
  "lights": [
    { "id": 1234, "name": "StatusLight_1", "color": "Green", "mode": "Blink" }
  ],
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
  ]
}
```

### 2. Light Update (`lightUpdate`)

Sent whenever a light changes its color, turns on, turns off, or starts blinking.

```json
{
  "type": "lightUpdate",
  "id": 1234,
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

---

## 💬 Support & Feedback

If you have any questions, encounter bugs, or need help implementing the API, feel free to reach out.

`Discord`: @invalidluca
