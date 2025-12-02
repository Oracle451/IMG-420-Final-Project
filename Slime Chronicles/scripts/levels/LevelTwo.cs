using Godot;
using System;

public partial class LevelTwo : Node2D
{
	public override void _Ready()
	{
		MusicManager.Instance.PlayMusic(MusicManager.MusicTrack.LevelsStart);
	}
}
