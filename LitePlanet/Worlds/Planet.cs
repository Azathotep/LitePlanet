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
using LiteEngine.Procedural;

namespace LitePlanet.Worlds
{
    public class Planet
    {
        LiteEngine.Textures.Texture _grassTexture = new LiteEngine.Textures.Texture("brownplanet");
        int _width = 100;
        int _height;

        PlanetTile[,] _tiles;
        CollisionFieldGenerator _collisionFieldGenerator;

        public bool Dirty = true;
        int _crustDepth = 100;

        /// <summary>
        /// Constructor
        /// 
        /// </summary>
        /// <param name="radius">radius of planet in tiles</param>
        public Planet(PhysicsCore physics, int radius)
        {
            _physics = physics;
            _collisionFieldGenerator = new CollisionFieldGenerator(this);

            _width = (int)(radius * Math.PI);
            _height = radius;
            _tiles = new PlanetTile[_width, _crustDepth];

            NoiseField noise = new NoiseField(_width, _crustDepth);
            noise.GenerateRandomNoise();
            NoiseField nf = noise.GenerateOctave(0.1f, 8f);
            nf.Add(noise.GenerateOctave(0.8f, 1f));
            nf.Normalize();

            for (int y = 0; y < _crustDepth; y++)
                for (int x = 0; x < _width; x++)
                {
                    WorldTileType type = WorldTileType.Rock;
                    if (y == _crustDepth - 1)
                        type = WorldTileType.Earth;

                    if (y < _crustDepth - 1)
                        if (LiteEngine.Core.Dice.Next(30) == 0)
                            type = WorldTileType.SolidRock;

                    if (y < _crustDepth - 10)
                        if (LiteEngine.Core.Dice.Next(1500) == 0)
                            type = WorldTileType.Gold;

                    _tiles[x, y] = new PlanetTile(this, x, _height - _crustDepth + y, type);

                    if (nf.Values[x, y] < 0.4f)
                        _tiles[x, y].Health = 0;
                }
        }

        public CollisionFieldGenerator CollisionFieldGenerator
        {
            get
            {
                return _collisionFieldGenerator;
            }
        }

        public int Radius
        {
            get
            {
                return _height;
            }
        }

        PhysicsCore _physics;
        public PhysicsCore Physics
        {
            get
            {
                return _physics;
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

        public Vector2 PolarToCartesian(Vector2 polar)
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
                    PlanetTile tile = GetTile(x, y);
                    if (tile == null)
                        continue;

                    //calculate the four corner positions in world space
                    Vector2 c1 = PolarToCartesian(new Vector2(x, y));
                    Vector2 c2 = PolarToCartesian(new Vector2(x, y + 1));
                    Vector2 c3 = PolarToCartesian(new Vector2(x + 1, y));
                    Vector2 c4 = PolarToCartesian(new Vector2(x + 1, y + 1));

                    Color color = Color.White;
                    bool destroyed = tile.Health <= 0;
                    bool dirt = y == _height;
                    bool lava = y < _height - _crustDepth;
                    float tx = 0;
                    float ty = 0;
                    float tw = 0.22f;
                    if (destroyed)
                    {
                        ty = 0.26f;
                    }
                    else
                    {
                        switch (tile.Type)
                        {
                            case WorldTileType.Sky:
                                tx = 0.52f;
                                ty = 0.26f;

                                float heightAboveSurface = (float)(y - _height);

                                float propToSpace = heightAboveSurface / 50;

                                float thickness = 2 - propToSpace;
                                thickness = MathHelper.Clamp(thickness, 0, 1);

                                float yy = thickness;
                                color = Color.Lerp(Color.FromNonPremultiplied(64, 198, 255, 0), Color.FromNonPremultiplied(64, 198, 255, 255), yy); //Color.FromNonPremultiplied(0,0,0,0), yy);
                                break;
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

                    if (!tile.Visible)
                        color = Color.Black;

                    //if (tile.CollisionBody._useCount == 1)
                    //    color = Color.Red;
                    //if (tile.CollisionBody._useCount == 2)
                    //    color = Color.Blue;
                    //if (tile.CollisionBody._useCount > 2)
                    //    color = Color.Purple;

                    vl.Add(new VertexPositionColorTexture(new Vector3(c1, 0), color, new Vector2(tx, ty + tw)));
                    vl.Add(new VertexPositionColorTexture(new Vector3(c2, 0), color, new Vector2(tx, ty)));
                    vl.Add(new VertexPositionColorTexture(new Vector3(c3, 0), color, new Vector2(tx + tw, ty + tw)));

                    vl.Add(new VertexPositionColorTexture(new Vector3(c3, 0), color, new Vector2(tx + tw, ty + tw)));
                    vl.Add(new VertexPositionColorTexture(new Vector3(c2, 0), color, new Vector2(tx, ty)));
                    vl.Add(new VertexPositionColorTexture(new Vector3(c4, 0), color, new Vector2(tx + tw, ty)));
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

        static PlanetTile _lavaTile = new PlanetTile(null, 0, 0, WorldTileType.Lava);
        static PlanetTile _skyTile = new PlanetTile(null, 0, 0, WorldTileType.Sky);

        public PlanetTile GetTile(int x, int y)
        {
            if (y >= _height)
                return null; // _skyTile;
            if (y < 0 || y >= _height)
                return null;
            x = (x % _width + _width) % _width;
            if (y < _height - _crustDepth)
                return _lavaTile;
            int ry = y - (_height - _crustDepth);
            if (ry < 0 || ry >= _crustDepth)
                return null;
            return _tiles[x, ry];
        }
    }

    public enum WorldTileType
    {
        Earth,
        Rock,
        SolidRock,
        Gold,
        Lava,
        Sky
    }
}
