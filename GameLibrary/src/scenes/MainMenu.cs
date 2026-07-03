using System.Numerics;
using Raylib_cs;

namespace GameLibrary;

public class MainMenu : Scene
{
    private Music _menuMusic = Resources.Musics["null_function"];
    private int _musicSelected = 0;
    private string _nowPlaying = "null_function";
    
    private List<Font> _testFonts = new List<Font>();
    
    public MainMenu()
    {
        Raylib.SetMusicVolume(_menuMusic, 0.5f);
        Raylib.PlayMusicStream(_menuMusic);
    }
    
    public override void Update()
    {
        Raylib.ClearBackground(Color.Blank);
        
        Raylib.DrawCircle(360, 360, 350, new Color(10, 15, 50));
        Raylib.DrawTexture(Resources.Sprites["bg"], 0, 0, Color.DarkBlue);

        Camera2D spin = new Camera2D();
        spin.Target = new Vector2(360, 360);
        spin.Offset = spin.Target;
        spin.Rotation = Time.Scaled * 60;
        spin.Zoom = 1;
        Game.SetCamera(spin);
        Raylib.DrawTextureEx(Resources.Sprites["logo"], new Vector2(270, 270), 0, 0.5f, Color.White);
        Game.SetCamera();
        
        if ((Time.Scaled/2) % 1 < 0.5f) {ImGui.DrawTextRadial(0, -280, "Click me for more tunes!");}
        ImGui.DrawTextRadial(0, 280, $"Now Playing: {_nowPlaying}");
        
        if (Raylib.CheckCollisionPointCircle(Raylib.GetMousePosition(), new Vector2(360, 360), 90))
        {
            Game.HoverInteractable = true;
            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                Raylib.PlaySound(Resources.Sounds["metronome"]);
                _musicSelected++;
                _musicSelected %= Resources.Musics.Count;
                Raylib.StopMusicStream(_menuMusic);
                var musics = Resources.Musics.ToList();
                _menuMusic = musics[_musicSelected].Value;
                _nowPlaying = musics[_musicSelected].Key;
                Raylib.PlayMusicStream(_menuMusic);
            }
        }

        Raylib.UpdateMusicStream(_menuMusic);
    }
}