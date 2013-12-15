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
            FixtureFactory.AttachPolygon(new Vertices(new Vector2[] { new Vector2(0f, -0.4f), new Vector2(0.35f, 0.4f), new Vector2(-0.35f, 0.4f) }), 1f, _body);
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
            get { throw new NotImplementedException(); }
        }

        public Vector2 Facing
        {
            get { throw new NotImplementedException(); }
        }

        public float Rotation
        {
            get 
            {
                return 0;
            }
        }

        public void ApplyForwardThrust(float amount)
        {
            _body.ApplyForce(new Vector2(0, 1f) * amount);
        }

        public void ApplyRotateThrust(float amount)
        {
            throw new NotImplementedException();
        }

        public void Draw(XnaRenderer renderer)
        {
            renderer.DrawSprite(_texture, new RectangleF(Position.X, Position.Y, 1f, 1f), Rotation);
        }
    }
}
