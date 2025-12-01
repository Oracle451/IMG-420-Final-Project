using Godot;
using System;

public partial class PlayerShoot : Node2D
{
    [Export]
    // drag Bullet.tscn here
    public PackedScene bulletPrefab;    

    [Export]
        // the "muzzle" 
    public Node2D firePoint;        

    [Export]
    // same as Unity
    public float fireRate = 0.25f;      

    private float nextShootTime = 0f;

    public override void _Process(double delta)
    {
        if (nextShootTime > 0)
            nextShootTime -= (float)delta;

        // left click shoots (using input map action)
        if (Input.IsActionPressed("shoot"))
        {
            if (nextShootTime <= 0f)
            {
                Shoot();
                nextShootTime = fireRate;
            }
        }
    }

    private void Shoot()
    {
        // spawn bullet from the PackedScene
        Bullet b = bulletPrefab.Instantiate() as Bullet;
        if (b == null)
        {
            GD.Print("Bullet prefab wasn't set correctly.");
            return;
        }

        // set bullet position
        b.GlobalPosition = firePoint.GlobalPosition;

        // get mouse pos in world space
        Vector2 mousePos = GetGlobalMousePosition();

        // direction from fire point to mouse
        b.direction = (mousePos - firePoint.GlobalPosition).Normalized();

        // add to the current scene tree
        GetTree().CurrentScene.AddChild(b);
    }
}
