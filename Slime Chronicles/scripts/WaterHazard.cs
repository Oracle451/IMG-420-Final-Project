using Godot;

public partial class WaterHazard : Node2D
{
	[Export]
	public Vector2 RespawnPosition = new Vector2(250, 1654);

	private Node _distortion;
	private Node _playerPendingTeleport;

	public override void _Ready()
	{
		_distortion = GetNode("DistortionSprite2D");

		Area2D area = GetNode<Area2D>("Area2D");
		area.BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node body)
	{
		if (!body.IsInGroup("player"))
			return;
		
		// Splash right away
		TriggerSplash(body);
		if (body is Player player)
		{
			player.Die(DeathType.Water);
		}
		
		// Store player so the timer callback knows who to move
		_playerPendingTeleport = body;

		// Create a 1-second timer (one-shot)
		Timer timer = new Timer
		{
			WaitTime = 0.5f,
			OneShot = true
		};

		AddChild(timer);

		timer.Timeout += OnTeleportTimeout;
		timer.Start();
	}

	private void OnTeleportTimeout()
	{
		if (_playerPendingTeleport != null)
		{
			_playerPendingTeleport.Set("global_position", RespawnPosition);
			_playerPendingTeleport = null;
		}
	}

	private void TriggerSplash(Node body)
	{
		if (_distortion == null)
			return;

		Vector2 hitPoint = ((Node2D)_distortion).ToLocal(
			(Vector2)body.Get("global_position")
		);

		Vector2 force = new Vector2(0, -50);

		_distortion.Call("apply_impulse", hitPoint, force);
	}
}
