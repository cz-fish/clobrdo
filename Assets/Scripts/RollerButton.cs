using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RollerButton : MonoBehaviour
{
    public GameObject dice;
    public Button button;

    public float boardDiameter = 6f;
    public float forceMin = 1000f;
    public float forceMax = 2000f;

    private System.Random m_random;

    public void RollDice()
    {
        Debug.Log("RollDice");

        if (m_random == null) {
            m_random = new System.Random();
        }

        // Disable the button while rolling, so that the player cannot
        // cheat and reroll
        button.gameObject.SetActive(false);

        var pos = new Vector3(
            (float)(m_random.NextDouble() - 0.5) * boardDiameter,
            (float)(m_random.NextDouble() + 2) * 2f,
            (float)(m_random.NextDouble() - 0.5) * boardDiameter
        );

        var rot = Quaternion.Euler(
            (float)(m_random.NextDouble()) * 360f,
            (float)(m_random.NextDouble()) * 360f,
            (float)(m_random.NextDouble()) * 360f
        );

        var negative = m_random.Next(8);
        var dx = (negative & 1) * 2 - 1;
        var dy = (negative & 2) - 1;
        var dz = (negative & 4) / 2 - 1;

        var push = new Vector3(
            dx * (float)(m_random.NextDouble()) * (forceMax - forceMin) + forceMin,
            dy * (float)(m_random.NextDouble()) * (forceMax - forceMin) + forceMin,
            dz * (float)(m_random.NextDouble()) * (forceMax - forceMin) + forceMin
        );

        dice.transform.position = pos;
        dice.transform.rotation = rot;
        dice.SetActive(true);
        dice.GetComponent<Rigidbody>().AddForce(push);
    }

}
