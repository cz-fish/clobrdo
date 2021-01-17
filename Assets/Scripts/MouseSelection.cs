using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseSelection : MonoBehaviour
{
    
    private GameState m_listener = null;
    public bool Enabled {get; protected set;}

    private int m_pieceLayer = 0;

    void Start() {
        Enabled = false;
        m_pieceLayer = LayerMask.NameToLayer("Pieces");
    }

    void Update()
    {
        if (!Enabled) {
            return;
        }
        if (Input.GetMouseButton(0)) {
            var piece = FindPointedPiece();
            if (piece == null) {
                return;
            }
            m_listener.OnPieceSelected(piece);
        }
    }

    // Update is called once per frame
    GameObject FindPointedPiece()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        int layerMask = 1 << m_pieceLayer;
        if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, layerMask)) {
            return hitInfo.transform.root.gameObject;
        } else {
            return null;
        }
    }

    public void EnablePieceSelection(GameState listener)
    {
        m_listener = listener;
        Enabled = true;
    }

    public void DisablePieceSelection()
    {
        Enabled = false;
    }
}
