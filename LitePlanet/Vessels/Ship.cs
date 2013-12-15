using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using LiteEngine.Rendering;
using LiteEngine.Textures;
using LiteEngine.Math;
using FarseerPhysics.Common;
using FarseerPhysics.Factories;
using FarseerPhysics.Dynamics;

namespace LitePlanet.Vessels
{
    class Ship : IShip
    {
        Body _body;
        Texture _texture = new Texture("rocketship");

        public Ship(Engine engine)
        {
            _body = engine.Physics.CreateBody();
            _body.BodyType = BodyType.Dynamic;
            _body.AngularDamping = 0.5f;
            _body.Friction = 1f;
            _body.Restitution = 0f;
            _body.Mass = 0.5f;
            _body.Rotation = 0f;
            _body.LinearVelocity = new Vector2(0, 5);
            FixtureFactory.AttachPolygon(new Vertices(new Vector2[] { new Vector2(0f, -0.4f), new Vector2(0.35f, 0.4f), new Vector2(-0.35f, 0.4f) }), 1f, _body);
            _body.CollisionCategories = Category.Cat2;
            _body.CollidesWith = Category.Cat1;
        }

        int _fuel = 200;
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
                return new Vector2((float)Math.Sin(Rotation), -(float)Math.Cos(Rotation));;
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
            renderer.DrawSprite(_texture, new RectangleF(Position.X, Position.Y, 1f, 1f), Rotation);
        }
    }
}
