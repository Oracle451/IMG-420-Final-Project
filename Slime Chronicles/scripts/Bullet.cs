using Godot;
using System;

public partial class Bullet : Area2D
{
	[Export] public float speed = 800f;
	[Export] public Vector2 direction;
	[Export] public float life = 2f;

	private float timer = 0f;

	public override void _Ready()
	{
		// Connect the Area2D body_entered signal
		BodyEntered += OnBodyEntered;
	}

	public override void _Process(double delta)
	{
		Position += direction * speed * (float)delta;

		timer += (float)delta;
		if (timer >= life)
			QueueFree();
	}

	private void OnBodyEntered(Node body)
	{
		// If enemy detected → kill enemy
		if (body.IsInGroup("enemy"))
		{
			body.QueueFree();   // remove the enemy
		}
		
		if (!body.IsInGroup("player"))
		{
			QueueFree();        // remove bullet after impact
		}
	}
}
