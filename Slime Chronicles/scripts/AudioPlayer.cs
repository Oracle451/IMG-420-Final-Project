using Godot;
using System;

public static class AudioPlayer
{
	private static readonly AudioStream _explosionSound = GD.Load<AudioStream>("res://path/to/Explosion.wav");
	public static void PlayExplosion(Node parentNode, float volumeDb = 0.0f)
	{
		// 1. Create a new AudioStreamPlayer node
		var player = new AudioStreamPlayer();
		player.Stream = _explosionSound;
		player.VolumeDb = volumeDb;

		// 2. Add it to the scene tree via the parent node
		parentNode.AddChild(player);
		
		// 3. Connect the Finished signal to delete itself (QueueFree)
		// This is crucial for cleanup!
		player.Finished += player.QueueFree;

		// 4. Start playback
		player.Play();
	}
}
