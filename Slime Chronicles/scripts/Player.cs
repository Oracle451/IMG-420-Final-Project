// Handles Player movement and applying impulses
using Godot;

public partial class Player : CharacterBody2D
{
	// Set the players speed, jump velocity, and gravity
	[Export] public float Speed = 300f;
	[Export] public float JumpVelocity = -400f;
	[Export] public float Gravity = 980f;

	// Variables to set how the much the player stretches and squishes upon certain actions
	[ExportGroup("Slime Deformation")]
	[Export] public float MoveStretch = 0.4f;
	[Export] public float JumpStretch = 0.6f;
	[Export] public float LandSquash = 0.8f;
	[Export] public float IdleBreathing = 0.3f;

	private Node _distortion;
	private bool _wasOnFloor;

	public override void _Ready()
	{
		// Get the sprite and set the mesh to its original state
		_distortion = GetNode("DistortionSprite2D");
		_distortion?.Call("reset_mesh");
	}

	public override void _PhysicsProcess(double delta)
	{
		float d = (float)delta;
		Vector2 vel = Velocity;

		// Gravity
		if (!IsOnFloor())
		{
			vel.Y += Gravity * d;
		}

		// Jump
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
		{
			vel.Y = JumpVelocity;
			ApplyClampedImpulse(new Vector2(0, 38), new Vector2(0, JumpStretch));
		}

		// Horizontal movement
		float dir = Input.GetAxis("ui_left", "ui_right");
		if (Mathf.Abs(dir) > 0.01f)
		{
			vel.X = dir * Speed;
			ApplyClampedImpulse(new Vector2(dir * 32, 0), new Vector2(dir * MoveStretch, 0));
		}
		else
		{
			vel.X = Mathf.MoveToward(vel.X, 0, Speed * 4f * d);
		}

		// Landing squash
		bool onFloorNow = IsOnFloor();
		if (onFloorNow && !_wasOnFloor && vel.Y > 120f)
		{
			float rawImpact = vel.Y * 0.002f; // scales with fall speed
			ApplyClampedImpulse(new Vector2(0, 52), new Vector2(0, -rawImpact * LandSquash));
		}
		_wasOnFloor = onFloorNow;

		// Idle breathing
		float t = Time.GetTicksMsec() * 0.001f;
		Vector2 breathe = new Vector2(
			Mathf.Sin(t * 2.3f) * 0.7f,
			Mathf.Cos(t * 1.6f + 0.8f) * 0.5f
		);
		ApplyClampedImpulse(Vector2.Zero, breathe * IdleBreathing);

		// Tiny velocity wobble
		ApplyClampedImpulse(Vector2.Zero, vel * 0.0008f);

		Velocity = vel;
		MoveAndSlide();
	}

	// Function to clamp every impulse into a defined range to keep exaggerated distortion from occuring
	private void ApplyClampedImpulse(Vector2 point, Vector2 desiredForce)
	{
		// Scale the vector so its length is clamped between 0.05 and 0.5
		float length = desiredForce.Length();
		if (length < 0.001f) return; // zero → skip

		float clampedLength = Mathf.Clamp(length, 0.05f, 0.5f);
		Vector2 finalForce = desiredForce.Normalized() * clampedLength;

		_distortion?.Call("apply_impulse", point, finalForce);
	}
}
