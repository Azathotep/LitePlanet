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
        int _maxRecharge;
        int _recharge = 0;
        Engine _engine;
        public Cannon(int recharge = 0)
        {
            _maxRecharge = recharge;
        }
        
        internal void Fire(Engine engine, Ship firer, Vector2 position, Vector2 direction)
        {
            _engine = engine;
            _recharge++;
            if (_recharge < _maxRecharge)
                return;
            position += Dice.RandomVector(0.15f);
            Particle particle = engine.Bullets.CreateBullet(position, direction * 150);
            particle.Body.ClearCollisionIgnores();
            particle.Body.IgnoreCollisionWith(firer.Body);
            _recharge = 0;
        }
    }
}
