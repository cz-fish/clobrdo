using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Assets;

public class GameState : MonoBehaviour
{
    public int numPlayers = 4;
    public int piecesPerPlayer = 4;

    public Material material;

    private List<GameObject> pieces = new List<GameObject>();
    void Start()
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
                pieces.Add(piece);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
