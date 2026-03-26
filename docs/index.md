# 🎡 FairgroundAPI

Welcome to the **FairgroundAPI**! This is a BepInEx / IL2CPP mod for the game [**Fairground Online**](https://store.steampowered.com/app/3310530/Fairground_Online/) that exposes the control panels of in-game rides via a **WebSocket** and a beautiful **Web Dashboard**. It enables you to control rides from your desktop browser, a mobile tablet, or even a physical hardware controller!

<p align="center">
  <img src="https://i.imgur.com/gnfmqcJ.png" width="48%" />
  <img src="https://i.imgur.com/IOmSEs8.png" width="48%" />
  <br>
  <img src="https://i.imgur.com/KVhHBMH.png" width="48%" />
  <img src="https://i.imgur.com/aJ14lt5.png" width="48%" />
  <br>
  <img src="https://i.imgur.com/nlHMwEZ.png" width="48%" />
  <img src="https://i.imgur.com/pJ3jWVd.png" width="48%" />
</p>

## ✨ Features

- **Zero-Latency Control:** Press buttons, flip switches, turn potentiometers, move joysticks, toggle stop buttons, and switch multy toggles remotely.
- **Bi-directional Sync:** The dashboard accurately reflects the real-time blinking state and colors of all the physical ride lights.
- **Plug & Play Web Dashboard:** Comes with a fully responsive, dark-mode dashboard.
- **Built-in Developer API Reference:** The web dashboard features a live, self-documenting API registry that displays exact JSON payloads and API names for every component on the current ride.

## 📦 Installation

_Note: You must have [BepInEx 6 Bleeding Edge (IL2CPP version)](https://builds.bepinex.dev/projects/bepinex_be) installed in your game directory. Read the [Installation Guide](https://docs.bepinex.dev/master/articles/user_guide/installation/unity_il2cpp.html) if you need help._

1. Go to the [Releases page](https://github.com/lucarab/FairgroundAPI/releases) and download the latest `.zip` file.
2. Extract **both** `FairgroundAPI.dll` and `websocket-sharp.dll` into your `BepInEx/plugins/` folder.
3. Keep the `web` folder anywhere on your PC (or host it locally).
4. Launch the game, walk up to a ride, take control, and then simply double-click `web/index.html` to open your control panel!

## ⚙️ Configuration

After running the game with the plugin installed for the first time, an auto-generated configuration file will appear at `BepInEx/config/com.invalidluca.fairground.api.cfg`.
You can edit this file to change:

- **Listen IP**: Change the bind IP (default: `0.0.0.0` to allow local-network devices like tablets to connect). Set this to `127.0.0.1` if you want to strictly restrict connections to your local PC.
- **WebSocket Port**: Change the listening port (default: `8765`). If you change this, make sure to update the URL in your web dashboard.
- **Poll Rate**: Adjust how frequently the API checks for ride state changes, like blinking lights (default: `0.5` seconds). Lower values mean faster light updates but consume more CPU usage.

## 📖 API Documentation

Do you want to build your own custom dashboard, a Discord integration, or a physical hardware controller (like Arduino/ESP32)?

We have fully documented the entire WebSocket interface (JSON payloads in and out).  
**Tip:** The included Web Dashboard (`web/index.html`) features a live **Developer API Reference** tab that automatically lists every single component on the ride you are currently controlling, complete with click-to-copy API names and JSON examples!

👉 **[Read the API Documentation here](https://lucarab.github.io/FairgroundAPI/API.html)**

## 🤝 Contributing & Issues

Found a bug or have a feature request? Please [open an issue](https://github.com/lucarab/FairgroundAPI/issues) on GitHub.
Pull requests are also highly welcome! If you want to contribute to the project, feel free to fork the repository, make your changes, and submit a PR.

## 💬 Support & Feedback

If you have any questions, encounter bugs, or need help implementing the API, feel free to reach out.

`Discord`: @invalidluca
