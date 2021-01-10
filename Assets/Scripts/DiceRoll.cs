using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceRoll : MonoBehaviour
{
    public GameState gameState;

    private Rigidbody m_rb;
    private bool m_hiding = false;
    private float m_rollingTime = 0;

    private const float MaxRollingTime = 10f;

    // Start is called before the first frame update
    void Start()
    {
        m_rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame, when the dice is active and rolling
    void Update()
    {
        if (!m_hiding) {
            // the dice is rolling
            m_rollingTime += Time.deltaTime;

            if ((m_rb.velocity.magnitude < 0.0001f && GetComponent<Transform>().position.y < 1f)  // the dice has stopped
                || (m_rollingTime > MaxRollingTime)  // or the dice has been rolling for too long
            ) {
                // take the dice value
                var dice = GetComponent<Die_d6>();
                gameState.OnDiceRoll(dice.value);

                m_hiding = true;
                // deactivate and hide the dice after a bit of time
                StartCoroutine(HideDice());
            }
        }
    }

    IEnumerator HideDice() {
        yield return new WaitForSeconds(0.4f);
        Debug.Log("Hiding the dice");
        m_hiding = false;
        m_rollingTime = 0f;
        gameObject.SetActive(false);
    }
}
