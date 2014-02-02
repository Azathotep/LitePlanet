using LiteEngine.Core;
using LiteEngine.Physics;
using LitePlanet.Vessels;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LitePlanet.Worlds
{
    /// <summary>
    /// Represents an entire star system, including star, planets, ships, etc
    /// </summary>
    class StarSystem
    {
        List<Planet> _planets = new List<Planet>();
        List<Ship> _ships = new List<Ship>();

        public StarSystem(PhysicsCore physics)
        {
            Planet planet = new Planet(physics, new Vector2(15000, 0), 500);
            planet.Name = "Planet A";
            planet.Description = "";
            planet.SurfaceColor = Color.Green;
            planet.AtmosphereColor = Color.White;
            planet.AtmosphereAlpha = 0.5f;
            _planets.Add(planet);

            planet = new Planet(physics, new Vector2(15000, -820), 200);
            planet.Name = "Planet B";
            planet.Description = "";
            planet.SurfaceColor = Color.Gray;
            planet.AtmosphereAlpha = 0.2f;
            planet.AtmosphereColor = Color.Blue;
            _planets.Add(planet);
        }

        public void AddShip(Ship ship)
        {
            _ships.Add(ship);
        }

        public void AddPlanet(Planet planet)
        {
            _planets.Add(planet);
        }

        public IList<Planet> Planets
        {
            get
            {
                return _planets;
            }
        }

        public IEnumerable<Ship> Ships { get { return _ships; } }

        internal void Update()
        {
            foreach (Planet planet in _planets)
                planet.Update();

            foreach (Ship ship in _ships)
            {
                ship.Update();
            }
        }        
    }
}
