using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiteEngine.Xna;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FarseerPhysics.Dynamics;
using LiteEngine.Physics;
using LiteEngine.Input;
using LiteEngine.Core;
using LiteEngine.Math;
using LiteEngine.Particles;
using LiteEngine.Rendering;
using LitePlanet.Worlds;
using LitePlanet.Vessels;
using LitePlanet.Effects;

namespace LitePlanet
{
    class Engine : LiteXnaEngine
    {
        LiteEngine.Textures.Texture _grassTexture = new LiteEngine.Textures.Texture("grass");
        ParticlePool _exhaustParticles;
        ParticlePool _smokeParticles;
        ParticlePool _bulletParticles;
        IShip _ship;
        IPlanet _planet;

        protected override void Initialize()
        {
            Physics.SetGlobalGravity(new Vector2(0, 15));
            _exhaustParticles = ParticleSystem.CreateParticleFactory();
            _smokeParticles = ParticleSystem.CreateParticleFactory();
            _bulletParticles = ParticleSystem.CreateParticleFactory();
            _ship = new Ship(this);
            _ship.Position = new Vector2(0, 20);
            Renderer.SetDeviceMode(800, 600, true);
            Renderer.Camera.SetAspect(80, 60);
            Renderer.Camera.LookAt(new Vector2(0, 0));

            PhysicsObject obj = Physics.CreateRectangleBody(60f,30f,1f);
            obj.Body.IsStatic = true;
            obj.Body.Restitution = 0.3f;
            obj.Body.Friction = 1f;
            obj.Body.Rotation = 0;
            obj.Body.Position = new Vector2(0, 40);
            obj.Body.CollisionCategories = Category.Cat1;
            obj.Body.CollidesWith = Category.All;
            _dock = new Dock(this);
            _dock.Position = new Vector2(10, 20);

            base.Initialize();
        }

        Dock _dock;

        public ParticlePool SmokeParticles
        {
            get
            {
                return _smokeParticles;
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
                    _ship.ApplyForwardThrust(16f);
                    if (_ship.Fuel > 0)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            Vector2 vel = _ship.Velocity - _ship.Facing * 5.1f;
                            vel.X += Dice.Next() * 1.6f - 0.8f;
                            vel.Y += Dice.Next() * 1.6f - 0.8f;
                            Particle exhaust = _exhaustParticles.CreateParticle(_ship.Position, vel, 50, true);
                            Vector2 p = _ship.Position - _ship.Facing * 0.7f + Dice.RandomVector(0.3f);
                            _smokeParticles.CreateParticle(p, _ship.Velocity * 0, 50, false);
                        }
                    }
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
                    Renderer.Camera.ChangeZoom(Renderer.Camera.Zoom + 0.05f);
                    break;
            }

            return base.OnKeyPress(key, gameTime);
        }

        float _fireAngle = (float)Math.PI / 3 * 2;
        Vector2[] _turrets = new Vector2[] { new Vector2(-40, -20), new Vector2(40, -20) };
        protected override void UpdateFrame(GameTime gameTime, XnaKeyboardHandler keyHandler)
        {
            _dock.Update();
                    

            foreach (Vector2 turret in _turrets)
            {
                if (Dice.Next(200) == 0)
                {
                    Vector2 diff = _ship.Position - turret;
                    _fireAngle = (float)Math.Atan2(diff.X, -diff.Y);
                    _fireAngle += Dice.Next() * 0.3f - 0.15f;
                    Vector2 fireDir = new Vector2((float)Math.Sin(_fireAngle), -(float)Math.Cos(_fireAngle));
                    Particle particle = _bulletParticles.CreateParticle(turret, fireDir * 50, 500, true);
                    particle.Body.Mass = 0.01f;
                    particle.Body.IsBullet = true;
                    particle.Body.CollidesWith = Category.Cat1 | Category.Cat2;
                    particle.Body.CollisionCategories = Category.Cat1;
                    particle.SetCollisionCallback(new CollisionCallbackHandler((i) =>
                        {
                            particle.Life = 0;
                            Explosion explosion = new Explosion(this);
                            explosion.Create(particle.Position);
                            Particle np = SmokeParticles.CreateParticle(particle.Position, Vector2.Zero, 30, false);
                        }));
                }
            }
        }

        protected override void DrawFrame(GameTime gameTime)
        {
            Renderer.Clear(Color.Black);
            Renderer.BeginDraw();

            Renderer.DrawDepth = 0.4f;
            _ship.Draw(Renderer);

            Renderer.DrawDepth = 0.5f;
            foreach (Particle p in _exhaustParticles.Particles)
            {
                float particleSize = 0.25f * (p.Life / 50f);
                float alpha = (float)p.Life * p.Life / (50 * 50);
                Color color = new Color(1, 1, (float)p.Life / 60f);
                p.Draw(Renderer, particleSize, color, alpha);
            }

            foreach (Particle p in _smokeParticles.Particles)
            {
                float particleSize = 0.4f;
                float alpha = (float)p.Life * p.Life / (50 * 50);
                float c = (float)p.Life / 100;
                Color color = new Color(c, c, c);
                p.Draw(Renderer, particleSize, color, alpha);
            }

            foreach (Particle p in _bulletParticles.Particles)
            {
                float particleSize = 0.4f;
                float alpha = 0.6f;
                Color color = Color.Cyan;
                p.Draw(Renderer, particleSize, color, alpha);
            }

            Renderer.DrawSprite(_grassTexture, new RectangleF(0,40,60,30), 0);

            if (Renderer.Camera.Zoom > 3)
            {

                float width = Renderer.Camera.Zoom;
                Renderer.DrawSprite(_grassTexture, new RectangleF(_ship.Position.X, _ship.Position.Y, width, width), 0, 0.2f);
            }

            _dock.Draw(Renderer);
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
        static LiteEngine.Textures.Texture _texture = new LiteEngine.Textures.Texture("grass");
        PhysicsObject _object;

        public Dock(Engine engine)
        {
            _object = engine.Physics.CreateRectangleBody(3, 1, 1);
            _object.Body.IsStatic = true;
        }

        public void Update()
        {
            float delta = -0.03f;
            //if (Position.Y > 30)
            //    delta *= -1;
            Position = new Vector2(Position.X, Position.Y + delta);
        }

        public Vector2 Position
        {
            get
            {
                return _object.Body.Position;
            }
            set
            {
                _object.Body.Position = value;
            }
        }

        public void Draw(XnaRenderer renderer)
        {
            renderer.DrawSprite(_texture, new RectangleF(Position.X, Position.Y, 3, 1), 0);
        }
    }
}
