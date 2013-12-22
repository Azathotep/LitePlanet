﻿using System;
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

        internal void GoTo(Vector2 destination)
        {
            Vector2 v = destination - _ship.Position;

            float angle = (float)Math.Atan2(v.X, -v.Y);

            RotateToFace(angle);

            if (Math.Abs(AngleBetween(_ship.Rotation, angle)) < 0.1f)
                _ship.ApplyForwardThrust(1f);

            //if (_aiShip.Rotation < 0.1f || _aiShip.Rotation > Math.PI * 2 - 0.1f)
            //    _aiShip.ApplyForwardThrust(1f);
        }
    }
}
