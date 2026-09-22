using Menu.Remix.MixedUI;
using UnityEngine;

namespace Jumpscare;

public class JumpscareOptions : OptionInterface
{
	public readonly Configurable<int> Chance;

	public readonly Configurable<int> Volume;

	public readonly Configurable<int> Frames;

	public JumpscareOptions()
	{
		Chance = config.Bind("chance", 10000, new ConfigurableInfo("Every second there is a 1 in N chance of a jumpscare.", new ConfigAcceptableRange<int>(1, 1000000)));
		Volume = config.Bind("volume", 100, new ConfigurableInfo("Jumpscare sound volume (%).", new ConfigAcceptableRange<int>(0, 200)));
		Frames = config.Bind("frames", 14, new ConfigurableInfo("Number of frames stacked vertically in the sprite sheet.", new ConfigAcceptableRange<int>(1, 100)));
	}

	public override void Initialize()
	{
		base.Initialize();
		OpTab tab = new OpTab(this, "Options");
		Tabs = new[] { tab };
		tab.AddItems(
			new OpLabel(10f, 550f, "Custom Jumpscare", true),
			new OpLabel(10f, 490f, "Chance each second:  1 in"),
			new OpUpdown(Chance, new Vector2(200f, 484f), 120f) { description = Chance.info.description },
			new OpLabel(10f, 440f, "Volume (%)"),
			new OpSlider(Volume, new Vector2(200f, 434f), 200) { description = Volume.info.description },
			new OpLabel(10f, 390f, "Sprite sheet frames"),
			new OpUpdown(Frames, new Vector2(200f, 384f), 120f) { description = Frames.info.description },
			new OpLabel(10f, 310f, "Custom files: put a .png vertical sprite sheet and/or a .wav/.ogg/.mp3 sound"),
			new OpLabel(10f, 290f, "in the mod's \"custom\" folder."),
			new OpLabel(10f, 270f, "(Usually found at `Rain World/RainWorld_Data/StreamingAssets/mods/`)"),
			new OpLabel(10f, 240f, "Frames are stacked top to bottom in the .png. Files are reloaded when you apply these settings."),
			new OpLabel(10f, 215f, "If either file is missing, the built in image or sound is used instead.")
		);
	}
}
