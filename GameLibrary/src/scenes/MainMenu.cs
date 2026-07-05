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
        ShuffleMusic();
        _physicsTest = new PhysicsTest();
    }
    
    public override void Update()
    {
        if (Raylib.IsKeyPressed(KeyboardKey.One  )) Game.MoveDevice(new Vector2(360, 360), 1,     1);
        if (Raylib.IsKeyPressed(KeyboardKey.Two  )) Game.MoveDevice(new Vector2( 90,  90), 0.25f, 1);
        if (Raylib.IsKeyPressed(KeyboardKey.Three)) Game.MoveDevice(new Vector2( 90, 630), 0.25f, 1);
        if (Raylib.IsKeyPressed(KeyboardKey.Four )) Game.MoveDevice(new Vector2(180, 540), 0.5f,  1);
        
        
        Raylib.ClearBackground(Color.DarkBlue);
        
        // Raylib.DrawCircle(360, 360, 350, Color.DarkBlue);
        Raylib.DrawTexturePro(
            Resources.Sprites["radial"], 
            new Rectangle(0, 0, Resources.Sprites["radial"].Dimensions), 
            new Rectangle(0, 0, 720, 720), 
            Vector2.Zero, 
            0, 
            Color.Black);
        for (int i = 0; i < 8; i++)
        {
            Raylib.DrawCircleLines(360, 360, 45 * i, Color.Black);
        }
        // Raylib.DrawLineEx(new Vector2(0, 0), new Vector2(720, 720), 4, Color.Black);
        Raylib.DrawLine(0, 0, 720, 720, Color.Black);
        Raylib.DrawLine(0, 720, 720, 0, Color.Black);
        Raylib.DrawLine(360, 0, 360, 720, Color.Black);
        Raylib.DrawLine(0, 360, 720, 360, Color.Black);
        
        DrawCirclePulse((Time.Scaled / 4) % 1);
        DrawCirclePulse(((Time.Scaled + 2) / 4) % 1);
        
        _physicsTest.Step();
        
        Camera2D spin = new Camera2D();
        spin.Target = new Vector2(360, 360);
        spin.Offset = spin.Target;
        spin.Rotation = Time.Scaled * 60;
        spin.Zoom = 1;
        Game.SetCamera(spin);
        Raylib.DrawTextureEx(Resources.Sprites["logo"], new Vector2(270, 270), 0, 0.5f, Color.White);
        Game.SetCamera();
        
        if (Raylib.CheckCollisionPointCircle(Raylib.GetMousePosition(), new Vector2(360, 360), 90))
        {
            Game.HoverInteractable = true;
            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                ShuffleMusic();
            }
        }

        if (!Raylib.IsMusicStreamPlaying(_menuMusic))
        {
            ShuffleMusic();
        }
        
        Game.Mask();

        Raylib.UpdateMusicStream(_menuMusic);
    }

    private void ShuffleMusic()
    {
        Raylib.PlaySound(Resources.Sounds["metronome"]);
        Raylib.StopMusicStream(_menuMusic);
        var musics = Resources.Musics.ToList().PickRandom();
        _menuMusic = musics.Value;
        _menuMusic.Looping = false;
        Game.ScrollText($"Now playing: {musics.Key}");
        Raylib.SetMusicVolume(_menuMusic, 0.5f);
        Raylib.PlayMusicStream(_menuMusic);
    }

    private void DrawCirclePulse(float t)
    {
        t = Easings.OutQuad(t);
        Rectangle src = Resources.Sprites["circle_soft"].Rect();
        Rectangle dst = new Rectangle(360, 360, new Vector2(820, 820) * t);
        Raylib.DrawTexturePro(
            Resources.Sprites["circle_soft"], 
            src, 
            dst, 
            dst.Size/2, 
            0, 
            new Color(255, 255, 255, 128));
    }
}