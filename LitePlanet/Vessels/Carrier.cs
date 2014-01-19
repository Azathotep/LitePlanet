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
        static Texture _texture = new Texture("rocketship");
        static Texture _redTexture = new Texture("redship");
        static Texture _circleTexture = new Texture("circleOverlay");
        
        bool _hostile;
        public Carrier(Engine engine) : base(engine)
        {
            
        }

        protected override Body CreateBody()
        {
            Body body = _engine.Physics.CreateBody(this);
            body.BodyType = BodyType.Dynamic;
            body.AngularDamping = 0.5f;
            body.Friction = 0f;
            body.Restitution = 1.1f;
            body.Mass = 0.5f;
            body.Rotation = 0f;
            body.LinearVelocity = new Vector2(0, 5);
            FixtureFactory.AttachPolygon(new Vertices(new Vector2[] { new Vector2(0f, -0.4f), new Vector2(0.35f, 0.4f), new Vector2(-0.35f, 0.4f) }), 1f, body);
            FixtureFactory.AttachPolygon(new Vertices(new Vector2[] { new Vector2(0f, 1.2f), new Vector2(0.35f, 0.4f), new Vector2(-0.35f, 0.4f) }), 1f, body);
            body.CollisionCategories = Category.Cat2;
            body.CollidesWith = Category.Cat1 | Category.Cat2;
            return body;
        }

        public override void Draw(XnaRenderer renderer)
        {
            if (_hull <= 0)
                return;
            Texture texture = _texture;
            if (_hostile)
                texture = _redTexture;
            renderer.DrawSprite(texture, Position, new Vector2(1f, 1f), Rotation);
            renderer.DrawSprite(texture, Position - Facing * 0.8f, new Vector2(1f, 1f), Rotation + (float)Math.PI);
        }
    }
}
