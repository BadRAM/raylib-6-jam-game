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

    // Target Resolution: 720x720
    private static Camera2D _defaultCamera = new Camera2D(new Vector2(0, 0), Vector2.Zero, 0, 1);
    private static Camera2D _activeCamera = _defaultCamera;

    public static void Load()
    {
        Raylib.SetConfigFlags(ConfigFlags.TransparentWindow | ConfigFlags.UndecoratedWindow);
        Raylib.InitWindow(720, 720, "Cool Game :)");
        Raylib.SetTargetFPS(Time.FrameRate);
        Raylib.InitAudioDevice();
        Raylib.SetExitKey(KeyboardKey.Null);
        
        Resources.Load();
        
        ActiveScene = new MainMenu();
    }
    
    public static void Update()
    {
        Time.UpdateTime();
        
        Raylib.BeginDrawing();
        SetCamera();
        
        ActiveScene.Update();
        
        while (LateActions.Count > 0) LateActions.Dequeue().Invoke();
        
        Raylib.EndMode2D();
        // ImGui.DrawText("FPS: " + Raylib.GetFPS(), 0, 0);
        _activeCamera = _defaultCamera;
        // Raylib.DrawTexture(Resources.Sprites["mask"], 0, 0, new Color(12, 12, 12));
        Raylib.EndDrawing();
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
