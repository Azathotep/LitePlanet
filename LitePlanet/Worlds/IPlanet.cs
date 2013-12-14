using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using LiteEngine.Math;

namespace LitePlanet.Worlds
{
    interface IPlanet
    {
        /// <summary>
        /// Converts coordinates from cartesian to polar world space
        /// </summary>
        /// <param name="cCoords">cartesian coordinates relative to planet center</param>
        /// <returns>polar coordinates relative to planet center</returns>
        Vector2 CartesianToPolar(Vector2 cCoords);

        /// <summary>
        /// Converts coordinates from polar to cartesian world space
        /// </summary>
        /// <param name="cCoords">polar coordinates relative to planet center</param>
        /// <returns>cartesian coordinates relative to planet center</returns>
        Vector2 PolarToCaresian(Vector2 pCoords);

        ITile GetTile(int x, int y);

        /// <summary>
        /// Returns a list of tiles that fall within the specified bounds
        /// </summary>
        /// <param name="cBounds">bounds in cartesian world space</param>
        /// <returns></returns>
        IEnumerable<ITile> GetTiles(RectangleF cBounds);
    }
}
