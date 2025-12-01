using Godot;
using System;

public partial class Bullet : Area2D
{
	[Export]
	// bullet speed in pixels/sec
	public float speed = 400f;  

	[Export]
	// set by player
	public Vector2 direction;    

	[Export]
	 // delete after a bit
	public float life = 2f;     

	private float timer = 0f;

	public override void _Process(double delta)
	{
		// just move in whatever direction the player set
		Position += direction * speed * (float)delta;

		// keep track of how long we've been alive
		timer += (float)delta;
		if (timer >= life)
		{
			QueueFree();
		}
	}
}
