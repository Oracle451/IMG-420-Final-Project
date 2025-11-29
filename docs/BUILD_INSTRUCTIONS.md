# DistortionSprite2D – Build Instructions
**Godot 4.4 – C++ GDExtension**

This extension is a standard Godot 4.4 C++ GDExtension built with **godot-cpp** and **SCons**.

### Prerequisites

| Tool                        | Minimum Version       | Download / Install                                                                 |
|-----------------------------|-----------------------|------------------------------------------------------------------------------------|
| Godot Engine                | **4.4** (stable)      | https://godotengine.org/download                                                  |
| C++ Compiler                | GCC 11+, Clang 14+, MSVC 2022 | Already included with most IDEs / build tools                                      |
| Python 3                    | 3.8+                  | https://www.python.org/downloads/                                                  |
| SCons                       | 4.0+                  | `pip install scons`                                                               |
| git                         | any                   | https://git-scm.com/                                                               |

### 1. Clone the Repository + godot-cpp

```bash
# Clone the extension
git clone https://github.com/Oracle451/IMG-420-Final-Project.git
cd DistortionSprite2D

# Clone the correct godot-cpp branch (must match your Godot version)
git clone --branch 4.4 https://github.com/godotengine/godot-cpp.git
```

### 2. Build godot-cpp (only once)

```bash
cd godot-cpp

# Windows
scons platform=windows target=template_debug -j8

# Linux
# scons platform=linux target=template_debug -j8

# macOS
# scons platform=macos target=template_debug -j8

cd ..
```

**Optional:** Repeat the exact same command but with `target=template_release` for an optimized release build of godot-cpp.

### 3. Build the Extension

From the project root (where the `SConstruct` file is located):

```bash
# Debug build (recommended for development)
scons platform=windows target=template_debug -j8

# Release build (smaller & faster – use for final builds/export)
scons platform=windows target=template_release -j8
```

**Supported platforms:** `windows`, `linux`, `macos`  
(Android & iOS are possible with additional setup – see Godot docs if needed.)

### 4. Output

Successful compilation produces files in the `bin/` folder


### 5. Use in Your Godot Project

1. In your Godot project, create the folder:  
   `addons/distortion_sprite_2d/`

2. Copy these files into that folder:
   - The compiled library (`.dll` / `.so` / `.dylib` – choose debug or release)
   - `distortion_sprite_2d.gdextension` (included in the repo)
   - (Optional) `icon.png` or any custom plugin icon

3. Open your project in Godot → **Project → Project Settings → Plugins**  
   → Find **DistortionSprite2D** → **Enable**

You can now drag **DistortionSprite2D nodes into any scene!

### Common Issues & Fixes

| Problem                                   | Solution                                                                                              |
|-------------------------------------------|-------------------------------------------------------------------------------------------------------|
| godot-cpp version mismatch                 | Ensure `godot-cpp` is checked out to the exact `4.4` branch (`git checkout 4.4`)                      |
| “Class not found” error in editor         | Rebuild with the same architecture & bitness as your Godot editor/export template (64-bit recommended)|
| DLL/.so/.dylib not found at runtime       | Place the compiled library next to the `.gdextension` file or in a `bin/` subfolder                   |
| SCons: “platform not found”               | Use only `platform=windows`, `linux`, or `macos` (do **not** use `win64`, `x11`, etc.)                |
| Linker errors on macOS/Linux              | Install build-essential (Linux) or Xcode Command Line Tools (macOS)                                   |

### Updating for Future Godot Versions (e.g. Godot 4.5+)

1. Inside the `godot-cpp` folder:

   ```bash
   git fetch && git checkout 4.5   # or the new stable branch/tag

2. Rebuild godot-cpp (repeat the godot-cpp build step)

3. Recompile the extension (repeat the extension build step)

**Happy building!**

**Created by**  
Cole Bishop, Tyler Jeffrey, John Zeledon  