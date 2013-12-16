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
using LitePlanet.Worlds;
using LitePlanet.Vessels;

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
            Physics.SetGlobalGravity(new Vector2(0, 5));
            _exhaustParticles = ParticleSystem.CreateParticleFactory();
            _smokeParticles = ParticleSystem.CreateParticleFactory();
            _bulletParticles = ParticleSystem.CreateParticleFactory();
            _ship = new Ship(this);
            Renderer.SetDeviceMode(800, 600, true);
            Renderer.Camera.SetViewField(80, 60);
            Renderer.Camera.LookAt(new Vector2(0, 0));

            PhysicsObject obj = Physics.CreateRectangleBody(60f,30f,1f);
            obj.Body.IsStatic = true;
            obj.Body.Restitution = 0.3f;
            obj.Body.Friction = 1f;
            obj.Body.Rotation = 0;
            obj.Body.Position = new Vector2(0, 40);
            obj.Body.CollisionCategories = Category.Cat1;
            obj.Body.CollidesWith = Category.All;
            base.Initialize();
        }

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
                    _ship.ApplyForwardThrust(3f);
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
                case Keys.Left:
                    _ship.ApplyRotateThrust(-0.1f);
                    break;
                case Keys.Right:
                    _ship.ApplyRotateThrust(0.1f);
                    break;
            }

            return base.OnKeyPress(key, gameTime);
        }

        float _fireAngle = (float)Math.PI / 3 * 2;
        Vector2[] _turrets = new Vector2[] { new Vector2(-40, -20), new Vector2(40, -20) };
        protected override void UpdateFrame(GameTime gameTime, XnaKeyboardHandler keyHandler)
        {
            foreach (Vector2 turret in _turrets)
            {
                if (Dice.Next(20) == 0)
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
            Renderer.EndDraw();

            Renderer.BeginDrawToScreen();
            string frameRate = FrameRate + " FPS";
            Renderer.DrawStringBox(frameRate, new RectangleF(10, 10, 120, 10), Color.White);

            Renderer.DrawStringBox("Fuel: " + _ship.Fuel, new RectangleF(10, 30, 120, 10), Color.White);

            Renderer.DrawStringBox("Hull: " + _ship.Hull, new RectangleF(10, 50, 120, 10), Color.White);
            Renderer.EndDraw();
        }
    }
}
