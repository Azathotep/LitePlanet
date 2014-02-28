using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using LitePlanet.Vessels;
using LiteEngine.Math;

namespace LitePlanet.AI
{
    class Pilot
    {
        Ship _ship;
        public Pilot(Ship ship)
        {
            _ship = ship;
        }

        public void RotateToFace(float angle)
        {
            float targetHeading = angle;
            float heading = _ship.Rotation + _ship.Body.AngularVelocity * 0.1f;
            float angleDist = Util.AngleBetween(heading, targetHeading);

            float thrust = 0.2f;
            if (_ship.Body.Mass > 1)
                thrust = 10f;

            if (angleDist > 0.01f)
                _ship.ApplyRotateThrust(thrust);
            else if (angleDist < -0.01f)
                _ship.ApplyRotateThrust(-thrust);
        }

        int s = 0;

        internal void Target(Engine engine, Ship target)
        {
            if (_ship.Hull <= 0)
                return;

            Vector2 aimPoint = target.Position + target.Velocity * LiteEngine.Core.Dice.Next() * 1;
            float dist = Vector2.Distance(target.Position, _ship.Position);
            float angle = Util.AngleBetween(_ship.Position, aimPoint);
            RotateToFace(angle);

            if (s > 0)
                s--;
            if (dist < 40 && s == 0)
            {
                if (Math.Abs(Util.AngleBetween(_ship.Rotation, angle)) < 0.2f)
                    _ship.PrimaryWeapon.Fire(engine, _ship, _ship.Position, _ship.Facing);
                s = 20;
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

            float thrust = 2f;
            if (_ship.Body.Mass > 1)
                thrust = 20f;
            if (Math.Abs(Util.AngleBetween(_ship.Rotation, angle)) < 0.3f)
                _ship.ApplyForwardThrust(thrust);
        }
    }
}
