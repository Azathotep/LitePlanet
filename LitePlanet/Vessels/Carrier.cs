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
        static Texture _turretTexture = new Texture("largeship", new RectangleI(0, 0, 32, 32));
        static Texture _redTexture = new Texture("redship");
        static Texture _circleTexture = new Texture("circleOverlay");
        Cannon _cannon;
        public Carrier(Engine engine) : base(engine)
        {
            _cannon = new Cannon();
            //Body.Mass = 10;
            _hull = 10000;
            _maxSpeed = 20;
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

            for (int i = 0; i < 3; i++)
            {
                _turretRot[i] += 0.02f * _turretSpinDir[i];
                if (Dice.Next(300) == 0)
                    _turretSpinDir[i] *= -1;
            }

            Texture texture = _texture;
            renderer.DrawDepth = 0.5f;
            renderer.DrawSprite(texture, Position, new Vector2(3f, 12f), Rotation);
            renderer.DrawDepth = 0.2f;

            f++;
            for (int i = 0; i < 3; i++)
            {
                float totalRot = Rotation + _turretRot[i];
                Vector2 turretPosition = Position + Vector2.Transform(new Vector2(0f, (i - 1) * 3.5f), Matrix.CreateRotationZ(Rotation));
                renderer.DrawSprite(_turretTexture, turretPosition, new Vector2(2f, 2f), totalRot);
                if (Dice.Next(125) == 0)
                {
                    _cannon.Fire(_engine, this, turretPosition, new Vector2((float)Math.Sin(totalRot), (float)-Math.Cos(totalRot)));
                }
            }
            
            //renderer.DrawSprite(texture, Position - Facing * 0.8f, new Vector2(1f, 1f), Rotation + (float)Math.PI);
        }
    }
}
