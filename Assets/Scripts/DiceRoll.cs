using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceRoll : MonoBehaviour
{
    public GameState gameState;

    private Rigidbody m_rb;
    private bool m_hiding = false;

    // Start is called before the first frame update
    void Start()
    {
        m_rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame, when the dice is active and rolling
    void Update()
    {
        if (!m_hiding && m_rb.velocity.magnitude < 0.0001f && GetComponent<Transform>().position.y < 1f) {
            // the dice has stopped
            var dice = GetComponent<Die_d6>();

            //if (dice.value > 0) {
                gameState.OnDiceRoll(dice.value);
            //}

            m_hiding = true;
            // deactivate and hide the dice after 1 second
            StartCoroutine(HideDice());
        }
    }

    IEnumerator HideDice() {
        yield return new WaitForSeconds(0.4f);
        Debug.Log("Hiding the dice");
        m_hiding = false;
        gameObject.SetActive(false);
    }
}
