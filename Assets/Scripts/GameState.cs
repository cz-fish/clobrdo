﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Assets;

public class GameState : MonoBehaviour
{
    // == Public types
        public enum PlayerType {
        Human,
        Ai
    }

    // == Game options
    // If true, each players starts with one piece already spawned
    public bool startWithOnePieceUp = true;

    public PlayerType[] players = {
        PlayerType.Human,
        PlayerType.Ai,
        PlayerType.Ai,
        PlayerType.Ai
    };

    // == Unity objects
    // sprites drawn on the UI canvas
    public Sprite[] playerSprites;
    public Sprite[] diceSprites;

    // == Private types
    // representation of a single move animation
    class MovingPiece {
        public GameObject piece {get;set;}
        public Vector3 start {get;set;}
        public Vector3 target {get;set;}
        public float phase {get;set;}
    }

    // == Private references to Unity objects
    // references to all game pieces, on the same order as GameLogic has them
    private List<GameObject> m_pieces = new List<GameObject>();
    private GameObject m_diceValueImg;
    private GameObject m_playerImg;
    private GameObject m_rollButton;
    private GameObject m_dice;

    // == Private members
    // implementation of the game rules and logic
    private GameLogic m_gameLogic = new GameLogic();
    private AiPlayer m_aiPlayer = new AiPlayer();

    // currently moving piece, or null if no piece is currently moving
    private MovingPiece m_move = null;
    // length of the move (jumping) animation
    private float m_jumpLength = 0.5f;

    // Game start
    void Start()
    {
        m_gameLogic.Start(startWithOnePieceUp);
        InitializePieces();

        // find references to Unity objects
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

    // Piece move animation
    void Update()
    {
        if (m_move != null) {
            m_move.phase += Time.deltaTime;
            float jumpPercent = m_move.phase / m_jumpLength;
            Vector3 pos = m_move.target;
            if (jumpPercent < 1.0f) {
                pos = m_move.start + (m_move.target - m_move.start) * jumpPercent;
                pos.y = (float)Math.Sin(Math.PI * jumpPercent);
                m_move.piece.transform.parent.position = pos;
            } else {
                // jumping is done
                m_move.piece.transform.parent.position = m_move.target;
                m_move = null;
            }
        }
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
                var pos = GetPieceCoords(playerNr, pieceNr);
                // The piece needs to be in an empty parent object so that its animations are local
                // to the parent's transformation. That way we can move the parent around and animation
                // of the piece will happen in the position of the parent and not the center of the board.
                var parent = new GameObject("pieceParent");
                parent.transform.position = pos;
                var piece = Instantiate(prefab, pos, Quaternion.identity).gameObject;
                piece.transform.parent = parent.transform;
                piece.SetActive(true);
                m_pieces.Add(piece);
            }
        }

        return;

        Vector3 GetPieceCoords(int playerNr, int pieceNr) {
            var piecePos = m_gameLogic.GetPiecePos(playerNr * GameLogic.NumPiecesPerPlayer + pieceNr);
            if (piecePos < 0) {
                return BoardCoords.getHomeCoords(playerNr, pieceNr);
            } else {
                return BoardCoords.getPositionCoords(piecePos);
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
        yield return PieceMove(move);
    }

    IEnumerator PieceMove(GameLogic.Move move) {
        var piece = m_pieces[move.pieceNr];
        Vector3 newCoords;
        if (move.toPos < GameLogic.BoardSize) {
            newCoords = BoardCoords.getPositionCoords(move.toPos);
        } else {
            newCoords = BoardCoords.getTargetCoords(m_gameLogic.CurrentPlayer, move.toPos % 1000);
        }

        GameObject bumpedPiece = null;
        Vector3? bumpTargetPos = null;
        if (move.pieceOnTargetPos.HasValue) {
            var bumpedPieceIdx = move.pieceOnTargetPos.Value;
            var bumpedOwner = m_gameLogic.WhosePlayerIsPiece(bumpedPieceIdx);
            var bumpHomePos = (-m_gameLogic.FindFreeHomePos(bumpedOwner)) - 1;
            bumpedPiece = m_pieces[bumpedPieceIdx];
            bumpTargetPos = BoardCoords.getHomeCoords(bumpedOwner, bumpHomePos);
        }

        m_move = new MovingPiece() {
            piece = piece,
            start = piece.transform.parent.position,
            target = newCoords,
            phase = 0f
        };

        yield return new WaitForSeconds(m_jumpLength);

        if (bumpedPiece != null) {
            bumpedPiece.transform.parent.position = bumpTargetPos.Value;
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
