using System.Numerics;
using Raylib_cs;

namespace GameLibrary;

public class MainMenu : Scene
{
    private Music _menuMusic = Resources.Musics["null_function"];
    private PhysicsTest _physicsTest;
    
    private List<Font> _testFonts = new List<Font>();
    
    public MainMenu()
    {
        Game.ShuffleMusic(Assets.Musics.Values.ToList());
        _physicsTest = new PhysicsTest();
    }
    
    public override void Update()
    {
        if (Raylib.IsKeyPressed(KeyboardKey.One  )) Game.MoveDevice(new Vector2(360, 360), 1,     1);
        if (Raylib.IsKeyPressed(KeyboardKey.Two  )) Game.MoveDevice(new Vector2( 90,  90), 0.25f, 1);
        if (Raylib.IsKeyPressed(KeyboardKey.Three)) Game.MoveDevice(new Vector2( 90, 630), 0.25f, 1);
        if (Raylib.IsKeyPressed(KeyboardKey.Four )) Game.MoveDevice(new Vector2(180, 540), 0.5f,  1);
        if (Raylib.IsKeyPressed(KeyboardKey.Five )) Game.MoveDevice(new Vector2(240, 480), 0.66f,  1);
        
        
        Raylib.ClearBackground(Color.DarkBlue);
        
        BackgroundDraw.Web();
        
        if (Raylib.IsKeyDown(KeyboardKey.A)) Raylib.ClearBackground(new Color(32, 32, 32, 255));
        
        BackgroundDraw.CirclePulse(Math.Max(0, Game.MusicPlaying.Beat() / 4 - 0.5f) % 1);
        BackgroundDraw.CirclePulse(Math.Max(0, Game.MusicPlaying.Beat() / 4 - 0.0f) % 1);
        ImGui.DrawTextRadial(0, 160, "beat: " + Game.MusicPlaying.Beat().ToString("N2"));
        
        BackgroundDraw.Waveform2();
        
        _physicsTest.Step();
        
        Camera2D spin = new Camera2D();
        spin.Target = new Vector2(360, 360);
        spin.Offset = spin.Target;
        spin.Rotation = Time.Scaled * 60;
        spin.Zoom = 1;
        Game.SetCamera(spin);
        // Raylib.DrawTextureEx(Resources.Sprites["logo"], new Vector2(270, 270), 0, 0.5f, Color.White);
        Game.SetCamera();
        
        // if (Raylib.CheckCollisionPointCircle(Raylib.GetMousePosition(), new Vector2(360, 360), 90))
        // {
        //     Game.HoverInteractable = true;
        //     if (Raylib.IsMouseButtonPressed(MouseButton.Left))
        //     {
        //         // Sound sound = Resources.Sounds["metronome"];
        //         // Raylib.SetSoundVolume(sound, 0.5f);
        //         // Raylib.PlaySound(sound);
        //         Game.ShuffleMusic(Assets.Musics.Values.ToList());
        //     }
        // }

        if (Game.DebugMode)
        {
            if (Game.MusicPlaying.IsBeatThisFrame())
            {
                Sound sound = Resources.Sounds["metronome"];
                Raylib.SetSoundVolume(sound, 0.5f);
                Raylib.PlaySound(sound);
            }

            if (Raylib.IsKeyPressed(KeyboardKey.Equal)) Game.MusicPlaying.FirstBeat += 0.01f;
            if (Raylib.IsKeyPressed(KeyboardKey.Minus)) Game.MusicPlaying.FirstBeat -= 0.01f;
            if (Raylib.IsKeyPressed(KeyboardKey.Enter)) Game.ScrollText(Game.MusicPlaying.Title + " " + Game.MusicPlaying.FirstBeat.ToString("N3"));
        }
        
        Game.Mask();

        Raylib.UpdateMusicStream(_menuMusic);
    }
}