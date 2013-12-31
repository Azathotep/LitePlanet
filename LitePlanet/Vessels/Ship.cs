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
namespace LitePlanet.Vessels
{
    class Ship : IPhysicsObject, IDamageSink
    {
        Cannon _cannon;
        Body _body;
        static Texture _texture = new Texture("rocketship");
        static Texture _redTexture = new Texture("redship");
        static Texture _circleTexture = new Texture("circleOverlay");
        
        Engine _engine;
        bool _hostile;
        public Ship(Engine engine, bool other=false)
        {
            _engine = engine;
            _body = engine.Physics.CreateBody(this);
            _body.BodyType = BodyType.Dynamic;
            _body.AngularDamping = 0.5f;
            _body.Friction = 0f;
            _body.Restitution = 1.1f;
            _body.Mass = 0.5f;
            _body.Rotation = 0f;
            _body.LinearVelocity = new Vector2(0, 5);
            FixtureFactory.AttachPolygon(new Vertices(new Vector2[] { new Vector2(0f, -0.4f), new Vector2(0.35f, 0.4f), new Vector2(-0.35f, 0.4f) }), 1f, _body);

            _body.CollisionCategories = Category.Cat2;
            _body.CollidesWith = Category.Cat1 | Category.Cat2;
            
            _cannon = new Cannon();

            if (other)
                _hostile = true;
            if (_hostile)
                _hull = 10000;
        }

        public Body Body
        {
            get
            {
                return _body;
            }
        }

        public Cannon PrimaryWeapon
        {
            get
            {
                return _cannon;
            }
        }


        int _hull = 2000;
        public int Hull
        {
            get
            {
                return _hull;
            }
        }

        int _fuel = 5500;
        public int Fuel
        {
            get
            {
                return _fuel;
            }
        }

        public Vector2 Position
        {
            get 
            {
                return _body.Position;
            }
            set
            {
                _body.Position = value;
            }
        }

        public Vector2 Velocity
        {
            get 
            {
                return _body.LinearVelocity;
            }
        }

        public Vector2 Facing
        {
            get 
            {
                return _body.GetWorldVector(new Vector2(0, -1));
            }
        }

        public float Rotation
        {
            get 
            {
                return _body.Rotation;
            }
            set
            {
                _body.Rotation = value;
            }
        }

        int s = 0;
        public void ApplyForwardThrust(float amount)
        {
            if (_fuel <= 0)
                return;
            _fuel--;
            _body.ApplyForce(Facing * amount);
            float len = _body.LinearVelocity.LengthSquared();

            float maxSpeed = 180;
            if (_hostile)
                maxSpeed = 120;
            if (len > maxSpeed)
                _body.LinearVelocity *= maxSpeed / len;
            int max = 0;
            if (s > 3)
            {
                s = 0;
                max = 1;
            }
            s++;
            for (int i = 0; i < max; i++)
            {
                Vector2 vel = Velocity - Facing * 5.1f;
                vel.X += Dice.Next() * 1.6f - 0.8f;
                vel.Y += Dice.Next() * 1.6f - 0.8f;
                Particle exhaust = _engine.ExhaustParticles.CreateParticle(Position, vel, 50);
                if (exhaust == null)
                    break;
                exhaust.Body.CollidesWith = Category.Cat1;
                exhaust.Body.CollisionCategories = Category.Cat6;
                Vector2 p = Position - Facing * 0.7f + Dice.RandomVector(0.3f);
                _engine.SmokeParticles.CreateParticle(p, Velocity * 0, 50);
            }
        }

        public void ApplyRotateThrust(float amount)
        {
            _body.ApplyTorque(amount);
        }

        public void Draw(XnaRenderer renderer)
        {
            if (_hull <= 0)
                return;
            Texture texture = _texture;
            if (_hostile)
                texture = _redTexture;
            renderer.DrawSprite(texture, new RectangleF(Position.X, Position.Y, 1f, 1f), Rotation);

            if (renderer.Camera.Zoom > 3)
            {
                float width = renderer.Camera.Zoom;

                Color c = Color.FromNonPremultiplied(0, 255, 0, 255);
                if (_hostile)
                    c = Color.FromNonPremultiplied(255, 0, 0, 255);
                renderer.DrawSprite(_circleTexture, new RectangleF(Position.X, Position.Y, width, width), 0, c, 1f);

                Vector2 front = Position + Facing * 2;
                renderer.DrawSprite(_circleTexture, new RectangleF(front.X, front.Y - 0.5f, width * 0.2f, width * 2), Rotation, c, 1f);
            }
        }

        public void OnCollideWith(IPhysicsObject self, IPhysicsObject other, float impulse)
        {
            TakeDamage((int)Math.Pow(impulse * 2, 2));

            //IDamageSink sink = other as IDamageSink;
            //sink.TakeDamage((int)impulse * 2);
        }

        public void TakeDamage(int damageAmount)
        {
            _hull -= damageAmount;
            if (_hull < 0)
            {
                _hull = 0;
                _fuel = 0;
                Explosion explosion = new Explosion(_engine);
                explosion.Create(Position);
                //_engine.Physics.RemoveBody(_body);
            }
        }
    }
}
