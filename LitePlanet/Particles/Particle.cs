using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace LitePlanet.Particles
{
    class Particle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public Color Color;
        public int Life;

        public Particle(Vector2 position, Vector2 velocity, Color color, int life)
        {
            Position = position;
            Velocity = velocity;
            Life = life;
            Color = color;
        }
    }
}
