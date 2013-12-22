using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using FarseerPhysics.Dynamics;
using LiteEngine.Physics;
using LiteEngine.Particles;
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
            if (_recharge < 10)
                return;
            Particle particle = engine.Bullets.CreateBullet(position, direction * 50);
            particle.Body.IgnoreCollisionWith(firer.Body);
            _recharge = 0;
        }
    }
}
