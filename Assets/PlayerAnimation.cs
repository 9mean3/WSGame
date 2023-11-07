using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    public Animator AnimatorCompo { get; private set; }

    private readonly int blendHash = Animator.StringToHash("blend");

    private void Awake()
    {
        AnimatorCompo = GetComponent<Animator>();
    }

    public void SetBlendHash(float blend)
    {
        AnimatorCompo.SetFloat(blendHash, blend);
    }
}
