using LiteEngine.Math;
using LiteEngine.Rendering;
using LiteEngine.Textures;
using LitePlanet.Vessels;
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
        Planet _sun = new Planet(null, Vector2.Zero, 1);

        public SystemMap(StarSystem system)
        {
            _system = system;
            _sun.SurfaceColor = Color.Yellow;
            _sun.AtmosphereAlpha = 0.8f;
            _sun.AtmosphereColor = Color.Orange;
            _selectedPlanet = system.Planets[0];
        }

        public Planet Origin;
        public Vector2I SelectedSector;

        Texture _starTexture = new Texture("planet");
        Texture _starField = new Texture("stars");
        Texture _selector = new Texture("circleOverlay");
        Texture _orbit = new Texture("orbit");

        Planet _selectedPlanet;

        int _selectedItemIndex = 0;

        public void Draw(XnaRenderer renderer)
        {
            //renderer.Camera.SetAspect(200000, 160000);
            renderer.Camera.SetAspect(100000, 80000);
            
            //ZoomCamera.Update(_system.Time);
            renderer.Camera.LookAt(ZoomCamera.Position, 0);
            renderer.Camera.ChangeZoom(ZoomCamera.Zoom);
            renderer.BeginDraw();

            Planet selectedPlanet = _system.Planets[0];

            _sun.DrawIcon(renderer, Vector2.Zero, 5000f);

            foreach (Planet planet in _system.Planets)
            {
                renderer.DrawDepth = 0.5f;

                float planetSize = planet.Radius * 2;

                //if (ZoomCamera.Zoom > 1)
                planet.DrawIcon(renderer, planet.Position, planetSize, ZoomCamera.Zoom);


                //if (ZoomCamera.Zoom < 0.4f)
                //{
                //    planet.DrawIcon(renderer, planet.Position, 0.02f);
                //}

                if (planet == _selectedPlanet)
                    renderer.DrawSprite(_selector, planet.Position, new Vector2(planetSize * 3f, planetSize * 3f), 0, Color.Green, 1);

                //foreach (Ship ship in _system.Ships)
                //{
                //    renderer.DrawSprite(_selector, ship.SystemCoordinates, new Vector2(1f, 1f), 0, Color.Green, 1);
                //}
            }

            //if (Origin != null)
            //{
            //    renderer.DrawSprite(_selector, Origin.Position, new Vector2(1.1f, 1.1f), 0, Color.White, 1);
            //}

            //if (_target != null)
            //{
            //    Vector2 diff = _target.Planet.Position - Origin.Position;
            //    float len = diff.Length();
            //    float angle = (float)Math.Atan2(diff.X, -diff.Y) - (float)Math.PI/2;
            //    renderer.DrawSprite(_starTexture, new RectangleF(Origin.Position.X, Origin.Position.Y, len, 0.1f), 0.5f, angle, Vector2.Zero, Color.Cyan);
            //    renderer.DrawSprite(_selector, _target.Planet.Position, new Vector2(1f, 1f), 0, Color.Cyan, 1);
            //}

            renderer.EndDraw();

            renderer.BeginDrawToScreen();

            renderer.DrawStringBox("System #1", new RectangleF(0, 0, 400, 10), Color.White);
            renderer.DrawStringBox("Press Tab to switch between targets", new RectangleF(0, 20, 400, 10), Color.White);
            renderer.DrawStringBox("Press Enter to select target", new RectangleF(0, 40, 400, 10), Color.White);

            //float x = 100;
            //foreach (PlanetInfo planet in _system.Planets)
            //{
            //    planet.DrawImage(renderer, new Vector2(x, 15), 20f);
            //    x += 30;
            //}

            
            //if (_target != null)
            //{
            //    PlanetInfo targetPlanet = _target.Planet;
            //    targetPlanet.DrawImage(renderer, new Vector2(1000, 150), 200f);

            //    renderer.DrawStringBox("Target: " + targetPlanet.Name, new RectangleF(900, 260, 300, 10), Color.White);
                
            //    Vector2 distance = Origin.Position - targetPlanet.Position;
            //    renderer.DrawStringBox("Distance to target: " + distance.Length(), new RectangleF(900, 280, 300, 10), Color.White);

            //    renderer.DrawStringBox(targetPlanet.Description, new RectangleF(900, 300, 300, 200), Color.White);

            //    //renderer.DrawStringBox("Press [J] to jump to target", new RectangleF(0, 140, 300, 10), Color.White);               
            //}
            renderer.EndDraw();
        }

        struct SystemPosition
        {
            StarSystem _system;
            Planet _planet;
            Vector2 _coordinates;
            
            public SystemPosition(StarSystem system, Planet planet)
            {
                _system = system;
                _planet = planet;
                _coordinates = Vector2.Zero;
            }

            public SystemPosition(StarSystem system, Vector2 coordinates)
            {
                _system = system;
                _planet = null;
                _coordinates = coordinates;
            }

            public Vector2 Position
            {
                get
                {
                    if (_planet != null)
                        return _planet.Position;
                    return _coordinates;
                }
            }

        }

        AutoZoom ZoomCamera = new AutoZoom();

        class AutoZoom
        {
            float _zoom = 1;
            public float Zoom
            {
                get
                {
                    return _zoom;
                }
            }

            Vector2 _position;
            public Vector2 Position
            {
                get
                {
                    return _position;
                }
            }

            float _zoomT=1;

            Vector2 _positionStart;
            SystemPosition _target;
            float _zoomStart;
            float _zoomEnd;

            public void ZoomTo(SystemPosition target, float zoom)
            {
                _zoomT = 0;
                _positionStart = _position;
                _target = target;
                _zoomStart = _zoom;
                _zoomEnd = zoom;
            }

            public void Update(float time)
            {
                if (_zoomT >= 1)
                {
                    _position = _target.Position;
                    return;
                }
                _zoomT += 0.02f;
                if (_zoomT > 1)
                    _zoomT = 1;
                _position = _positionStart + (_target.Position - _positionStart) * _zoomT;
                _zoom = _zoomStart + (_zoomEnd - _zoomStart) * _zoomT;
            }
        }

        ViewMode _viewMode = ViewMode.InnerSystem;

        enum ViewMode
        {
            OuterSystem,
            InnerSystem,
            PlanetSystem,
            SurfaceDetail          
        }

        void GotoView(ViewMode mode)
        {
            switch (mode)
            {
                case ViewMode.OuterSystem:
                    ZoomCamera.ZoomTo(new SystemPosition(_system, new Vector2(0, 0)), 2f);
                    break;
                case ViewMode.InnerSystem:
                    ZoomCamera.ZoomTo(new SystemPosition(_system, new Vector2(0, 0)), 1f);
                    break;
                case ViewMode.PlanetSystem:
                    ZoomCamera.ZoomTo(new SystemPosition(_system, _selectedPlanet), 0.01f);
                    break;
                case ViewMode.SurfaceDetail:
                    ZoomCamera.ZoomTo(new SystemPosition(_system, _selectedPlanet), 0.001f);
                    break;
            }
        }

        internal int OnKeyPress(Keys key)
        {
            switch (key)
            {
                case Keys.Back:
                    switch (_viewMode)
                    {
                        case ViewMode.SurfaceDetail:
                            _viewMode = ViewMode.PlanetSystem;
                            break;
                        case ViewMode.PlanetSystem:
                            _viewMode = ViewMode.InnerSystem;
                            break;
                        case ViewMode.InnerSystem:
                            _viewMode = ViewMode.OuterSystem;
                            break;
                    }
                    GotoView(_viewMode);
                    return -1;
                case Keys.Enter:
                    switch (_viewMode)
                    {
                        case ViewMode.OuterSystem:
                            _viewMode = ViewMode.InnerSystem;
                            break;
                        case ViewMode.InnerSystem:
                            _viewMode = ViewMode.PlanetSystem;
                            break;
                        case ViewMode.PlanetSystem:
                            _viewMode = ViewMode.SurfaceDetail;
                            break;
                    }
                    GotoView(_viewMode);
                    return -1;
                case Keys.Tab:
                    switch (_viewMode)
                    {
                        case ViewMode.InnerSystem:
                            int index = _system.Planets.IndexOf(_selectedPlanet);
                            index++;
                            if (index >= _system.Planets.Count)
                                index = 0;
                            _selectedPlanet = _system.Planets[index];
                            break;
                        case ViewMode.PlanetSystem:
                            //need list of items in planet system
                            //(moons, stations?
                            break;
                    }


                    
                    //if (_target == null)
                    //{
                    //    _target = new NavigationNode(_system.Planets[1]);
                    //}
                    //else
                    //{
                    //    int i = _system.Planets.IndexOf(_target.Planet);
                    //    i++;
                    //    if (i >= _system.Planets.Count)
                    //        i = 0;
                    //    _target.Planet = _system.Planets[i];

                    //}

                    //_selectedPlanetIndex++;
                    //if (_selectedPlanetIndex >= _system.Planets.Count)
                    //    _selectedPlanetIndex = 0;
                    return -1;
                case Keys.Right:
                    SelectedSector.X++;
                    return 5;
                case Keys.Left:
                    SelectedSector.X--;
                    return 5;
                case Keys.Up:
                    SelectedSector.Y--;
                    return 5;
                case Keys.Down:
                    SelectedSector.Y++;
                    return 5;
            }
            return 0;
        }
    }

    interface IDestination
    {
        
    }


}
