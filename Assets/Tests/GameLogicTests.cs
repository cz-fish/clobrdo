using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class GameLogicTests : Assets.GameLogic
    {
        [Test]
        public void BoardIsInitialized()
        {
            //var gl = new Assets.GameLogic();
            this.Start();
            Assert.AreEqual(0, this.CurrentPlayer);
            foreach(var piece in this.m_piecePositions) {
                Assert.True(piece < 0);
            }
        }

        // A Test behaves as an ordinary method
        [Test]
        public void GameLogicTestsSimplePasses()
        {
            // Use the Assert class to test conditions
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator GameLogicTestsWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
