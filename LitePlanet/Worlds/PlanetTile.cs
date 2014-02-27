using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using LiteEngine.Core;
using LiteEngine.Physics;
using LiteEngine.Rendering;
using LiteEngine.Textures;
using LitePlanet.Projectiles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LitePlanet.Worlds
{
    public class PlanetTile : IPhysicsObject, IDamageSink
    {
        TileCollisionBody _collisionBody;
        Planet _planet;
        WorldTileType _type;
        int _x, _y;
        Vector2[] _vertices = new Vector2[4];

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="planet">planet the tile belongs to</param>
        /// <param name="x">X coordinate of tile in planet polar coordinates</param>
        /// <param name="y">Y coordinate of tile in planet polar coordinates</param>
        /// <param name="type"></param>
        public PlanetTile(Planet planet, int x, int y, WorldTileType type)
        {
            _planet = planet;
            _x = x;
            _y = y;
            _type = type;
            _collisionBody = new TileCollisionBody(this);

            //generate the vertices of the tile's corners
            if (planet != null)
            {
                _vertices[0] = planet.PolarToCartesian(new Vector2(x, y));
                _vertices[1] = planet.PolarToCartesian(new Vector2(x, y + 1));
                _vertices[2] = planet.PolarToCartesian(new Vector2(x + 1, y));
                _vertices[3] = planet.PolarToCartesian(new Vector2(x + 1, y + 1));
            }

            if (_type == WorldTileType.Gold)
                Health = 50;
        }

        public int X
        {
            get
            {
                return _x;
            }
        }

        public int Y
        {
            get
            {
                return _y;
            }
        }

        public Body Body
        {
            get
            {
                return _collisionBody.Body;
            }
        }

        public Vector2[] Vertices
        {
            get
            {
                return _vertices;
            }
        }

        public TileCollisionBody CollisionBody
        {
            get
            {
                return _collisionBody;
            }
        }

        public Planet Planet
        {
            get
            {
                return _planet;
            }
        }

        public WorldTileType Type
        {
            get
            {
                return _type;
            }
        }

        public void OnCollideWith(IPhysicsObject self, IPhysicsObject other, float impulse)
        {

        }

        public void TakeDamage(int damageAmount)
        {
            if (damageAmount < 1)
                return;
            if (Type == WorldTileType.Lava || Type == WorldTileType.SolidRock || Type == WorldTileType.Sky)
                return;
            if (Health == 0)
                return;
            Health = 0;
            _planet.UpdateTile(this);
            _planet.CommitChanges();
            _collisionBody.DestroyBody();

            if (Type == WorldTileType.Gold)
            {
                _planet.DropItem(_planet.PolarToCartesian(new Vector2(_x, _y)));
            }
        }

        public int Health = 5;

        public bool Visible { get; set; }
    }

    public class Item : IPhysicsObject
    {
        Body _body;
        Planet _planet;
        public Item(Planet planet, PhysicsCore physics, Vector2 position)
        {
            _planet = planet;
            _body = physics.CreateBody(this);

            _body.Enabled = false;
            
            Fixture f = FixtureFactory.AttachCircle(0.3f, 1f, _body, Vector2.Zero);
            _body.BodyType = BodyType.Dynamic;
            _body.Mass = 0.0001f;
            _body.CollisionCategories = Category.Cat3;
            _body.CollidesWith = Category.Cat1 | Category.Cat2;
            _body.Position = position;
            _body.LinearVelocity = Dice.RandomVector(0.1f);
            _body.Enabled = true;
        }

        public void Draw(XnaRenderer renderer)
        {
            Texture texture = new Texture("particle");
            renderer.DrawSprite(texture, _body.Position, new Vector2(0.3f, 0.3f), 0, Color.Yellow, 0.8f); 
        }

        public Body Body
        {
            get { return _body; }
        }

        public void OnCollideWith(IPhysicsObject self, IPhysicsObject other, float impulse)
        {
            //throw new NotImplementedException();
        }

        internal void Remove()
        {
            _planet.Physics.RemoveBody(_body);
            _planet.RemoveItem(this);
        }
    }

    /// <summary>
    /// Manages the farseer static collision body object for a tile.
    /// </summary>
    public class TileCollisionBody
    {
        Body _body;
        PlanetTile _tile;

        public TileCollisionBody(PlanetTile tile)
        {
            _tile = tile;
        }

        void CreateBody()
        {
            if (_tile.Planet == null)
                return;
            PhysicsCore physics = _tile.Planet.Physics;
            _body = physics.CreateBody(_tile);
            _body.Restitution = -0.5f;
            _body.Friction = 0f;
            _body.IsStatic = true;
            Vertices v = new Vertices(_tile.Vertices);
            FixtureFactory.AttachPolygon(v, 1, _body, null);
        }

        public void DestroyBody()
        {
            if (_body != null)
            {
                _tile.Planet.Physics.RemoveBody(_body);
                _body = null;
            }
        }

        public int _useCount = 0;
        public void BeginUse()
        {
            if (_tile.Health <= 0)
                return;
            _useCount++;
            if (_useCount == 1)
                CreateBody();
        }

        public void EndUse()
        {
            _useCount--;
            if (_useCount == 0)
                DestroyBody();
        }

        public Body Body
        {
            get
            {
                return _body;
            }
        }
    }
}
