using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using LitePlanet.Worlds;
using LitePlanet;

namespace UnitTests
{
    [TestClass]
    public class PlanetTests
    {
        [TestMethod]
        public void TestPolarToCartesian()
        {
            IPlanet planet = new Planet();
            Vector2 polar = planet.CartesianToPolar(new Vector2(0, 0));
            Assert.AreEqual(new Vector2(0, 0), polar, "");
        }
    }
}
