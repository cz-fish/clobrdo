using System.Collections.Generic;

namespace Assets {
    public class GameLogic {

        public const int NumPlayers = 4;
        public const int NumPiecesPerPlayer = 4;
        // Number of positions between two adjacent players' starting positions
        public const int PlayerPosOffset = 10;
        public const int BoardSize = NumPlayers * PlayerPosOffset;

        // positions of all players' pieces serialized (first player, second player, third, ...)
        // positions on the board are 0 .. BoardSize
        // positions at home are -1, -2, -3, -4
        // positions in the target place are 1000 .. 1003 for first player, 2000 .. 2003 for second player, ...
        protected List<int> m_piecePositions;

        // If true, player who rolled 6 plays again
        protected bool m_rollAgainOnSix;

        public class NextPlayer {
            public int nextPlayer;
        }

        public class Move {
            public int pieceNr;
            public int fromPos;
            public int toPos;
            public int? pieceOnTargetPos;
            // fromPos, but adjusted by player's starting position, so that
            // higher logicalBoardPos always means closer to target
            public int logicalBoardPos;
            public int rollValue;
        }

        public class GameOver {
            public int winningPlayer;
        }

        public class MoveResult {
            public NextPlayer nextPlayer = null;
            public GameOver gameOver = null;
        }

        public int CurrentPlayer {
            get; protected set;
        }
        public int LastDiceRoll {
            get; protected set;
        }

        // If startWithOnePieceUp, all players will start with one piece already spawned,
        // so they don't need to wait to roll 6 first
        public void Start(bool startWithOnePieceUp = false, bool rollAgainOnSix = false) {
            m_rollAgainOnSix = rollAgainOnSix;
            CurrentPlayer = 0;
            // place are pieces to homes
            m_piecePositions = new List<int>();
            for (var player = 0; player < NumPlayers; player++) {
                for (var piece = 1; piece <= NumPiecesPerPlayer; piece++) {
                    if (startWithOnePieceUp && piece == 1) {
                        m_piecePositions.Add(PlayerStartingPos(player));
                    } else {
                        m_piecePositions.Add(-piece);
                    }
                }
            }
            /*
            // FIXME: just for gameover testing
            m_piecePositions = new List<int>() {
                1000, 1001, 1002, 37,
                -1, -2, -3, -4,
                -1, -2, -3, -4,
                -1, -2, -3, -4
            };
            */
        }

        public List<Move> OnDiceRoll(int value) {
            LastDiceRoll = value;
            var moves = GetPossibleMoves(CurrentPlayer, value);
            if (moves.Count == 0) {
                // No possible moves, on to the next player
                CurrentPlayer = (CurrentPlayer + 1) % NumPlayers;
            }
            return moves;
        }

        public int GetPiecePos(int pieceIndex) {
            return m_piecePositions[pieceIndex];
        }

        public int PlayerStartingPos(int playerNumber) {
            return playerNumber * PlayerPosOffset;
        }

        protected int? WhichPieceIsAtPos(int pos) {
            for (var pieceNr = 0; pieceNr < NumPlayers * NumPiecesPerPlayer; pieceNr++) {
                if (m_piecePositions[pieceNr] == pos) {
                    return pieceNr;
                }
            }
            return null;
        }

        public int WhosePlayerIsPiece(int pieceNr) {
            return pieceNr / NumPiecesPerPlayer;
        }

        protected List<Move> GetPossibleMoves(int playerNumber, int diceRoll) {
            var moves = new List<Move>();
            for (int piece = playerNumber * NumPiecesPerPlayer; piece < (playerNumber + 1) * NumPiecesPerPlayer; piece++) {

                var piecePos = m_piecePositions[piece];

                if (piecePos < 0) {
                    // piece is in home, need to roll 6 to spawn it
                    if (diceRoll == 6) {
                        AddBoardMoveIfAllowed(piece, piecePos, PlayerStartingPos(playerNumber), piecePos, diceRoll);
                    }
                    continue;
                }

                if (piecePos >= 1000) {
                    // piece already in target
                    AddTargetMoveIfAllowed(piece, piecePos, piecePos + diceRoll, diceRoll);
                    continue;
                }

                var logicalPos = (piecePos - PlayerStartingPos(playerNumber));
                if (logicalPos < 0) {
                    logicalPos += BoardSize;
                }
                var nextLogicalPos = logicalPos + diceRoll;
                if (nextLogicalPos >= BoardSize) {
                    // piece will move to the target with this step
                    AddTargetMoveIfAllowed(piece, piecePos, nextLogicalPos % BoardSize + (playerNumber + 1) * 1000, diceRoll);
                    continue;
                }

                // else piece is on the board and will stay on the board
                AddBoardMoveIfAllowed(piece, piecePos, (piecePos + diceRoll) % BoardSize, logicalPos, diceRoll);

            }
            return moves;

            void AddBoardMoveIfAllowed(int piece, int piecePos, int newPos, int logicalPos, int diceValue) {
                int? occupier = WhichPieceIsAtPos(newPos);
                // we can only move piece from piecePos to newPos if the newPos is empty,
                // or occuppied by an other player's piece but not our own piece
                if (!occupier.HasValue || WhosePlayerIsPiece(occupier.Value) != playerNumber) {
                    moves.Add(new Move {
                        pieceNr = piece,
                        fromPos = piecePos,
                        toPos = newPos,
                        pieceOnTargetPos = occupier,
                        logicalBoardPos = logicalPos,
                        rollValue = diceValue
                    });
                }
            }

            void AddTargetMoveIfAllowed(int piece, int piecePos, int newPos, int diceValue) {
                // we can only move within the target if the newPos is within the target's capacity,
                // and if the newPos is not occuppied yet
                if (newPos % 1000 < NumPiecesPerPlayer && !WhichPieceIsAtPos(newPos).HasValue) {
                    // can move further into the target
                    moves.Add(new Move {
                        pieceNr = piece,
                        fromPos = piecePos,
                        toPos = newPos,
                        pieceOnTargetPos = null,
                        logicalBoardPos = 0,    // not that important to the AI
                        rollValue = diceValue
                    });
                }
            }
        }

        public MoveResult ExecuteMove(Move move) {
            MoveResult result = new MoveResult();

            m_piecePositions[move.pieceNr] = move.toPos;
            if (move.pieceOnTargetPos.HasValue) {
                var bumpedPiece = move.pieceOnTargetPos.Value;
                var bumpedOwner = WhosePlayerIsPiece(bumpedPiece);
                var bumpHomePos = FindFreeHomePos(bumpedOwner);
                m_piecePositions[bumpedPiece] = bumpHomePos;
            }

            var piecesInTarget = CountPiecesInTarget(CurrentPlayer);
            if (piecesInTarget == NumPiecesPerPlayer) {
                result.gameOver = new GameOver {winningPlayer = CurrentPlayer};
            } else {
                UnityEngine.Debug.Log($"roll value {move.rollValue}, roll again on six {m_rollAgainOnSix}");
                if (move.rollValue != 6 || !m_rollAgainOnSix) {
                    CurrentPlayer = (CurrentPlayer + 1) % NumPlayers;
                }
                result.nextPlayer = new NextPlayer {nextPlayer = CurrentPlayer};
            }
            return result;
        }

        public int FindFreeHomePos(int playerNr) {
            var used = new HashSet<int>();
            for (int pieceNr = playerNr * NumPiecesPerPlayer; pieceNr < (playerNr + 1) * NumPiecesPerPlayer; pieceNr++) {
                if (m_piecePositions[pieceNr] < 0) {
                    used.Add(m_piecePositions[pieceNr]);
                }
            }
            for (int opt = -1; opt >= -NumPiecesPerPlayer; opt--) {
                if (!used.Contains(opt)) {
                    return opt;
                }
            }
            throw new System.Exception("Home is full");
        }

        public int CountPiecesInTarget(int playerNr) {
            var counter = 0;
            for (int pieceNr = playerNr * NumPiecesPerPlayer; pieceNr < (playerNr + 1) * NumPiecesPerPlayer; pieceNr++) {
                if (m_piecePositions[pieceNr] >= 1000) {
                    counter++;
                }
            }
            return counter;
        }

    }

}