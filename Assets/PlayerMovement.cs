using Cinemachine.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] Transform orientation;
    [SerializeField] Transform player;

    [SerializeField] float rotationSpeed;

    private void Awake()
    {
        
    }

    private void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 viewDir = new Vector3(transform.position.x, player.position.y, transform.position.z);
        orientation.forward = viewDir.normalized;

        Vector3 inputDir = orientation.forward * v + orientation.right * h;
        if(inputDir != Vector3.zero)
        {
            player.forward = Vector3.Slerp(player.forward, inputDir, rotationSpeed * Time.deltaTime);
        }
    }
}
