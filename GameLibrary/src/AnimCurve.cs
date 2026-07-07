using System.Numerics;
using Raylib_cs;

namespace GameLibrary;

public abstract class AnimCurve<T>
{
    public T Start;
    public T End;
    public float Duration;
    public Func<float, float> Easing;
    public float StartTime;
    
    protected AnimCurve(T start, T end, float duration, Func<float, float>? easing = null, float? startTime = null)
    {
        Start = start;
        End = end;
        Duration = duration;
        Easing = easing ?? Easings.Linear;
        StartTime = startTime ?? Time.Scaled;
    }

    protected AnimCurve(T end)
    {
        Start = end;
        End = end;
        Duration = 0;
        Easing = Easings.Linear;
        StartTime = 0;
    }
    
    public abstract T Sample(float time);

    public bool IsComplete(float time) => time - StartTime > Duration;

    protected float Progress(float time)
    {
        if (Duration == 0) return 1f;
        return Easing.Invoke(Math.Clamp((time - StartTime) / Duration, 0, 1));
    }
}

public class AnimFloat : AnimCurve<float>
{
    public AnimFloat(float start, float end, float duration, Func<float, float>? easing = null, float? startTime = null) : base(start, end, duration, easing, startTime) { }
    public AnimFloat(float end) : base(end) { }

    public override float Sample(float time)
    {
        return float.Lerp(Start, End, Progress(time));
    }
}

public class AnimVector2 : AnimCurve<Vector2>
{
    public AnimVector2(Vector2 start, Vector2 end, float duration, Func<float, float>? easing = null, float? startTime = null) : base(start, end, duration, easing, startTime) { }
    public AnimVector2(Vector2 end) : base(end) { }

    public override Vector2 Sample(float time)
    {
        return Vector2.Lerp(Start, End, Progress(time));
    }
}

public class AnimCamera : AnimCurve<Camera2D>
{
    public AnimCamera(Camera2D start, Camera2D end, float duration, Func<float, float>? easing = null, float? startTime = null) : base(start, end, duration, easing, startTime) { }
    public AnimCamera(Camera2D end) : base(end) { }

    public override Camera2D Sample(float time)
    {
        float t = Progress(time);
        Camera2D result = new Camera2D()
        {
            Target = Vector2.Lerp(Start.Target, End.Target, t),
            Offset = Vector2.Lerp(Start.Offset, End.Offset, t),
            Rotation = float.Lerp(Start.Rotation, End.Rotation, t),
            Zoom = float.Lerp(Start.Zoom, End.Zoom, t),
        };
        return result;
    }
}

public class AnimRect : AnimCurve<Rectangle>
{
    public AnimRect(Rectangle start, Rectangle end, float duration, Func<float, float>? easing = null, float? startTime = null) : base(start, end, duration, easing, startTime) { }
    public AnimRect(Rectangle end) : base(end) { }

    public override Rectangle Sample(float time)
    {
        float t = Progress(time);
        Rectangle result = new Rectangle()
        {
            Position = Vector2.Lerp(Start.Position, End.Position, t),
            Size = Vector2.Lerp(Start.Size, End.Size, t),
        };
        return result;
    }
}