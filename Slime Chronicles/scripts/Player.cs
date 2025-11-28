using Godot;

public partial class Player : CharacterBody2D
{
	[Export] public float Speed = 220f;
	[Export] public float JumpVelocity = -480f;
	[Export] public float Gravity = 1500f;

	private Node2D _body;
	private bool _wasOnFloor = false;

	public override void _Ready()
	{
		_body = GetNode<Node2D>("Body");
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;
		Vector2 velocity = Velocity;

		float dir = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");
		velocity.X = dir * Speed;

		if (!IsOnFloor())
			velocity.Y += Gravity * dt;
		else if (velocity.Y > 0)
			velocity.Y = 0;

		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
			ApplySlimeImpulse(new Vector2(0, 1) * 80);
		}

		Velocity = velocity;
		MoveAndSlide();

		if (!_wasOnFloor && IsOnFloor())
			ApplySlimeImpulse(new Vector2(0, -1) * 160);

		if (IsOnFloor() && Mathf.Abs(dir) > 0.1f)
			ApplySlimeImpulse(new Vector2(-dir, -0.2f) * 40);

		_wasOnFloor = IsOnFloor();
	}

	private void ApplySlimeImpulse(Vector2 force)
	{
		if (_body != null)
			_body.Call("apply_impulse", GlobalPosition, force);
	}
}
