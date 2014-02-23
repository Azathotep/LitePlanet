using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiteEngine.Xna;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Controllers;
using LiteEngine.Physics;
using LiteEngine.Input;
using LiteEngine.Core;
using LiteEngine.Math;
using LiteEngine.Particles;
using LiteEngine.Rendering;
using LiteEngine.Fov;
using LitePlanet.Worlds;
using LitePlanet.Vessels;
using LitePlanet.Effects;
using LitePlanet.Weapons;
using LitePlanet.Projectiles;
using LitePlanet.AI;
using LitePlanet.Maps;

namespace LitePlanet
{
    public class Engine : LiteXnaEngine
    {
        LiteEngine.Textures.Texture _starsTexture = new LiteEngine.Textures.Texture("stars");
        LiteEngine.Textures.Texture _planetTexture = new LiteEngine.Textures.Texture("planet");
        LiteEngine.Textures.Texture _pointTexture = new LiteEngine.Textures.Texture("point");
        LiteEngine.Textures.Texture _particleTexture = new LiteEngine.Textures.Texture("particle");

        PlanetFovHandler _fov;
        ParticlePool _exhaustParticles;
        ParticlePool _smokeParticles;
        Bullets _bullets;
        Ship _ship;
        List<Ship> _aiShips = new List<Ship>();
        List<Pilot> _aiPilots = new List<Pilot>();
        Dock _dock;
        Building _building;
        Building _building2;
        StarSystem _system;
        SystemMap _systemMap;
        
        protected override void Initialize()
        {
            _fov = new PlanetFovHandler();
            Physics.SetGlobalGravity(new Vector2(0f, 0f));

            GravityController gc = new GravityController(1500, 10000, 1);

            gc.Enabled = true;
            gc.AddPoint(new Vector2(0, 0));
            Physics.World.AddController(gc);

            _system = new StarSystem(Physics);

            //foreach (Planet planet in _system.Planets)
            //    gc.AddPoint(planet.Position);

            _exhaustParticles = ParticleSystem.CreateParticleFactory();
            _smokeParticles = ParticleSystem.CreateParticleFactory();
            _bullets = new Bullets(this);
            
            _ship = new Ship(this);
            _ship.Position = _system.Planets[0].Position - new Vector2(0, _system.Planets[0].Radius + 5);
            _ship.Rotation = (float)Math.PI;

            foreach (Planet planet in _system.Planets)
                planet.CollisionFieldGenerator.RegisterCollisionField(_ship);
            
            _system.AddShip(_ship);

            _systemMap = new SystemMap(_system);

            //_building = new Building(this, new Vector2(-10, 22), 6, 6);
            //_building2 = new Building(this, new Vector2(10, 22), 6, 6);
            
            //_dock = new Dock(this);
            //_dock.Position = new Vector2(-20, 7);

            for (int i = 0; i < 1; i++)
            {
                Ship aiShip = new Ship(this, true);
                aiShip.Position = new Vector2(_ship.Position.X + 60 + i * 2, _ship.Position.Y - 30);
                aiShip.Body.Rotation = 1f;
                Pilot pilot = new Pilot(aiShip);
                _aiShips.Add(aiShip);
                _aiPilots.Add(pilot);
                foreach (Planet planet in _system.Planets)
                    planet.CollisionFieldGenerator.RegisterCollisionField(aiShip);
            }
            
            Renderer.SetDeviceMode(800, 600, true);
            Renderer.Camera.SetAspect(80, 60);
            Renderer.Camera.LookAt(new Vector2(0, 0));
            base.Initialize();
        }

        bool _mapMode = false;

        public ParticlePool SmokeParticles
        {
            get
            {
                return _smokeParticles;
            }
        }

        public ParticlePool ExhaustParticles
        {
            get
            {
                return _exhaustParticles;
            }
        }

        public Bullets Bullets
        {
            get
            {
                return _bullets;
            }
        }

        bool _freeCamera;

        protected override int OnKeyPress(Keys key, GameTime gameTime)
        {
            if (_mapMode)
            {
                if (key == Keys.Escape || key == Keys.M)
                {
                    _mapMode = false;
                    return 10;
                }
                return _systemMap.OnKeyPress(key);
            }
            switch (key)
            {
                case Keys.Escape:
                    Exit();
                    break;
                case Keys.Up:
                    _ship.ApplyForwardThrust(10f);
                    break;
                case Keys.L:
                    _ship.ApplyForwardThrust(-0.001f);
                    break;
                case Keys.Left:
                    _ship.ApplyRotateThrust(-0.1f);
                    break;
                case Keys.Right:
                    _ship.ApplyRotateThrust(0.1f);
                    break;
                case Keys.M:
                    _mapMode = !_mapMode;
                    ShowSystemMap(_mapMode);
                    return 10;
                case Keys.C:
                    _freeCamera = !_freeCamera;
                    return 10;
                case Keys.J:
                    //float altitude = _ship.Position.Length() - _planet.Radius;
                    return 10;
                case Keys.Z:
                    Renderer.Camera.ChangeZoom(Renderer.Camera.Zoom * 1.05f);
                    break;
                case Keys.X:
                    float zoom = Renderer.Camera.Zoom * 0.95f;
                    if (zoom < 0.5f)
                        zoom = 0.5f;
                    Renderer.Camera.ChangeZoom(zoom);
                    break;
                case Keys.Space:
                    _ship.PrimaryWeapon.Fire(this, _ship, _ship.Position, _ship.Facing);
                    break;
            }
            return base.OnKeyPress(key, gameTime);
        }

        private void ShowSystemMap(bool show)
        {
            if (show)
            {
                _systemMap.Origin = _system.Planets[0];
            }
        }

        protected override void UpdateFrame(GameTime gameTime, XnaKeyboardHandler keyHandler)
        {
            foreach (Planet planet in _system.Planets)
            {
                Vector2 diff = _ship.Position - planet.Position;
                float altitude = diff.Length() - planet.Radius;
                if (altitude > 50)
                    continue;

                Vector2 eye = planet.CartesianToPolar(_ship.Position);
                _fov.RunFov(planet, (int)Math.Floor(eye.X), (int)Math.Floor(eye.Y), 30);
                planet.CollisionFieldGenerator.UpdateFields();
            }

            foreach (Pilot p in _aiPilots)
                p.Target(this, _ship);

            _system.Update();
        }

        Planet GetNearPlanet(Vector2 position)
        {
            foreach (Planet planet in _system.Planets)
            {
                Vector2 diff = position - planet.Position;
                float altitude = diff.Length() - planet.Radius;
                if (altitude < 70)
                    return planet;
            }
            return null;
        }

        internal void DrawSun(XnaRenderer renderer)
        {
            Color color = Color.Yellow;
            color *= 0.02f;
            for (float f = 10000; f > 2; f -= 100f)
            {
                float n = f + Dice.Next() * 100f;
                renderer.DrawSprite(_pointTexture, new Vector2(20, 0), new Vector2(n, n), 0, color, 0f);
            }

            color = Color.Orange;
            color *= 0.03f;
            for (float f = 10200; f > 2; f -= 300f)
            {
                float n = f;// +Dice.Next() * 2f;
                renderer.DrawSprite(_pointTexture, new Vector2(20, 0), new Vector2(n, n), 0, color, 0f);
            }

            color = Color.White;
            color *= 0.1f;
            for (float f = 10000; f > 2; f -= 500f)
            {
                float n = f +Dice.Next() * 200f;
                renderer.DrawSprite(_pointTexture, new Vector2(20, 0), new Vector2(n, n), 0, color, 0f);
            }
        }

        Planet _nearPlanet;

        Vector2I _shipPos;
        protected override void DrawFrame(GameTime gameTime)
        {
            Renderer.Clear(Color.Black);

            if (_mapMode)
            {
                _systemMap.Draw(Renderer);
                return;
            }

            if (_nearPlanet != null)
                if (_nearPlanet.Altitude(_ship.Position) > 70)
                    _nearPlanet = null;

            if (_nearPlanet == null)
                _nearPlanet = GetNearPlanet(_ship.Position);

            Renderer.Camera.SetAspect(80, 60);
            float angle = 0; // (float)Math.Atan2(_ship.Position.X - _system.Planets[0].Position.X, -_ship.Position.Y - _system.Planets[0].Position.Y);
            if (!_freeCamera)
                Renderer.Camera.LookAt(_ship.Position, angle);

            DrawStars(Renderer);

            foreach (Planet planet in _system.Planets)
            {
                if (planet.TileTexture == null)
                    planet.GenerateTileTexture(GraphicsDevice);
                planet.Draw(Renderer);
            }
            Renderer.BeginDraw();
            _ship.Draw(Renderer);

            foreach (Ship s in _aiShips)
                s.Draw(Renderer);

            DrawSun(Renderer);


            Renderer.DrawDepth = 0.5f;
            foreach (Particle p in _exhaustParticles.Particles)
            {
                float particleSize = 0.8f * (p.Life / 50f);
                float alpha = (float)p.Life * p.Life / (50 * 50);
                Color color = new Color(1, 1, (float)p.Life / 60f);
                p.Draw(Renderer, particleSize, color, 0); //alpha);
            }

            foreach (Particle p in _smokeParticles.Particles)
            {
                float particleSize = 1.4f;
                float alpha = (float)p.Life * p.Life / (50 * 50);
                float c = (float)p.Life / 100;
                Color color = new Color(c, c, c);
                p.Draw(Renderer, particleSize, color, alpha);
            }

            foreach (Particle p in _bullets.Particles)
            {
                float particleSize = 0.5f;
                float alpha = 0f;
                Color color = Color.Lerp(Color.Cyan, Color.Blue, Dice.Next());
                p.Draw(Renderer, particleSize, color, alpha);

                particleSize = 0.9f;
                p.Draw(Renderer, particleSize, color, alpha);

                particleSize = 1.2f;
                p.Draw(Renderer, particleSize, Color.White, alpha);
            }

            //_building.Draw(Renderer);
            //_building2.Draw(Renderer);

            Renderer.EndDraw();

            Renderer.BeginDrawToScreen();
            
            string frameRate = FrameRate + " FPS";
            Renderer.DrawStringBox(frameRate, new RectangleF(10, 10, 120, 10), Color.White);
            Renderer.DrawStringBox("Fuel: " + _ship.Fuel, new RectangleF(10, 30, 120, 10), Color.White);
            Renderer.DrawStringBox("Hull: " + _ship.Hull, new RectangleF(10, 50, 120, 10), Color.White);
            Renderer.DrawStringBox(_ship.Position.X + ", " + _ship.Position.Y, new RectangleF(11, 71, 200, 10), Color.White);

            Renderer.EndDraw();
            //altitude = _ship.Position.Length() - planetToDraw.Radius;
            //float zoom = 1;
            //if (altitude > 25)
            //    zoom = altitude / 25;
 
            //Renderer.DrawStringBox("Altitude: " + (_ship.Position.Length() - _planet.Radius), new RectangleF(11, 71, 200, 10), Color.White);
            
            //if (_ship.JumpDriveCharging)
            //    Renderer.DrawStringBox("Jump Drive Charging (destination: " + _systemMap.Target.Name + "): " + _ship.JumpDriveCharge.ToString("0.00") + "%", new RectangleF(11, 91, 500, 10), Color.Red);
        }

        private void DrawStars(XnaRenderer renderer)
        {
            renderer.BeginDrawToScreen();
            renderer.DrawSprite(_starsTexture, new RectangleF(0, 0, Renderer.ScreenWidth, Renderer.ScreenHeight), 0.2f, 0, new Vector2(0f, 0f), Color.White, false, false);
            renderer.EndDraw();
        }
    }

    class Dock
    {
        static LiteEngine.Textures.Texture _texture = new LiteEngine.Textures.Texture("building");
        Body _body;

        public Dock(Engine engine)
        {
            _body = engine.Physics.CreateRectangleBody(null, 4f, 20f, 1);
            _body.IsStatic = true;
        }

        public void Update()
        {
            float delta = -0.03f;
            Position = new Vector2(Position.X, Position.Y + delta);
        }

        public Vector2 Position
        {
            get
            {
                return _body.Position;
            }
            set
            {
                _body.Position = value;
            }
        }

        public void Draw(XnaRenderer renderer)
        {
            for (int y=0;y<5;y++)
                renderer.DrawSprite(_texture, new RectangleF(Position.X, Position.Y + y * 4, 4, 4), 0);
        }
    }
}
