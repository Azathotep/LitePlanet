﻿using System;
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
using LitePlanet.Worlds;
using LitePlanet.Vessels;
using LitePlanet.Effects;
using LitePlanet.Weapons;
using LitePlanet.Projectiles;
using LitePlanet.AI;

namespace LitePlanet
{
    public class Engine : LiteXnaEngine
    {
        LiteEngine.Textures.Texture _grassTexture = new LiteEngine.Textures.Texture("grass");
        
        ParticlePool _exhaustParticles;
        ParticlePool _smokeParticles;
        Bullets _bullets;
        Ship _ship;
        Planet _planet;
        List<Ship> _aiShips = new List<Ship>();
        List<Pilot> _aiPilots = new List<Pilot>();
        Dock _dock;
        Building _building;
        Building _building2;

        VertexBuffer _vb;
        VertexPositionColorTexture[] vertices;

        protected override void Initialize()
        {
            Physics.SetGlobalGravity(new Vector2(0, 0f));

            GravityController gc = new GravityController(1550, 10000, 1);
            gc.Enabled = true;
            gc.AddPoint(new Vector2(0, 0));
            Physics.World.AddController(gc);

            _exhaustParticles = ParticleSystem.CreateParticleFactory();
            _smokeParticles = ParticleSystem.CreateParticleFactory();
            _planet = new Planet(500);
            _bullets = new Bullets(this);
            _ship = new Ship(this);
            _ship.Position = new Vector2(0, 510);

            //_building = new Building(this, new Vector2(-10, 22), 6, 6);
            //_building2 = new Building(this, new Vector2(10, 22), 6, 6);
            
            //_dock = new Dock(this);
            //_dock.Position = new Vector2(-20, 7);

            for (int i = 0; i < 0; i++)
            {
                Ship aiShip = new Ship(this, true);
                aiShip.Position = new Vector2(60 + i * 2, -20);
                aiShip.Body.Rotation = 1f;
                Pilot pilot = new Pilot(aiShip);
                _aiShips.Add(aiShip);
                _aiPilots.Add(pilot);
            }
            
            Renderer.SetDeviceMode(800, 600, true);
            Renderer.Camera.SetAspect(80, 60);
            Renderer.Camera.LookAt(new Vector2(0, 0));

            numPrimitives = 40000;

            //_vb = Renderer.CreateVertexBuffer(numPrimitives);
            //List<VertexPositionColorTexture> vl = new List<VertexPositionColorTexture>();
            //for (int i = 0; i < numPrimitives; i++)
            //{
            //    float w = 1;
            //    float x = (i % 100) *w;
            //    float y = i / 100 * w;
            //    vl.Add(new VertexPositionColorTexture(new Vector3(x, y, 0), Color.Red, new Vector2(0, 0)));
            //    vl.Add(new VertexPositionColorTexture(new Vector3(x+w, y, 0), Color.Red, new Vector2(1, 0)));
            //    vl.Add(new VertexPositionColorTexture(new Vector3(x+w, y+w, 0), Color.Red, new Vector2(1, 1)));
            //}
            //vertices = vl.ToArray();

            //Body body = Physics.CreateRectangleBody(null, 200f,30f,1f);
            //body.IsStatic = true;
            //body.Restitution = 0.3f;
            //body.Friction = 1f;
            //body.Rotation = 0;
            //body.Position = new Vector2(0, 40);
            //body.CollisionCategories = Category.Cat1;
            //body.CollidesWith = Category.All;
            base.Initialize();
        }

        int numPrimitives;

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

        protected override int OnKeyPress(Keys key, GameTime gameTime)
        {
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
                case Keys.N:
                    Renderer.Camera.ChangeZoom(Renderer.Camera.Zoom * 1.01f);
                    break;
                case Keys.Space:
                    _ship.PrimaryWeapon.Fire(this, _ship, _ship.Position, _ship.Facing);
                    break;
            }

            return base.OnKeyPress(key, gameTime);
        }

        protected override void UpdateFrame(GameTime gameTime, XnaKeyboardHandler keyHandler)
        {
            //_vb.SetData(vertices, 0, vertices.Length / 3);

            foreach (Pilot p in _aiPilots)
                p.Target(this, _ship);
        }

        protected override void DrawFrame(GameTime gameTime)
        {
            if (_planet.Dirty)
            {
                Vector2 v = _planet.CartesianToPolar(_ship.Position);
                int x = (int)v.X - 30;
                int y = (int)v.Y - 40;
                _planet.GenerateVertexBuffer(this, Renderer, x, y, 60, 80);

                _planet.GenerateStaticBodies(this, (int)v.X, (int)v.Y);
                //_planet.Dirty = false;
            }

            float angle = (float)Math.Atan2(_ship.Position.X, -_ship.Position.Y);
            Renderer.Camera.LookAt(_ship.Position, angle);

            Renderer.Clear(Color.Black);

            //TODO why have to do this?
            Renderer.BeginDraw();
            Renderer.EndDraw();

            _planet.Draw(Renderer);
            

            Renderer.BeginDraw();

            Renderer.DrawDepth = 0.4f;

            _ship.Draw(Renderer);

            foreach (Ship s in _aiShips)
                s.Draw(Renderer);

            //_dock.Draw(Renderer);

            Renderer.DrawDepth = 0.5f;
            foreach (Particle p in _exhaustParticles.Particles)
            {
                float particleSize = 0.45f * (p.Life / 50f);
                float alpha = (float)p.Life * p.Life / (50 * 50);
                Color color = new Color(1, 1, (float)p.Life / 60f);
                p.Draw(Renderer, particleSize, color, alpha);
            }

            foreach (Particle p in _smokeParticles.Particles)
            {
                float particleSize = 0.6f;
                float alpha = (float)p.Life * p.Life / (50 * 50);
                float c = (float)p.Life / 100;
                Color color = new Color(c, c, c);
                p.Draw(Renderer, particleSize, color, alpha);
            }

            foreach (Particle p in _bullets.Particles)
            {
                float particleSize = 0.4f;
                float alpha = 0.8f;
                Color color = Color.Cyan;
                p.Draw(Renderer, particleSize, color, alpha);
            }

            //_building.Draw(Renderer);
            //_building2.Draw(Renderer);

            //Renderer.DrawSprite(_grassTexture, new RectangleF(0,40,200,30), 0);

            Renderer.EndDraw();

            Renderer.BeginDrawToScreen();
            string frameRate = FrameRate + " FPS";
            Renderer.DrawStringBox(frameRate, new RectangleF(10, 10, 120, 10), Color.White);

            Renderer.DrawStringBox("Fuel: " + _ship.Fuel, new RectangleF(10, 30, 120, 10), Color.White);

            Renderer.DrawStringBox("Hull: " + _ship.Hull, new RectangleF(10, 50, 120, 10), Color.White);
            Renderer.EndDraw();
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
