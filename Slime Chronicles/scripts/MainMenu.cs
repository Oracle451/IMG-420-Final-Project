using Godot;
using System;

public partial class MainMenu : Control
{
	// can just change the scene here if we want it to go somewhere else
	private const string LevelOnePath = "res://scenes/levels/level_one.tscn";
	public override void _Ready()
	{
		MusicManager.Instance.PlayMusic(MusicManager.MusicTrack.Title);
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
}
