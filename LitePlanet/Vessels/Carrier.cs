using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using LiteEngine.Core;
using LiteEngine.Rendering;
using LiteEngine.Textures;
using LiteEngine.Math;
using LiteEngine.Physics;
using LiteEngine.Particles;
using FarseerPhysics.Common;
using FarseerPhysics.Factories;
using FarseerPhysics.Dynamics;
using LitePlanet.Effects;
using LitePlanet.Weapons;
using LitePlanet.Projectiles;
using LitePlanet.Worlds;

namespace LitePlanet.Vessels
{
    class Carrier : Ship
    {
        static Texture _texture = new Texture("largeship", new RectangleI(128-38,0, 38, 128));
        static Texture _redTexture = new Texture("redship");
        static Texture _circleTexture = new Texture("circleOverlay");
        Cannon _cannon;
        Turret[] _turrets = new Turret[3];
        public Carrier(Engine engine) : base(engine)
        {
            _cannon = new Cannon();
            //Body.Mass = 10;
            _hull = 10000;
            _maxSpeed = 20;

            for (int i = 0; i < 3; i++)
            {
                Vector2 turretPosition = new Vector2(0f, (i - 1) * 3.5f);
                _turrets[i] = new Turret(this, turretPosition);
            }

            _engineMaxThrust = 10f;
        }

        protected override Body CreateBody()
        {
            Body body = _engine.Physics.CreateBody(this);
            body.BodyType = BodyType.Dynamic;
            body.AngularDamping = 0.5f;
            body.Friction = 0f;
            body.Restitution = 1.1f;
            body.Mass = 10f;
            body.Rotation = 0f;
            body.LinearVelocity = new Vector2(0, 0);


            FixtureFactory.AttachPolygon(new Vertices(new Vector2[] { new Vector2(-1.5f, -5f), new Vector2(1.5f, -6f), new Vector2(1.5f, 5f), new Vector2(-1.5f, 6f) }), 1f, body);

            //FixtureFactory.AttachPolygon(new Vertices(new Vector2[] { new Vector2(0f, -0.4f), new Vector2(0.35f, 0.4f), new Vector2(-0.35f, 0.4f) }), 1f, body);
            //FixtureFactory.AttachPolygon(new Vertices(new Vector2[] { new Vector2(0f, 1.2f), new Vector2(0.35f, 0.4f), new Vector2(-0.35f, 0.4f) }), 1f, body);
            body.CollisionCategories = Category.Cat2;
            body.CollidesWith = Category.Cat1 | Category.Cat2;
            return body;
        }

        float[] _turretRot = new float[3];
        int[] _turretSpinDir = { 1, 1, 1 };
        int f = 0;
        public override void Draw(XnaRenderer renderer)
        {
            if (_hull <= 0)
                return;

            Texture texture = _texture;
            renderer.DrawDepth = 0.5f;
            renderer.DrawSprite(texture, Position, new Vector2(3f, 12f), Rotation);
            renderer.DrawDepth = 0.2f;

            f++;
            foreach (Turret turret in _turrets)
            {
                turret.Draw(renderer);
            }

            float power = _enginePercent;
            Texture t = new Texture("particle");
            Color c = Color.Cyan;
            Vector2 d = Dice.RandomVector(0.02f);
            c *= 0.4f * power;
            renderer.DrawSprite(t, Position - Facing * 5.5f + d, new Vector2(1.2f, 0.8f), Rotation, c, 0);
            c = Color.White;
            c *= 0.6f * power;
            renderer.DrawSprite(t, Position - Facing * 5.5f + d, new Vector2(1f, 0.4f) * power, Rotation, c, 0);
        }

        public void TurnTurretTowards(Turret turret, Vector2 target)
        {
            float a = Util.AngleBetween(turret.Position, target);
            float angle = Util.AngleBetween(turret.Rotation, a);
            if (angle > 0.1f)
                turret.Rotate(0.05f);
            else if (angle < -0.1f)
                turret.Rotate(-0.05f);
            
            if (Math.Abs(angle) < 0.2f && Dice.Next(20) == 0 && Vector2.Distance(Position, target) < 40f)
                turret.Fire(_engine);
        }

        internal void Target(Ship ship)
        {
            foreach (Turret t in _turrets)
                TurnTurretTowards(t, ship.Position);
        }
    }

    class Turret
    {
        static Texture _turretTexture = new Texture("largeship", new RectangleI(0, 0, 32, 32));
        Cannon _cannon = new Cannon();
        Ship _owner;
        float _relativeRotation;
        Vector2 _relativePosition;

        public Turret(Ship owner, Vector2 relativePosition)
        {
            _owner = owner;
            _relativePosition = relativePosition;
        }

        public Vector2 Position
        {
            get
            {
                Vector2 ret = _owner.Position + Vector2.Transform(_relativePosition, Matrix.CreateRotationZ(_owner.Rotation));
                return ret;
            }
        }

        public float Rotation
        {
            get
            {
                return _relativeRotation + _owner.Rotation;
            }
        }

        public void Rotate(float amount)
        {
            _relativeRotation += amount;
        }

        public void Draw(XnaRenderer renderer)
        {
            float totalRot = Rotation;
            renderer.DrawSprite(_turretTexture, Position, new Vector2(2f, 2f), totalRot);
        }

        internal void Fire(Engine engine)
        {
            float totalRot = Rotation;
            _cannon.Fire(engine, _owner, Position, new Vector2((float)Math.Sin(totalRot), (float)-Math.Cos(totalRot)));
        }
    }
}
