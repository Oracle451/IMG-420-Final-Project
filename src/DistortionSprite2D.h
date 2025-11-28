// File: include/DistortionSprite2D.h
#ifndef DISTORTIONSPRITE2D_H
#define DISTORTIONSPRITE2D_H


#pragma once

#include <godot_cpp/classes/node2d.hpp>
#include <godot_cpp/core/class_db.hpp>
#include <godot_cpp/variant/vector2.hpp>
#include <godot_cpp/variant/packed_vector2_array.hpp>

#include <vector>

using namespace godot;

class DistortionSprite2D : public Node2D {
	GDCLASS(DistortionSprite2D, Node2D);

private:
	// Editable parameters (exposed to Inspector)
	float elasticity = 20.0f;
	float damping = 8.0f;
	float noise_amplitude = 4.0f;
	float noise_speed = 1.0f;
	int subdivision = 8;

	// Internal vertex data (rest positions, current positions, velocities)
	PackedVector2Array rest_vertices;
	PackedVector2Array vertices;
	PackedVector2Array velocities;

	// Bounding rectangle used to build mesh/vertex positions
	Vector2 extents = Vector2(64, 64);

	// Dirty flag: when vertex count or parameters change we need to rebuild
	bool needs_rebuild = true;

protected:
	static void _bind_methods();

public:
	DistortionSprite2D();
	~DistortionSprite2D();

	// Godot callbacks
	void _ready() override;
	void _process(double delta) override;
	void _draw() override;


	// API
	void apply_impulse(const Vector2 &point, const Vector2 &force);
	void set_elasticity(float p_elasticity);
	float get_elasticity() const;
	void set_damping(float p_damping);
	float get_damping() const;
	void set_noise_strength(float p_noise);
	float get_noise_strength() const;
	void set_noise_speed(float p_speed);
	float get_noise_speed() const;
	void set_subdivision(int p_subdiv);
	int get_subdivision() const;
	void reset_mesh();

	// Internal helpers
	void build_grid();
	void integrate(float dt);
	float sample_noise(const Vector2 &pos, float t) const;
};

#endif