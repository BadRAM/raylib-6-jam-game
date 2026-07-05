using System.Numerics;
using Box2D.NET;
using Raylib_cs;

namespace GameLibrary;

// Designated dumping ground for extension methods and math functions
public static class Utils
{
    public static Vector2 Size(this Texture2D tex) => new(tex.Width, tex.Height);
    public static Rectangle Rect(this Texture2D tex) => new(Vector2.Zero, tex.Width, tex.Height);
    
    public static float MoveTowards(this float start, float target, float maxDistanceDelta)
    {
        if (Math.Abs(target - start) < maxDistanceDelta)
        {
            return target;
        } 
        return (start > target) ? (start - maxDistanceDelta) : (start + maxDistanceDelta);
    }

    public static Vector2 AngleLength(float angle, float length)
    {
        angle *= MathF.PI / 180;
        return new Vector2(MathF.Sin(angle), MathF.Cos(angle)) * length;
    }
    
    public static B2Vec2 ToB2(this Vector2 vec)
    {
        return new B2Vec2(vec.X, vec.Y);
    }

    public static Vector2 ToVec2(this B2Vec2 vec)
    {
        return new Vector2(vec.X, vec.Y);
    }
    
    public static Color ToRaylib(this B2HexColor color)
    {
        int i = (int)color;        
        return new Color((i >> 16) & 0xFF, (i >> 8) & 0xFF, i & 0xFF, 255);
    }
    
    public static T PickRandom<T>(this List<T> list, Random? random = null)
    {
        random ??= Random.Shared;
        return list[random.Next(list.Count)];
    }
}