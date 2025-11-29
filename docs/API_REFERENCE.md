# **DistortionSprite2D -- API Reference**

A custom **2D mesh-distortion sprite** implemented as a GDExtension for
Godot 4.4
It subdivides a texture into a deformable grid, applies spring physics,
optional noise distortion, and allows impulse-based impacts.

------------------------------------------------------------------------

## **Class: `DistortionSprite2D`**

### **Inherits:**

`Node2D`

### **Description**

`DistortionSprite2D` renders a textured grid whose vertices simulate
spring-like elasticity, damping, and optional noise distortion.
You can apply impulses to create shockwaves, impacts, and wobble
effects. Useful for slime characters, soft bodies, wavy surfaces, and
ambient distortion.

------------------------------------------------------------------------

# **Properties**

### `texture : Texture2D`

The texture drawn and used for UV mapping.

- **Setter:** `set_texture(texture)`
- **Getter:** `get_texture()`

------------------------------------------------------------------------

### `grid_size : Vector2i`

Number of columns * rows of vertices.
Higher values = smoother deformation but slower.

- Range: 3--64 for both axes
- **Setter:** `set_grid_size(value)`
- **Getter:** `get_grid_size()`

------------------------------------------------------------------------

### `elasticity : float`

Spring force that pulls vertices back toward rest position.

- Range: 0--100
- Higher = snappier return
- **Setter:** `set_elasticity(value)`
- **Getter:** `get_elasticity()`

------------------------------------------------------------------------

### `damping : float`

Friction applied to velocity each frame.

- Range: 0--50
- Higher = quicker settling
- **Setter:** `set_damping(value)`
- **Getter:** `get_damping()`

------------------------------------------------------------------------

### `noise_amplitude : float`

Strength of procedural Perlin noise applied to each vertex.

- Range: 0--30
- **Setter:** `set_noise_strength(value)`
- **Getter:** `get_noise_strength()`

------------------------------------------------------------------------

### `noise_speed : float`

Speed of the evolving noise field.

- Range: 0--10
- **Setter:** `set_noise_speed(value)`
- **Getter:** `get_noise_speed()`

------------------------------------------------------------------------

# **Methods**

### `apply_impulse(point: Vector2, force: Vector2) -> void`

### `apply_impulse(point: Vector2, force: Vector2)`

**Parameters:**

| Name    | Type     | Description                              |
|---------|----------|------------------------------------------|
| `point` | `Vector2`| World-space (or local-space if you use `to_local()`) location of the impact |
| `force` | `Vector2`| Direction and strength of the impulse    |

Uses a 120px radius and quadratic falloff.

------------------------------------------------------------------------

### `reset_mesh() -> void`

Fully resets all vertices and velocities.

------------------------------------------------------------------------

### Other getters/setters

Includes: - `set_texture` - `get_texture` - `set_grid_size` -
`get_grid_size` - `set_elasticity` - `get_elasticity` - `set_damping` -
`get_damping` - `set_noise_strength` - `get_noise_strength` -
`set_noise_speed` - `get_noise_speed`

------------------------------------------------------------------------

# **Lifecycle Methods**

### `_ready()`

Builds grid.

### `_process(delta)`

Updates physics and redraw.

### `_draw()`

Renders mesh.

------------------------------------------------------------------------

# **Usage Example**

``` gdscript
extends Node2D

@onready var dist = $DistortionSprite2D

func _ready():
    dist.texture = preload("res://slime.png")
    dist.elasticity = 20.0
    dist.damping = 6.0
    dist.noise_amplitude = 5.0

func _input(event):
    if event is InputEventMouseButton and event.pressed:
        dist.apply_impulse(event.position, Vector2(0, -150))
```

------------------------------------------------------------------------

# **Recommended Settings**

### Soft slime

-   Grid: 18×18\
-   Elasticity: 10--20\
-   Damping: 4--10\
-   Noise: 2--5

### Water surface

-   Grid: 32×32\
-   Elasticity: 3--8\
-   Noise: medium\
-   Damping: low

### Cartoon impacts

-   Elasticity: 25--40\
-   Damping: medium\
-   Noise: off
