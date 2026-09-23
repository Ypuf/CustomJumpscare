using System;
using System.Collections;
using System.IO;
using System.Linq;
using BepInEx;
using UnityEngine;
using UnityEngine.Networking;

namespace Jumpscare;

[BepInPlugin("ypuf.jumpscare", "Custom chance for a custom jumpscare every second", "1.1.0")]
public class JumpscareMod : BaseUnityPlugin
{
	private const string DefaultImage = "assets/Jumpscare_Image";

	private const string CustomImage = "JumpscareCustomImage";

	private float frameindex = 0f;

	private double PrevTime = 0.0;

	private bool initialized;

	private JumpscareOptions Options;

	private FContainer Jumpscare;

	private FSprite JumpscareSprite;

	private Texture2D customTexture;

	private AudioClip customClip;

	private AudioSource audioSource;

	public static SoundID JumpscareSound;

	private string CustomFolder => Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Info.Location)), "custom");

	private void OnEnable()
	{
		RegisterValues();
		On.RainWorld.OnModsInit += OnModsInit;
		On.RainWorld.Update += JumpscareChance;
	}

	private void OnDisable()
	{
		UnregisterValues();
	}

	private void OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
	{
		orig(self);
		if (!initialized)
		{
			initialized = true;
			Options = new JumpscareOptions(CustomFolder);
			Options.OnConfigChanged += LoadCustomFiles;
			MachineConnector.SetRegisteredOI("ypuf.jumpscare", Options);
		}
		LoadCustomFiles();
	}

	private void LoadCustomFiles()
	{
		try
		{
			Directory.CreateDirectory(CustomFolder);
			LoadCustomImage();
			LoadCustomSound();
		}
		catch (Exception e)
		{
			Logger.LogError("Failed to load custom jumpscare files: " + e);
		}
	}

	private void LoadCustomImage()
	{
		if (customTexture != null)
		{
			Futile.atlasManager.UnloadAtlas(CustomImage);
			Destroy(customTexture);
			customTexture = null;
		}
		string file = Directory.GetFiles(CustomFolder, "*.png").OrderBy(f => f).FirstOrDefault();
		if (file == null)
		{
			return;
		}
		Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, false);
		if (!texture.LoadImage(File.ReadAllBytes(file)))
		{
			Logger.LogError("Could not read image " + file);
			Destroy(texture);
			return;
		}
		texture.wrapMode = TextureWrapMode.Clamp;
		customTexture = texture;
		Futile.atlasManager.LoadAtlasFromTexture(CustomImage, texture, false);
		Logger.LogInfo("Loaded custom jumpscare image " + file);
	}

	private void LoadCustomSound()
	{
		StopAllCoroutines();
		if (customClip != null)
		{
			Destroy(customClip);
			customClip = null;
		}
		string file = Directory.GetFiles(CustomFolder)
			.Where(f => GetAudioType(f) != AudioType.UNKNOWN)
			.OrderBy(f => f)
			.FirstOrDefault();
		if (file != null)
		{
			StartCoroutine(LoadSound(file));
		}
	}

	private static AudioType GetAudioType(string file)
	{
		switch (Path.GetExtension(file).ToLowerInvariant())
		{
			case ".wav":
				return AudioType.WAV;
			case ".ogg":
				return AudioType.OGGVORBIS;
			case ".mp3":
				return AudioType.MPEG;
			default:
				return AudioType.UNKNOWN;
		}
	}

	private IEnumerator LoadSound(string file)
	{
		AudioType type = GetAudioType(file);
		using UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(new Uri(file).AbsoluteUri, type);
		yield return request.SendWebRequest();
		if (request.result != UnityWebRequest.Result.Success)
		{
			Logger.LogError("Could not read sound " + file + ": " + request.error);
			yield break;
		}
		customClip = DownloadHandlerAudioClip.GetContent(request);
		Logger.LogInfo("Loaded custom jumpscare sound " + file);
	}

	public void JumpscareChance(On.RainWorld.orig_Update orig, RainWorld self)
	{
		orig.Invoke(self);
		if (Options == null)
		{
			return;
		}
		double second = Math.Floor(Time.fixedTime);
		int frames = Options.Frames.Value;
		if (frameindex > 0f || (UnityEngine.Random.Range(0, Options.Chance.Value) == 0 && second != PrevTime))
		{
			if (frameindex == 0.5f)
			{
				PlaySound(self);
			}
			ShowFrame(self, Mathf.Min((int)frameindex, frames - 1), frames);
			frameindex += 0.5f;
		}
		else if (JumpscareSprite != null)
		{
			JumpscareSprite.isVisible = false;
		}
		if (frameindex > Mathf.Max(60f, frames))
		{
			frameindex = 0f;
		}
		PrevTime = second;
	}

	private void ShowFrame(RainWorld self, int frame, int frames)
	{
		string image = customTexture != null ? CustomImage : DefaultImage;
		if (!Futile.atlasManager.DoesContainElementWithName(image))
		{
			Futile.atlasManager.LoadImage(image);
		}
		if (Jumpscare == null)
		{
			Jumpscare = new FContainer();
		}
		if (Jumpscare.container == null)
		{
			Futile.stage.AddChild(Jumpscare);
		}
		Jumpscare.MoveToFront();
		if (JumpscareSprite == null)
		{
			JumpscareSprite = new FSprite(image, true);
			JumpscareSprite.anchorX = 0f;
			JumpscareSprite.anchorY = 0f;
			Jumpscare.AddChild(JumpscareSprite);
		}
		else if (JumpscareSprite.element != Futile.atlasManager.GetElementWithName(image))
		{
			JumpscareSprite.SetElementByName(image);
		}
		JumpscareSprite.isVisible = true;
		Vector2 size = JumpscareSprite.element.sourceSize;
		JumpscareSprite.scaleX = self.screenSize.x / size.x;
		JumpscareSprite.scaleY = self.screenSize.y / size.y * frames;
		JumpscareSprite.y = -self.screenSize.y * (frames - 1 - frame);
	}

	private void PlaySound(RainWorld self)
	{
		float volume = Options.Volume.Value / 100f;
		if (customClip != null)
		{
			if (audioSource == null)
			{
				GameObject go = new GameObject("JumpscareAudio");
				DontDestroyOnLoad(go);
				audioSource = go.AddComponent<AudioSource>();
				audioSource.playOnAwake = false;
				audioSource.spatialBlend = 0f;
			}
			audioSource.clip = customClip;
			audioSource.volume = volume * self.options.soundEffectsVolume;
			audioSource.Play();
		}
		else if (self.processManager.menuMic != null)
		{
			self.processManager.menuMic.PlaySound(JumpscareSound, 0f, volume, 1f);
		}
		else if (self.processManager.currentMainLoop is RainWorldGame game)
		{
			game.cameras[0].virtualMicrophone.PlaySound(JumpscareSound, 0f, volume, 1f, 1);
		}
	}

	public static void RegisterValues()
	{
		JumpscareSound = new SoundID("Jumpscare_Audio", true);
	}

	public static void UnregisterValues()
	{
		if ((ExtEnum<SoundID>)(object)JumpscareSound != (ExtEnum<SoundID>)null)
		{
			((ExtEnum<SoundID>)(object)JumpscareSound).Unregister();
			JumpscareSound = null;
		}
	}
}
