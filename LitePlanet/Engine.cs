using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiteEngine.Xna;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using LiteEngine.Input;

namespace LitePlanet
{
    class Engine : LiteXnaEngine
    {
        protected override void UpdateFrame(GameTime gameTime, XnaKeyboardHandler keyHandler)
        {
            if (keyHandler.IsKeyDown(Keys.Escape))
                Exit();
        }

        protected override void DrawFrame(GameTime gameTime)
        {
            Renderer.Clear(Color.Aqua);
        }
    }
}
