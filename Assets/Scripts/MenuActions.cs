using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using Assets;

public class MenuActions : MonoBehaviour
{
    public GameObject OptionsMenu;
    public Sprite PlainBackground;
    public Sprite MainBackground;

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

        GameObject.Find("MenuPanel").GetComponent<Image>().sprite = MainBackground;
        return;

        bool GetToggleValue(string name) {
            return OptionsMenu.WithChild(name).GetComponent<Toggle>().isOn;
        }
    }

    public void LoadGameOptions()
    {
        GameObject.Find("MenuPanel").GetComponent<Image>().sprite = PlainBackground;
        SetToggleValue("ToggleManualDice", GameOptions.ManualDice);
        SetToggleValue("TogglePlaySix", GameOptions.SixPlaysAgain);
        SetToggleValue("ToggleStartWithOne", GameOptions.StartWithOnePiece);
        return;

        void SetToggleValue(string name, bool value) {
            OptionsMenu.WithChild(name).GetComponent<Toggle>().isOn = value;
        }

    }
}
