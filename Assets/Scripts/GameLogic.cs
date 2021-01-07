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

        public class NextPlayer {
            public int nextPlayer;
        }

        public class Move {
            public int pieceNr;
            public int fromPos;
            public int toPos;
            public int? pieceOnTargetPos;
        }

        public class PossibleMoves {
            public List<Move> Moves;
        }

        // poor man's variant type. OnDiceRoll will return an instance
        // that will have exactly one of the the members non-null.
        public class DiceResult {
            public NextPlayer nextPlayer = null;
            public PossibleMoves humanTurn = null;
            public PossibleMoves aiTurn = null;
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

        public void Start() {
            CurrentPlayer = 0;
            // place are pieces to homes
            m_piecePositions = new List<int>();
            for (var player = 0; player < NumPlayers; player++) {
                for (var piece = 1; piece <= NumPiecesPerPlayer; piece++) {
                    m_piecePositions.Add(-piece);
                }
            }
        }

        public DiceResult OnDiceRoll(int value) {
            LastDiceRoll = value;
            var result = new DiceResult();

            var moves = GetPossibleMoves(CurrentPlayer, value);
            if (moves.Count == 0) {
                // No possible moves, on to the next player
                CurrentPlayer = (CurrentPlayer + 1) % NumPlayers;
                result.nextPlayer = new NextPlayer {nextPlayer = CurrentPlayer};
                return result;
            }

            if (IsHumanPlayer(CurrentPlayer)) {
                result.humanTurn = new PossibleMoves {Moves = moves};
                return result;
            } else {
                result.aiTurn = new PossibleMoves {Moves = moves};
                return result;
            }
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
                        AddBoardMoveIfAllowed(piece, piecePos, PlayerStartingPos(playerNumber));
                    }
                    continue;
                }

                if (piecePos >= 1000) {
                    // piece already in target
                    AddTargetMoveIfAllowed(piece, piecePos, piecePos + diceRoll);
                    continue;
                }

                var logicalPos = (piecePos - PlayerStartingPos(playerNumber));
                if (logicalPos < 0) {
                    logicalPos += BoardSize;
                }
                var nextLogicalPos = logicalPos + diceRoll;
                if (nextLogicalPos >= BoardSize) {
                    // piece will move to the target with this step
                    AddTargetMoveIfAllowed(piece, piecePos, nextLogicalPos % BoardSize + (playerNumber + 1) * 1000);
                    continue;
                }

                // else piece is on the board and will stay on the board
                AddBoardMoveIfAllowed(piece, piecePos, (piecePos + diceRoll) % BoardSize);

            }
            return moves;

            void AddBoardMoveIfAllowed(int piece, int piecePos, int newPos) {
                int? occupier = WhichPieceIsAtPos(newPos);
                // we can only move piece from piecePos to newPos if the newPos is empty,
                // or occuppied by an other player's piece but not our own piece
                if (!occupier.HasValue || WhosePlayerIsPiece(occupier.Value) != playerNumber) {
                    moves.Add(new Move {
                        pieceNr = piece,
                        fromPos = piecePos,
                        toPos = newPos,
                        pieceOnTargetPos = occupier
                    });
                }
            }

            void AddTargetMoveIfAllowed(int piece, int piecePos, int newPos) {
                // we can only move within the target if the newPos is within the target's capacity,
                // and if the newPos is not occuppied yet
                if (newPos % 1000 < NumPiecesPerPlayer && !WhichPieceIsAtPos(newPos).HasValue) {
                    // can move further into the target
                    moves.Add(new Move {
                        pieceNr = piece,
                        fromPos = piecePos,
                        toPos = newPos,
                        pieceOnTargetPos = null
                    });
                }
            }
        }

        public bool IsHumanPlayer(int playerNumber) {
            // FIXME
            return false;
            //return playerNumber == 0;
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
                CurrentPlayer = (CurrentPlayer + 1) % NumPlayers;
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