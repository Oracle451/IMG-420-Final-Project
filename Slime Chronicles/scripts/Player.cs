// Handles Player movement and applying impulses
using Godot;

public enum DeathType 
{
	Generic,
	Water,
	EnemyKill
}

public partial class Player : CharacterBody2D
{
	// Set the players speed, jump velocity, and gravity
	[ExportGroup("Movement")]
	[Export] public float Speed = 300f;
	[Export] public float JumpVelocity = -500f;
	[Export] public float Gravity = 980f;
	
	// Sets the dash speed, the duration, and the cooldown
	[ExportGroup("Dash")]
	[Export] public float DashSpeed = 1000f;
	[Export] public float DashDuration = 0.2f;
	[Export] public float DashCooldown = 2.0f;
	
	// Variables to set how the much the player stretches and squishes upon certain actions
	[ExportGroup("Slime Deformation")]
	[Export] public float MoveStretch = 0.4f;
	[Export] public float JumpStretch = 0.6f;
	[Export] public float LandSquash = 0.8f;
	[Export] public float IdleBreathing = 0.3f;

	private Node _distortion;
	private bool _wasOnFloor;
	
	// dash state variables
	private bool _isDashing = false;
	private float _dashTimer = 0.0f;
	private float _canDashTimer = 0.0f;
	private Vector2 _dashDirection;
	private AudioStreamPlayer _dashSound;
	private AudioStreamPlayer _popSound;
	private AudioStreamPlayer _hitSound;
	
	// for the player scaling
	private readonly Vector2 DEFAULT_SCALE = Vector2.One;
	private readonly Vector2 SMALL_SCALE = new Vector2(0.5f, 0.5f);
	private readonly Vector2 BIG_SCALE = new Vector2(1.5f, 1.5f);
	private const float SCALE_LERP_SPEED = 5.0f;
	
	private float _defaultShapeRadius;
	private float _defaultShapeHeight;
	private CollisionShape2D _collisionShape;
	private Vector2 _targetScale = Vector2.One;

	public override void _Ready()
	{
		// Get the sprite and set the mesh to its original state
		_distortion = GetNode("DistortionSprite2D");
		_distortion?.Call("reset_mesh");
		_dashSound = GetNode<AudioStreamPlayer>("DashSound");
		_popSound = GetNode<AudioStreamPlayer>("PopSound");
		_hitSound = GetNode<AudioStreamPlayer>("HitSound");
		
		_collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
		if (_collisionShape.Shape is CapsuleShape2D capsuleShape)
		{
			_defaultShapeRadius = capsuleShape.Radius;
			_defaultShapeHeight = capsuleShape.Height;
		}
		
		_targetScale = DEFAULT_SCALE;
	}
	
	public override void _Process(double delta)
	{
		HandleScaling((float)delta);
	}

	public override void _PhysicsProcess(double delta)
	{
		float d = (float)delta;
		
		if (_canDashTimer > 0)
		{
			_canDashTimer -= d;
		}

		// check for dash input
		if (Input.IsKeyPressed(Key.E) && _canDashTimer <= 0 && !_isDashing)
		{
			StartDash();
		}

		if (_isDashing)
		{
			HandleDashState(d);
		}
		// slime moves normally while not dashing
		else
		{
			NormalMovement(d);
		}
		// moved the slime effects to a function to clear up this function
		HandleSlimeEffects();
		PushRigidBodies();
		MoveAndSlide();
	}
	
	private void PushRigidBodies()
	{
		// Cast to see if we are touching anything
		var collisionCount = GetSlideCollisionCount();
		for (int i = 0; i < collisionCount; i++)
		{
			KinematicCollision2D collision = GetSlideCollision(i);

			if (collision.GetCollider() is RigidBody2D rb)
			{
				// Direction of the push
				Vector2 pushDir = collision.GetNormal() * -1.0f;

				// Apply force proportional to player velocity
				rb.ApplyCentralImpulse(pushDir * 40f);
			}
		}
	}
	
	// this is the function that handles all normal player movement
	private void NormalMovement(float d)
	{
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

		// Landing squash logic
		bool onFloorNow = IsOnFloor();
		if (onFloorNow && !_wasOnFloor && vel.Y > 120f)
		{
			float rawImpact = vel.Y * 0.002f;
			ApplyClampedImpulse(new Vector2(0, 52), new Vector2(0, -rawImpact * LandSquash));
		}
		_wasOnFloor = onFloorNow;

		Velocity = vel;
	}
	
	// handles the slime effects
	private void HandleSlimeEffects()
	{
		// Idle breathing
		float t = Time.GetTicksMsec() * 0.001f;
		Vector2 breathe = new Vector2(
			Mathf.Sin(t * 2.3f) * 0.7f,
			Mathf.Cos(t * 1.6f + 0.8f) * 0.5f
		);
		ApplyClampedImpulse(Vector2.Zero, breathe * IdleBreathing);

		// Tiny velocity wobble
		ApplyClampedImpulse(Vector2.Zero, Velocity * 0.0008f);
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
	
	// function that begins a dash
	private void StartDash()
	{
		_isDashing = true;
		_dashTimer = DashDuration;
		_canDashTimer = DashCooldown;
		_dashSound.Play();

		// check which way the player is facing
		float inputX = Input.GetAxis("ui_left", "ui_right");
		float inputY = Input.GetAxis("ui_up", "ui_down");

		Vector2 dir = new Vector2(inputX, 0);

		// vertical dash
		if (Input.IsActionPressed("ui_up"))
		{
			dir = Vector2.Up;
		}
		// horizontal dash
		else if (Mathf.Abs(inputX) > 0.1f)
		{
			dir = new Vector2(inputX, 0).Normalized();
		}
		// default to right if the player isn't holding any direction key down
		else
		{
			dir = Vector2.Right; 
		}
		
		_dashDirection = dir;
		
		// applies the impulse in the direction of the dash
		ApplyClampedImpulse(Vector2.Zero, _dashDirection);
	}
	
	// controls what happens while the slime is dashing
	private void HandleDashState(float delta)
	{
		// ignores gravity while dashing
		Velocity = _dashDirection * DashSpeed;
		
		_dashTimer -= delta;
		if (_dashTimer <= 0)
		{
			_isDashing = false;
			// cut velocity after dash ends
			Velocity *= 0.5f; 
		}
	}
	// handles the scaling for slime
	private void HandleScaling(float delta)
	{
		if (Input.IsKeyPressed(Key.Q))
		{
			_targetScale = SMALL_SCALE;
		}
		else if (Input.IsKeyPressed(Key.R))
		{
			_targetScale = BIG_SCALE;
		}
		else
		{
			_targetScale = DEFAULT_SCALE;
		}
		
		// scaling the sprite
		if (_distortion is Node2D distortion2D)
		{
			distortion2D.Scale = distortion2D.Scale.Lerp(
				_targetScale,
				delta * SCALE_LERP_SPEED
			);
		}
		
		// scaling the collision shape
		if (_collisionShape?.Shape is CapsuleShape2D capsuleShape)
		{
			float targetRadius = _defaultShapeRadius * _targetScale.X; // Assuming X and Y scale equally
			float targetHeight = _defaultShapeHeight * _targetScale.Y;

			// Lerp the current Radius towards the target Radius
			capsuleShape.Radius = Mathf.Lerp(
				capsuleShape.Radius,
				targetRadius,
				delta * SCALE_LERP_SPEED
			);

			// Lerp the current Height towards the target Height
			capsuleShape.Height = Mathf.Lerp(
				capsuleShape.Height,
				targetHeight,
				delta * SCALE_LERP_SPEED
			);
			
			float halfDefaultHeight = (_defaultShapeHeight / 2.0f) + _defaultShapeRadius;
			float halfCurrentHeight = (capsuleShape.Height / 2.0f) + capsuleShape.Radius;
			
			// The amount to shift vertically (difference between half heights)
			float correctionY = halfDefaultHeight - halfCurrentHeight;
			
			// Apply the correction to the local position of the CharacterBody2D
			// Use Lerp for smooth position correction too
			Position = Position.Lerp(
				new Vector2(Position.X, Position.Y + correctionY),
				delta * SCALE_LERP_SPEED
			);
		}
	}
	
	public void Die(DeathType typeOfDeath)
	{
		switch (typeOfDeath)
		{
			case DeathType.Water:
				_hitSound.Play();
				break;
			case DeathType.EnemyKill:
				_popSound.Play();
				break;
			case DeathType.Generic:
			default:
				break;
		}
	}
}
