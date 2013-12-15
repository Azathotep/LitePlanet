using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using LiteEngine.Rendering;
using LiteEngine.Textures;
using LiteEngine.Math;

namespace LitePlanet.Vessels
{
    class Ship : IShip
    {
        Texture _texture = new Texture("rocketship");

        public Vector2 Position
        {
            get 
            {
                return new Vector2(0, 0);
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
            throw new NotImplementedException();
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
