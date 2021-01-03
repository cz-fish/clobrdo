using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Assets;

public class GameState : MonoBehaviour
{
    public int numPlayers = 4;
    public int piecesPerPlayer = 4;
    public Sprite[] playerSprites;
    public Sprite[] diceSprites;

    private List<GameObject> m_pieces = new List<GameObject>();
    private GameObject m_diceValueImg;
    private GameObject m_playerImg;
    private GameObject m_rollButton;
    private GameLogic m_gameLogic = new GameLogic();

    void Start()
    {
        m_gameLogic.Start();
        InitializePieces();
        m_diceValueImg = GameObject.Find("diceValue");
        m_diceValueImg.SetActive(false);
        m_playerImg = GameObject.Find("nextPlayerIcon");
        m_rollButton = GameObject.Find("rollButton");
    }

    private void InitializePieces()
    {
        for (int playerNr = 0; playerNr < numPlayers; playerNr++)
        {
            var playerColor = BoardCoords.playerOrder[playerNr];
            var prefabName = "piece" + playerColor.Substring(0, 1).ToUpper() + playerColor.Substring(1);
            var prefab = Resources.Load(prefabName) as GameObject;
            for (int pieceNr = 0; pieceNr < piecesPerPlayer; pieceNr++)
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
        m_diceValueImg.GetComponent<Image>().sprite = diceSprites[value - 1];
        m_diceValueImg.SetActive(true);
        var diceResult = m_gameLogic.OnDiceRoll(value);
        if (diceResult.nextPlayer != null) {
            StartCoroutine("NextPlayer");

        } else if (diceResult.humanTurn != null) { 
        } else if (diceResult.aiTurn != null) {
        }
    }

    IEnumerator NextPlayer() {
        yield return new WaitForSeconds(1.5f);
        m_diceValueImg.SetActive(false);
        m_playerImg.GetComponent<Image>().sprite = playerSprites[m_gameLogic.CurrentPlayer];
        m_rollButton.SetActive(true);
    }
}
