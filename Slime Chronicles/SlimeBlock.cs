using Godot;

public partial class SlimeBlock : RigidBody2D
{
	private Node _distortion;

	public override void _Ready()
	{
		_distortion = GetNode("DistortionSprite2D");
		_distortion?.Call("reset_mesh");
	}

	public override void _IntegrateForces(PhysicsDirectBodyState2D state)
	{
		// Soft sliding behavior (helps prevent sticking)
		LinearVelocity *= 0.99f;

		// Idle wobble (keeps it alive)
		float t = Time.GetTicksMsec() * 0.001f;
		Vector2 breathe = new Vector2(
			Mathf.Sin(t * 3.1f) * 0.2f,
			Mathf.Cos(t * 2.3f + 1.0f) * 0.2f
		);
		_distortion?.Call("apply_impulse", Vector2.Zero, breathe * 0.05f);

		// Detect collisions and apply impact wobble
		for (int i = 0; i < state.GetContactCount(); i++)
		{
			Vector2 point = state.GetContactLocalPosition(i);
			Vector2 impulse = state.GetContactImpulse(i);

			if (impulse.Length() > 50f)
			{
				Vector2 force = impulse * 10.0f;
				_distortion?.Call("apply_impulse", point, force);
			}
		}
	}
}
