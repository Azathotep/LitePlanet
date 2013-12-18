using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using FarseerPhysics.Dynamics;
using LiteEngine.Physics;
using LiteEngine.Particles;
using LitePlanet.Effects;

namespace LitePlanet.Weapons
{
    class Cannon
    {
        int _recharge = 0;
        internal void Fire(Engine engine, Vector2 position, Vector2 direction)
        {
            _recharge++;
            if (_recharge < 10)
                return;
            Particle particle = engine.BulletParticles.CreateParticle(position, direction * 50, 100, true);
            particle.Body.Mass = 0.01f;
            particle.Body.IsBullet = true;
            particle.Body.CollidesWith = Category.Cat1 | Category.Cat2;
            particle.Body.CollisionCategories = Category.Cat6;
            particle.SetCollisionCallback(new CollisionCallbackHandler((i) =>
            {
                particle.Life = 0;
                Explosion explosion = new Explosion(engine);
                explosion.Create(particle.Position);
                Particle np = engine.SmokeParticles.CreateParticle(particle.Position, Vector2.Zero, 30, false);
            }));
            _recharge = 0;
        }
    }
}
