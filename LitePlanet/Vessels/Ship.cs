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

namespace LitePlanet.Vessels
{
    class Ship : IShip
    {
        PhysicsObject _object;
        Texture _texture = new Texture("rocketship");
        Engine _engine;
        public Ship(Engine engine)
        {
            _engine = engine;
            _object = engine.Physics.CreateBody();
            _object.Body.BodyType = BodyType.Dynamic;
            _object.Body.AngularDamping = 0.5f;
            _object.Body.Friction = 1f;
            _object.Body.Restitution = 0f;
            _object.Body.Mass = 0.5f;
            _object.Body.Rotation = 0f;
            _object.Body.LinearVelocity = new Vector2(0, 5);
            FixtureFactory.AttachPolygon(new Vertices(new Vector2[] { new Vector2(0f, -0.4f), new Vector2(0.35f, 0.4f), new Vector2(-0.35f, 0.4f) }), 1f, _object.Body);
            _object.Body.CollisionCategories = Category.Cat2;
            _object.Body.CollidesWith = Category.Cat1;
            _object.SetCollisionCallback(OnCollision);
        }

        void OnCollision(float impulse)
        {
            _hull -= (int)Math.Pow(impulse*4, 2);
            if (_hull < 0)
            {
                _hull = 0;
                _fuel = 0;
                for (int i=0;i<40;i++)
                    _engine.SmokeParticles.CreateParticle(Position + Dice.RandomVector(0.1f), Dice.RandomVector(1), 80, true);
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
                return _object.Body.Position;
            }
        }

        public Vector2 Velocity
        {
            get 
            {
                return _object.Body.LinearVelocity;
            }
        }

        public Vector2 Facing
        {
            get 
            {
                return _object.Body.GetWorldVector(new Vector2(0, -1));
            }
        }

        public float Rotation
        {
            get 
            {
                return _object.Body.Rotation;
            }
        }

        public void ApplyForwardThrust(float amount)
        {
            if (_fuel <= 0)
                return;
            _fuel--;
            _object.Body.ApplyForce(Facing * amount);
        }

        public void ApplyRotateThrust(float amount)
        {
            _object.Body.ApplyTorque(amount);
        }

        public void Draw(XnaRenderer renderer)
        {
            if (_hull <= 0)
                return;
            renderer.DrawSprite(_texture, new RectangleF(Position.X, Position.Y, 1f, 1f), Rotation);
        }
    }
}
