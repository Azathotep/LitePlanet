using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LitePlanet.Particles
{
    class ParticleList
    {
        LinkedList<Particle> _activeParticles;

        public ParticleList(int capacity)
        {
            _activeParticles = new LinkedList<Particle>();
        }

        public void AddParticle(Particle particle)
        {
            _activeParticles.AddFirst(particle);
        }

        public void Update()
        {
            LinkedListNode<Particle> next;
            for (var node = _activeParticles.First; node != null; node = next)
            {
                next = node.Next;
                Particle p = node.Value;
                p.Position += p.Velocity;
                p.Life--;
                if (p.Life <= 0)
                {
                    _activeParticles.Remove(node);
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
