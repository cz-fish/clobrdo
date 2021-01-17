using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Assets;

public class GameState : MonoBehaviour
{
    // == Public types
        public enum PlayerType {
        Human,
        Ai
    }

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
    private MouseSelection m_mouseSelection;

    // == Private members
    // implementation of the game rules and logic
    private GameLogic m_gameLogic = new GameLogic();
    private AiPlayer m_aiPlayer = new AiPlayer();

    // currently moving piece, or null if no piece is currently moving
    private MovingPiece m_move = null;
    // list of all moves that the current human player can choose from
    private List<GameLogic.Move> m_pendingMoves = null;
    // length of the move (jumping) animation
    private float m_jumpLength = 0.5f;

    private bool m_paused = false;
    private bool m_mouseWasEnabledBeforePause = false;

    // Game start
    void Start()
    {
        m_gameLogic.Start();
        InitializePieces();

        // find references to Unity objects
        m_diceValueImg = GameObject.Find("diceValue");
        m_diceValueImg.SetActive(false);
        m_playerImg = GameObject.Find("nextPlayerIcon");

        m_rollButton = GameObject.Find("rollButton");

        m_dice = GameObject.Find("dice");
        m_dice.SetActive(false);

        m_mouseSelection = GameObject.Find("MouseManager").GetComponent<MouseSelection>();

        StartCoroutine(NextPlayer());
    }

    // Piece move animation
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape)) {
            if (m_paused) {
                Resume();
            } else {
                Pause();
            }
        }

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
        int pieceLayer = LayerMask.NameToLayer("Pieces");

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
                piece.AddComponent<PieceAnimation>();
                piece.SetActive(true);

                // Add animator and animation controller
                var animator = piece.AddComponent<Animator>();
                animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Anim/pieceBase");
                // To test that the animator works:
                //animator.SetBool("Jump", true);

                // Put all pieces on a layer, for raycasting.
                piece.layer = pieceLayer;
                foreach(Transform child in piece.transform) {
                    child.gameObject.layer = pieceLayer;
                }

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

        // FIXME: just for gameover testing
        //value = 6;

        Debug.Log($"Dice roll completed: value {value}");

        m_diceValueImg.GetComponent<Image>().sprite = diceSprites[value - 1];
        m_diceValueImg.SetActive(true);
        var possibleMoves = m_gameLogic.OnDiceRoll(value);
        if (possibleMoves.Count == 0) {
            Debug.Log("No possible moves, moving to next player");
            StartCoroutine(NextPlayer());
        } else {
            var playerType = players[m_gameLogic.CurrentPlayer];
            if (playerType == PlayerType.Human) {
                Debug.Log($"Human turn, {possibleMoves.Count} moves");
                HighlightPossibleMoves(possibleMoves);
                EnablePlayerInput();
            } else if (playerType == PlayerType.Ai) {
                Debug.Log($"AI turn, {possibleMoves.Count} moves");
                StartCoroutine(AiPlayerMove(possibleMoves));
            }
        }
    }

    void HighlightPossibleMoves(List<GameLogic.Move> moves) {
        m_pendingMoves = moves;
        foreach (var move in moves) {
            var piece = m_pieces[move.pieceNr];
            var animator = piece.GetComponent<Animator>();
            animator.SetBool("Jump", true);
            //animator.GetCurrentAnimatorStateInfo(0).normalizedTime = 0;
        }
    }

    void EnablePlayerInput() {
        m_mouseSelection.EnablePieceSelection(this);
    }

    void DisablePlayerInput() {
        m_mouseSelection.DisablePieceSelection();
        foreach (var move in m_pendingMoves) {
            var piece = m_pieces[move.pieceNr];
            piece.GetComponent<Animator>().SetBool("Jump", false);
        }
        m_pendingMoves = null;
    }

    bool IsHumanPlayer() {
        var playerType = players[m_gameLogic.CurrentPlayer];
        return playerType == PlayerType.Human;
    }

    IEnumerator NextPlayer() {
        // Note: this wait time has to be greater than that of DiceRoll.HideDice(), otherwise it will
        //       hide our dice as soon as the next ai player rolls it.
        yield return new WaitForSeconds(0.6f);
        m_diceValueImg.SetActive(false);
        m_playerImg.GetComponent<Image>().sprite = playerSprites[m_gameLogic.CurrentPlayer];
        if (IsHumanPlayer() && GameOptions.ManualDice) {
            // Show the Roll button for the player to roll at their convenience
            m_rollButton.SetActive(true);
        } else {
            // Auto roll dice
            var roller = m_dice.GetComponent<RollerButton>();
            roller.RollDice();
        }
    }

    IEnumerator AiPlayerMove(List<GameLogic.Move> possibleMoves) {
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
            GameOver();
        } else {
            StartCoroutine(NextPlayer());
        }
    }

    string PlayerName(int playerId) {
        // TODO: give player some names
        var players = new Dictionary<int, string>() {
            {0, "blue"},
            {1, "yellow"},
            {2, "red"},
            {3, "green"}
        };
        if (players.ContainsKey(playerId)) {
            return players[playerId];
        } else {
            return "unknown";
        }
    }

    void GameOver() {
        var gameCanvas = GameObject.Find("Canvas");
        var endgameCanvas = GameObject.Find("GameOver");
        var particles = GameObject.Find("ParticleEffects");

        // disable main game canvas
        foreach (Transform child in gameCanvas.transform) {
            child.gameObject.SetActive(false);
        }

        bool win = IsHumanPlayer();
        if (win) {
            endgameCanvas.WithChild("TextWin").SetActive(true);
            endgameCanvas.WithChild("WinnerName").SetActive(false);
            var part = particles.WithChild("Win");
            part.SetActive(true);
            part.GetComponent<ParticleSystem>().Play();
        } else {
            endgameCanvas.WithChild("TextWin").SetActive(false);
            var text = endgameCanvas.WithChild("WinnerName");
            var tm = text.GetComponent<TextMeshProUGUI>();
            tm.text = $"{PlayerName(m_gameLogic.CurrentPlayer)} WON!";
            text.SetActive(true);
            var part = particles.WithChild("Lose");
            part.SetActive(true);
            part.GetComponent<ParticleSystem>().Play();
        }
    }


    public void OnPieceSelected(GameObject pieceParent) {
        // The mouse raycast returns the piece parent object
        // We want to find the actual piece
        GameObject piece = null;
        if (pieceParent.transform.childCount == 1) {
            piece = pieceParent.transform.GetChild(0).gameObject;
        } else {
            piece = pieceParent;
        }
        // find out the index of the selected piece
        int selectedPieceIdx = -1;
        for (var i = 0; i < m_pieces.Count; i++) {
            if (piece == m_pieces[i]) {
                selectedPieceIdx = i;
                break;
            }
        }
        if (selectedPieceIdx == -1) {
            return;
        }

        // check that the piece is playable
        GameLogic.Move selectedMove = null;
        foreach(var move in m_pendingMoves) {
            if (move.pieceNr == selectedPieceIdx) {
                selectedMove = move;
                break;
            }
        }
        if (selectedMove == null) {
            return;
        }

        DisablePlayerInput();
        StartCoroutine(PieceMove(selectedMove));
    }

    public void Pause()
    {
        if (m_paused) {
            return;
        }
        Time.timeScale = 0;
        m_paused = true;
        m_mouseWasEnabledBeforePause = m_mouseSelection.Enabled;
        m_mouseSelection.DisablePieceSelection();
        var menu = GameObject.Find("GameMenu").WithChild("InGameMenu");
        menu.SetActive(true);
    }

    public void Resume()
    {
        m_paused = false;
        var menu = GameObject.Find("GameMenu").WithChild("InGameMenu");
        menu.SetActive(false);
        Time.timeScale = 1;
        if (m_mouseWasEnabledBeforePause) {
            EnablePlayerInput();
        }
    }

    public void QuitToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
