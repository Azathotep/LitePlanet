using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using FarseerPhysics.Dynamics;
using LiteEngine.Rendering;
using LiteEngine.Physics;
using LiteEngine.Textures;
using LiteEngine.Math;

namespace LitePlanet.Worlds
{
    class Building
    {
        Body _body;
        float _width;
        float _height;
        static Texture _texture = new Texture("building");
        public Building(Engine engine, Vector2 position, float width, float height)
        {
            _body = engine.Physics.CreateRectangleBody(null, width, height, 1f);
            _body.IsStatic = true;
            _body.Restitution = 0.3f;
            _body.Friction = 1f;
            _body.Rotation = 0;
            _body.Position = position;
            _body.CollisionCategories = Category.Cat1;
            _body.CollidesWith = Category.All;
            _width = width;
            _height = height;
        }


        public void Draw(XnaRenderer renderer)
        {
            renderer.DrawSprite(_texture, new RectangleF(_body.Position.X, _body.Position.Y, _width, _height), 0);
        }
    }
}
