using System.Numerics;
using System.Runtime.InteropServices;
using Box2D.NET;
using GameLibrary;
using static Box2D.NET.B2Joints;
using static Box2D.NET.B2Ids;
using static Box2D.NET.B2Types;
using static Box2D.NET.B2MathFunction;
using static Box2D.NET.B2Bodies;
using static Box2D.NET.B2Shapes;
using static Box2D.NET.B2Worlds;
using static Box2D.NET.B2Geometries;
using static Box2D.NET.B2Diagnostics;
using Raylib_cs;

public class PhysicsTest
{
    private B2WorldId WorldId;
    private static B2DebugDraw _debugDraw = Box2dDebugDrawRaylib.Create();
    private Texture2D _ballTex;
    float timeStep = 1.0f / 60.0f;
    int subStepCount = 4;
    
    private List<Ball> _balls = new List<Ball>();
    private List<Ball> _ballsToDestroy = new List<Ball>();
    private List<Color> _colors = new List<Color>() { Color.Red, Color.Yellow, Color.DarkBlue, Color.SkyBlue };
    
    public PhysicsTest()
    {
        _ballTex = Resources.Sprites["raysan"];
        
        B2WorldDef worldDef = b2DefaultWorldDef();
        
        worldDef.gravity = new B2Vec2(0.0f, 0.0f);
        WorldId = b2CreateWorld(worldDef);

        // for (int i = 0; i < 360; i+=2)
        // {
        //     B2BodyDef groundBodyDef = b2DefaultBodyDef();
        //     groundBodyDef.position = (Utils.AngleLength(i, 312f) + new Vector2(360, 360)).ToB2();
        //     groundBodyDef.rotation = b2MakeRot(-i * (MathF.PI / 180f));
        //
        //     B2BodyId groundId = b2CreateBody(WorldId, groundBodyDef);
        //
        //     B2Polygon groundBox = b2MakeBox(16.0f, 4.0f);
        //
        //     B2ShapeDef groundShapeDef = b2DefaultShapeDef();
        //     b2CreatePolygonShape(groundId, groundShapeDef, groundBox);
        // }
    }
    
    public void Step()
    {
        if (Time.Frame % 10 == 0 || Raylib.IsKeyDown(KeyboardKey.L))
        {
            B2BodyDef bodyDef = b2DefaultBodyDef();
            bodyDef.type  = B2BodyType.b2_dynamicBody;
            // bodyDef.position = (Vector2.Normalize(Utils.RandomInsideUnitCircle(Random.Shared)) * 4 + new Vector2(36f, 36f)).ToB2();
            bodyDef.position = new B2Vec2(0, 0);
            bodyDef.linearDamping = 0.05f;
            B2BodyId bodyId = b2CreateBody(WorldId, bodyDef);
        
            B2Circle dynamicCircle;
            dynamicCircle.center = (Vector2.Normalize(Utils.RandomInsideUnitCircle(Random.Shared)) * 32f + new Vector2(36, 36)).ToB2();
            dynamicCircle.radius = 1.4f;
        
            B2ShapeDef shapeDef = b2DefaultShapeDef();
            shapeDef.density = 1.0f;
            shapeDef.material.friction = 0.3f;
            shapeDef.material.restitution = 0.75f;
        
            b2CreateCircleShape(bodyId, shapeDef, dynamicCircle);
            _balls.Add(new Ball(bodyId, _colors.PickRandom()));
        }
        
        b2World_Step(WorldId, timeStep, subStepCount);
        
        foreach (Ball ball in _balls)
        {
            B2Vec2 pos = b2Body_GetWorldCenterOfMass(ball.Body);
            if (Raylib.IsMouseButtonPressed(MouseButton.Left) && Raylib.CheckCollisionPointCircle(Raylib.GetMousePosition(), pos.ToVec2() * 10, 14))
            {
                _ballsToDestroy.AddRange(MergeBalls(ball));
            }
            float rot = b2Rot_GetAngle(b2Body_GetRotation(ball.Body)) * 180 / MathF.PI;
            Vector2 delta = new Vector2(36f, 36f) - pos.ToVec2();
            Vector2 g = Vector2.Normalize(delta) * 100;
            b2Body_ApplyForce(ball.Body, g.ToB2(), pos, true);
            Raylib.DrawTexturePro(_ballTex, new Rectangle(0, 0, _ballTex.Dimensions), new Rectangle(pos.ToVec2() * 10, _ballTex.Dimensions/2), _ballTex.Dimensions/4, rot, ball.Color);
        }

        foreach (Ball ball in _ballsToDestroy)
        {
            b2DestroyBody(ball.Body);
            _balls.Remove(ball);
        }
        _ballsToDestroy.Clear();
        
        if (Raylib.IsKeyDown(KeyboardKey.V))
            b2World_Draw(WorldId, _debugDraw);
    }

    private List<Ball> MergeBalls(Ball origin)
    {
        List<Ball> ballsToMerge = new List<Ball>();
        List<Ball> ballsToCheck = new List<Ball>() {origin};
        
        while (ballsToCheck.Count > 0)
        {
            Vector2 pos = b2Body_GetWorldCenterOfMass(ballsToCheck[0].Body).ToVec2();
            foreach (Ball ball in _balls)
            {
                if (ball.Color.Compare(ballsToCheck[0].Color) &&
                    !ballsToMerge.Contains(ball) &&
                    Raylib.CheckCollisionCircles(b2Body_GetWorldCenterOfMass(ball.Body).ToVec2(), 1.5f, pos, 1.5f))
                {
                    ballsToMerge.Add(ball);
                    ballsToCheck.Add(ball);
                }
            }
            ballsToCheck.RemoveAt(0);
        }

        return ballsToMerge;
    }
    
    public void Destroy()
    {
        b2DestroyWorld(WorldId);
    }
    
    static class Box2dDebugDrawRaylib
    {
        public static B2DebugDraw Create()
        {
            var debugDraw = b2DefaultDebugDraw();
            debugDraw.DrawPolygonFcn = DrawPolygon;
            debugDraw.DrawSolidPolygonFcn = DrawSolidPolygon;
            debugDraw.DrawCircleFcn = DrawCircle;
            debugDraw.DrawSolidCircleFcn = DrawSolidCircle;
            debugDraw.DrawSolidCapsuleFcn = DrawSolidCapsule;
            debugDraw.drawLineFcn = DrawSegment;
            debugDraw.DrawTransformFcn = DrawTransform;
            debugDraw.DrawPointFcn = DrawPoint;
            debugDraw.DrawStringFcn = DrawString;

            debugDraw.drawShapes = true;
            debugDraw.drawJoints = true;
            debugDraw.drawJointExtras = true;
            // debugDraw.drawBounds = true;
            debugDraw.drawMass = true;
            debugDraw.drawBodyNames = true;
            debugDraw.drawGraphColors = true;
            debugDraw.drawContactNormals = true;
            // debugDraw.drawContactForces = true;
            // debugDraw.drawContactFeatures = true;
            // debugDraw.drawFrictionForces = true;
            debugDraw.drawIslands = true;

            return debugDraw;
        }
        
        private static void DrawPolygon(ReadOnlySpan<B2Vec2> vertices, int vertexCount, B2HexColor color, object o)
        {
            for (int i = 0; i < vertexCount; i++)
            {
                Raylib.DrawLineV(vertices[i].ToVec2(), vertices[(i + 1) % vertexCount].ToVec2(), color.ToRaylib());
            }
        }
        
        private static void DrawSolidPolygon(in B2Transform transform, ReadOnlySpan<B2Vec2> vertices, int vertexCount, float radius, B2HexColor color, object o)
        {
            Vector2[] verts = vertices.ToArray().Select(v => v.ToVec2()).ToArray();

            for (var i = 0; i < verts.Length; i++)
            {
                verts[i] = new Vector2(verts[i].X * transform.q.c - verts[i].Y * transform.q.s, verts[i].X * transform.q.s + verts[i].Y * transform.q.c);
                verts[i] += transform.p.ToVec2();
            }
            for (int i = 0; i < vertexCount; i++)
            {
                Raylib.DrawLineV(verts[i], verts[(i + 1) % vertexCount], color.ToRaylib());
            }
        }
        
        private static void DrawCircle(in B2Vec2 center, float radius, B2HexColor color, object o)
        {
            Raylib.DrawCircleLinesV(center.ToVec2(), radius, color.ToRaylib());
        }
        
        private static void DrawSolidCircle(in B2Transform transform, float radius, B2HexColor color, object o)
        {
            Vector2 center = transform.p.ToVec2();
            Raylib.DrawCircleV(center, radius, Raylib.ColorAlpha(color.ToRaylib(), 0.25f));
            Raylib.DrawCircleLinesV(center, radius, color.ToRaylib());
            Raylib.DrawLineV(center, center + new Vector2(transform.q.c, transform.q.s) * radius, color.ToRaylib());
        }
        
        private static void DrawSolidCapsule(in B2Vec2 point1, in B2Vec2 point2, float radius, B2HexColor color, object o)
        {
            Vector2 p1 = point1.ToVec2();
            Vector2 p2 = point2.ToVec2();
            Raylib.DrawCircleV(p1, radius, color.ToRaylib());
            Raylib.DrawCircleV(p2, radius, color.ToRaylib());
            Raylib.DrawLineEx(p1, p2, radius, color.ToRaylib());
        }
        
        private static void DrawSegment(in B2Vec2 point1, in B2Vec2 point2, B2HexColor color, object o)
        {
            Vector2 p1 = point1.ToVec2();
            Vector2 p2 = point2.ToVec2();
            Raylib.DrawLineV(p1, p2, color.ToRaylib());
        }
        
        private static void DrawTransform(in B2Transform transform, object o)
        {
            DrawSolidCircle(transform, 4, B2HexColor.b2_colorAqua, o);
        }
        
        private static void DrawPoint(in B2Vec2 point, float size, B2HexColor color, object o)
        {
            Raylib.DrawCircleV(point.ToVec2(), size, color.ToRaylib());
        }
        
        private static void DrawString(in B2Vec2 point, string s, B2HexColor color, object o)
        {
            Vector2 p = point.ToVec2();
            Raylib.DrawText(s, (int)p.X, (int)p.Y, 4, color.ToRaylib());
        }
    }
}

public class Ball
{
    public B2BodyId Body;
    public Color Color;

    public Ball(B2BodyId body, Color color)
    {
        Body = body;
        Color = color;
    }
}