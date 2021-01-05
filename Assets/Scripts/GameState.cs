using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Assets;

public class GameState : MonoBehaviour
{
    public Sprite[] playerSprites;
    public Sprite[] diceSprites;

    private List<GameObject> m_pieces = new List<GameObject>();
    private GameObject m_diceValueImg;
    private GameObject m_playerImg;
    private GameObject m_rollButton;
    private GameObject m_dice;
    private GameLogic m_gameLogic = new GameLogic();
    private AiPlayer m_aiPlayer = new AiPlayer();

    void Start()
    {
        m_gameLogic.Start();
        InitializePieces();
        m_diceValueImg = GameObject.Find("diceValue");
        m_diceValueImg.SetActive(false);
        m_playerImg = GameObject.Find("nextPlayerIcon");
        m_rollButton = GameObject.Find("rollButton");
        m_dice = GameObject.Find("dice");
        m_dice.SetActive(false);

        // This starts the first player as an AI player
        // FIXME: the first player should be human
        StartCoroutine(NextPlayer());
    }

    private void InitializePieces()
    {
        for (int playerNr = 0; playerNr < GameLogic.NumPlayers; playerNr++)
        {
            var playerColor = BoardCoords.playerOrder[playerNr];
            var prefabName = "piece" + playerColor.Substring(0, 1).ToUpper() + playerColor.Substring(1);
            var prefab = Resources.Load(prefabName) as GameObject;
            for (int pieceNr = 0; pieceNr < GameLogic.NumPiecesPerPlayer; pieceNr++)
            {
                var pos = BoardCoords.getHomeCoords(playerNr, pieceNr);
                var piece = Instantiate(prefab, pos, Quaternion.identity).gameObject;
                piece.SetActive(true);
                m_pieces.Add(piece);
            }
        }
    }

    public void OnDiceRoll(int value)
    {
        if (value == 0) {
            Debug.Log("Dice fell on its edge, roll again");
            StartCoroutine(NextPlayer());
            return;
        }

        Debug.Log($"Dice roll completed: value {value}");

        m_diceValueImg.GetComponent<Image>().sprite = diceSprites[value - 1];
        m_diceValueImg.SetActive(true);
        var diceResult = m_gameLogic.OnDiceRoll(value);
        if (diceResult.nextPlayer != null) {
            Debug.Log("No possible moves, moving to next player");
            StartCoroutine(NextPlayer());
        } else if (diceResult.humanTurn != null) {
            Debug.Log($"Human turn, {diceResult.humanTurn.Moves.Count} moves");
            // TODO
        } else if (diceResult.aiTurn != null) {
            Debug.Log($"AI turn, {diceResult.aiTurn.Moves.Count} moves");
            StartCoroutine(AiPlayerMove(diceResult.aiTurn));
        }
    }

    IEnumerator NextPlayer() {
        // Note: this wait time has to be greater than that of DiceRoll.HideDice(), otherwise it will
        //       hide our dice as soon as the next ai player rolls it.
        yield return new WaitForSeconds(0.6f);
        m_diceValueImg.SetActive(false);
        m_playerImg.GetComponent<Image>().sprite = playerSprites[m_gameLogic.CurrentPlayer];
        if (m_gameLogic.IsHumanPlayer(m_gameLogic.CurrentPlayer)) {
            Debug.Log("Next player is human, showing diceroll button");
            m_rollButton.SetActive(true);
        } else {
            Debug.Log("Next player is AI, rolling the dice");
            var roller = m_dice.GetComponent<RollerButton>();
            roller.RollDice();
        }
    }

    IEnumerator AiPlayerMove(GameLogic.PossibleMoves possibleMoves) {
        yield return new WaitForSeconds(0.2f);
        var move = m_aiPlayer.ChooseMove(possibleMoves);

        Debug.Log($"Executing AI move: piece {move.pieceNr} to pos  {move.toPos}");

        // TODO: animate the move
        var piece = m_pieces[move.pieceNr];
        Vector3 newCoords;
        if (move.toPos < GameLogic.BoardSize) {
            newCoords = BoardCoords.getPositionCoords(move.toPos);
        } else {
            newCoords = BoardCoords.getTargetCoords(m_gameLogic.CurrentPlayer, move.toPos % 1000);
        }
        piece.transform.position = newCoords;

        if (move.pieceOnTargetPos.HasValue) {
            var bumpedPiece = move.pieceOnTargetPos.Value;
            var bumpedOwner = m_gameLogic.WhosePlayerIsPiece(bumpedPiece);
            var bumpHomePos = (-m_gameLogic.FindFreeHomePos(bumpedOwner)) - 1;
            m_pieces[bumpedPiece].transform.position = BoardCoords.getHomeCoords(bumpedOwner, bumpHomePos);
        }

        var result = m_gameLogic.ExecuteMove(move);
        if (result.gameOver != null) {
            Debug.Log("Game over");
            // TODO: game over
        } else {
            StartCoroutine(NextPlayer());
        }
    }

}
