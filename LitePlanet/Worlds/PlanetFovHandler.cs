using LiteEngine.Fov;
using LiteEngine.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LitePlanet.Worlds
{
    /// <summary>
    /// Manages FOV calculations on a planet
    /// </summary>
    class PlanetFovHandler : IFovInfo
    {
        RecursiveShadowcast _fov = new RecursiveShadowcast();
        Planet _planet;

        public bool TileBlocksLight(int x, int y)
        {
            PlanetTile tile = _planet.GetTile(x, y);
            if (tile == null)
                return false;
            if (tile.Health == 0)
                return false;
            return true;
        }

        public void OnTileVisible(int x, int y)
        {
            PlanetTile tile = _planet.GetTile(x, y);
            if (tile != null)
                tile.Visible = true;
        }

        public void RunFov(Planet planet, int x, int y, int viewRadius)
        {
            _planet = planet;
            _fov.CalculateFov(new Vector2I(x, y), viewRadius, this);
        }
    }
}
