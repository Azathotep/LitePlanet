using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiteEngine.Math;
using Microsoft.Xna.Framework;

namespace LitePlanet.Worlds
{
    public class Planet : IPlanet
    {
        public Vector2 CartesianToPolar(Vector2 cCoords)
        {
            throw new NotImplementedException();
        }

        public Vector2 PolarToCaresian(Vector2 pCoords)
        {
            throw new NotImplementedException();
        }

        public ITile GetTile(int x, int y)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ITile> GetTiles(RectangleF cBounds)
        {
            throw new NotImplementedException();
        }
    }
}
