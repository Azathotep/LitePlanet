using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using LiteEngine.Physics;
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
            if (Type == WorldTileType.Gold || Type == WorldTileType.Lava || Type == WorldTileType.SolidRock || Type == WorldTileType.Sky)
                return;
            if (Health == 0)
                return;
            Health = 0;
            _planet.UpdateTile(_x, _y, Color.Black);
            _collisionBody.DestroyBody();
        }

        public int Health = 5;

        public bool Visible { get; set; }
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
