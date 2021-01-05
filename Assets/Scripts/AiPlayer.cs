namespace Assets
{
    public class AiPlayer {
        public GameLogic.Move ChooseMove(GameLogic.PossibleMoves possibleMoves) {
            // spawn new piece
            foreach (var move in possibleMoves.Moves) {
                if (move.fromPos < 0) {
                    return move;
                }
            }

            // vacate spawn spot
            foreach (var move in possibleMoves.Moves) {
                if (move.fromPos % 10 == 0) {
                    return move;
                }
            }

            // move to target
            foreach (var move in possibleMoves.Moves) {
                if (move.fromPos < 1000 && move.toPos >= 1000) {
                    return move;
                }
            }

            // bump other player
            foreach (var move in possibleMoves.Moves) {
                if (move.pieceOnTargetPos.HasValue) {
                    return move;
                }
            }

            // any other move
            return possibleMoves.Moves[0];
        }
    }
}