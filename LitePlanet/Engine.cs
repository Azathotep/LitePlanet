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
using LitePlanet.Weapons;
using LitePlanet.Projectiles;

namespace LitePlanet
{
    public class Engine : LiteXnaEngine
    {
        LiteEngine.Textures.Texture _grassTexture = new LiteEngine.Textures.Texture("grass");
        LiteEngine.Textures.Texture _circleTexture = new LiteEngine.Textures.Texture("circleOverlay");
        
        ParticlePool _exhaustParticles;
        ParticlePool _smokeParticles;
        Bullets _bullets;
        Ship _ship;
        Ship _aiShip;
        IPlanet _planet;

        protected override void Initialize()
        {
            Physics.SetGlobalGravity(new Vector2(0, 1));
            _exhaustParticles = ParticleSystem.CreateParticleFactory();
            _smokeParticles = ParticleSystem.CreateParticleFactory();
            _bullets = new Bullets(this);
            _ship = new Ship(this);
            _ship.Position = new Vector2(0, 20);

            _aiShip = new Ship(this, true);
            _aiShip.Position = new Vector2(10, 20);
            Renderer.SetDeviceMode(800, 600, true);
            Renderer.Camera.SetAspect(80, 60);
            Renderer.Camera.LookAt(new Vector2(0, 0));

            Body body = Physics.CreateRectangleBody(null, 60f,30f,1f);
            body.IsStatic = true;
            body.Restitution = 0.3f;
            body.Friction = 1f;
            body.Rotation = 0;
            body.Position = new Vector2(0, 40);
            body.CollisionCategories = Category.Cat1;
            body.CollidesWith = Category.All;
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
                    _ship.ApplyForwardThrust(16f);
                    if (_ship.Fuel > 0)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            Vector2 vel = _ship.Velocity - _ship.Facing * 5.1f;
                            vel.X += Dice.Next() * 1.6f - 0.8f;
                            vel.Y += Dice.Next() * 1.6f - 0.8f;
                            Particle exhaust = _exhaustParticles.CreateParticle(_ship.Position, vel, 50);

                            Vector2 p = _ship.Position - _ship.Facing * 0.7f + Dice.RandomVector(0.3f);
                            _smokeParticles.CreateParticle(p, _ship.Velocity * 0, 50);
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
                    Renderer.Camera.ChangeZoom(Renderer.Camera.Zoom * 1.01f);
                    break;
                case Keys.Space:
                    _ship.PrimaryWeapon.Fire(this, _ship, _ship.Position, _ship.Facing);
                    break;
            }

            return base.OnKeyPress(key, gameTime);
        }

        float _fireAngle = (float)Math.PI / 3 * 2;
        protected override void UpdateFrame(GameTime gameTime, XnaKeyboardHandler keyHandler)
        {
            _dock.Update();
        }

        protected override void DrawFrame(GameTime gameTime)
        {
            Renderer.Clear(Color.Black);
            Renderer.BeginDraw();

            Renderer.DrawDepth = 0.4f;
            _ship.Draw(Renderer);

            _aiShip.Draw(Renderer);

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

            foreach (Particle p in _bullets.Particles)
            {
                float particleSize = 0.2f;
                float alpha = 1f;
                Color color = Color.Yellow;
                p.Draw(Renderer, particleSize, color, alpha);
            }

            Renderer.DrawSprite(_grassTexture, new RectangleF(0,40,60,30), 0);

            if (Renderer.Camera.Zoom > 3)
            {
                float width = Renderer.Camera.Zoom;

                Color c = Color.FromNonPremultiplied(0, 255, 0, 255);
                Renderer.DrawSprite(_circleTexture, new RectangleF(_ship.Position.X, _ship.Position.Y, width, width), 0, c, 1f);

                Vector2 front = _ship.Position + _ship.Facing * 2;
                Renderer.DrawSprite(_circleTexture, new RectangleF(front.X, front.Y, width*0.4f, width*2), _ship.Rotation, c, 1f);
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
        Body _body;

        public Dock(Engine engine)
        {
            _body = engine.Physics.CreateRectangleBody(null, 3, 1, 1);
            _body.IsStatic = true;
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
                return _body.Position;
            }
            set
            {
                _body.Position = value;
            }
        }

        public void Draw(XnaRenderer renderer)
        {
            renderer.DrawSprite(_texture, new RectangleF(Position.X, Position.Y, 3, 1), 0);
        }
    }
}
