// src/DistortionSprite2D.cpp
#include "DistortionSprite2D.h"
#include <godot_cpp/core/class_db.hpp>
#include <godot_cpp/core/math.hpp>

using namespace godot;

// -------------------------
// BINDING & PROPERTY SETUP
// -------------------------

void DistortionSprite2D::_bind_methods()
{
    // Bind ALL custom methods
    ClassDB::bind_method(D_METHOD("apply_impulse", "point", "force"), &DistortionSprite2D::apply_impulse);
    ClassDB::bind_method(D_METHOD("reset_mesh"), &DistortionSprite2D::reset_mesh);
    ClassDB::bind_method(D_METHOD("set_texture", "texture"), &DistortionSprite2D::set_texture);
    ClassDB::bind_method(D_METHOD("get_texture"), &DistortionSprite2D::get_texture);
    ClassDB::bind_method(D_METHOD("set_grid_size", "value"), &DistortionSprite2D::set_grid_size);
    ClassDB::bind_method(D_METHOD("get_grid_size"), &DistortionSprite2D::get_grid_size);
    ClassDB::bind_method(D_METHOD("set_elasticity", "value"), &DistortionSprite2D::set_elasticity);
    ClassDB::bind_method(D_METHOD("get_elasticity"), &DistortionSprite2D::get_elasticity);
    ClassDB::bind_method(D_METHOD("set_damping", "value"), &DistortionSprite2D::set_damping);
    ClassDB::bind_method(D_METHOD("get_damping"), &DistortionSprite2D::get_damping);
    ClassDB::bind_method(D_METHOD("set_noise_strength", "value"), &DistortionSprite2D::set_noise_strength);
    ClassDB::bind_method(D_METHOD("get_noise_strength"), &DistortionSprite2D::get_noise_strength);
    ClassDB::bind_method(D_METHOD("set_noise_speed", "value"), &DistortionSprite2D::set_noise_speed);
    ClassDB::bind_method(D_METHOD("get_noise_speed"), &DistortionSprite2D::get_noise_speed);

    // Expose all editable properties to the godot inspector
    ADD_PROPERTY(PropertyInfo(Variant::FLOAT, "elasticity", PROPERTY_HINT_RANGE, "0,100,0.1"), "set_elasticity", "get_elasticity");
    ADD_PROPERTY(PropertyInfo(Variant::FLOAT, "damping", PROPERTY_HINT_RANGE, "0,50,0.1"), "set_damping", "get_damping");
    ADD_PROPERTY(PropertyInfo(Variant::FLOAT, "noise_amplitude", PROPERTY_HINT_RANGE, "0,30,0.1"), "set_noise_strength", "get_noise_strength");
    ADD_PROPERTY(PropertyInfo(Variant::FLOAT, "noise_speed", PROPERTY_HINT_RANGE, "0,10,0.01"), "set_noise_speed", "get_noise_speed");
    ADD_PROPERTY(PropertyInfo(Variant::VECTOR2I, "grid_size", PROPERTY_HINT_RANGE, "3,64,1|3,64,1"), "set_grid_size", "get_grid_size");
    ADD_PROPERTY(PropertyInfo(Variant::OBJECT, "texture", PROPERTY_HINT_RESOURCE_TYPE, "Texture2D"), "set_texture", "get_texture");
}

// -------------------------
// CONSTRUCTOR
// -------------------------

DistortionSprite2D::DistortionSprite2D()
{
    // Classic Perlin permutation table (256 values, duplicated to 512)
    // This generates repeatable semi random gradients for noise
    static const int permutation[256] = {
        151,160,137,91,90,15,131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
        190,6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,88,237,149,56,87,174,20,
        125,136,171,168,68,175,74,165,71,134,139,48,27,166,77,146,158,231,83,111,229,122,60,211,133,230,220,
        105,92,41,55,46,245,40,244,102,143,54,65,25,63,161,1,216,80,73,209,76,132,187,208,89,18,169,200,196,
        135,130,116,188,159,86,164,100,109,198,173,186,3,64,52,217,226,250,124,123,5,202,38,147,118,126,255,
        82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,223,183,170,213,119,248,152,2,44,154,163,70,221,
        153,101,155,167,43,172,9,129,22,39,253,19,98,108,110,79,113,224,232,178,185,112,104,218,246,97,228,
        251,34,242,193,238,210,144,12,191,179,162,241,81,51,145,235,249,14,239,107,49,192,214,31,181,199,
        106,157,184,84,204,176,115,121,50,45,127,4,150,254,138,236,205,93,222,114,67,29,24,72,243,141,128,
        195,78,66,215,61,156,180
    };

    // Duplicate into 512 entries for fast lookup
    for (int i = 0; i < 256; i++)
    {
        perm[i] = perm[i + 256] = permutation[i];
    }
}

// -------------------------
// PROPERTY SETTERS
// -------------------------

void DistortionSprite2D::set_grid_size(const Vector2i &size)
{
    // Clamp grid resolution and mark mesh to be rebuilt
    grid_size.x = CLAMP(size.x, 3, 64);
    grid_size.y = CLAMP(size.y, 3, 64);
    needs_rebuild = true;
}

void DistortionSprite2D::set_texture(const Ref<Texture2D> &tex)
{
    texture = tex;
    // Updates extends so points are generated correctly
    if (texture.is_valid())
    {
        extents = texture->get_size() / 2.0f;
    }
    needs_rebuild = true;
}

// -------------------------
// NODE LIFECYCLE
// -------------------------

void DistortionSprite2D::_ready()
{
    // Rebuilds the mesh if the texture changed or the mesh is empty
    if (needs_rebuild || rest_vertices.size() == 0)
    {
        build_grid();
        needs_rebuild = false;
    }
}

void DistortionSprite2D::_process(double delta)
{
    // Run physics simulation on the grid
    integrate(static_cast<float>(delta));
    // Trigger Godot to redraw the deformed mesh
    queue_redraw();
}

// -------------------------
// DISTORTION INTERACTIONS
// -------------------------

void DistortionSprite2D::apply_impulse(const Vector2 &point, const Vector2 &force)
{
    // Apply a splash of velocity to nearby grid points
    const float radius = 120.0f;
    
    for (int i = 0; i < vertices.size(); i++)
    {
        float dist = vertices[i].distance_to(point);
        if (dist < radius)
        {
            // Smooth the radial falloff
            float falloff = 1.0f - (dist / radius);
            falloff = falloff * falloff;
            // Add force scaled by falloff
            velocities[i] = velocities[i] + force * falloff * 40.0f;
        }
    }
}

void DistortionSprite2D::reset_mesh()
{
    // Reset vertecies to rest position and zero out velocities
    vertices = rest_vertices;
    for (int i = 0; i < velocities.size(); i++)
    {
        velocities[i] = Vector2(0, 0);
    }
}

// -------------------------
// GRID CREATION
// -------------------------

void DistortionSprite2D::build_grid()
{
    // Recompute texture extents (half width and height)
    if (texture.is_valid())
    {
        extents = texture->get_size() / 2.0f;
    }

    int cols = grid_size.x;
    int rows = grid_size.y;
    int count = cols * rows;

    // Resize arrays to correct size
    rest_vertices.resize(count);
    vertices.resize(count);
    velocities.resize(count);
    uvs.resize(count);

    // Direct write access
    Vector2 *rest_w = rest_vertices.ptrw();
    Vector2 *pos_w  = vertices.ptrw();
    Vector2 *vel_w  = velocities.ptrw();
    Vector2 *uv_w   = uvs.ptrw();

    // Fill mesh grid
    for (int y = 0; y < rows; y++)
    {
        for (int x = 0; x < cols; x++)
        {
            // Normalized UV
            float u = x / float(cols - 1);
            float v = y / float(rows - 1);

            // Map into actual 2D sprite coordinates
            Vector2 p = Vector2(Math::lerp(-extents.x, extents.x, u), Math::lerp(-extents.y, extents.y, v));

            // Store data
            int i = y * cols + x;
            rest_w[i] = p;
            pos_w[i]  = p;
            vel_w[i]  = Vector2(0, 0);
            uv_w[i]   = Vector2(u, v);
        }
    }
}

// -------------------------
// PHYSICS SIMULATION
// -------------------------

void DistortionSprite2D::integrate(float dt)
{
    static float time = 0.0f;
    time += dt;

    const Vector2 *rest = rest_vertices.ptr();
    Vector2 *pos = vertices.ptrw();
    Vector2 *vel = velocities.ptrw();

    for (int i = 0; i < vertices.size(); i++)
    {
        Vector2 spring = (rest[i] - pos[i]) * elasticity;
        Vector2 noise_force = sample_noise(pos[i], time) * noise_amplitude;

        vel[i] += (spring + noise_force) * dt;
        vel[i] *= 1.0f / (1.0f + damping * dt);
        pos[i] += vel[i] * dt;
    }
}

// -------------------------
// PERLIN NOISE
// -------------------------

Vector2 DistortionSprite2D::sample_noise(const Vector2 &pos, float t) const {
    // Small position scaling = smoother noise field
    float scale = 0.04f;

    // Offset channels by 100 so that the noise isnt identical
    float nx = (perlin(pos.x * scale,       pos.y * scale,       t * noise_speed) * 2.0f - 1.0f);
    float ny = (perlin(pos.x * scale + 100, pos.y * scale + 100, t * noise_speed) * 2.0f - 1.0f);
    
    return Vector2(nx, ny);
}

// Gradient vector lookup (perlin noise)
float DistortionSprite2D::grad(int hash, float x, float y, float z) const {
    int h = hash & 15;
    float u = h < 8 ? x : y;
    float v = h < 4 ? y : (h == 12 || h == 14 ? x : z);
    return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
}

float DistortionSprite2D::perlin(float x, float y, float z) const {
    // Identify Cube Cell
    int X = (int)Math::floor(x) & 255;
    int Y = (int)Math::floor(y) & 255;
    int Z = (int)Math::floor(z) & 255;

    // Distance inside cube
    x -= Math::floor(x);
    y -= Math::floor(y);
    z -= Math::floor(z);

    // Fade curves for interpolation
    float u = fade(x);
    float v = fade(y);
    float w = fade(z);

    // Hash corner coordinates
    int A  = perm[X]     + Y; int AA = perm[A] + Z; int AB = perm[A+1] + Z;
    int B  = perm[X+1] + Y; int BA = perm[B] + Z; int BB = perm[B+1] + Z;

    float res = Math::lerp(w,
        Math::lerp(v,
            Math::lerp(u, grad(perm[AA],   x,   y,   z),
                          grad(perm[BA],   x-1, y,   z)),
            Math::lerp(u, grad(perm[AB],   x,   y-1, z),
                          grad(perm[BB],   x-1, y-1, z))),
        Math::lerp(v,
            Math::lerp(u, grad(perm[AA+1], x,   y,   z-1),
                          grad(perm[BA+1], x-1, y,   z-1)),
            Math::lerp(u, grad(perm[AB+1], x,   y-1, z-1),
                          grad(perm[BB+1], x-1, y-1, z-1))));

    return res;
}

// -------------------------
// DRAWING THE DEFORMED MESH
// -------------------------

void DistortionSprite2D::_draw()
{
    if (texture.is_null()) return;

    int cols = grid_size.x;
    int rows = grid_size.y;

    // Temporary buffers for a single triangle
    PackedVector2Array points;
    PackedVector2Array uvs_arr;
    PackedColorArray colors;

    points.resize(3);
    uvs_arr.resize(3);
    colors.resize(3);
    colors.fill(Color(1,1,1,1));

    const Vector2 *vtx = vertices.ptr();
    const Vector2 *uv   = uvs.ptr();

    // Loop through grid and draw each quad as 2 triangles
    for (int y = 0; y < rows - 1; y++) {
        for (int x = 0; x < cols - 1; x++) {
            int i00 = y * cols + x;
            int i10 = y * cols + x + 1;
            int i01 = (y + 1) * cols + x;
            int i11 = (y + 1) * cols + x + 1;

            // Triangle 1
            points.set(0, vtx[i00]); uvs_arr.set(0, uv[i00]);
            points.set(1, vtx[i10]); uvs_arr.set(1, uv[i10]);
            points.set(2, vtx[i11]); uvs_arr.set(2, uv[i11]);
            draw_primitive(points, colors, uvs_arr, texture);

            // Triangle 2
            points.set(1, vtx[i11]); uvs_arr.set(1, uv[i11]);
            points.set(2, vtx[i01]); uvs_arr.set(2, uv[i01]);
            draw_primitive(points, colors, uvs_arr, texture);
        }
    }
}