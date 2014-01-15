using LiteEngine.Math;
using LiteEngine.Rendering;
using LiteEngine.Textures;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LitePlanet.Worlds
{
    /// <summary>
    /// Represents information about a planet but not any of the details about it's tile geometry
    /// </summary>
    public class PlanetInfo
    {
        Texture _basePlanetTexture = new Texture("planets", new RectangleI(0, 0, 128, 128));
        Texture _cloudPlanetTexture = new Texture("planets", new RectangleI(128, 0, 128, 128));

        public string Name;
        public string Description;
        public Vector2 SystemCoordinates;
        public Color SurfaceColor = Color.Gray;
        public Color AtmosphereColor = Color.White;
        public float AtmosphereAlpha = 0f;

        public void DrawImage(XnaRenderer renderer, Vector2 position, float diameter)
        {
            renderer.DrawSprite(_basePlanetTexture, position, new Vector2(diameter, diameter), 0, SurfaceColor, 1f);
            renderer.DrawSprite(_cloudPlanetTexture, position, new Vector2(diameter * 1.1f, diameter * 1.1f), 0, AtmosphereColor, AtmosphereAlpha);
        }
    }
}
