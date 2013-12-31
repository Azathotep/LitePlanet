using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using FarseerPhysics.Dynamics;
using LiteEngine.Physics;
using LiteEngine.Particles;
using LiteEngine.Core;
using LitePlanet.Effects;
using LitePlanet.Vessels;

namespace LitePlanet.Weapons
{
    class Cannon
    {
        int _recharge = 0;
        Engine _engine;
        internal void Fire(Engine engine, Ship firer, Vector2 position, Vector2 direction)
        {
            _engine = engine;
            _recharge++;
            if (_recharge < 3)
                return;
            position += Dice.RandomVector(0.15f);
            Particle particle = engine.Bullets.CreateBullet(position, direction * 150);
            particle.Body.IgnoreCollisionWith(firer.Body);
            _recharge = 0;
        }
    }
}
