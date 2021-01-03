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
    private int m_lastDiceRoll = 0;
    private GameObject m_diceValueImg;

    void Start()
    {
        InitializePieces();
        m_diceValueImg = GameObject.Find("diceValue");
        m_diceValueImg.SetActive(false);
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
        m_lastDiceRoll = value;
        m_diceValueImg.GetComponent<Image>().sprite = diceSprites[value - 1];
        m_diceValueImg.SetActive(true);
    }

}
