// File: src/DistortionSprite2D.cpp

#include "DistortionSprite2D.h"
#include <godot_cpp/variant/utility_functions.hpp>
#include <godot_cpp/core/class_db.hpp>
#include <cmath>

using namespace godot;

void DistortionSprite2D::_bind_methods() {
	ClassDB::bind_method(D_METHOD("apply_impulse", "point", "force"), &DistortionSprite2D::apply_impulse);

	ClassDB::bind_method(D_METHOD("set_elasticity", "value"), &DistortionSprite2D::set_elasticity);
	ClassDB::bind_method(D_METHOD("get_elasticity"), &DistortionSprite2D::get_elasticity);
	ADD_PROPERTY(PropertyInfo(Variant::FLOAT, "elasticity"), "set_elasticity", "get_elasticity");

	ClassDB::bind_method(D_METHOD("set_damping", "value"), &DistortionSprite2D::set_damping);
	ClassDB::bind_method(D_METHOD("get_damping"), &DistortionSprite2D::get_damping);
	ADD_PROPERTY(PropertyInfo(Variant::FLOAT, "damping"), "set_damping", "get_damping");

	ClassDB::bind_method(D_METHOD("set_noise_strength", "value"), &DistortionSprite2D::set_noise_strength);
	ClassDB::bind_method(D_METHOD("get_noise_strength"), &DistortionSprite2D::get_noise_strength);
	ADD_PROPERTY(PropertyInfo(Variant::FLOAT, "noise_amplitude"), "set_noise_strength", "get_noise_strength");

	ClassDB::bind_method(D_METHOD("set_noise_speed", "value"), &DistortionSprite2D::set_noise_speed);
	ClassDB::bind_method(D_METHOD("get_noise_speed"), &DistortionSprite2D::get_noise_speed);
	ADD_PROPERTY(PropertyInfo(Variant::FLOAT, "noise_speed"), "set_noise_speed", "get_noise_speed");

	ClassDB::bind_method(D_METHOD("set_subdivision", "value"), &DistortionSprite2D::set_subdivision);
	ClassDB::bind_method(D_METHOD("get_subdivision"), &DistortionSprite2D::get_subdivision);
	ADD_PROPERTY(PropertyInfo(Variant::INT, "subdivision"), "set_subdivision", "get_subdivision");

	ClassDB::bind_method(D_METHOD("reset_mesh"), &DistortionSprite2D::reset_mesh);
}

DistortionSprite2D::DistortionSprite2D() {
}

DistortionSprite2D::~DistortionSprite2D() {}

void DistortionSprite2D::_ready() {
	if (rest_vertices.size() == 0 || needs_rebuild) {
		build_grid();
		needs_rebuild = false;
	}
}

void DistortionSprite2D::_process(double delta) {
	float dt = static_cast<float>(delta);
	integrate(dt);
	// NOTE: This class computes vertex positions and exposes them in `vertices`.
	// Uploading vertices to a Godot mesh (ArrayMesh, Mesh2D, etc.) is engine-specific
	// and can be implemented in `build_mesh_from_vertices()` which the user can add.
}

void DistortionSprite2D::apply_impulse(const Vector2 &point, const Vector2 &force) {
	// Apply force to nearby vertices (simple distance falloff)
	for (int i = 0; i < vertices.size(); ++i) {
		Vector2 pos = vertices[i];
		float dist = pos.distance_to(point);
		float r = Math::max(0.0001f, dist);
		float falloff = 1.0f / (1.0f + r * 0.5f);
		Vector2 add = force * falloff;
		velocities.set(i, velocities[i] + add);
	}
}

void DistortionSprite2D::set_elasticity(float p_elasticity) { elasticity = p_elasticity; }
float DistortionSprite2D::get_elasticity() const { return elasticity; }

void DistortionSprite2D::set_damping(float p_damping) { damping = p_damping; }
float DistortionSprite2D::get_damping() const { return damping; }

void DistortionSprite2D::set_noise_strength(float p_noise) { noise_amplitude = p_noise; }
float DistortionSprite2D::get_noise_strength() const { return noise_amplitude; }

void DistortionSprite2D::set_noise_speed(float p_speed) { noise_speed = p_speed; }
float DistortionSprite2D::get_noise_speed() const { return noise_speed; }

void DistortionSprite2D::set_subdivision(int p_subdiv) {
	subdivision = Math::clamp(p_subdiv, 1, 64);
	needs_rebuild = true;
}
int DistortionSprite2D::get_subdivision() const { return subdivision; }

void DistortionSprite2D::reset_mesh() {
	vertices = rest_vertices;
	for (int i = 0; i < velocities.size(); ++i) velocities.set(i, Vector2());
}

void DistortionSprite2D::build_grid() {
	// Build a simple rectangular grid of points inside [-extents, +extents]
	int cols = subdivision + 1;
	int rows = subdivision + 1;
	rest_vertices.resize(cols * rows);
	vertices.resize(cols * rows);
	velocities.resize(cols * rows);

	for (int y = 0; y < rows; ++y) {
		for (int x = 0; x < cols; ++x) {
			float fx = (float)x / (cols - 1);
			float fy = (float)y / (rows - 1);
			Vector2 p = Vector2(Math::lerp(-extents.x, extents.x, fx), Math::lerp(-extents.y, extents.y, fy));
			int idx = y * cols + x;
			rest_vertices.set(idx, p);
			vertices.set(idx, p);
			velocities.set(idx, Vector2());
		}
	}
}

void DistortionSprite2D::integrate(float dt) {
	// Simple semi-implicit Euler integration per-vertex with spring to rest position and noise
	static float time_accum = 0.0f;
	time_accum += dt;
	float t = time_accum;
	for (int i = 0; i < vertices.size(); ++i) {
		Vector2 rest = rest_vertices[i];
		Vector2 pos = vertices[i];
		Vector2 vel = velocities[i];

		// spring force
		Vector2 spring = (rest - pos) * elasticity;

		// noise force (small)
		Vector2 sample_pos = pos;
		float n = sample_noise(sample_pos, t * noise_speed);
		Vector2 noise_force = Vector2(n, n) * noise_amplitude;

		// integrate
		vel += (spring + noise_force) * dt;
		// damping
		vel *= (1.0f / (1.0f + damping * dt));
		pos += vel * dt;

		vertices.set(i, pos);
		velocities.set(i, vel);
	}
}

// A tiny, fast pseudo-noise function used for placeholder noise. Replace with Perlin/Simplex if desired.
float DistortionSprite2D::sample_noise(const Vector2 &pos, float t) const {
	// 3D-ish hash noise (position.x, position.y, time)
	// NOTE: This is a placeholder. For higher quality noise, integrate a proper Perlin or Simplex implementation.
	uint32_t h = 2166136261u;
	auto mix_u32 = [&](uint32_t v) {
		h ^= v;
		h *= 16777619u;
		return h;
	};

	uint32_t xi = (uint32_t) (pos.x * 73856093.0f);
	uint32_t yi = (uint32_t) (pos.y * 19349663.0f);
	uint32_t ti = (uint32_t) (t * 2654435761.0f);

	mix_u32(xi); mix_u32(yi); mix_u32(ti);
	float out = (float)(h & 0xFFFF) / (float)0xFFFF; // [0,1]
	return (out - 0.5f) * 2.0f; // [-1,1]
}