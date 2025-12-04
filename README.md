# Slime Chronicles

## Team Members:

- **Cole Bishop** – Systems Developer\
Core C++ GDExtension implementation, gameplay mechanics outlining, performance optimization.

- **Tyler Jeffrey** – Gameplay Programmer\
Integration of procedural animation system into gameplay, collision response, slime behavior.

- **John Zeledon** – Designer / Technical Artist\
Parameter tuning, visual style development, test scenes, UI setup.

## Module Functionality Description

The Procedural Animation & Distortion GDExtension introduces a new node type: **DistortionSprite2D**.

This node enables real-time mesh deformation in Godot 2D, simulating soft-body physics and noise-driven motion. Unlike shaders or pre-baked animations, this extension provides physics-aware deformation directly accessible from scripts and the Godot editor.

Key capabilities:
- Physics-based spring lattice for jelly-like deformation.
- Noise field oscillation for idle wobble and rippling.
- Collision impulses for reactive squash and stretch effects.
- Full Inspector integration with parameters like elasticity, damping, noise amplitude, and subdivision.
- C# API access for programmatic control of deformation states.

## Features List

- **Procedural deformation** – Real-time vertex updates using spring-damper physics.
- **Noise distortion** – Perlin/Simplex noise for natural idle wobble and ripple effects.
- **Impulse response** – Collision impacts propagate vertex displacements dynamically.
- **Custom node type** – DistortionSprite2D extends Node2D for easy integration.
- **Editor parameters** – Elasticity, damping, noise amplitude/speed, subdivision, texture.
- **C# API methods** – Trigger impulses, set deformation states, freeze motion.
- **TileMap integration** – Seamless per-tile deformation without visible seams.

## Controls and Gameplay Instructions

- **Arrow Keys** – Move left/right.
- **Space** – Jump.
- **E** – Dash.
- **Q/R** – Scale slime bigger/smaller.
- **Left Mouse Button** – Shoot slime ball.

Gameplay highlights:
- Slime reacts to collisions with squash/stretch.
- Idle wobble driven by noise-based deformation.
- Enemies, UI, and environment share the same distortion system for consistency.

## Credits and Acknowledgments

**Team:** Cole Bishop, Tyler Jeffrey, John Zeledon

**Inspiration:**
- Slime Rancher (Monomi Park)
- Gish (Cryptic Sea)
- Celeste (Maddy Makes Games)

**Documentation & Tutorials**
- [Godot 4 GDExtension Docs](https://docs.godotengine.org)
- [Godot CPP Bindings GitHub](https://github.com/godotengine/godot-cpp)

## [Link to Demo Video](https://youtu.be/VdhjqB3qmBI)
