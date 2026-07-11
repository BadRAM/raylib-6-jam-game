using System.Numerics;
using Raylib_cs;

namespace GameLibrary;

public class MainMenu : Scene
{
    private Music _menuMusic = Resources.Musics["null_function"];
    
    private List<Font> _testFonts = new List<Font>();
    private AnimCurve<float> _loadAnim = new AnimCurve<float>(1);
    
    public MainMenu(bool fromBlack)
    {
        Game.ShuffleMusic(Assets.Musics.Values.ToList());
        if (fromBlack) _loadAnim = AnimCurve.NewFloat(0, 1, 1);
    }
    
    public override void Update()
    {
        if (Raylib.IsKeyPressed(KeyboardKey.One  )) Game.MoveDevice(new Vector2(360, 360), 1,     1);
        if (Raylib.IsKeyPressed(KeyboardKey.Two  )) Game.MoveDevice(new Vector2( 90,  90), 0.25f, 1);
        if (Raylib.IsKeyPressed(KeyboardKey.Three)) Game.MoveDevice(new Vector2( 90, 630), 0.25f, 1);
        if (Raylib.IsKeyPressed(KeyboardKey.Four )) Game.MoveDevice(new Vector2(180, 540), 0.5f,  1);
        if (Raylib.IsKeyPressed(KeyboardKey.Five )) Game.MoveDevice(new Vector2(240, 480), 0.66f,  1);
        if (Raylib.IsKeyPressed(KeyboardKey.G    )) Game.ActiveScene = new GameScene();
        
        
        Raylib.ClearBackground(Color.DarkBlue);
        
        BackgroundDraw.Web();
        
        if (Raylib.IsKeyDown(KeyboardKey.A)) Raylib.ClearBackground(new Color(32, 32, 32, 255));
        
        BackgroundDraw.CirclePulse(Math.Max(0, Game.MusicPlaying.Beat() / 4 - 0.5f) % 1);
        BackgroundDraw.CirclePulse(Math.Max(0, Game.MusicPlaying.Beat() / 4 - 0.0f) % 1);
        BackgroundDraw.Waveform2();
        
        Texture2D logo = Resources.Sprites["logo"];
        Raylib.DrawTexturePro(logo, logo.Rect(), new Rectangle(360, 360, logo.Size()/2), logo.Size()/4, Time.Scaled * 60, Color.White);
        
        if (Raylib.IsKeyPressed(KeyboardKey.N))
        {
            Game.ShuffleMusic(Assets.Musics.Values.ToList());
        }
        
        if (Game.DebugMode)
        {
            if (Game.MusicPlaying.IsBeatThisFrame())
            {
                Resources.Sounds["metronome"].Play(volume:0.5f);
            }

            if (Raylib.IsKeyPressed(KeyboardKey.Equal)) Game.MusicPlaying.FirstBeat += 0.01f;
            if (Raylib.IsKeyPressed(KeyboardKey.Minus)) Game.MusicPlaying.FirstBeat -= 0.01f;
            if (Raylib.IsKeyPressed(KeyboardKey.Enter)) Game.ScrollText(Game.MusicPlaying.Title + " " + Game.MusicPlaying.FirstBeat.ToString("N3"));
        }

        if (_loadAnim.Sample() < 1) Raylib.DrawCircle(360, 360, 350, Raylib.ColorAlpha(Game.ScreenBlack, 1 - _loadAnim.Sample()));
        
        Raylib.UpdateMusicStream(_menuMusic);
    }
}