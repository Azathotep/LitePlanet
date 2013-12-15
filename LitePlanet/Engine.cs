using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiteEngine.Xna;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using LiteEngine.Input;
using LitePlanet.Worlds;
using LitePlanet.Vessels;

namespace LitePlanet
{
    class Engine : LiteXnaEngine
    {
        IShip _ship;
        IPlanet _planet;

        protected override void Initialize()
        {
            _ship = new Ship();
            Renderer.SetDeviceMode(800, 600, true);
            Renderer.Camera.SetViewField(80, 60);
            Renderer.Camera.LookAt(new Vector2(0, 0));
            base.Initialize();
        }

        protected override void UpdateFrame(GameTime gameTime, XnaKeyboardHandler keyHandler)
        {
            if (keyHandler.IsKeyDown(Keys.Escape))
                Exit();
        }

        protected override void DrawFrame(GameTime gameTime)
        {
            Renderer.Clear(Color.Black);
            Renderer.BeginDraw();
            _ship.Draw(Renderer);
            Renderer.EndDraw();
        }
    }
}
