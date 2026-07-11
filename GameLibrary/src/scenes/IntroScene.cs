using System.Numerics;
using Raylib_cs;

namespace GameLibrary;

public class IntroScene : Scene
{
    private int _step;
    private List<AnimCurve<float>> _pulses = new List<AnimCurve<float>>();
    private AnimCurve<float> _nextPulse = new AnimCurve<float>(0);
    private AnimCurve<float> _pulseInterval;
    private AnimCurve<float> _stepProgress;
    
    public IntroScene()
    {
        Game.MoveDevice(new Vector2(464, 110), 0.15f, 0);
    }
    
    public override void Update()
    {
        DoPulses();

        bool cheat = Raylib.IsKeyPressed(KeyboardKey.G);
        
        Raylib.ClearBackground(_step >= 4 ? Game.ScreenBlack : Color.Black);

        if (_step == 0)
        {
            if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), new Rectangle(287, 410, 146, 50)) ||
                Raylib.CheckCollisionPointCircle(Raylib.GetMousePosition(), Game.GetDevicePos(), 360f * 0.15f))
            {
                Game.HoverInteractable = true;
                if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                {
                    _step++;
                    _nextPulse = new AnimCurve<float>(1);
                    _pulseInterval = AnimCurve.NewFloat(3, 0.15f, 15);
                }
            }
        }
        else if (_step == 1)
        {
            if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), new Rectangle(287, 410, 146, 50)) ||
                Raylib.CheckCollisionPointCircle(Raylib.GetMousePosition(), Game.GetDevicePos(), 360f * 0.15f))
            {
                Game.HoverInteractable = true;
            }
            
            if (_pulseInterval.Sample() < 0.4 || cheat)
            {
                _step++;
                Game.MoveDevice(new Vector2(360, 360), 1, 5, Easings.InOutQuad);
            }
        }
        else if (_step == 2)
        {
            if (!Game.IsDeviceMoving() || cheat)
            {
                _nextPulse = new AnimCurve<float>(0);
                _stepProgress = AnimCurve.NewFloat(0, 2, 2);
                _step++;
            }
        }
        else if (_step == 3)
        {
            if (_stepProgress.IsComplete() || cheat)
            {
                Resources.Sounds["startup"].Play();
                _stepProgress = AnimCurve.NewFloat(0, 4, 4);
                _step++;
            }
        }
        else if (_step == 4)
        {
            Texture2D logo = Resources.Sprites["logo"];
            Raylib.DrawTexturePro(logo, logo.Rect(), new Rectangle(360, 360, logo.Size()/2), logo.Size()/4, 0, Color.White);
            ImGui.DrawTextRadial(0, -120, "powered by");
            Raylib.DrawCircle(360, 360, 350, Raylib.ColorAlpha(Game.ScreenBlack, 1 - (_stepProgress.Sample() - 1f)));
            
            if (_stepProgress.IsComplete() || cheat)
            {
                _stepProgress = AnimCurve.NewFloat(0, 1, 1);
                _step++;
            }
        }
        else if (_step == 5)
        {
            Texture2D logo = Resources.Sprites["logo"];
            Raylib.DrawTexturePro(logo, logo.Rect(), new Rectangle(360, 360, logo.Size()/2), logo.Size()/4, 0, Color.White);
            ImGui.DrawTextRadial(0, -120, "powered by");
            Raylib.DrawCircle(360, 360, 350, Raylib.ColorAlpha(Game.ScreenBlack, _stepProgress.Sample()));

            if (_stepProgress.IsComplete() || cheat)
            {
                _stepProgress = AnimCurve.NewFloat(0, 1, 1);
                _step++;
            }
        }
        else if (_step == 6)
        {
            if (_stepProgress.IsComplete() || cheat)
            {
                Game.ActiveScene = new MainMenu(true);
            }
        }
    }

    private void DoPulses()
    {
        if (_nextPulse.Sample() >= 1)
        {
            _nextPulse = AnimCurve.NewFloat(0, 1, _pulseInterval.Sample());
            Resources.Sounds["throb"].Play();
            _pulses.Add(AnimCurve.NewFloat(0, 1, 3));
        }
        
        foreach (AnimCurve<float> pulse in _pulses)
        {
            BackgroundDraw.CirclePulse(pulse.Sample());
        }
        _pulses.RemoveAll(p => p.IsComplete());
    }
}