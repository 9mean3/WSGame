using Cinemachine.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] Transform camPivot;

    private void Awake()
    {
        
    }

    private void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        if(Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f)
        {
            
        }
    }
}
