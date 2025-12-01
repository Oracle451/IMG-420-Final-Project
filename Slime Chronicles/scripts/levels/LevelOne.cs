using Godot;
using System;

public partial class LevelOne : Node2D
{
	[Export(PropertyHint.File, "*.tscn")]
	public string NextLevelPath;
	
	public override void _Ready()
	{
		MusicManager.Instance.PlayMusic(MusicManager.MusicTrack.LevelsStart);
	}
	// switches the scene after the player reaches the pipe
	public void OnPipeActivated()
	{
		CallDeferred(MethodName.DeferredSwitchScene);
	}
	private void DeferredSwitchScene()
	{
		if (NextLevelPath != null)
		{
			GetTree().ChangeSceneToFile(NextLevelPath);
		}
	}
}
