using System.Collections.Generic;

namespace Assets {
public static class GameOptions {
    // If true, player has to press the Roll button to roll the dice,
    // otherwise it just rolls automatically.
    public static bool ManualDice = false;
    // If true, player who rolled six keeps playing
    public static bool SixPlaysAgain = true;
    // If true, each players starts with one piece already spawned
    public static bool StartWithOnePiece = true;

    public enum PlayerType {
        Human,
        Ai
    }

    public static PlayerType[] playerType = {
        PlayerType.Human,
        PlayerType.Ai,
        PlayerType.Ai,
        PlayerType.Ai
    };

    public static Dictionary<int, string> PlayerColors = new Dictionary<int, string>(){
        {0, "Blue"},
        {1, "Yellow"},
        {2, "Red"},
        {3, "Green"}
    };

    public static Dictionary<int, string> AiPlayerNames = new Dictionary<int, string>() {
        {0, "Droppy"},
        {1, "Lightmon"},
        {2, "Carred"},
        {3, "Broccus"}
    };
}

}