using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiteEngine.Core;

namespace LitePlanet.Effects
{
    class Explosion
    {
        private Engine _engine;

        public Explosion(Engine engine)
        {
            this._engine = engine;
        }

        internal void Create(Microsoft.Xna.Framework.Vector2 Position)
        {
            for (int i = 0; i < 10; i++)
                _engine.SmokeParticles.CreateParticle(Position + Dice.RandomVector(0.1f), Dice.RandomVector(2), 50);
        }
    }
}
