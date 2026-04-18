using System;
using System.Threading.Tasks;
using Game.Utilities;
using Godot;

namespace Game;

public partial class Manager : Node
{
    public static Manager Instance { get; private set; } = null!;

    [Export]
    string configPath = "user://config_user.dat";

    const string defaultConfigPath = "user://config_default.dat";

    public GameConfig Config { get; set; } = null!;

    [Export]
    PackedScene titleScene = null!;

    public GameData Data { get; set; } = new();
    private AudioStreamPlayer? bombardmentPlayer;
    private AudioStream? bombardmentStream;

    public Manager()
    {
        // Just so that other scripts can cache a reference.
        // Config and game data won't be loaded until _Ready() is called
        if (Instance == null)
        {
            Instance = this;

            // Do other stuff
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // At this point all other autoloads are also ready
        // Now we should do actual game stuff (e.g. loading config)

        // Load config vars
        LoadConfig();
    }

    public override void _Notification(int what)
    {
        if (what == NotificationWMCloseRequest)
        {
            QuitGame();
        }
    }

    public void ExitToTitle()
    {
        GetTree().Paused = false;

        // Free the mouse if it was captured
        Input.MouseMode = Input.MouseModeEnum.Visible;

        GetTree().ChangeSceneToPacked(titleScene);
    }

    public void QuitGame()
    {
        GetTree().Quit();
    }

    public void LoadConfig(bool defaultFile = false)
    {
        if (!defaultFile)
        {
            Config = DataUtils.LoadFromFileOrNull<GameConfig>(configPath)!;
        }

        // Fall back to default config
        if (Config == null || defaultFile)
        {
            Config = DataUtils.LoadFromFileOrNull<GameConfig>(defaultConfigPath)!;
        }

        // Use current settings if no default either
        Config ??= new();

        Config.ApplyConfig();

        // If there's no default config yet (e.g. first game start)
        if (!FileAccess.FileExists(defaultConfigPath))
        {
            DataUtils.SaveData(defaultConfigPath, Config);
        }
    }

    public void SaveConfig()
    {
        DataUtils.SaveData(configPath, Config);
    }

    public async Task PlayBombardmentSfx(int blasts = 3, float intervalSeconds = 0.22f)
    {
        bombardmentStream ??= ResourceLoader.Load<AudioStream>("res://assets/sounds/pulse.wav");
        if (bombardmentStream == null)
        {
            return;
        }

        if (!GodotObject.IsInstanceValid(bombardmentPlayer))
        {
            bombardmentPlayer = new AudioStreamPlayer();
            AddChild(bombardmentPlayer);
        }

        bombardmentPlayer.Stream = bombardmentStream;
        for (int i = 0; i < blasts; i++)
        {
            // Slight pitch variation helps the sequence feel less repetitive.
            bombardmentPlayer.PitchScale = 0.85f + (i * 0.08f);
            bombardmentPlayer.Play();
            await ToSignal(GetTree().CreateTimer(intervalSeconds), SceneTreeTimer.SignalName.Timeout);
        }
    }
}
