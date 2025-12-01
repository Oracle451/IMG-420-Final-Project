using Godot;
using System;

public partial class ExitPipe : Area2D
{
	[Signal]
	public delegate void PlayerEnteredPipeEventHandler();

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body.Name == "Player")
		{
			EmitSignal(SignalName.PlayerEnteredPipe);
		}
	}
}
