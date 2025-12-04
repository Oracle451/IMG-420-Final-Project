using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
	[Export] public float Speed = 100.0f;
	[Export] public float Gravity = 980.0f;
	
	[ExportGroup("Tar Deformation")]
	[Export] public float MoveStretch = 0.25f;
	[Export] public float WallImpactSquash = 0.6f;
	[Export] public float IdleWobble = 0.15f;

	private float _direction = 1.0f;
	private AudioStreamPlayer _popSound;
	private Node _distortion;
	private bool _isDead = false;

	public override void _Ready()
	{
		_popSound = GetNode<AudioStreamPlayer>("PopSound");
		_distortion = GetNode("DistortionSprite2D");

		// reset mesh on spawn
		_distortion?.Call("reset_mesh");
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_isDead) return;
		
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
			
			// Apply distortion on impact
			ApplyClampedImpulse(
				new Vector2(_direction * 24, 0),     // strong push
				new Vector2(-_direction * MoveStretch, -WallImpactSquash)
			);
		}

		// 3. Apply Horizontal Movement
		velocity.X = Speed * _direction;
		
		// Apply movement stretch
		ApplyClampedImpulse(
			new Vector2(_direction * 20, 0),
			new Vector2(_direction * MoveStretch, 0)
		);
		
		// Idle wobble
		float t = Time.GetTicksMsec() * 0.001f;
		Vector2 wobble = new Vector2(
			Mathf.Sin(t * 1.4f) * 0.3f,
			Mathf.Cos(t * 2.0f) * 0.25f
		);
		ApplyClampedImpulse(Vector2.Zero, wobble * IdleWobble);

		// 4. Move the enemy
		Velocity = velocity;
		MoveAndSlide();
	}
	
	private void ApplyClampedImpulse(Vector2 point, Vector2 desiredForce)
	{
		float length = desiredForce.Length();
		if (length < 0.001f) return;

		float clampedLength = Mathf.Clamp(length, 0.05f, 0.45f);
		Vector2 finalForce = desiredForce.Normalized() * clampedLength;

		_distortion?.Call("apply_impulse", point, finalForce);
	}
	
	private void OnBodyEntered(Node body)
	{
		if (!body.IsInGroup("player"))
			return;
			
		if (body is Player player)
		{
			player.Die(DeathType.EnemyKill);
			player.GlobalPosition = new Vector2(250, 1654);
		}
	}
	public void Die()
	{
		if (_isDead) return;
		_isDead = true;
		SetProcess(false);
		SetPhysicsProcess(false);
		CallDeferred(nameof(_DeathSequence));
	}
	private void _DeathSequence()
	{
		
		GetNode<CollisionShape2D>("CollisionShape2D").Disabled = true; 
		_popSound.Finished += OnPopSoundFinished;
		_popSound.Play();
	}
	private void OnPopSoundFinished()
	{
		_popSound.Finished -= OnPopSoundFinished;
		QueueFree();
	}
}
