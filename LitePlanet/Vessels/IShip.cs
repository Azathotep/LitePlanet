using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using LiteEngine.Rendering;

namespace LitePlanet.Vessels
{
    interface IShip
    {
        Vector2 Position
        {
            get;
        }

        Vector2 Velocity
        {
            get;
        }

        Vector2 Facing
        {
            get;
        }

        float Rotation
        {
            get;
        }

        void ApplyForwardThrust(float amount);

        void ApplyRotateThrust(float amount);

        void Draw(XnaRenderer renderer);
    }
}
