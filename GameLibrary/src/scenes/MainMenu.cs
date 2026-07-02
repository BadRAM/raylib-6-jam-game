using System.Numerics;
using Raylib_cs;

namespace GameLibrary;

public class MainMenu : Scene
{
    private Music _menuMusic = Resources.Musics["null_function"];
    private int _musicSelected = 0;
    
    private List<Font> _testFonts = new List<Font>();
    
    public MainMenu()
    {
        Raylib.SetMusicVolume(_menuMusic, 0.5f);
        Raylib.PlayMusicStream(_menuMusic);
    }
    
    public override void Update()
    {
        Raylib.ClearBackground(Color.Blank);
        
        Raylib.DrawCircle(360, 360, 360, new Color(10, 15, 50));

        Camera2D spin = new Camera2D();
        spin.Target = new Vector2(360, 360);
        spin.Offset = spin.Target;
        spin.Rotation = Time.Scaled * 60;
        spin.Zoom = 1;
        Game.SetCamera(spin);
        Raylib.DrawTexture(Resources.Sprites["logo"], 180, 180, Color.White);
        Game.SetCamera();
        
        // if ((Time.Scaled/2) % 1 < 0.5f) {ImGui.DrawText("Press '1' to start", 300, 680, 20);}
        
        if (Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            Raylib.PlaySound(Resources.Sounds["metronome"]);
            _musicSelected++;
            _musicSelected %= Resources.Musics.Count;
            Raylib.StopMusicStream(_menuMusic);
            _menuMusic = Resources.Musics.ToList()[_musicSelected].Value;
            Raylib.PlayMusicStream(_menuMusic);
            Raylib.SetMusicVolume(_menuMusic, 0);
            Raylib.UpdateMusicStream(_menuMusic);
            Raylib.SetMusicVolume(_menuMusic, 1);
        }
        else
        {
            Raylib.UpdateMusicStream(_menuMusic);
        }
        
    }
}