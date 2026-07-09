using System.Numerics;
using Raylib_cs;

namespace GameLibrary;

public class IntroScene : Scene
{
    private int step;
    private List<AnimFloat> _pulses = new List<AnimFloat>();
    
    public IntroScene()
    {
        Game.MoveDevice(new Vector2(464, 110), 0.15f, 0);
    }
    
    public override void Update()
    {
        Raylib.ClearBackground(Color.Black);
        
        if (Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            Console.WriteLine(Raylib.GetMousePosition());
        }

        if (step == 0)
        {
            foreach (AnimFloat pulse in _pulses)
            {
                BackgroundDraw.CirclePulse(pulse.Sample());
            }
            _pulses.RemoveAll(p => p.IsComplete());
            
            if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), new Rectangle(287, 410, 146, 50)))
            {
                Game.HoverInteractable = true;
                if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                {
                    Throb();
                }
            }
            if (Raylib.CheckCollisionPointCircle(Raylib.GetMousePosition(), Game.GetDevicePos(), 360f * 0.15f))
            {
                Game.HoverInteractable = true;
                if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                {
                    step++;
                }
            }
        }
        else if (step == 1)
        {
            Game.MoveDevice(new Vector2(360, 360), 1, 3, Easings.InOutSine);
            Game.ActiveScene = new MainMenu();
        }
    }

    private void Throb()
    {
        Raylib.PlaySound(Resources.Sounds["throb"]);
        _pulses.Add(new AnimFloat(0, 1, 3));
    }
}