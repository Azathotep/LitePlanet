using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LitePlanet.Projectiles
{
    public interface IDamageSink
    {
        void TakeDamage(int damageAmount);
    }
}
