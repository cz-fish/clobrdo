using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseSelection : MonoBehaviour
{
    
    private GameState m_listener = null;
    private bool m_enabled = false;

    private int m_pieceLayer = 0;

    void Start() {
        m_pieceLayer = LayerMask.NameToLayer("Pieces");
    }

    void Update()
    {
        if (!m_enabled) {
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
        m_enabled = true;
    }

    public void DisablePieceSelection()
    {
        m_enabled = false;
    }
}
