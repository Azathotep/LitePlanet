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
            Particle particle = engine.BulletParticles.CreateParticle(position, direction * 50, 100, true);
            particle.Body.Mass = 0.01f;
            particle.Body.IsBullet = true;
            particle.Body.CollidesWith = Category.Cat1 | Category.Cat2;
            particle.Body.CollisionCategories = Category.Cat2;
            particle.Body.IgnoreCollisionWith(firer.Body);
            particle.OnCollideWithOther += particle_OnCollideWithOther;
            _recharge = 0;
        }

        void particle_OnCollideWithOther(Particle particle, IPhysicsObject other, float impulse)
        {
            particle.Life = 0;
            Explosion explosion = new Explosion(_engine);
            explosion.Create(particle.Position);
            Particle np = _engine.SmokeParticles.CreateParticle(particle.Position, Vector2.Zero, 30, false);
        }
    }
}
