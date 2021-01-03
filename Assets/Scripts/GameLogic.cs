using System.Collections.Generic;

namespace Assets {
    public class GameLogic {

        const int NumPlayers = 4;

        public class NextPlayer {
            public int nextPlayer { get; protected set; }
            public NextPlayer(int nextPlayerNumber) {
                nextPlayer = nextPlayerNumber;
            }
        }

        public class PlayablePieces {
            public List<int> pieces { get; protected set; }
            public PlayablePieces(List<int> playablePieces) {
                pieces = playablePieces;
            }
        }

        // poor man's variant type. OnDiceRoll will return an instance
        // that will have exactly one of the the members non-null.
        public class DiceResult {
            public NextPlayer nextPlayer = null;
            public PlayablePieces humanTurn = null;
            public PlayablePieces aiTurn = null;
        }

        public int CurrentPlayer {
            get; protected set;
        }
        public int LastDiceRoll {
            get; protected set;
        }

        public void Start() {
            CurrentPlayer = 0;
        }

        public DiceResult OnDiceRoll(int value) {
            LastDiceRoll = value;
            var result = new DiceResult();

            var playable = GetPlayablePieces();
            if (playable.Count == 0) {
                CurrentPlayer = (CurrentPlayer + 1) % NumPlayers;
                result.nextPlayer = new NextPlayer(CurrentPlayer);
                return result;
            }

            if (IsHumanPlayer(CurrentPlayer)) {
                result.humanTurn = new PlayablePieces(playable);
                return result;
            } else {
                result.aiTurn = new PlayablePieces(playable);
                return result;
            }
        }

        private List<int> GetPlayablePieces() {
            // TODO: find playable pieces of the current player
            return new List<int>();
        }

        private bool IsHumanPlayer(int playerNumber) {
            return playerNumber == 0;
        }

    }

}