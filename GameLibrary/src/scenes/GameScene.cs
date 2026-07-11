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
    float timeStep = 1.0f / 60.0f;
    int subStepCount = 4;
    
    private List<Ball> _balls = new List<Ball>();
    
    private int _score;
    private int _displayedScore;
    private int _targetScore = 20000;
    private int _combo;
    private float _lastClear;
    private List<ScoreAnim> _scoreAnims = new List<ScoreAnim>();
    private AnimCurve<float> _drainAnim = new AnimCurve<float>(1);

    private State _state = State.Gaming;
    
    enum State
    {
        Gaming,
        Draining
    }
    
    public GameScene()
    {
        B2WorldDef worldDef = b2DefaultWorldDef();
        
        worldDef.gravity = new B2Vec2(0.0f, 0.0f);
        worldDef.hitEventThreshold = 15f;
        WorldId = b2CreateWorld(worldDef);
    }
    
    public override void Update()
    {
        Raylib.ClearBackground(Color.DarkBlue);
        
        {
            float height = 720 - (float)_displayedScore / _targetScore * 720;
            Color col = Raylib.ColorAlpha(Color.SkyBlue, 128);
            Resources.Sprites["charge_meter"].Draw(0, height, 720, 720, tint: col);
        }

        if (_state == State.Draining)
        {
            BackgroundDraw.Web(1 - _drainAnim.Sample());
            BackgroundDraw.Spiral(_drainAnim.Sample());
        }
        else
        {
            BackgroundDraw.Web();
        }
        
        
        if (Raylib.IsKeyDown(KeyboardKey.A)) Raylib.ClearBackground(new Color(32, 32, 32, 255));
        
        BackgroundDraw.CirclePulse(Math.Max(0, Game.MusicPlaying.Beat() / 4 - 0.5f) % 1);
        BackgroundDraw.CirclePulse(Math.Max(0, Game.MusicPlaying.Beat() / 4 - 0.0f) % 1);
        BackgroundDraw.Waveform2();

        int ballFreq = _combo >= 2 && _balls.Count < 200 ? 5 : 10;
        if (_state == State.Gaming && (Time.Frame % ballFreq == 0 || _balls.Count < 60 || Raylib.IsKeyDown(KeyboardKey.L)))
        {
            Vector2 pos = Vector2.Normalize(Utils.RandomInsideUnitCircle(Random.Shared)) * 32f + new Vector2(36, 36);
            MakeBall(pos, Ball.RandomDefaultType());
        }
        
        b2World_Step(WorldId, timeStep, subStepCount);

        B2ContactEvents contactEvents = b2World_GetContactEvents(WorldId);

        foreach (Ball ball in _balls)
        {
            ball.Draw();
            
            Vector2 delta = new Vector2(36f, 36f) - ball.Position;
            Vector2 g = Vector2.Normalize(delta) * 100;
            b2Body_ApplyForce(ball.Body, g.ToB2(), ball.Position.ToB2(), true);
        }
        
        for (var i = 0; i < contactEvents.hitCount; i++)
        {
            var contact = contactEvents.hitEvents[i];
            // Raylib.DrawCircleV(contact.point.ToVec2() * 10, 3, new Color(255, 0, 0, 192));
            float volume = 0.3f;
            if      (contact.approachSpeed > 40) { Resources.Sounds["collision1"].PlayRandomPitch(1 - (contact.point.ToVec2().X / 72), 1f, 1f   * volume); }
            else if (contact.approachSpeed > 35) { Resources.Sounds["collision2"].PlayRandomPitch(1 - (contact.point.ToVec2().X / 72), 1f, 0.9f * volume); }
            else if (contact.approachSpeed > 30) { Resources.Sounds["collision3"].PlayRandomPitch(1 - (contact.point.ToVec2().X / 72), 1f, 0.8f * volume); }
            else if (contact.approachSpeed > 25) { Resources.Sounds["collision4"].PlayRandomPitch(1 - (contact.point.ToVec2().X / 72), 1f, 0.7f * volume); }
            else if (contact.approachSpeed > 20) { Resources.Sounds["collision5"].PlayRandomPitch(1 - (contact.point.ToVec2().X / 72), 1f, 0.6f * volume); }
            else                                 { Resources.Sounds["collision6"].PlayRandomPitch(1 - (contact.point.ToVec2().X / 72), 1f, 0.5f * volume); }
            // Console.WriteLine($"BALLS COLLIDING with speed:{contact.approachSpeed:N5} at: {contact.point.ToVec2()}");
        }

        if (_state == State.Gaming)
        {
            HandleInput();
            
            if (_combo > 1 && Time.Scaled - _lastClear > 3f)
            {
                EndCombo();
            }
        
            if (_balls.Count > 350 && Time.Scaled % 1f < 0.5f)
            {
                Color col = new Color(255, 0, 0, _balls.Count - 300);
                Resources.Sprites["radial_inverted"].Draw(0, 0, 720, 720, tint: col);
                ImGui.DrawTextCentered("Overflow\nImminent", 360, 360, 40);
            }
        
            if (_balls.Count > 400)
            {
                _score = 0;
                Drain();
                Game.ScrollText("Orb Overflow! Charge vented.");
            }

            if (_displayedScore >= _targetScore)
            {
                Drain();
                _targetScore += 10000;
                _displayedScore = _targetScore;
            }
        }
        
        if (_state == State.Draining)
        {
            if (_balls.Count == 0 && _drainAnim.IsComplete())
            {
                if (_drainAnim.Sample() == 0)
                {
                    _state = State.Gaming;
                }
                else
                {
                    _drainAnim = AnimCurve.NewFloat(1, 0, 0.5f, Easings.InQuad);
                }
            }

            
            float size = 200 * _drainAnim.Sample();
            Resources.Sprites["mask"].DrawCentered(360, 360, size, size, tint: Color.Black );
            
            List<Ball> ballsToRemove = new List<Ball>();
            foreach (Ball ball in _balls)
            {
                if (Vector2.Distance(new Vector2(36, 36), ball.Position) < size / 20 - 1)
                {
                    ballsToRemove.Add(ball);
                }
            }
            
            foreach (Ball ball in ballsToRemove)
            {
                b2DestroyBody(ball.Body);
                _balls.Remove(ball);
            }
        }
        
        foreach (ScoreAnim scoreAnim in _scoreAnims)
        {
            scoreAnim.Draw();
        }

        _scoreAnims.RemoveAll(s => s.IsComplete());

        if (_displayedScore < _score - 6000) _displayedScore += 100;
        if (_displayedScore > _score) _displayedScore -= 100;
        if (_displayedScore < _score) _displayedScore += 10;
        if (!Game.IsScrolling()) ImGui.DrawTextRadial(0, -280, $"Score: {_displayedScore}");
        // ImGui.DrawTextRadial(0, -240, $"Balls: {_balls.Count}");
        if (_combo > 1)
        {
            ImGui.DrawTextRadial(0, 280, $"{_combo}x Combo!");
            int comboPercentage = (int)Math.Clamp((Time.Scaled - _lastClear) * 10f, 0, 29);
            ImGui.DrawTextRadial(0, 240, "..............................".Substring(comboPercentage));
        }
        
        if (Raylib.IsKeyDown(KeyboardKey.V))
            b2World_Draw(WorldId, _debugDraw);
    }

    private void HandleInput()
    {
        Ball? hoveredBall = GetBallUnderCursor(16);
        List<Ball> ballsToMerge = new List<Ball>();
        bool makeBomb = false;

        if (hoveredBall != null)
        {
            List<Ball> mergeBalls = MergeBalls(hoveredBall, out makeBomb);

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
                            ballsToMerge.Add(ball);
                        }
                    }
                    
                    B2ExplosionDef explode = b2DefaultExplosionDef();
                    explode.position = hoveredBall.Position.ToB2();
                    explode.radius = 15;
                    explode.impulsePerLength = 75;
                    
                    b2World_Explode(WorldId, explode);
                    
                    Resources.Sounds["bomb"].Play();
                }
            }
            else if (mergeBalls.Count < 4) {} // Don't clear normal groups less than 4
            else if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                ballsToMerge.AddRange(mergeBalls);
                if (makeBomb)
                {
                    MakeBall(hoveredBall.Position, Ball.Type.Bomb);
                }
                
                if (hoveredBall.BallType is Ball.Type.BlueCrystal or Ball.Type.YellowCrystal or Ball.Type.PurpleCrystal or Ball.Type.RedCrystal)
                {
                    Resources.Sounds["match_super"].Play();
                }
            }
            else
            {
                Color col = mergeBalls.Count > _combo ? Color.White : Color.Black;
                col = Raylib.ColorAlpha(col, 0.25f);
                foreach (Ball ball in mergeBalls)
                {
                    Vector2 pos = b2Body_GetWorldCenterOfMass(ball.Body).ToVec2() * 10;
                    Resources.Sprites["mask"].DrawCentered(pos, new Vector2(28, 28), tint: col);
                }
            }
        }

        if (ballsToMerge.Count > 0)
        {
            _lastClear = Time.Scaled;
            if ((hoveredBall?.IsSpecial() ?? false) || makeBomb || ballsToMerge.Count > _combo)
            { }
            else
            {
                EndCombo();
            }

            _combo++;
            Resources.Sounds[$"match{Math.Clamp(_combo, 1, 14)}"].Play();
            int mergeScore = ballsToMerge.Count * _combo * 10;
            _score += mergeScore;
            _scoreAnims.Add(new ScoreAnim($"{ballsToMerge.Count}0x{_combo} = {mergeScore}", Raylib.GetMousePosition(), ballsToMerge));
        }
        
        foreach (Ball ball in ballsToMerge)
        {
            b2DestroyBody(ball.Body);
            _balls.Remove(ball);
        }
    }

    private void EndCombo()
    {
        if (_combo >= 8)
        {
            Resources.Sounds["combo_end_big"].Play();
        }
        else if (_combo >= 4)
        {
            Resources.Sounds["combo_end_small"].Play();
        }
        
        while (_combo >= 8)
        {
            Vector2 pos = Vector2.Normalize(Utils.RandomInsideUnitCircle(Random.Shared)) * 32f + new Vector2(36, 36);
            MakeBall(pos, new []{Ball.Type.BlueCrystal, Ball.Type.PurpleCrystal, Ball.Type.YellowCrystal, Ball.Type.RedCrystal}[Random.Shared.Next(4)]);
            _combo -= 4;
        }
        
        _combo = 0;
    }

    private void Drain()
    {
        EndCombo();
        _score = 0;
        _state = State.Draining;
        _drainAnim = AnimCurve.NewFloat(0, 1, 2, Easings.OutQuad);
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
    
    private List<Ball> MergeBalls(Ball origin, out bool makeBomb)
    {
        makeBomb = false;
        if (origin.BallType == Ball.Type.BlueCrystal)   return _balls.Where(b => b.BallType is Ball.Type.Blue   or Ball.Type.BlueCrystal  ).ToList();
        if (origin.BallType == Ball.Type.YellowCrystal) return _balls.Where(b => b.BallType is Ball.Type.Yellow or Ball.Type.YellowCrystal).ToList();
        if (origin.BallType == Ball.Type.PurpleCrystal) return _balls.Where(b => b.BallType is Ball.Type.Purple or Ball.Type.PurpleCrystal).ToList();
        if (origin.BallType == Ball.Type.RedCrystal)    return _balls.Where(b => b.BallType is Ball.Type.Red    or Ball.Type.RedCrystal   ).ToList();
        
        List<Ball> ballsToMerge = new List<Ball>();
        List<Ball> ballsToCheck = new List<Ball>() {origin};
        bool first = true;
        makeBomb = false;
        
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
                makeBomb = true;
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
        shapeDef.enableHitEvents = true;
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
            Bomb,
            RedCrystal,
            BlueCrystal,
            YellowCrystal,
            PurpleCrystal
        }

        public Ball(B2BodyId body, Type ballType)
        {
            Body = body;
            BallType = ballType;
        }
        
        public bool IsSpecial() => !(BallType is Type.Blue or Type.Purple or Type.Yellow or Type.Red);

        public void Draw()
        {
            Position = b2Body_GetWorldCenterOfMass(Body).ToVec2();
            Vector2 pos = Position * 10;
            Vector2 size = new Vector2(28, 28);
            Sprite baseSprite = Resources.Sprites["orb_base"];
            Sprite? spinPattern = null;
            Sprite specular = Resources.Sprites["orb_shine"];
            Color col = Color.Pink;
            switch (BallType)
            {
                case Type.Red:
                    col = Color.Red;
                    break;
                case Type.Blue:
                    col = Color.SkyBlue;
                    break;
                case Type.Yellow:
                    col = Color.Yellow;
                    break;
                case Type.Purple:
                    col = Color.Purple;
                    break;
                case Type.Bomb:
                    col = Color.DarkGray;
                    break;
                case Type.BlueCrystal:
                    spinPattern = Resources.Sprites["orb_spiral"];
                    col = Color.SkyBlue;
                    break;
                case Type.PurpleCrystal:
                    spinPattern = Resources.Sprites["orb_spiral"];
                    col = Color.Purple;
                    break;
                case Type.RedCrystal:
                    spinPattern = Resources.Sprites["orb_spiral"];
                    col = Color.Red;
                    break;
                case Type.YellowCrystal:
                    spinPattern = Resources.Sprites["orb_spiral"];
                    col = Color.Yellow;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            baseSprite.DrawCentered(pos, size, tint: col);
            if (spinPattern != null) spinPattern.DrawCentered(pos, size, rotation: Rotation, tint: Color.Lerp(col, Color.White, Easings.FullSine((Game.MusicPlaying?.Beat() ?? 0f) % 1f)/4f+0.5f));
            specular.DrawCentered(pos, size, tint: col);
        }

    }
    
    public class ScoreAnim
    {
        private string _text;
        private AnimCurve<float> _anim;
        private Vector2 _pos;
        private List<Ball> _balls;

        public ScoreAnim(string text, Vector2 pos, List<Ball> balls)
        {
            _text = text;
            _pos = pos;
            _balls = balls;
            _anim = AnimCurve.NewFloat(0, 1, 2);
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