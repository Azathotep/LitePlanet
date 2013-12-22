using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using FarseerPhysics.Dynamics;
using LiteEngine.Particles;
using LiteEngine.Physics;
using LitePlanet.Effects;

namespace LitePlanet.Projectiles
{
    public class Bullets
    {
        Engine _engine;
        ParticlePool _pool;
        public Bullets(Engine engine)
        {
            _engine = engine;
            _pool = engine.ParticleSystem.CreateParticleFactory();
        }

        public Particle CreateBullet(Vector2 position, Vector2 velocity)
        {
            Particle particle = _pool.CreateParticle(position, velocity, 30, false);
            particle.Body.Mass = 0.01f;
            particle.Body.IsBullet = true;
            particle.Body.CollidesWith = Category.Cat1 | Category.Cat2;
            particle.Body.CollisionCategories = Category.Cat2;
            particle.OnCollideWithOther += particle_OnCollideWithOther;
            return particle;
        }

        void particle_OnCollideWithOther(Particle particle, IPhysicsObject other, float impulse)
        {
            particle.Life = 0;
            Explosion explosion = new Explosion(_engine);
            explosion.Create(particle.Position);
            Particle np = _engine.SmokeParticles.CreateParticle(particle.Position, Vector2.Zero, 30, false);
        }

        public IEnumerable<Particle> Particles 
        {
            get
            {
                return _pool.Particles;
            }
        }
    }
}
