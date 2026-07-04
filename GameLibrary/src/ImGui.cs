using System.Numerics;
using Raylib_cs;

namespace GameLibrary;

// Immediate mode GUI functions, similar to raygui.
public static class ImGui
{
    private static Sound _buttonPress;
    private static Sound _buttonRelease;
    private static Font _defaultFont;
    private static int _defaultFontSize = 40;
    private static int _defaultTextSpacing = 3;

    static ImGui()
    {
        _buttonPress = Resources.Sounds["metronome"];
        _buttonRelease = Resources.Sounds["metronome"];
        _defaultFont = Resources.Fonts["conthrax"];
        Raylib.SetTextureFilter(_defaultFont.Texture, TextureFilter.Bilinear);
    }

    public static void DrawText(string text, Vector2 position, int size = 20)
    {
        Raylib.DrawTextEx(_defaultFont, text, position, size, 1, Color.White);
    }

    public static void DrawText(string text, int x, int y, int size = 20)
    {
        DrawText(text, new Vector2(x, y), size);
    }

    public static void DrawTextRadial(float angle, float radius, string text, float maxArc = 180)
    {
        Vector2 size = Raylib.MeasureTextEx(_defaultFont, text, _defaultFontSize, _defaultTextSpacing);
        Camera2D spin = new Camera2D
        {
            Target = new Vector2(0, 0),
            Offset = new Vector2(360, 360),
            Rotation = MeasureTextAngle(radius, text) / 2 + angle,
            Zoom = 1
        };
        
        for (int i = 0; i < text.Length; i++)
        {
            string character = text.Substring(i, 1);
            float width = Raylib.MeasureTextEx(_defaultFont, character, _defaultFontSize, _defaultTextSpacing).X;
            float rotateBy = (180 * (width + _defaultTextSpacing)) / (MathF.PI * radius);
            spin.Rotation -= rotateBy / 2;
            if (spin.Rotation < maxArc/2 && spin.Rotation > -maxArc/2)
            {
                Game.SetCamera(spin);
                Raylib.DrawTextCodepoint(_defaultFont, character[0], new Vector2(-width/2, radius - size.Y/2), _defaultFontSize, Color.White);
            }
            spin.Rotation -= rotateBy / 2;
        }
        Game.SetCamera();
    }

    public static float MeasureTextAngle(float radius, string text)
    {
        Vector2 size = Raylib.MeasureTextEx(_defaultFont, text, _defaultFontSize, _defaultTextSpacing);
        return (180 * size.X) / (MathF.PI * radius);
    }
    
    public static bool Button(string label, int x, int y)
    {
        Rectangle rect = new Rectangle(x, y, 100, 30);
        bool hovered = Raylib.CheckCollisionPointRec(Game.GetCursorPos(), rect);
        bool pressed = hovered && Raylib.IsMouseButtonDown(MouseButton.Left);
        Color color = hovered ? (pressed ? Color.DarkGray : Color.LightGray) : Color.Gray;
        
        if (hovered && Raylib.IsMouseButtonPressed(MouseButton.Left)) Raylib.PlaySound(_buttonPress);
        
        Raylib.DrawRectangleRec(rect, color);
        DrawText(label, x + 5, y + 5);

        if (hovered && Raylib.IsMouseButtonReleased(MouseButton.Left))
        {
            Raylib.PlaySound(_buttonRelease);
            return true;
        }

        return false;
    }
}