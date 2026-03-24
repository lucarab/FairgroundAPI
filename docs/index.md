# 🎡 FairgroundAPI

Welcome to the **FairgroundAPI**! This is a BepInEx / IL2CPP mod for the game [**Fairground Online**](https://store.steampowered.com/app/3310530/Fairground_Online/) that exposes the control panels of in-game rides via a **WebSocket** and a beautiful **Web Dashboard**. It enables you to control rides from your desktop browser, a mobile tablet, or even a physical hardware controller!

<p align="center">
  <img src="https://i.imgur.com/cYcF9RW.png" width="48%" />
  <img src="https://i.imgur.com/g65umLl.png" width="48%" />
  <br>
  <img src="https://i.imgur.com/P0ePvgf.png" width="48%" />
  <img src="https://i.imgur.com/Mgz20AQ.png" width="48%" />
</p>

## ✨ Features

- **Zero-Latency Control:** Press buttons, flip switches, turn potentiometers, and move joysticks remotely.
- **Bi-directional Sync:** The dashboard accurately reflects the real-time blinking state and colors of all the physical ride lights.
- **Plug & Play Web Dashboard:** Comes with a fully responsive, dark-mode dashboard.

## 📦 Installation

_Note: You must have [BepInEx 6 Bleeding Edge (IL2CPP version)](https://builds.bepinex.dev/projects/bepinex_be) installed in your game directory. Read the [Installation Guide](https://docs.bepinex.dev/master/articles/user_guide/installation/unity_il2cpp.html) if you need help._

1. Go to the [Releases page](https://github.com/lucarab/FairgroundAPI/releases) and download the latest `.zip` file.
2. Extract **both** `FairgroundAPI.dll` and `websocket-sharp.dll` into your `BepInEx/plugins/` folder.
3. Keep the `web` folder anywhere on your PC (or host it locally).
4. Launch the game, walk up to a ride, take control, and then simply double-click `web/index.html` to open your control panel!

## 📖 API Documentation

Do you want to build your own custom dashboard, a Discord integration, or a physical hardware controller (like Arduino/ESP32)?

We have fully documented the entire WebSocket interface (JSON payloads in and out).

👉 **[Read the API Documentation here](https://lucarab.github.io/FairgroundAPI/API.html)**

---

## 💬 Support & Feedback

If you have any questions, encounter bugs, or need help implementing the API, feel free to reach out.

`Discord`: @invalidluca
