using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceAnimation : MonoBehaviour
{
    private Animator m_syncAnimator;

    // Start is called before the first frame update
    void Awake()
    {
        m_syncAnimator = GameObject.Find("SyncAnimator").GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        var animator = GetComponent<Animator>();
        animator.Play(0, -1, m_syncAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime);
    }
}
