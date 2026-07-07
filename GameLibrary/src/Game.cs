using Raylib_cs;
using System.Numerics;

namespace GameLibrary;

public static class Game
{
    public static bool IsWeb;
    public static bool ShouldQuit;
    public static string Dir = "";
    public static Queue<Action> LateActions = new Queue<Action>(); // LateActions are dequeued and invoked after everything else has updated.
    public static Scene ActiveScene;
    public static bool HoverInteractable;
    public static bool DebugMode;
    public static List<MusicAsset> Playlist;
    public static MusicAsset? MusicPlaying;

    // Target Resolution: 720x720
    private static Camera2D _defaultCamera = new Camera2D(new Vector2(0, 0), Vector2.Zero, 0, 1);
    private static Camera2D _activeCamera = _defaultCamera;
    private static RenderTexture2D _renderTexture;
    private static Shader _screenShader;
    private static int _screenShaderMaskLocation;

    private static AnimCamera _deviceCameraAnim = new AnimCamera(new Camera2D(Vector2.Zero, Vector2.Zero, 0, 1));

    private static ConfigFlags _defaultFlags = ConfigFlags.TransparentWindow | ConfigFlags.UndecoratedWindow | ConfigFlags.Msaa4xHint;
    private static Vector2 _lastWindowDelta;

    private static List<string> _scrollerTexts = new List<string>();
    private static float _scrollerAngle = 500;
    

    public static void Load(bool isWeb)
    {
        IsWeb = isWeb;
        Raylib.SetConfigFlags(_defaultFlags);
        Raylib.InitWindow(720, 720, "Cool Game :)");
        Raylib.SetTargetFPS(Time.FrameRate);
        Raylib.InitAudioDevice();
        Raylib.SetExitKey(KeyboardKey.Null);
        _renderTexture = Raylib.LoadRenderTexture(720, 720);
        
        Resources.Load();
        Assets.Load();
        _screenShader = Resources.Shaders["screen_fragment"];
        _screenShaderMaskLocation = Raylib.GetShaderLocation(_screenShader, "mask");
        
        ActiveScene = new IntroScene();
        
        // ScrollText("SCROLLING TEXT, ON A CIRCLE. ONLY POSSIBLE IN RAYLIB THROUGH BADRAM'S MAD SKILLS.");
        // ScrollText("DID YOU THINK I WAS DONE? I'VE ONLY BEGUN TO SCROLL MY TEXT!");
        // ScrollText("I'M A TEXT SCROLLING MACHINE!");
        // ScrollText("TEXT SCROLLS WILL RETURN.");
    }
    
    public static void Update()
    {
        Time.UpdateTime();

        if (Raylib.IsKeyPressed(KeyboardKey.F3)) DebugMode = !DebugMode;

        HoverInteractable = false;
        // if (!Raylib.CheckCollisionPointCircle(Raylib.GetMousePosition(), new Vector2(360, 360), 360))
        // {
        //     // :'(
        //     // Raylib.SetWindowState(ConfigFlags.MousePassthroughWindow);
        // }
        // else
        // {
        //     if (!Raylib.CheckCollisionPointCircle(Raylib.GetMousePosition(), new Vector2(360, 360), 310))
        //     {
        //         HoverInteractable = true;
        //         if (Raylib.IsMouseButtonDown(MouseButton.Left))
        //         {
        //             _lastWindowDelta = Raylib.GetMouseDelta() + _lastWindowDelta;
        //             Vector2 winPos = Raylib.GetWindowPosition() + _lastWindowDelta;
        //             Raylib.SetWindowPosition((int)winPos.X, (int)winPos.Y);
        //         }
        //     }
        // }

        MusicPlaying?.Update();
        if (MusicPlaying != null && !Raylib.IsMusicStreamPlaying(MusicPlaying.Music))
        {
            ShuffleMusic(Playlist);
        }
        
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Blank);
        Raylib.DrawTexturePro(Resources.Sprites["fakebanner"], Resources.Sprites["fakebanner"].Rect(), new Rectangle(360, 360, Resources.Sprites["fakebanner"].Size()), Resources.Sprites["fakebanner"].Size()/2, 0, Color.White);
        Raylib.BeginTextureMode(_renderTexture);
        Raylib.ClearBackground(Color.Blank);
        SetCamera();
        
        ActiveScene.Update();
        if (_scrollerTexts.Count > 0)
        {
            ImGui.DrawTextRadial(_scrollerAngle, -280, _scrollerTexts[0], 160);
            _scrollerAngle -= 1;
            if (_scrollerAngle < -80 + -ImGui.MeasureTextAngle(280, _scrollerTexts[0]) / 2)
            {
                _scrollerTexts.RemoveAt(0);
                if (_scrollerTexts.Count > 0)
                {
                    _scrollerAngle = ImGui.MeasureTextAngle(280, _scrollerTexts[0]) / 2 + 80;
                }
            }
        }
        
        while (LateActions.Count > 0) LateActions.Dequeue().Invoke();
        
        Mask();
        Raylib.EndMode2D();
        Raylib.EndTextureMode();
        _activeCamera = _defaultCamera;

        Raylib.BeginMode2D(_deviceCameraAnim.Sample(Time.Scaled));
        
        if (!DebugMode)
        {
            Raylib.BeginShaderMode(_screenShader);
            Raylib.SetShaderValueTexture(_screenShader, _screenShaderMaskLocation, Resources.Sprites["screen_mask_colored"]);
            Raylib.DrawTextureRec(_renderTexture.Texture, new Rectangle(0, 0, 720, -720), Vector2.Zero, Color.White);
            Raylib.EndShaderMode();
        }
        else
        {
            Raylib.DrawTextureRec(_renderTexture.Texture, new Rectangle(0, 0, 720, -720), Vector2.Zero, Color.White);
        }
        
        DrawRing(Time.Scaled * 2, MathF.Sin(Time.Scaled / 2f) / 2f + 0.5f);

        Raylib.BeginBlendMode(BlendMode.CustomSeparate);
        Rlgl.SetBlendFactorsSeparate(Rlgl.ZERO, Rlgl.ONE, Rlgl.ONE, Rlgl.ZERO, Rlgl.FUNC_ADD, Rlgl.FUNC_ADD);
        Raylib.DrawRectangle(0, 0, 720, 720, Color.White);
        Raylib.EndBlendMode();
        
        Raylib.EndMode2D();
        
        Raylib.EndDrawing();
        
        Raylib.SetMouseCursor(HoverInteractable ? MouseCursor.PointingHand : MouseCursor.Default);
    }

    public static void MoveDevice(Vector2 center, float zoom, float duration, Func<float, float>? easing = null)
    {
        _deviceCameraAnim = new AnimCamera
        (
            _deviceCameraAnim.Sample(Time.Scaled),
            new Camera2D(center, new Vector2(360, 360), 0, zoom), 
            duration, 
            easing ?? Easings.OutQuint
        );
    }
    
    // angle is 0-360, tilt is 0-1
    private static void DrawRing(float angle, float tilt)
    {
        int frame = (int)MathF.Floor((tilt % 1) * 10);
        float subframe = ((tilt % 1) * 10) % 1;

        Rectangle src = new Rectangle(0, 0, 720, 720);
        Rectangle dst = new Rectangle(360, 360, 720, 720);
        Raylib.DrawTexturePro(Resources.Sprites[$"glass_shine"], src, dst, new Vector2(360, 360), -40, Color.White);
        Raylib.DrawTexturePro(Resources.Sprites[$"ring{frame}"], src, dst, new Vector2(360, 360), angle, Color.White);
        Raylib.DrawTexturePro(Resources.Sprites[$"ring{Math.Min(frame + 1, 9)}"], src, dst, new Vector2(360, 360), angle, new Color(255, 255, 255, (int)(255 * subframe)));
        
        // ImGui.DrawTextRadial(0, -240, $"F:{frame} S:{subframe:N2} A:{angle:N0}");
    }

    public static void Mask()
    {
        Raylib.BeginBlendMode(BlendMode.CustomSeparate);
        Rlgl.SetBlendFactorsSeparate(Rlgl.ZERO, Rlgl.ONE, Rlgl.ONE, Rlgl.ZERO, Rlgl.FUNC_ADD, Rlgl.FUNC_ADD);
        Raylib.DrawTexture(Resources.Sprites["mask"], 0, 0, Color.White);
        // Raylib.DrawRectangle(0, 0, 720, 720, Color.White);
        Raylib.EndBlendMode();
    }
    
    public static void ScrollText(string text)
    {
        _scrollerTexts.Add(text);
        if (_scrollerTexts.Count == 1)
        {
            _scrollerAngle = ImGui.MeasureTextAngle(280, _scrollerTexts[0]) / 2 + 80;
        }
    }
    
    public static void SetCamera(Camera2D? camera = null)
    {
        Camera2D cam = camera ?? _defaultCamera;
        
        if (_activeCamera.Offset == cam.Offset &&      // ReSharper disable once CompareOfFloatsByEqualityOperator
            _activeCamera.Zoom == cam.Zoom &&          // ReSharper disable once CompareOfFloatsByEqualityOperator
            _activeCamera.Rotation == cam.Rotation &&
            _activeCamera.Target == cam.Target)
        {
            return;
        }
        Raylib.EndMode2D();
        _activeCamera = cam;
        Raylib.BeginMode2D(_activeCamera);
    }
    
    public static void ShuffleMusic(List<MusicAsset> music)
    {
        Playlist = music;
        music = new List<MusicAsset>(Playlist); // clone list so we can modify it without breaking referenced static lists.
        if (MusicPlaying != null) music.Remove(MusicPlaying); // Prevent shuffle from picking the song that was already playing.

        if (music.Count == 0) return;
        
        MusicPlaying?.Stop();
        MusicPlaying = music.PickRandom();
        MusicPlaying.Music.Looping = false;
        MusicPlaying.Play();
        
        ScrollText($"Now Playing: {MusicPlaying.Title} - {MusicPlaying.ArtistName}");
    }

    public static Camera2D GetActiveCamera() => _activeCamera;

    public static Vector2 GetCursorPos() => Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), _activeCamera);
}
