using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using Assets;

public class MenuActions : MonoBehaviour
{
    public GameObject OptionsMenu;

    private Transform m_panelMain;
    private Transform m_panelGameOpts;
    private float m_offset = 0;
    private float m_targetOffset = 0;
    private float m_width = 0;
    private Vector3 m_basePosMain;
    private Vector3 m_basePosGameOpts;

    public void QuitClicked()
    {
        Application.Quit();
    }

    public void PlayClicked()
    {
        SceneManager.LoadScene("Game");
    }

    public void SaveGameOptions()
    {
        GameOptions.ManualDice = GetToggleValue("ToggleManualDice");
        GameOptions.SixPlaysAgain = GetToggleValue("TogglePlaySix");
        GameOptions.StartWithOnePiece = GetToggleValue("ToggleStartWithOne");
        m_targetOffset = 0;
        return;

        bool GetToggleValue(string name) {
            return OptionsMenu.WithChild(name).GetComponent<Toggle>().isOn;
        }
    }

    public void LoadGameOptions()
    {
        SetToggleValue("ToggleManualDice", GameOptions.ManualDice);
        SetToggleValue("TogglePlaySix", GameOptions.SixPlaysAgain);
        SetToggleValue("ToggleStartWithOne", GameOptions.StartWithOnePiece);
        m_targetOffset = m_width;
        return;

        void SetToggleValue(string name, bool value) {
            OptionsMenu.WithChild(name).GetComponent<Toggle>().isOn = value;
        }

    }

    void Awake()
    {
        m_panelGameOpts = GameObject.Find("PanelGameOpts").transform;
        m_panelMain = GameObject.Find("PanelMain").transform;
        m_basePosMain = m_panelMain.position;
        m_basePosGameOpts = m_panelGameOpts.position;
        m_width = m_basePosGameOpts.x - m_basePosMain.x;
        m_offset = m_basePosMain.x;
        m_targetOffset = m_offset;
    }

    void Update()
    {
        if (m_targetOffset == m_offset) {
            return;
        }
        float step = m_width * Time.deltaTime * 2;
        if (m_offset > m_targetOffset) {
            step = -step;
        }
        var newOffset = m_offset + step;
        if (System.Math.Sign(newOffset - m_targetOffset) != System.Math.Sign(m_offset - m_targetOffset)) {
            m_offset = m_targetOffset;
        } else {
            m_offset = newOffset;
        }
        var move = new Vector3(-m_offset, 0, 0);
        m_panelMain.position = m_basePosMain + move;
        m_panelGameOpts.position = m_basePosGameOpts + move;
   }
}
