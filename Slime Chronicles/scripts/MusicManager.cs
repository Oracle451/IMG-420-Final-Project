using Godot;
using System;

public partial class MusicManager : Node
{
	public static MusicManager Instance {get; private set; }
	private AudioStreamPlayer2D _audioPlayer;
	
	private readonly AudioStream _titleMusic = (AudioStream)ResourceLoader.Load("res://assets/sfx/TitleMusic.mp3");
	private readonly AudioStream _levelMusic = (AudioStream)ResourceLoader.Load("res://assets/sfx/LevelMusic.mp3");
	
	public enum MusicTrack
	{
		// I only added 2 working ones but we can add more soundtracks easily
		Title,
		LevelsStart,
		LevelsEnd
	}
	
	public override void _Ready()
	{
		if (Instance == null)
		{
			Instance = this;
			this.SetProcessMode(ProcessModeEnum.Always);
		}
		else
		{
			QueueFree();
			return;
		}
		
		_audioPlayer = GetNode<AudioStreamPlayer2D>("AudioPlayer");
	}
	
	public void PlayMusic(MusicTrack track)
	{
		AudioStream newStream = null;
		
		switch(track)
		{
			// adding more soundtracks just means adding more cases
			case MusicTrack.Title:
				newStream = _titleMusic;
				break;
			case MusicTrack.LevelsStart:
				newStream = _levelMusic;
				break;
			default:
				GD.PrintErr("Unknown music track requested.");
				return;
		}
		
		if(_audioPlayer.Stream == newStream && _audioPlayer.Playing)
		{
			return;
		}
		
		_audioPlayer.Stop();
		_audioPlayer.Stream = newStream;
		_audioPlayer.Play();
	}
}
