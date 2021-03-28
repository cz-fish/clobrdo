using System.Collections;
using System.Collections.Generic;
using TMPro;
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

    private void SetPlayer(int playerNum)
    {
        var type = GameOptions.playerType[playerNum];
        var color = GameOptions.PlayerColors[playerNum];
        var humanButton = GameObject.Find(color + "Human").GetComponent<Button>();
        var aiButton = GameObject.Find(color + "Ai").GetComponent<Button>();
        var nameLabel = GameObject.Find(color + "Name").GetComponent<TextMeshProUGUI>();
        if (type == GameOptions.PlayerType.Human) {
            ButtonSelected(humanButton);
            ButtonDeselected(aiButton);
            nameLabel.text = "Human Player";
        } else {
            ButtonSelected(aiButton);
            ButtonDeselected(humanButton);
            nameLabel.text = GameOptions.AiPlayerNames[playerNum];
        }
        return;

        void ButtonSelected(Button button) {
            var buttonColor = button.colors;
            buttonColor.normalColor = new Color(1f, 1f, 1f);
            button.colors = buttonColor;
        }
        void ButtonDeselected(Button button) {
            var buttonColor = button.colors;
            buttonColor.normalColor = new Color(0.3f, 0.3f, 0.3f);
            button.colors = buttonColor;
        }
    }

    public void SavePlayerOptions()
    {
        GameObject.Find("MenuPanel").GetComponent<Image>().sprite = MainBackground;
    }

    public void LoadPlayerOptions()
    {
        GameObject.Find("MenuPanel").GetComponent<Image>().sprite = PlainBackground;
        for (int player = 0; player < 4; player++) {
            SetPlayer(player);
        }
    }

    public void PlayerHumanToggle(int playerNum) {
        GameOptions.playerType[playerNum] = GameOptions.PlayerType.Human;
        SetPlayer(playerNum);
    }

    public void PlayerAiToggle(int playerNum) {
        GameOptions.playerType[playerNum] = GameOptions.PlayerType.Ai;
        SetPlayer(playerNum);
    }

    public void SetSimpleBackground() {
        GameObject.Find("MenuPanel").GetComponent<Image>().sprite = PlainBackground;
    }

    public void SetMainBackground() {
        GameObject.Find("MenuPanel").GetComponent<Image>().sprite = MainBackground;
    }
}
