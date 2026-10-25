# DataCenter-MoreModules
A mod for the game Data Center that adds bulk boxes of 32 and 64 network modules for every module type to the computer shop.

---
## ✨ Features
- Boxes of 32 and 64 modules for each module type, sold in the **Mods** section of the computer shop
- The price follows the game: a bulk box costs the same per module as the regular box of 5 (the price is read from the shop, not hard-coded)
- Bulk boxes are locked until the regular box of that type is unlocked, and unlock together with it
- Clicking a locked bulk box unlocks the module type for the usual XP cost, exactly like the regular box
- Bulk boxes are saved and restored with the save game, including the modules still inside
- A counter shows how many modules are left in a box (`N / Max modules`):
  - as a label under the crosshair while looking at a box or at a module inside it
  - as a line on the HUD while holding a box
---
## 🛠 Requirements
* **Data Center** (Steam version)
* **MelonLoader (Il2Cpp)** - Version 0.6.0 or higher.
---

## 🚀 Installation
1. **Install MelonLoader:**
   - Download the [MelonLoader Installer](https://github.com/LavaGang/MelonLoader/releases).
   - Run the installer and select your `Data Center.exe`.
   - Ensure the version is set to **latest** and the game type is **Il2Cpp**.
   - Click **Install**.

2. **Run the Game Once:**
   - Start the game once to allow MelonLoader to initialize the Il2Cpp assemblies.
   - Close the game once you reach the main menu.

3. **Install the Mod:**
   - Download the `DataCenter-MoreModules.{Version}.zip`.
   - Extract the zip file.
   - Place `DataCenter-MoreModules.dll` into the `Mods` folder in your game directory.

4. **Verify Installation:**
   - Launch the game. The MelonLoader console should show `More Modules Mod` has loaded successfully.
---

## 🎮 How to Use
1. Open the **Computer Shop** and scroll to the **Mods** section.
2. Add a 32x or 64x box to the cart and check out like any other item. Bulk boxes get their own cart line, separate from regular boxes of the same type.
3. The box is delivered full. Use it like a regular box: hold it and click a port to take a module, or hold a module and click the box to put it back.
4. Look at a box, or hold it, to see how many modules are inside.

## 🧱 Building from source
1. Copy your game paths into `LocalConfig.props` one folder above the project (the file is not part of the repository). It must define `DataCenterGameDir` and reference the MelonLoader and Il2Cpp assemblies of your installation.
2. Build `DataCenter-MoreModules.sln` with the .NET SDK (target `net6.0`).
3. Copy the resulting `DataCenter-MoreModules.dll` into the game's `Mods` folder.

## ⚠️ Notes
- Multiplayer (coop): every player with the mod announces it in the Steam lobby. Bulk boxes are only offered while everybody in the lobby has the mod; otherwise the bulk cards are hidden and bulk items are removed from the cart at checkout. This has not been tested with two players yet.
- Checkout XP is counted per cart unit, so a bulk box gives the same XP as a regular box.

## 📜 Credits
- Powered by **MelonLoader** and **HarmonyLib**.
