using Godot;
using System;
using 勇者传说.globals;

public partial class VolumeSlider : HSlider
{
	[Export] public SoundManager.Bus Bus;
	[Export] public AudioStream Bgm;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Value = SoundManager.Instance.GetVolume(Bus);
		ValueChanged += OnValueChanged;
	}
	private void OnValueChanged(double value)
	{
		SoundManager.Instance.SetVolume(Bus, (float)value);
		Game.Instance.SaveConfig();
		if (Bgm is not null)
		{
			SoundManager.Instance.PlayBgm(Bgm);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
