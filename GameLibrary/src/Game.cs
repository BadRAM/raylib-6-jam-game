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

    // Target Resolution: 720x720
    private static Camera2D _defaultCamera = new Camera2D(new Vector2(0, 0), Vector2.Zero, 0, 1);
    private static Camera2D _activeCamera = _defaultCamera;
    private static RenderTexture2D _renderTexture;
    private static Shader _screenShader;
    private static int _screenShaderMaskLocation;

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
        _screenShader = Resources.Shaders["screen_fragment"];
        _screenShaderMaskLocation = Raylib.GetShaderLocation(_screenShader, "mask");
        
        ActiveScene = new MainMenu();
        
        ScrollText("SCROLLING TEXT, ON A CIRCLE. ONLY POSSIBLE IN RAYLIB THROUGH THE DARK MAJICKS OF BADRAM AND HIS MAD SKILLS.");
        ScrollText("DID YOU THINK I WAS DONE? I'VE ONLY BEGUN TO SCROLL MY TEXT!");
        ScrollText("TEXT SCROLLS WILL RETURN");
    }
    
    public static void Update()
    {
        Time.UpdateTime();

        if (Raylib.IsKeyPressed(KeyboardKey.F3)) DebugMode = !DebugMode;

        HoverInteractable = false;
        if (!Raylib.CheckCollisionPointCircle(Raylib.GetMousePosition(), new Vector2(360, 360), 360))
        {
            // :'(
            // Raylib.SetWindowState(ConfigFlags.MousePassthroughWindow);
        }
        else
        {
            if (Raylib.IsMouseButtonDown(MouseButton.Left))
            {
                _lastWindowDelta = Raylib.GetMouseDelta() + _lastWindowDelta;
                Vector2 winPos = Raylib.GetWindowPosition() + _lastWindowDelta;
                Raylib.SetWindowPosition((int)winPos.X, (int)winPos.Y);
            }
        }
        
        Raylib.BeginDrawing();
        Raylib.BeginTextureMode(_renderTexture);
        SetCamera();
        
        ActiveScene.Update();
        if (_scrollerTexts.Count > 0)
        {
            ImGui.DrawTextRadial(_scrollerAngle, -280, _scrollerTexts[0]);
            _scrollerAngle -= 1;
            if (_scrollerAngle < -90 + -ImGui.MeasureTextAngle(280, _scrollerTexts[0]) / 2)
            {
                _scrollerTexts.RemoveAt(0);
                if (_scrollerTexts.Count > 0)
                {
                    _scrollerAngle = ImGui.MeasureTextAngle(280, _scrollerTexts[0]) / 2 + 90;
                }
            }
        }
        
        while (LateActions.Count > 0) LateActions.Dequeue().Invoke();
        
        Raylib.EndMode2D();
        Raylib.EndTextureMode();
        _activeCamera = _defaultCamera;

        if (!DebugMode)
        {
            Raylib.BeginShaderMode(_screenShader);
            Raylib.SetShaderValueTexture(_screenShader, _screenShaderMaskLocation, Resources.Sprites["screen_mask"]);
            Raylib.DrawTextureRec(_renderTexture.Texture, new Rectangle(0, 0, 720, -720), Vector2.Zero, Color.White);
            Raylib.EndShaderMode();
        }
        else
        {
            Raylib.DrawTextureRec(_renderTexture.Texture, new Rectangle(0, 0, 720, -720), Vector2.Zero, Color.White);
        }
        
        // DrawRing(Raylib.GetMousePosition().Y, Raylib.GetMousePosition().X / 720f);
        DrawRing(Time.Scaled * 2, MathF.Sin(Time.Scaled / 2f) / 2f + 0.5f);

        Mask();
        
        Raylib.EndDrawing();
        
        Raylib.SetMouseCursor(HoverInteractable ? MouseCursor.PointingHand : MouseCursor.Default);
    }
    
    // angle is 0-360, tilt is 0-1
    private static void DrawRing(float angle, float tilt)
    {
        int frame = (int)MathF.Floor((tilt % 1) * 10);
        float subframe = ((tilt % 1) * 10) % 1;
        Camera2D spin = new Camera2D();
        spin.Target = new Vector2(360, 360);
        spin.Offset = spin.Target;
        spin.Rotation = angle;
        spin.Zoom = 1;
        Game.SetCamera(spin);
        Raylib.DrawTexture(Resources.Sprites[$"ring{frame}"], 0, 0, Color.White);
        Raylib.DrawTexture(Resources.Sprites[$"ring{Math.Min(frame + 1, 9)}"], 0, 0, new Color(255, 255, 255, (int)(255 * subframe)));
        Game.SetCamera();
        
        // ImGui.DrawTextRadial(0, -240, $"F:{frame} S:{subframe:N2} A:{angle:N0}");
    }

    private static void Mask()
    {
        Raylib.BeginBlendMode(BlendMode.CustomSeparate);
        Rlgl.SetBlendFactorsSeparate(Rlgl.ZERO, Rlgl.ONE, Rlgl.ONE, Rlgl.ZERO, Rlgl.FUNC_ADD, Rlgl.FUNC_ADD);
        Raylib.DrawTexture(Resources.Sprites["mask"], 0, 0, Color.White);
        Raylib.EndBlendMode();
    }

    public static void ScrollText(string text)
    {
        _scrollerTexts.Add(text);
        if (_scrollerTexts.Count == 1)
        {
            _scrollerAngle = ImGui.MeasureTextAngle(280, _scrollerTexts[0]) / 2 + 180;
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

    public static Camera2D GetActiveCamera() => _activeCamera;

    public static Vector2 GetCursorPos() => Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), _activeCamera);
}
