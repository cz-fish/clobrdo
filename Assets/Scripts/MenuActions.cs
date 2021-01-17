using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using Assets;

public class MenuActions : MonoBehaviour
{
    public GameObject OptionsMenu;

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
        return;

        void SetToggleValue(string name, bool value) {
            OptionsMenu.WithChild(name).GetComponent<Toggle>().isOn = value;
        }

    }
}
