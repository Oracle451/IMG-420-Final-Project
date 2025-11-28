// include/DistortionSprite2D.h
#ifndef DISTORTIONSPRITE2D_H
#define DISTORTIONSPRITE2D_H

#include <godot_cpp/classes/node2d.hpp>
#include <godot_cpp/classes/texture2d.hpp>
#include <godot_cpp/core/class_db.hpp>
#include <godot_cpp/variant/vector2.hpp>
#include <godot_cpp/variant/vector2i.hpp>
#include <godot_cpp/variant/packed_vector2_array.hpp>

using namespace godot;

class DistortionSprite2D : public Node2D {
    GDCLASS(DistortionSprite2D, Node2D);

private:
    // Exposed parameters
    float elasticity = 20.0f;
    float damping = 8.0f;
    float noise_amplitude = 4.0f;
    float noise_speed = 1.0f;
    Vector2i grid_size = Vector2i(10, 10);  // Number of points (10x10 = 9x9 quads)

    Ref<Texture2D> texture;

    // Vertex data
    PackedVector2Array rest_vertices;
    PackedVector2Array vertices;
    PackedVector2Array velocities;
    PackedVector2Array uvs;

    Vector2 extents = Vector2(64, 64);
    bool needs_rebuild = true;

    // Perlin noise permutation table
    std::array<int, 512> perm;

    float fade(float t) const { return t * t * t * (t * (t * 6 - 15) + 10); }
    float grad(int hash, float x, float y, float z) const;
    float perlin(float x, float y, float z) const;

protected:
    static void _bind_methods();

public:
    DistortionSprite2D();
    ~DistortionSprite2D() override = default;

    void _ready() override;
    void _process(double delta) override;
    void _draw() override;

    // Public API
    void apply_impulse(const Vector2 &point, const Vector2 &force);

    void set_elasticity(float v) { elasticity = v; }
    float get_elasticity() const { return elasticity; }

    void set_damping(float v) { damping = v; }
    float get_damping() const { return damping; }

    void set_noise_strength(float v) { noise_amplitude = v; }
    float get_noise_strength() const { return noise_amplitude; }

    void set_noise_speed(float v) { noise_speed = v; }
    float get_noise_speed() const { return noise_speed; }

    void set_grid_size(const Vector2i &size);
    Vector2i get_grid_size() const { return grid_size; }

    void set_texture(const Ref<Texture2D> &tex);
    Ref<Texture2D> get_texture() const { return texture; }

    void reset_mesh();

private:
    void build_grid();
    void integrate(float dt);
    Vector2 sample_noise(const Vector2 &pos, float t) const;
};

#endif // DISTORTIONSPRITE2D_H