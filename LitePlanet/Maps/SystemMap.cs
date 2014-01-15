using LiteEngine.Math;
using LiteEngine.Rendering;
using LiteEngine.Textures;
using LitePlanet.Worlds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LitePlanet.Maps
{
    /// <summary>
    /// Interactive solar system map
    /// </summary>
    class SystemMap
    {
        StarSystem _system;
        int _selectedPlanetIndex = 0;
        PlanetInfo _sun = new PlanetInfo();

        public SystemMap(StarSystem system)
        {
            _system = system;
            _sun.SurfaceColor = Color.Yellow;
            _sun.AtmosphereAlpha = 0.8f;
            _sun.AtmosphereColor = Color.Orange;
        }

        PlanetInfo SelectedPlanet
        {
            get
            {
                return _system.Planets[_selectedPlanetIndex];
            }
        }

        Texture _starTexture = new Texture("planet");
        Texture _starField = new Texture("stars");
        Texture _selector = new Texture("circleOverlay");
        public void Draw(XnaRenderer renderer)
        {
            renderer.Camera.LookAt(Vector2.Zero, 0);
            //renderer.Camera.SetAspect(100, 100);
            renderer.Camera.ChangeZoom(1);
            renderer.BeginDraw();

            _sun.DrawImage(renderer, Vector2.Zero, 1.2f);

            foreach (PlanetInfo planet in _system.Planets)
            {
                planet.DrawImage(renderer, planet.SystemCoordinates, 0.4f);
                if (planet == SelectedPlanet)
                    renderer.DrawSprite(_selector, planet.SystemCoordinates, new Vector2(1f, 1f), 0, Color.Green, 1);
            }

            renderer.EndDraw();

            renderer.BeginDrawToScreen();

            renderer.DrawStringBox("System #1", new RectangleF(0, 0, 100, 10), Color.White);


            float x = 50;
            foreach (PlanetInfo planet in _system.Planets)
            {
                renderer.DrawSprite(_starTexture, new Vector2(x, 15), new Vector2(20f, 20f), 0);
                x += 20;
            }

            SelectedPlanet.DrawImage(renderer, new Vector2(1000, 150), 200f);

            renderer.DrawStringBox(SelectedPlanet.Name, new RectangleF(900, 260, 300, 10), Color.White);
            renderer.DrawStringBox(SelectedPlanet.Description, new RectangleF(900, 280, 300, 200), Color.White);

            renderer.EndDraw();
        }

        internal int OnKeyPress(Keys key)
        {
            switch (key)
            {
                case Keys.Tab:
                    _selectedPlanetIndex++;
                    if (_selectedPlanetIndex >= _system.Planets.Count)
                        _selectedPlanetIndex = 0;
                    return -1;
            }
            return 0;
        }
    }

    /// <summary>
    /// Represents a star system made up of planets
    /// </summary>
    public class StarSystem
    {
        List<PlanetInfo> _planets = new List<PlanetInfo>();

        public void AddPlanet(PlanetInfo planet)
        {
            _planets.Add(planet);
        }

        public IList<PlanetInfo> Planets
        {
            get
            {
                return _planets;
            }
        }
    }
}
