using System.Numerics;
using Raylib_cs;

namespace GameLibrary;

// Immediate mode GUI functions, similar to raygui.
public static class ImGui
{
    private static Sound _buttonPress;
    private static Sound _buttonRelease;
    private static Font _defaultFont;

    static ImGui()
    {
        _buttonPress = Resources.Sounds["metronome"];
        _buttonRelease = Resources.Sounds["metronome"];
        _defaultFont = Resources.Fonts["conthrax"];
    }

    public static void DrawText(string text, Vector2 position, int size = 20)
    {
        Raylib.DrawTextEx(_defaultFont, text, position, size, 1, Color.White);
    }

    public static void DrawText(string text, int x, int y, int size = 20)
    {
        DrawText(text, new Vector2(x, y), size);
    }

    public static void DrawTextRadial(float angle, float radius, string text)
    {
        
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