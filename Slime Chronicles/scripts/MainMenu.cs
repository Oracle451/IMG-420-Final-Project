using Godot;
using System;

public partial class MainMenu : Control
{
	[ExportGroup("Button Deformation")]
	[Export] public float IdleBreathing = 0.3f;
	
	// private Node _distortion;
	
	// can just change the scene here if we want it to go somewhere else
	private const string LevelOnePath = "res://scenes/levels/level_one.tscn";
	public override void _Ready()
	{
		MusicManager.Instance.PlayMusic(MusicManager.MusicTrack.Title);
		
		// _distortion = GetNode("DistortionSprite2D1");
		// _distortion?.Call("reset_mesh");
	}
	
	public override void _PhysicsProcess(double delta)
	{
		/*
		// Idle breathing
		float t = Time.GetTicksMsec() * 0.001f;
		Vector2 breathe = new Vector2(
			Mathf.Sin(t * 2.3f) * 0.7f,
			Mathf.Cos(t * 1.6f + 0.8f) * 0.5f
		);
		ApplyClampedImpulse(Vector2.Zero, breathe * IdleBreathing);
		*/
	}
	
	// moves to the level one scene
	private void _on_start_button_pressed()	
	{
		if (GetTree() != null)
		{
			GetTree().ChangeSceneToFile(LevelOnePath);
		}
	}
	
	// quits the game when the exit button is pressed
	private void _on_exit_button_pressed()
	{
		if (GetTree() != null)
		{
			GetTree().Quit();
		}
	}
	
	/*
	private void ApplyClampedImpulse(Vector2 point, Vector2 desiredForce)
	{
		// Scale the vector so its length is clamped between 0.05 and 0.5
		float length = desiredForce.Length();
		if (length < 0.001f) return; // zero → skip

		float clampedLength = Mathf.Clamp(length, 0.05f, 0.5f);
		Vector2 finalForce = desiredForce.Normalized() * clampedLength;

		_distortion?.Call("apply_impulse", point, finalForce);
	}
	*/
}
