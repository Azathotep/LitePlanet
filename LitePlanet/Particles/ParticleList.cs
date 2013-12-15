using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using LiteEngine.Physics;

namespace LitePlanet.Particles
{
    class ParticleList
    {
        PhysicsCore _physics;
        LinkedList<Particle> _activeParticles;
        Queue<Particle> _unusedParticles = new Queue<Particle>();
        int _capacity = 500;

        public ParticleList(PhysicsCore physics, int capacity)
        {
            _physics = physics;
            _activeParticles = new LinkedList<Particle>();
        }

        public void CreateParticle(Vector2 position, Vector2 velocity, Color color, int life)
        {
            Particle newParticle;
            if (_unusedParticles.Count == 0)
            {
                //no unused particles so need to create a new one
                //if the maximum capacity of active particles has been reached then exit
                //TODO destroy and reuse the oldest particle instead
                if (_activeParticles.Count >= _capacity)
                    return;
                newParticle = new Particle();
            }
            else
                newParticle = _unusedParticles.Dequeue();
            //setup the particle properties
            newParticle.Initialize(_physics, position, velocity, color, life);
            _activeParticles.AddFirst(newParticle);
        }

        public void Update()
        {
            LinkedListNode<Particle> next;
            for (var node = _activeParticles.First; node != null; node = next)
            {
                next = node.Next;
                Particle p = node.Value;
                //p.Position += p.Velocity;
                p.Life--;
                if (p.Life <= 0)
                {
                    _activeParticles.Remove(node);
                    p.Deinitialize();
                    _unusedParticles.Enqueue(p);
                }
            }
        }

        public IEnumerable<Particle> Particles
        {
            get
            {
                return _activeParticles;
            }
        }
    }
}
