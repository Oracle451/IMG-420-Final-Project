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
		if (body is Enemy enemy)
		{
			// hide and remove enemy
			enemy.Visible = false;
			enemy.Die();
		}
		if (!body.IsInGroup("player"))
		{
			// remove bullet
			QueueFree();
		}
	}
}
