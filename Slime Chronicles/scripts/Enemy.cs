using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
	[Export] public float Speed = 100.0f;
	[Export] public float Gravity = 980.0f;

	private float _direction = 1.0f;

	private Sprite2D _sprite;

	public override void _Ready()
	{
		_sprite = GetNode<Sprite2D>("Sprite2D");
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		// 1. Apply Gravity
		if (!IsOnFloor())
		{
			velocity.Y += Gravity * (float)delta;
		}

		// 2. Check for Wall Collision
		// IsOnWall() returns true if MoveAndSlide() hit a wall in the previous frame
		if (IsOnWall())
		{
			// Flip direction
			_direction *= -1;
			
			// Flip the visual sprite
			_sprite.FlipH = (_direction < 0);
		}

		// 3. Apply Horizontal Movement
		velocity.X = Speed * _direction;

		// 4. Move the enemy
		Velocity = velocity;
		MoveAndSlide();
	}
}
