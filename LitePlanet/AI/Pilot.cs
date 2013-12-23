using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using LitePlanet.Vessels;

namespace LitePlanet.AI
{
    class Pilot
    {
        Ship _ship;
        public Pilot(Ship ship)
        {
            _ship = ship;
        }

        /// <summary>
        /// Distance in radians to go from a1 to a2
        /// </summary>
        /// <param name="a1">angle in radians</param>
        /// <param name="a2">angle in radians</param>
        /// <returns></returns>
        float AngleBetween(float a1, float a2)
        {
            float dif = MathHelper.WrapAngle(a2 - a1);
            return dif;
        }

        public void RotateToFace(float angle)
        {
            float targetHeading = angle;
            float heading = _ship.Rotation + _ship.Body.AngularVelocity * 0.1f;
            float angleDist = AngleBetween(heading, targetHeading);
            if (angleDist > 0.01f)
                _ship.ApplyRotateThrust(0.2f);
            else if (angleDist < -0.01f)
                _ship.ApplyRotateThrust(-0.2f);
        }

        float AngleFrom(Vector2 start, Vector2 destination)
        {
            Vector2 v = destination - start;
            return (float)Math.Atan2(v.X, -v.Y);
        }

        internal void Target(Engine engine, Ship target)
        {
            Vector2 aimPoint = target.Position + target.Velocity * LiteEngine.Core.Dice.Next() * 1;

            float dist = Vector2.Distance(target.Position, _ship.Position);

            float angle = AngleFrom(_ship.Position, aimPoint);
            RotateToFace(angle);

            if (dist < 40)
            {
                if (Math.Abs(AngleBetween(_ship.Rotation, angle)) < 0.1f)
                    _ship.PrimaryWeapon.Fire(engine, _ship, _ship.Position, _ship.Facing);
            }
            else
            {
                GoTo(engine, target.Position);
            }
        }

        internal void GoTo(Engine engine, Vector2 destination)
        {
            float dist = Vector2.Distance(destination, _ship.Position);
            Vector2 v = destination - (_ship.Position + _ship.Velocity*2f);
            float angle = (float)Math.Atan2(v.X, -v.Y);

            RotateToFace(angle);

            if (Math.Abs(AngleBetween(_ship.Rotation, angle)) < 0.3f)
                _ship.ApplyForwardThrust(3f);

        }
    }
}
