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

namespace GameLibrary;

public class GameScene : Scene
{
    private B2WorldId WorldId;
    private static B2DebugDraw _debugDraw = Box2dDebugDrawRaylib.Create();
    private Texture2D _ballTex;
    float timeStep = 1.0f / 60.0f;
    int subStepCount = 4;
    
    private List<Ball> _balls = new List<Ball>();
    private List<Ball> _ballsToDestroy = new List<Ball>();
    private List<Color> _colors = new List<Color>() { Color.Red, Color.Yellow, Color.Purple, Color.SkyBlue };
    
    private int _score;
    private int _displayedScore;
    private int _targetScore;
    private int _combo;
    private float _lastClear;
    private List<ScoreAnim> _scoreAnims = new List<ScoreAnim>();
    
    public GameScene()
    {
        _ballTex = Resources.Sprites["graysan"];
        
        B2WorldDef worldDef = b2DefaultWorldDef();
        
        worldDef.gravity = new B2Vec2(0.0f, 0.0f);
        WorldId = b2CreateWorld(worldDef);
    }
    
    public override void Update()
    {
        Raylib.ClearBackground(Color.DarkBlue);
        
        BackgroundDraw.Web();
        
        if (Raylib.IsKeyDown(KeyboardKey.A)) Raylib.ClearBackground(new Color(32, 32, 32, 255));
        
        BackgroundDraw.CirclePulse(Math.Max(0, Game.MusicPlaying.Beat() / 4 - 0.5f) % 1);
        BackgroundDraw.CirclePulse(Math.Max(0, Game.MusicPlaying.Beat() / 4 - 0.0f) % 1);
        BackgroundDraw.Waveform2();
        
        if (Time.Frame % 10 == 0 || _balls.Count < 60 || Raylib.IsKeyDown(KeyboardKey.L))
        {
            Vector2 pos = Vector2.Normalize(Utils.RandomInsideUnitCircle(Random.Shared)) * 32f + new Vector2(36, 36);
            MakeBall(pos, Ball.RandomDefaultType());
        }
        
        b2World_Step(WorldId, timeStep, subStepCount);
        
        foreach (Ball ball in _balls)
        {
            ball.Draw();
            
            Vector2 delta = new Vector2(36f, 36f) - ball.Position;
            Vector2 g = Vector2.Normalize(delta) * 100;
            b2Body_ApplyForce(ball.Body, g.ToB2(), ball.Position.ToB2(), true);
        }

        Ball? hoveredBall = GetBallUnderCursor(16);

        if (hoveredBall != null)
        {
            List<Ball> mergeBalls = MergeBalls(hoveredBall, out bool isBomb);

            if (Raylib.IsKeyPressed(KeyboardKey.B))
            {
                hoveredBall.BallType = Ball.Type.Bomb;
            }

            if (hoveredBall.BallType == Ball.Type.Bomb)
            {
                if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                {
                    foreach (Ball ball in _balls)
                    {
                        if (Raylib.CheckCollisionPointCircle(ball.Position, hoveredBall.Position, 12f))
                        {
                            _ballsToDestroy.Add(ball);
                        }
                    }
                    
                    // B2ExplosionDef explode = b2DefaultExplosionDef();
                    // explode.position = hoveredBall.Position.ToB2();
                    // explode.radius = 15;
                    // explode.impulsePerLength = 75;
                    //
                    // b2World_Explode(WorldId, explode);
                    
                    // _ballsToDestroy.Add(hoveredBall);
                }
            }
            else if (mergeBalls.Count < 4) {} // Don't clear normal groups less than 4
            else if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                _ballsToDestroy.AddRange(mergeBalls);
                if (isBomb)
                {
                    MakeBall(mergeBalls[0].Position, Ball.Type.Bomb);
                }
            }
            else
            {
                Texture2D glow = Resources.Sprites["graysan"];
                Color col = mergeBalls.Count > _combo ? Color.White : Color.Black;
                col = Raylib.ColorAlpha(col, 0.25f);
                foreach (Ball ball in mergeBalls)
                {
                    Vector2 pos = b2Body_GetWorldCenterOfMass(ball.Body).ToVec2() * 10;
                    Raylib.DrawTexturePro(glow, glow.Rect(), new Rectangle(pos, glow.Dimensions/2), glow.Dimensions/4, 0, col);
                }
            }
        }

        if (_ballsToDestroy.Count > 0)
        {
            _lastClear = Time.Scaled;
            if (hoveredBall?.BallType == Ball.Type.Bomb || _ballsToDestroy.Count > _combo)
            {
                _combo++;
            }
            else
            {
                _combo = 1;
            }
            Raylib.PlaySound(Resources.Sounds[$"match{Math.Clamp(_combo, 1, 14)}"]);
            int mergeScore = _ballsToDestroy.Count * _combo * 10;
            _score += mergeScore;
            _scoreAnims.Add(new ScoreAnim($"{_ballsToDestroy.Count}0x{_combo} = {mergeScore}", Raylib.GetMousePosition(), new List<Ball>(_ballsToDestroy)));
        }

        if (_combo > 1 && Time.Scaled - _lastClear > 3f)
        {
            _combo = 0;
        }

        foreach (Ball ball in _ballsToDestroy)
        {
            b2DestroyBody(ball.Body);
            _balls.Remove(ball);
        }
        _ballsToDestroy.Clear();

        foreach (ScoreAnim scoreAnim in _scoreAnims)
        {
            scoreAnim.Draw();
        }

        _scoreAnims.RemoveAll(s => s.IsComplete());

        if (_displayedScore < _score - 6000) _displayedScore = _score - 6000;
        if (_displayedScore < _score) _displayedScore += 10;
        ImGui.DrawTextRadial(0, -280, $"Score: {_displayedScore}");
        ImGui.DrawTextRadial(0, -240, $"Balls: {_balls.Count}");
        if (_combo > 1)
        {
            ImGui.DrawTextRadial(0, 280, $"{_combo}x Combo!");
            int comboPercentage = (int)Math.Clamp((Time.Scaled - _lastClear) * 10f, 0, 29);
            ImGui.DrawTextRadial(0, 240, "..............................".Substring(comboPercentage));
        }
        
        if (Raylib.IsKeyDown(KeyboardKey.V))
            b2World_Draw(WorldId, _debugDraw);
    }

    private Ball? GetBallUnderCursor(float maxDistance)
    {
        maxDistance /= 10;
        KeyValuePair<Ball?, float> nearestBall = new KeyValuePair<Ball, float>(null, maxDistance);
        Vector2 cursorPos = Raylib.GetMousePosition() / 10f;
        foreach (Ball ball in _balls)
        {
            B2Vec2 pos = b2Body_GetWorldCenterOfMass(ball.Body);
            float dist = Vector2.Distance(pos.ToVec2(), cursorPos);
            if (dist < nearestBall.Value)
            {
                nearestBall = new KeyValuePair<Ball?, float>(ball, dist);
            }
        }
        return nearestBall.Key;
    }
    
    private List<Ball> MergeBalls(Ball origin, out bool isBomb)
    {
        List<Ball> ballsToMerge = new List<Ball>();
        List<Ball> ballsToCheck = new List<Ball>() {origin};
        bool first = true;
        isBomb = false;
        
        while (ballsToCheck.Count > 0)
        {
            Vector2 pos = b2Body_GetWorldCenterOfMass(ballsToCheck[0].Body).ToVec2();
            foreach (Ball ball in _balls)
            {
                if (ball.BallType == ballsToCheck[0].BallType &&
                    !ballsToMerge.Contains(ball) &&
                    Raylib.CheckCollisionCircles(b2Body_GetWorldCenterOfMass(ball.Body).ToVec2(), 1.6f, pos, 1.6f))
                {
                    ballsToMerge.Add(ball);
                    ballsToCheck.Add(ball);
                }
            }
            ballsToCheck.RemoveAt(0);
            if (first && ballsToMerge.Count >= 7)
            {
                isBomb = true;
                return ballsToMerge;
            }

            first = false;
        }

        return ballsToMerge;
    }
    
    public void Destroy()
    {
        b2DestroyWorld(WorldId);
    }

    private void MakeBall(Vector2 pos, Ball.Type ballType)
    {
        B2BodyDef bodyDef = b2DefaultBodyDef();
        bodyDef.type  = B2BodyType.b2_dynamicBody;
        bodyDef.position = new B2Vec2(0, 0);
        bodyDef.linearDamping = 0.05f;
        B2BodyId bodyId = b2CreateBody(WorldId, bodyDef);
        
        B2Circle dynamicCircle;
        dynamicCircle.center = pos.ToB2();
        dynamicCircle.radius = 1.4f;
        
        B2ShapeDef shapeDef = b2DefaultShapeDef();
        shapeDef.density = 1.0f;
        shapeDef.material.friction = 0.3f;
        shapeDef.material.restitution = 0.75f;
        
        b2CreateCircleShape(bodyId, shapeDef, dynamicCircle);
        _balls.Add(new Ball(bodyId, ballType));
    }
    
    public class Ball
    {
        public B2BodyId Body;
        public Type BallType;
        public Vector2 Position;
        
        public float Rotation => b2Rot_GetAngle(b2Body_GetRotation(Body)) * 180 / MathF.PI;
        
        private static readonly Type[] DefaultTypes = { Type.Red, Type.Blue, Type.Yellow, Type.Purple };
        public static Type RandomDefaultType() => DefaultTypes[Random.Shared.Next(DefaultTypes.Length)];

        public enum Type
        {
            Red,
            Blue,
            Yellow,
            Purple,
            Bomb
        }

        public Ball(B2BodyId body, Type ballType)
        {
            Body = body;
            BallType = ballType;
        }

        public void Draw()
        {
            Position = b2Body_GetWorldCenterOfMass(Body).ToVec2();
            Texture2D tex = Resources.Sprites["graysan"];
            Color col = Color.Pink;
            switch (BallType)
            {
                case Type.Red:
                    tex = Resources.Sprites["graysan"];
                    col = Color.Red;
                    break;
                case Type.Blue:
                    tex = Resources.Sprites["graysan"];
                    col = Color.SkyBlue;
                    break;
                case Type.Yellow:
                    tex = Resources.Sprites["graysan"];
                    col = Color.Yellow;
                    break;
                case Type.Purple:
                    tex = Resources.Sprites["graysan"];
                    col = Color.Purple;
                    break;
                case Type.Bomb:
                    tex = Resources.Sprites["graysan"];
                    col = Color.DarkGray;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            Raylib.DrawTexturePro(tex, tex.Rect(), new Rectangle(Position * 10, tex.Dimensions/2), tex.Dimensions/4, Rotation, col);
        }
    }
    
    public class ScoreAnim
    {
        private string _text;
        private AnimFloat _anim;
        private Vector2 _pos;
        private List<Ball> _balls;

        public ScoreAnim(string text, Vector2 pos, List<Ball> balls)
        {
            _text = text;
            _pos = pos;
            _balls = balls;
            _anim = new AnimFloat(0, 1, 2);
        }

        public void Draw()
        {
            ImGui.DrawText(_text, _pos + new Vector2(0, -48) * _anim.Sample(), color: Raylib.ColorAlpha(Color.White, Easings.OutQuint(1 - _anim.Sample())));
        }

        public bool IsComplete() => _anim.IsComplete();
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