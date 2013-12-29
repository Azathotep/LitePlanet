using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using LiteEngine.Rendering;
using LiteEngine.Math;
using LiteEngine.Physics;
using LitePlanet.Projectiles;

namespace LitePlanet.Worlds
{
    public class Planet
    {
        LiteEngine.Textures.Texture _grassTexture = new LiteEngine.Textures.Texture("brownplanet");
        int _width = 100;
        int _height;

        WorldTile[,] _tiles;

        public bool Dirty = true;
        int _tHeight = 100;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="radius">radius of planet in tiles</param>
        public Planet(int radius)
        {
            _width = 1800;
            _height = radius;
            _tiles = new WorldTile[_width, _tHeight];
            for (int y = 0; y < _tHeight; y++)
                for (int x = 0; x < _width; x++)
                {
                    WorldTileType type = WorldTileType.Rock;

                    if (y == _tHeight - 1)
                        type = WorldTileType.Earth;

                    if (y < _tHeight - 1)
                        if (LiteEngine.Core.Dice.Next(10) == 0)
                            type = WorldTileType.SolidRock;

                    if (y < _tHeight - 10)
                        if (LiteEngine.Core.Dice.Next(1500) == 0)
                            type = WorldTileType.Gold;

                    _tiles[x, y] = new WorldTile(this, type);
                }
        }

        public Vector2 CartesianToPolar(Vector2 cCoords)
        {
            float y = (float)Math.Sqrt(cCoords.X * cCoords.X + cCoords.Y * cCoords.Y);
            float angle = (float)Math.Atan2(cCoords.X, -cCoords.Y);// +(float)Math.PI;
            //angle is now between 0 and 2.pi
            float x = angle / (2 * (float)Math.PI) * _width;
            return new Vector2(x, y);
        }

        Vector2 PolarToCartesian(Vector2 polar)
        {
            float angle = 2f * (float)Math.PI * polar.X / _width;
            float radius = polar.Y; // _width / (2f * (float)Math.PI) + polar.Y;
            float px = (float)Math.Sin(angle) * radius;
            float py = -(float)Math.Cos(angle) * radius;
            return new Vector2(px, py);
        }

        public void GenerateVertexBuffer(Engine engine, XnaRenderer renderer, int xMin, int yMin, int width, int height)
        {
            List<VertexPositionColorTexture> vl = new List<VertexPositionColorTexture>();
            for (int py = yMin; py < yMin + height; py++)
                for (int px = xMin; px < xMin + width; px++)
                {
                    int x = (px % _width + _width) % _width;
                    int y = py;
                    WorldTile tile = GetTile(x, y);
                    if (tile == null)
                        continue;
                    //calculate the four corner positions in world space
                    Vector2 c1 = PolarToCartesian(new Vector2(x, y));
                    Vector2 c2 = PolarToCartesian(new Vector2(x, y + 1));
                    Vector2 c3 = PolarToCartesian(new Vector2(x + 1, y));
                    Vector2 c4 = PolarToCartesian(new Vector2(x + 1, y + 1));

                    bool destroyed = tile.Health == 0;
                    bool dirt = y == _height;
                    bool stone = y < _height - 1;
                    bool lava = y < _height - _tHeight;
                    float tx = 0;
                    float ty = 0;
                    float tw = 0.22f;
                    if (destroyed)
                    {
                        ty = 0.26f;
                    }
                    else if (stone)
                    {
                        switch (tile.Type)
                        {
                            case WorldTileType.Earth:
                                tx = 0f;
                                ty = 0f;
                                break;
                            case WorldTileType.Rock:
                                tx = 0.26f;
                                ty = 0f;
                                break;
                            case WorldTileType.Lava:
                                tx = 0.26f;
                                ty = 0.26f;
                                break;
                            case WorldTileType.SolidRock:
                                tx = 0.52f;
                                ty = 0f;
                                break;
                            case WorldTileType.Gold:
                                tx = 0.77f;
                                ty = 0f;
                                break;
                        }
                    }
                    vl.Add(new VertexPositionColorTexture(new Vector3(c1, 0), Color.White, new Vector2(tx, ty + tw)));
                    vl.Add(new VertexPositionColorTexture(new Vector3(c2, 0), Color.White, new Vector2(tx, ty)));
                    vl.Add(new VertexPositionColorTexture(new Vector3(c3, 0), Color.White, new Vector2(tx + tw, ty + tw)));

                    vl.Add(new VertexPositionColorTexture(new Vector3(c3, 0), Color.White, new Vector2(tx + tw, ty + tw)));
                    vl.Add(new VertexPositionColorTexture(new Vector3(c2, 0), Color.White, new Vector2(tx, ty)));
                    vl.Add(new VertexPositionColorTexture(new Vector3(c4, 0), Color.White, new Vector2(tx + tw, ty)));
                }
            _vCount = vl.Count;
            if (vl.Count == 0)
                return;

            if (_vb == null)
                _vb = renderer.CreateVertexBuffer(30000);

            var vertices = vl.ToArray();
            _vCount = vertices.Length;
            _vb.SetData(vertices, 0, _vCount); //vertices.Length);
        }

        int _vCount;


        VertexBuffer _vb;

        public void Draw(XnaRenderer renderer)
        {
            if (_vb != null)
            renderer.DrawUserPrimitives(_vb, _grassTexture, _vCount / 3);
        }

        static WorldTile _lavaTile = new WorldTile(null, WorldTileType.Lava);

        public WorldTile GetTile(int x, int y)
        {
            if (y < 0 || y >= _height)
                return null;
            x = (x % _width + _width) % _width;
            if (y < _height - _tHeight)
                return _lavaTile;
            int ry = y - (_height - _tHeight);
            if (ry < 0 || ry >= _tHeight)
                return null;
            return _tiles[x, ry];
        }

        public IEnumerable<ITile> GetTiles(RectangleF cBounds)
        {
            throw new NotImplementedException();
        }

        internal void GenerateStaticBodies(Engine engine, int xMin, int yMin)
        {
            int width=30;
            int height=30;
            yMin-=15;
            xMin-=15;
            for (int py = yMin; py < yMin + height; py++)
                for (int px = xMin; px < xMin + width; px++)
                {
                    int x = (px % _width + _width) % _width;
                    int y = py;
                    WorldTile tile = GetTile(x, y);
                    if (tile == null)
                        continue;
                    if (tile.Health <= 0)
                        continue;
                    //calculate the four corner positions in world space
                    Vector2 c1 = PolarToCartesian(new Vector2(x, y));
                    Vector2 c2 = PolarToCartesian(new Vector2(x, y + 1));
                    Vector2 c3 = PolarToCartesian(new Vector2(x + 1, y));
                    Vector2 c4 = PolarToCartesian(new Vector2(x + 1, y + 1));
                    tile.SetBody(engine.Physics, new Vector2[] { c1, c2, c4, c3 });
                }
        }
    }

    public enum WorldTileType
    {
        Earth,
        Rock,
        SolidRock,
        Gold,
        Lava
    }

    public class WorldTile : IPhysicsObject, IDamageSink
    {
        Body _body;
        Planet _planet;
        PhysicsCore _physics;
        WorldTileType _type;
        public WorldTile(Planet planet, WorldTileType type)
        {
            _planet = planet;
            _type = type;
        }

        public WorldTileType Type
        {
            get
            {
                return _type;
            }
        }

        public void SetBody(PhysicsCore physics, Vector2[] vertices)
        {
            _physics = physics;
            if (_body == null && Health > 0)
            {
                _body = physics.CreateBody(this);
                _body.IsStatic = true;
                _body.Restitution = 0.5f;
                _body.Friction = 20f;
                Vertices v = new Vertices(vertices);
                FixtureFactory.AttachPolygon(v, 1, _body, null);
            }
        }

        public Body Body
        {
            get 
            {
                return _body;
            }
        }

        public void OnCollideWith(IPhysicsObject self, IPhysicsObject other, float impulse)
        {

        }

        public void TakeDamage(int damageAmount)
        {
            if (damageAmount < 1)
                return;
            if (Type == WorldTileType.Gold || Type == WorldTileType.Lava || Type == WorldTileType.SolidRock)
                return;
            if (Health == 0)
                return;
            Health = 0;
            _planet.Dirty = true;
            if (_body != null)
            {
                _physics.RemoveBody(_body);
                _body = null;
            }
        }

        public int Health = 5;
    }
}
