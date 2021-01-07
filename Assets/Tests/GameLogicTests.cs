using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        [Test]
        public void WhichPieceIsAtPosWorksForBoard()
        {
            this.m_piecePositions = new List<int>() {
                1, 2, 3, 4,
                10, 15, 20, 25,
                6, -2, -3, -4,
                4002, 39, -3, -4
            };
            Assert.False(this.WhichPieceIsAtPos(5).HasValue);
            Assert.AreEqual(0, this.WhichPieceIsAtPos(1).Value);
            Assert.AreEqual(7, this.WhichPieceIsAtPos(25).Value);
        }

        [Test]
        public void WhichPieceIsAtPosWorksForTarget()
        {
            this.m_piecePositions = new List<int>() {
                1, 2, 3, 4,
                10, 15, 20, 25,
                6, -2, -3, -4,
                4002, 39, -3, -4
            };
            Assert.False(this.WhichPieceIsAtPos(1000).HasValue);
            Assert.AreEqual(12, this.WhichPieceIsAtPos(4002).Value);
        }

        [Test]
        public void FindFreeHomePosFindsLowestEmptyPosition()
        {
            this.m_piecePositions = new List<int>() {
                -1, -2, -3, -4,
                10, 15, 20, 25,
                6, -2, -3, -4,
                -1, 39, -3, -4
            };
            Assert.AreEqual(-1, this.FindFreeHomePos(2));
            Assert.AreEqual(-2, this.FindFreeHomePos(3));
            Assert.Throws<System.Exception>(delegate {this.FindFreeHomePos(0);});
        }

        [Test]
        public void CountPiecesInTargetReturnsRightCount()
        {
            this.m_piecePositions = new List<int>() {
                1001, 1000, 1003, 1002,
                10, 15, 20, 25,
                6, -2, -3, -4,
                4002, 39, -3, -4
            };
            Assert.AreEqual(4, this.CountPiecesInTarget(0));
            Assert.AreEqual(0, this.CountPiecesInTarget(2));
            Assert.AreEqual(1, this.CountPiecesInTarget(3));
        }

        [Test]
        public void GetPossibleMoves_Spawning()
        {
            this.m_piecePositions = new List<int>() {
                25, -2, -3, -4,
                10, 20, -3, -4,
                -1, -2, -3, -4,
                1, 2, 3, 4
            };

            List<Move> moves;
            // rolled 1, cannot spawn
            moves = this.GetPossibleMoves(2, 1);
            Assert.AreEqual(0, moves.Count);

            // rolled 6, can spawn any of the 3 pieces in home
            moves = this.GetPossibleMoves(0, 6);
            var spawns = getSpawnMoves(moves);
            Assert.AreEqual(3, spawns.Count);

            // rolled 6, but another piece is already on spawn spot
            moves = this.GetPossibleMoves(1, 6);
            spawns = getSpawnMoves(moves);
            Assert.AreEqual(0, spawns.Count);

            // rolled 6 and someone else's piece is on the spawn spot
            // can spawn any of the 4 pieces
            moves = this.GetPossibleMoves(2, 6);
            spawns = getSpawnMoves(moves);
            Assert.AreEqual(4, spawns.Count);
            Assert.AreEqual(5, spawns[0].pieceOnTargetPos.Value);
            Assert.AreEqual(5, spawns[1].pieceOnTargetPos.Value);
            Assert.AreEqual(5, spawns[2].pieceOnTargetPos.Value);
            Assert.AreEqual(5, spawns[3].pieceOnTargetPos.Value);

            // rolled 6, but all pieces already in game, cannot spawn more
            moves = this.GetPossibleMoves(3, 6);
            spawns = getSpawnMoves(moves);
            Assert.AreEqual(0, spawns.Count);

            return;

            List<Move> getSpawnMoves(IEnumerable<Move> allMoves) {
                return (from move in allMoves where move.fromPos < 0 select move).ToList();
            }
        }

        [Test]
        public void GetPossibleMoves_BoardMoves()
        {
            this.m_piecePositions = new List<int>() {
                20, 22, -3, -4,
                38, 18, -3, -4,
                -1, -2, -3, -4,
                -1, -2, -3, -4
            };

            List<Move> moves;
            List<Move> sorted;
            // player 0 rolls 1, both pieces can move
            moves = this.GetPossibleMoves(0, 1);
            sorted = sortByFromPos(moves);
            Assert.AreEqual(2, sorted.Count);
            Assert.AreEqual(21, sorted[0].toPos);
            Assert.AreEqual(23, sorted[1].toPos);

            // player 0 rolls 2, piece on 20 is blocked by piece on 22
            moves = this.GetPossibleMoves(0, 2);
            sorted = sortByFromPos(moves);
            Assert.AreEqual(1, sorted.Count);
            Assert.AreEqual(24, sorted[0].toPos);

            // player 1 rolls 4, piece on 38 wraps around
            // and piece on 18 bumps opponent piece on 22.
            moves = this.GetPossibleMoves(1, 4);
            sorted = sortByFromPos(moves);
            Assert.AreEqual(2, sorted.Count);
            Assert.AreEqual(22, sorted[0].toPos);
            Assert.AreEqual(1, sorted[0].pieceOnTargetPos.Value);
            Assert.AreEqual(2, sorted[1].toPos);

            // player 2 rolls 1, but has no pieces
            moves = this.GetPossibleMoves(2, 1);
            sorted = sortByFromPos(moves);
            Assert.AreEqual(0, sorted.Count);

            return;

            List<Move> sortByFromPos(IEnumerable<Move> allMoves) {
                return (from move in allMoves orderby move.fromPos ascending select move).ToList();
            }
        }
 
        [Test]
        public void GetPossibleMoves_TargetMoves()
        {
            this.m_piecePositions = new List<int>() {
                1000, 1001, 39, -4,
                2002, 9, -3, -4,
                18, -2, -3, -4,
                25, 4002, -3, -4
            };

            List<Move> moves;
            List<Move> sorted;
            // player 0 rolls 3, pieces on 39 and 1000 can move
            moves = this.GetPossibleMoves(0, 3);
            sorted = sortByFromPos(moves);
            Assert.AreEqual(2, sorted.Count);
            Assert.AreEqual(39, sorted[0].fromPos);
            Assert.AreEqual(1002, sorted[0].toPos);
            Assert.AreEqual(1000, sorted[1].fromPos);
            Assert.AreEqual(1003, sorted[1].toPos);

            // player 0 rolls 1, only piece on 1001 can move
            moves = this.GetPossibleMoves(0, 1);
            Assert.AreEqual(1, moves.Count);
            Assert.AreEqual(1001, moves[0].fromPos);
            Assert.AreEqual(1002, moves[0].toPos);

            // player 1 rolls 4, piece on 9 goes to target
            moves = this.GetPossibleMoves(1, 4);
            Assert.AreEqual(1, moves.Count);
            Assert.AreEqual(9, moves[0].fromPos);
            Assert.AreEqual(2003, moves[0].toPos);

            // player 2 rolls 5, piece on 18 goes to target
            moves = this.GetPossibleMoves(2, 5);
            Assert.AreEqual(1, moves.Count);
            Assert.AreEqual(18, moves[0].fromPos);
            Assert.AreEqual(3003, moves[0].toPos);

            // player 3 rolls 5, piece on 25 goes to target
            moves = this.GetPossibleMoves(3, 5);
            Assert.AreEqual(1, moves.Count);
            Assert.AreEqual(25, moves[0].fromPos);
            Assert.AreEqual(4000, moves[0].toPos);

            // player 3 rolls 1, piece on 25 goes to 26 and 4002 to 4003
            moves = this.GetPossibleMoves(3, 1);
            sorted = sortByFromPos(moves);
            Assert.AreEqual(2, sorted.Count);
            Assert.AreEqual(25, sorted[0].fromPos);
            Assert.AreEqual(26, sorted[0].toPos);
            Assert.AreEqual(4002, sorted[1].fromPos);
            Assert.AreEqual(4003, sorted[1].toPos);

            return;

            List<Move> sortByFromPos(IEnumerable<Move> allMoves) {
                return (from move in allMoves orderby move.fromPos ascending select move).ToList();
            }
        }
    }
}
