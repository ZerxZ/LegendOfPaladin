using Godot;
using System;

public partial class SoundManager : Node
{
	public enum Bus:int
	{
		Master,
		Sfx,
		Bgm
	}
	public static   SoundManager      Instance;
	[Export] public Node              Sfx;
	[Export] public AudioStreamPlayer BgmPlayer;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Instance = this;
	}
	
	public void PlaySfx(string name)
	{
		var sfx = Sfx.GetNode<AudioStreamPlayer>(name);
		sfx?.Play();
	}
	public void PlayBgm(AudioStream stream)
	{
		if (BgmPlayer.Stream == stream && BgmPlayer.Playing)
		{
			return;
		}
		BgmPlayer.Stream = stream;
		BgmPlayer.Play();

	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	public void SetupUiSounds(Node node)
	{
		if (node is Button button)
		{
			button.Pressed += OnSfxPressed;
			button.FocusEntered += OnSfxFocusEntered;
		}
		foreach (var child in node.GetChildren())
		{
			SetupUiSounds(child);
		}
	}
	private void OnSfxFocusEntered()
	{
		PlaySfx("UIFocus");
	}
	private void OnSfxPressed()
	{
		PlaySfx("UIPress");
	}
	public float GetVolume(Bus busIndex) => GetVolume((int)busIndex);
	public float GetVolume(int busIndex)
	{
		var db = AudioServer.GetBusVolumeDb(busIndex);
		return Mathf.DbToLinear(db);
	}
	public void SetVolume(Bus busIndex,float volume) => SetVolume((int)busIndex,volume);
	public void SetVolume(int busIndex,float volume)
	{
		var db = Mathf.LinearToDb(volume);
		AudioServer.SetBusVolumeDb(busIndex,db);
	}
}
