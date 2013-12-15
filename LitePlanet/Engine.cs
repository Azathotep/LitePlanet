using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiteEngine.Xna;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FarseerPhysics.Dynamics;
using LiteEngine.Input;
using LiteEngine.Core;
using LiteEngine.Math;
using LitePlanet.Worlds;
using LitePlanet.Vessels;
using LitePlanet.Particles;

namespace LitePlanet
{
    class Engine : LiteXnaEngine
    {
        LiteEngine.Textures.Texture _particleTexture = new LiteEngine.Textures.Texture("grass");
        ParticleList _exhaustParticles = new ParticleList(100);
        IShip _ship;
        IPlanet _planet;

        protected override void Initialize()
        {
            _ship = new Ship(this);
            Renderer.SetDeviceMode(800, 600, true);
            Renderer.Camera.SetViewField(80, 60);
            Renderer.Camera.LookAt(new Vector2(0, 0));

            Body body = Physics.CreateRectangleBody(10f,10f,1f);
            body.IsStatic = true;
            body.Restitution = 0.5f;
            body.Friction = 0.3f;
            body.Rotation = 0;
            body.Position = new Vector2(0, 0);
            base.Initialize();
        }

        protected override int OnKeyPress(Keys key, GameTime gameTime)
        {
            switch (key)
            {
                case Keys.Escape:
                    Exit();
                    break;
                case Keys.Up:
                    _ship.ApplyForwardThrust(2f);
                    
                    float m = Math.Min((float)gameTime.ElapsedGameTime.TotalSeconds, (1f / 30f));
                    for (int i = 0; i < 5; i++)
                    {
                        Vector2 vel = _ship.Velocity * m - _ship.Facing * 0.1f;
                        vel.X += Dice.Next() * 0.01f - 0.005f;
                        vel.Y += Dice.Next() * 0.01f - 0.005f;

                        _exhaustParticles.AddParticle(new Particle(_ship.Position, vel, Color.Yellow, 50));
                    }
                    break;
                case Keys.Left:
                    _ship.ApplyRotateThrust(-0.3f);
                    break;
                case Keys.Right:
                    _ship.ApplyRotateThrust(0.3f);
                    break;
            }

            return base.OnKeyPress(key, gameTime);
        }

        protected override void UpdateFrame(GameTime gameTime, XnaKeyboardHandler keyHandler)
        {
            _exhaustParticles.Update();
        }

        protected override void DrawFrame(GameTime gameTime)
        {
            Renderer.Clear(Color.Black);
            Renderer.BeginDraw();

            Renderer.DrawDepth = 0.4f;
            _ship.Draw(Renderer);

            Renderer.DrawDepth = 0.5f;
            foreach (Particle p in _exhaustParticles.Particles)
                Renderer.DrawSprite(_particleTexture, new RectangleF(p.Position.X, p.Position.Y, 0.1f, 0.1f), 0);

            Renderer.DrawSprite(_particleTexture, new RectangleF(0,0,10,10), 0);
            Renderer.EndDraw();

            Renderer.BeginDrawToScreen();
            string frameRate = FrameRate + " FPS";
            Renderer.DrawStringBox(frameRate, new RectangleF(10, 10, 120, 10), Color.White);
            Renderer.EndDraw();
        }
    }
}
