# DistortionSprite2D – User Guide
**Juicy deformable sprites for Godot 4.4**

A soft-body-style 2D sprite with real-time mesh distortion, perfect for:
- Jelly buttons & UI
- Water / liquid surfaces
- Flags & cloth
- Hit reactions
- Wobbly text/logos

### Quick Start

1. Install the plugin (see BUILD_INSTRUCTIONS.md or download a prebuilt release)
2. Enable the plugin: **Project → Project Settings → Plugins → DistortionSprite2D → Enable**
3. Add a **DistortionSprite2D** node to your scene
4. Assign any `Texture2D` (PNG, sprite, icon, etc.)

Done – it already wobbles!

### Inspector Properties

| Property           | Recommended Range | What It Does                                      |
|--------------------|-------------------|---------------------------------------------------|
| **Texture**        | —                 | Your image/sprite                                 |
| **Grid Size**      | 10×10 → 24×24 (mobile)<br>32×32+ (desktop) | Higher = smoother deformation, more CPU cost      |
| **Elasticity**     | 10 – 40           | How fast it snaps back (higher = bouncier)       |
| **Damping**        | 5 – 20            | How quickly motion stops (higher = less wobble)   |
| **Noise Amplitude**| 0 – 15            | Constant gentle turbulence (0 = off)              |
| **Noise Speed**    | 0.2 – 3.0         | Speed of the breathing/turbulence animation       |

### Scripting – Applying Impulses

```gdscript
@onready var distortion = $DistortionSprite2D

func _input(event):
    if event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT:
        # Convert screen click to local node coordinates
        var local_pos = distortion.to_local(event.position)
        distortion.apply_impulse(local_pos, Vector2(0, -12))
```

### Common impulse examples:

```gdscript
# Upward hit (like a jump or pop)
distortion.apply_impulse(local_pos, Vector2(0, -15))

# Radial explosion
distortion.apply_impulse(local_pos, (local_pos - distortion.get_rect().get_center()).normalized() * -20)

# Horizontal shake
distortion.apply_impulse(local_pos, Vector2(randf_range(-10,10), 0))
```

### Useful Methods

| Method                    | Use Case                                   |
|---------------------------|--------------------------------------------|
| `apply_impulse(pos, force)` | Click/hit reactions, explosions, pokes     |
| `reset_mesh()`            | Instantly return to perfect flat/rest state |
| `queue_redraw()`          | Force an immediate redraw (rarely needed)  |

### Performance Tips

| Grid Size | Approx. Vertices | Good For                              |
|-----------|------------------|---------------------------------------|
| 10×10     | 100              | Mobile UI, many instances             |
| 16×16     | 256              | Balanced quality/performance (recommended) |
| 24×24     | 576              | High-quality desktop effects          |
| 40×40+    | 1600+           | Only for very few nodes / hero effects   |

**Tip:** Set `noise_amplitude = 0` when you don’t need constant wobble/turbulence – it saves a small amount of CPU.

### Ideas & Extensions

- Animate `elasticity` or `damping` with a `Tween` for slow-motion or "super jelly" effects
- Fire multiple rapid `apply_impulse()` calls to create wave/ripple propagation
- Use the node as a `ViewportTexture` source for distorted reflections or liquid reflections
- Combine with a `ShaderMaterial` on a duplicate sprite for glow, outline, or refraction effects
- Parent a `GPUParticles2D` or `CPUParticles2D` to the mesh and trigger on impulse for juicy impacts
- Modulate `noise_speed` over time for breathing/pulsing UI elements

**Enjoy the wobble!**

**Created by**  
Cole Bishop, Tyler Jeffrey, John Zeledon  

**IMG-420 – 2D Game Engines – December 2025**
