using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using LiteEngine.Rendering;
using LiteEngine.Math;
using LiteEngine.Physics;
using LitePlanet.Projectiles;
using LiteEngine.Procedural;
using LiteEngine.Textures;
using Microsoft.Xna.Framework.Graphics;

namespace LitePlanet.Worlds
{
    public class Planet
    {
        static LiteEngine.Textures.Texture _basePlanetTexture = new LiteEngine.Textures.Texture("planets", new RectangleI(0, 0, 128, 128));
        static LiteEngine.Textures.Texture _cloudPlanetTexture = new LiteEngine.Textures.Texture("planets", new RectangleI(128, 0, 128, 128));

        static LiteEngine.Textures.Texture _pointTexture = new LiteEngine.Textures.Texture("point");
        static LiteEngine.Textures.Texture _particleTexture = new LiteEngine.Textures.Texture("particle");

        LiteEngine.Textures.Texture _grassTexture = new LiteEngine.Textures.Texture("brownplanet");
        int _width = 100;
        int _height;

        public string Name;
        public string Description;
        public Color SurfaceColor = Color.Gray;
        public Color RockColor = Color.Gray;
        public Color AtmosphereColor = Color.White;
        public float AtmosphereAlpha = 0f;

        PlanetTile[,] _tiles;
        CollisionFieldGenerator _collisionFieldGenerator;

        public bool Dirty = true;
        int _crustDepth = 100;
        bool _grassOnTop = false;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="radius">radius of planet in tiles</param>
        public Planet(PhysicsCore physics, Vector2 position, int radius, bool grassOnTop=false)
        {
            _grassOnTop = grassOnTop;
            _physics = physics;
            _position = position;
            _width = (int)(radius * Math.PI * 2);
            _height = radius;
            if (_physics != null)
            {
                _collisionFieldGenerator = new CollisionFieldGenerator(this);
                GenerateGeometry();
            }
        }

        public float MinX(float x1, float x2)
        {
            if (x2 > x1 + _width * 0.5f)
                return x2;
            if (x2 < x1 - _width * 0.5f)
                return x1;
            if (x1 < x2)
                return x1;
            return x2;
        }

        public float MaxX(float x1, float x2)
        {
            if (MinX(x1, x2) == x1)
                return x2;
            return x1;
        }

        void GenerateGeometry()
        {
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
                    if (_grassOnTop && y == _crustDepth - 1)
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

        Vector2 _position;
        public Vector2 Position
        {
            get
            {
                return _position;
            }
        }

        public CollisionFieldGenerator CollisionFieldGenerator
        {
            get
            {
                return _collisionFieldGenerator;
            }
        }

        public void DrawIcon(XnaRenderer renderer, Vector2 position, float diameter, bool zoomedOut, float alpha = 1)
        {
            renderer.DrawDepth = 0.5f;
            renderer.DrawSprite(_particleTexture, Position, Vector2.One * Radius * 2.7f, 0, AtmosphereColor, 1f);
            renderer.DrawSprite(_pointTexture, Position, Vector2.One * Radius * 2.7f, 0, Color.White, 0f);


            if (zoomedOut)
            {
                renderer.DrawDepth = 0.2f; // 0.8f;
                renderer.DrawSprite(_basePlanetTexture, position, new Vector2(diameter, diameter), 0, SurfaceColor, alpha);
            }
                //renderer.DrawDepth = 0.4f;
            //renderer.DrawSprite(_cloudPlanetTexture, position, new Vector2(diameter * 1.1f, diameter * 1.1f), 0, AtmosphereColor, AtmosphereAlpha * alpha);
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
            Vector2 relcCoords = cCoords - Position;
            
            float y = (float)Math.Sqrt(relcCoords.X * relcCoords.X + relcCoords.Y * relcCoords.Y);
            float angle = (float)Math.Atan2(relcCoords.X, -relcCoords.Y);// +(float)Math.PI;
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
            return new Vector2(px + Position.X, py + Position.Y);
        }   

        //public void GenerateVertexBuffer(Engine engine, XnaRenderer renderer, int xMin, int yMin, int width, int height)
        //{
        //    List<Microsoft.Xna.Framework.Graphics.VertexPositionColorTexture> vl = new List<Microsoft.Xna.Framework.Graphics.VertexPositionColorTexture>();
        //    for (int py = yMin; py < yMin + height; py++)
        //        for (int px = xMin; px < xMin + width; px++) 
        //        {
        //            int x = (px % _width + _width) % _width;
        //            int y = py;
        //            PlanetTile tile = GetTile(x, y);
        //            if (tile == null)
        //                continue;

        //            //calculate the four corner positions in world space
        //            Vector2 c1 = PolarToCartesian(new Vector2(x, y));
        //            Vector2 c2 = PolarToCartesian(new Vector2(x, y + 1));
        //            Vector2 c3 = PolarToCartesian(new Vector2(x + 1, y));
        //            Vector2 c4 = PolarToCartesian(new Vector2(x + 1, y + 1));

        //            if (!renderer.Camera.IsPointOnScreen(c1) && !renderer.Camera.IsPointOnScreen(c2) &&
        //                !renderer.Camera.IsPointOnScreen(c3) && !renderer.Camera.IsPointOnScreen(c4))
        //                continue;

        //            Color color = Color.White;
        //            bool destroyed = tile.Health <= 0;
        //            bool dirt = y == _height;
        //            bool lava = y < _height - _crustDepth;
        //            float tx = 0;
        //            float ty = 0;
        //            float tw = 0.22f;
        //            if (destroyed)
        //            {
        //                ty = 0.26f;
        //            }
        //            else
        //            {
        //                switch (tile.Type)
        //                {
        //                    case WorldTileType.Sky:
        //                        tx = 0.52f;
        //                        ty = 0.26f;

        //                        float heightAboveSurface = (float)(y - _height);

        //                        float propToSpace = heightAboveSurface / 50;

        //                        float thickness = 2 - propToSpace;
        //                        thickness = MathHelper.Clamp(thickness, 0, 1);

        //                        float yy = thickness;
        //                        color = Color.Lerp(Color.FromNonPremultiplied(64, 198, 255, 0), Color.FromNonPremultiplied(64, 198, 255, 255), yy); //Color.FromNonPremultiplied(0,0,0,0), yy);
        //                        break;
        //                    case WorldTileType.Earth:
        //                        tx = 0f;
        //                        ty = 0f; 
        //                        break;
        //                    case WorldTileType.Rock:
        //                        tx = 0.52f;
        //                        ty = 0.26f;
        //                        color = RockColor;
        //                        break;
        //                    case WorldTileType.Lava:
        //                        tx = 0.26f;
        //                        ty = 0.26f;
        //                        break;
        //                    case WorldTileType.SolidRock:
        //                        tx = 0.52f;
        //                        ty = 0f;
        //                        break;
        //                    case WorldTileType.Gold:
        //                        tx = 0.77f;
        //                        ty = 0f;
        //                        break;
        //                }
        //            }

        //            if (!tile.Visible)
        //                color = Color.Black;

        //            //if (tile.CollisionBody._useCount == 1)
        //            //    color = Color.Red;
        //            //if (tile.CollisionBody._useCount == 2)
        //            //    color = Color.Blue;
        //            //if (tile.CollisionBody._useCount > 2)
        //            //    color = Color.Purple;

        //            Color ccc = Color.FromNonPremultiplied(100,100,255,255);
        //            ccc.A = 0;

        //            vl.Add(new Microsoft.Xna.Framework.Graphics.VertexPositionColorTexture(new Vector3(c1, 0), color, new Vector2(tx, ty + tw)));
        //            vl.Add(new Microsoft.Xna.Framework.Graphics.VertexPositionColorTexture(new Vector3(c2, 0), color, new Vector2(tx, ty)));
        //            vl.Add(new Microsoft.Xna.Framework.Graphics.VertexPositionColorTexture(new Vector3(c3, 0), color, new Vector2(tx + tw, ty + tw)));

        //            vl.Add(new Microsoft.Xna.Framework.Graphics.VertexPositionColorTexture(new Vector3(c3, 0), color, new Vector2(tx + tw, ty + tw)));
        //            vl.Add(new Microsoft.Xna.Framework.Graphics.VertexPositionColorTexture(new Vector3(c2, 0), color, new Vector2(tx, ty)));
        //            vl.Add(new Microsoft.Xna.Framework.Graphics.VertexPositionColorTexture(new Vector3(c4, 0), color, new Vector2(tx + tw, ty)));
        //        }
        //    _vCount = vl.Count;
        //    if (vl.Count == 0)
        //        return;

        //    if (_vb == null)
        //        _vb = renderer.CreateVertexBuffer(30000);

        //    var vertices = vl.ToArray();
        //    _vCount = vertices.Length;
        //    _vb.SetData(vertices, 0, _vCount);
        //}

        VertexBuffer _quadBuffer = null;
        public void Draw(XnaRenderer renderer)
        {
            if (_quadBuffer == null)
            {
                _quadBuffer = renderer.CreateVertexBuffer(4);
                VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[4];
                vertices[0] = new VertexPositionColorTexture(new Vector3(renderer.Camera.Position + new Vector2(3000, -3000), 0), Color.White, new Vector2(0, 0));
                vertices[1] = new VertexPositionColorTexture(new Vector3(renderer.Camera.Position + new Vector2(-3000, -3000), 0), Color.White, new Vector2(1, 0));
                vertices[2] = new VertexPositionColorTexture(new Vector3(renderer.Camera.Position + new Vector2(3000, 3000), 0), Color.White, new Vector2(1, 1));
                vertices[3] = new VertexPositionColorTexture(new Vector3(renderer.Camera.Position + new Vector2(-3000, 3000), 0), Color.White, new Vector2(1, 1));
                _quadBuffer.SetData(vertices);
            }

            Texture2D tileTexture = TileTexture;
            Texture2D detailTexture = renderer.ContentManager.Load<Texture2D>("brownplanet");
            
            Effect effect = renderer.ContentManager.Load<Effect>("planet.mgfxo");
            effect.Parameters["xWorld"].SetValue(renderer.Camera.World);
            effect.Parameters["xProjection"].SetValue(renderer.Camera.Projection);
            effect.Parameters["xView"].SetValue(renderer.Camera.View);
            
            effect.Parameters["xTexture"].SetValue(tileTexture);
            effect.Parameters["xTilesTexture"].SetValue(detailTexture);
            
            effect.Parameters["planetPos"].SetValue(Position);
            effect.Parameters["planetWidth"].SetValue(_width);
            effect.Parameters["zoom"].SetValue(renderer.Camera.Zoom);
            
            effect.Techniques["Planet"].Passes[0].Apply();

            renderer.GraphicsDevice.SetVertexBuffer(_quadBuffer);
            renderer.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            renderer.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            renderer.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            renderer.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            renderer.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
        }

        /// <summary>
        /// Returns the distance of a point from the surface of the planet
        /// </summary>
        public float Altitude(Vector2 point)
        {
            Vector2 diff = point - Position;
            return diff.Length() - Radius;
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

        internal void Update()
        {
            
        }

        public float Width
        {
            get
            {
                return _width;
            }
        }

        Microsoft.Xna.Framework.Graphics.Texture2D _texture;
        public Microsoft.Xna.Framework.Graphics.Texture2D TileTexture
        {
            get
            {
                return _texture;
            }
        }

        int _chunkWidth = 8;
        int _numHorizontalChunks;
        public void GenerateTileTexture(Microsoft.Xna.Framework.Graphics.GraphicsDevice device)
        {
            int bmWidth = ((int)(_width / _chunkWidth) + 1) * _chunkWidth;
            _numHorizontalChunks = bmWidth / _chunkWidth;
            int height = 512;
            _texture = new Microsoft.Xna.Framework.Graphics.Texture2D(device, bmWidth, height, false, Microsoft.Xna.Framework.Graphics.SurfaceFormat.Color);
            byte[] pixels = new byte[bmWidth * height * 4];
            
            for (int y = 0; y < height; y++)
                for (int x = 0; x < _width; x++)
                {
                    int i = (y * bmWidth + x) * 4; 
                    PlanetTile tile = GetTile(x, y);
                    Color tileValue = GetTileValue(tile);
                    pixels[i] = tileValue.R;
                    pixels[i+1] = tileValue.G;
                    pixels[i+2] = tileValue.B;
                    pixels[i+3] = tileValue.A;
                }
            _texture.SetData<byte>(pixels);
        }

        int GetTileTextureId(PlanetTile tile)
        {
            switch (tile.Type)
            {
                case WorldTileType.Sky:
                    return 6;
                case WorldTileType.Earth:
                    return 0;
                case WorldTileType.Rock:
                    return 1;
                case WorldTileType.Lava:
                    return 5;
                case WorldTileType.SolidRock:
                    return 2;
                case WorldTileType.Gold:
                    return 3;
            }
            return 6;
        }
        
        Color GetTileValue(PlanetTile tile)
        {
            Color ret = new Color();
            if (tile != null && tile.Type != WorldTileType.Sky && tile.Health > 0)
            {
                int textureId = GetTileTextureId(tile);
                float tx = (float)(textureId % 4) * 0.25f + 0.01f;
                float ty = (float)(textureId / 4) * 0.25f + 0.01f;
                ret.R = (byte)(tx * 255);
                ret.G = (byte)(ty * 255);
                if (tile.Type == WorldTileType.Earth)
                    ret.B = 255;
                ret.A = 255;
            }
            return ret;
        }

        HashSet<int> _modifiedChunks = new HashSet<int>();
        internal void UpdateTile(int x, int y, Color color)
        {
            int chunkX = x / _chunkWidth;
            int chunkY = y / _chunkWidth;
            int chunkId = chunkY * _numHorizontalChunks + chunkX;
            _modifiedChunks.Add(chunkId);
            CommitChanges();
        }


        byte[] _chunkData = new byte[8 * 8 * 4];
        public void CommitChanges()
        {
            foreach (int chunkId in _modifiedChunks)
            {
                int chunkOffset = chunkId * _chunkWidth * _chunkWidth * 4;
                int chunkY = chunkId / _numHorizontalChunks;
                int chunkX = chunkId % _numHorizontalChunks;
                int d = 0;
                for (int y = 0; y < _chunkWidth; y++)
                    for (int x = 0; x < _chunkWidth; x++)
                    {
                        int tileX = chunkX * _chunkWidth + x;
                        int tileY = chunkY * _chunkWidth + y;
                        PlanetTile tile = GetTile(tileX, tileY);
                        Color tileValue = GetTileValue(tile);

                        _chunkData[d] = tileValue.R;
                        _chunkData[d + 1] = tileValue.G;
                        _chunkData[d + 2] = tileValue.B;
                        _chunkData[d + 3] = tileValue.A;
                        d += 4;
                    }
                Rectangle r = new Rectangle(chunkX * _chunkWidth, chunkY * _chunkWidth, _chunkWidth, _chunkWidth);
                _texture.SetData<byte>(0, r, _chunkData, 0, 0);
            }
            _modifiedChunks.Clear();
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
