using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using LiteEngine.Math;

namespace LitePlanet.Worlds
{
    /// <summary>
    /// Maintains a number of collision fields on a planet. Only planet tiles in a collision field are ever given
    /// farseer static collision bodies. The generator regularly checks the position of collision fields on the planet
    /// to either add static bodies to tiles that have entered a field or remove static bodies from tiles that are no
    /// longer in any field
    /// </summary>
    public class CollisionFieldGenerator
    {
        Dictionary<ICollisionField, Vector2> _oldFieldPositions = new Dictionary<ICollisionField, Vector2>();
        HashSet<ICollisionField> _fields = new HashSet<ICollisionField>();
        Planet _planet;

        public CollisionFieldGenerator(Planet planet)
        {
            _planet = planet;
        }

        /// <summary>
        /// A registered collision field will have collision bodies added for tiles within it 
        /// </summary>
        public void RegisterCollisionField(ICollisionField field)
        {
            _fields.Add(field);
        }

        public void UnregisterCollisionField(ICollisionField field)
        {
            _fields.Remove(field);
        }

        /// <summary>
        /// Updates tiles within the registered collision fields and tiles that are no longer
        /// in collision fields
        /// </summary>
        public void UpdateFields()
        {
            foreach (ICollisionField source in _fields)
            {
                Vector2 lastPosition;
                bool hadLastPosition = false;

                //only update if the field position has changed sufficiently
                if (_oldFieldPositions.TryGetValue(source, out lastPosition))
                {
                    Vector2 deltaPos = source.Position - lastPosition;
                    if (deltaPos.LengthSquared() < 16)
                        continue;
                    hadLastPosition = true;
                }
                _oldFieldPositions[source] = source.Position;

                //convert the last position and new field position to planet polar coordinates
                Vector2 lastPolar = _planet.CartesianToPolar(lastPosition);
                Vector2 currentPolar = _planet.CartesianToPolar(source.Position);

                //rectangles for the old and new fields
                Rectangle oldR = new Rectangle((int)lastPolar.X - source.Size, (int)lastPolar.Y - source.Size, source.Size * 2 + 1, source.Size * 2 + 1);
                Rectangle newR = new Rectangle((int)currentPolar.X - source.Size, (int)currentPolar.Y - source.Size, source.Size * 2 + 1, source.Size * 2 + 1);

                //tiles have a collision use count. When a tile falls into a field this use count is incremented and
                //decremented when the tile falls out of a field. When the use count hits zero the tile's collision body
                //is removed
                if (hadLastPosition)
                {
                    //go through the old field. If any tiles are not in the new field then these tiles
                    //have their collision use count decremented
                    for (int y = oldR.Y; y < oldR.Bottom; y++)
                        for (int x = oldR.X; x < oldR.Right; x++)
                        {
                            if (!newR.Contains(x, y))
                            {
                                //decrement the tile collision use count
                                PlanetTile tile = _planet.GetTile(x, y);
                                if (tile == null)
                                    continue;
                                tile.CollisionBody.EndUse();
                            }
                        }
                }

                //go through the tiles in the new field. If any of them weren't in the old field then they
                //are new in this field and have their collision use count incremented
                for (int y = newR.Y; y < newR.Bottom; y++)
                    for (int x = newR.X; x < newR.Right; x++)
                    {
                        if (!hadLastPosition || !oldR.Contains(x, y))
                        {
                            //increment the tile collision use count
                            PlanetTile tile = _planet.GetTile(x, y);
                            if (tile == null)
                                continue;
                            tile.CollisionBody.BeginUse();
                        }
                    }
            }
        }
    }

    /// <summary>
    /// Defines a collision field centered on a position in cartesian coordinates with the specified size
    /// </summary>
    public interface ICollisionField
    {
        /// <summary>
        /// Position of field in cartesian coordinates
        /// </summary>
        Vector2 Position
        {
            get;
        }

        int Size
        {
            get;
        }

        //TODO property referencing the coordinate system
    }
}
