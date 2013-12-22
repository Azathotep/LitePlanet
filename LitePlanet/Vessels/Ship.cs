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
        Texture _texture = new Texture("rocketship");
        Engine _engine;
        public Ship(Engine engine, bool other=false)
        {
            _engine = engine;
            _body = engine.Physics.CreateBody(this);
            _body.BodyType = BodyType.Dynamic;
            _body.AngularDamping = 0.5f;
            _body.Friction = 1f;
            _body.Restitution = 0f;
            _body.Mass = 0.5f;
            _body.Rotation = 0f;
            _body.LinearVelocity = new Vector2(0, 5);
            FixtureFactory.AttachPolygon(new Vertices(new Vector2[] { new Vector2(0f, -0.4f), new Vector2(0.35f, 0.4f), new Vector2(-0.35f, 0.4f) }), 1f, _body);

            _body.CollisionCategories = Category.Cat2;
            _body.CollidesWith = Category.Cat1 | Category.Cat2;
            
            _cannon = new Cannon();
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


        int _hull = 100;
        public int Hull
        {
            get
            {
                return _hull;
            }
        }

        int _fuel = 2500;
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
        }

        public void ApplyForwardThrust(float amount)
        {
            if (_fuel <= 0)
                return;
            _fuel--;
            _body.ApplyForce(Facing * amount);
        }

        public void ApplyRotateThrust(float amount)
        {
            _body.ApplyTorque(amount);
        }

        public void Draw(XnaRenderer renderer)
        {
            if (_hull <= 0)
                return;
            renderer.DrawSprite(_texture, new RectangleF(Position.X, Position.Y, 1f, 1f), Rotation);
        }

        public void OnCollideWith(IPhysicsObject self, IPhysicsObject other, float impulse)
        {
            TakeDamage((int)Math.Pow(impulse * 2, 2));
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
            }
        }
    }
}
