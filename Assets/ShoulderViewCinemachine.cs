using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ShoulderViewCinemachine : MonoBehaviour
{
    [SerializeField] LayerMask whatIsObstacle;
    CinemachineVirtualCamera thisCam;
    [SerializeField] Transform target;
    [SerializeField] float followDistance;
    Vector3 dir;

    private void Start()
    {
        thisCam = GetComponent<CinemachineVirtualCamera>();
        thisCam.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.z = followDistance;
    }

    void Update()
    {
        float x = Input.GetAxis("Mouse X");
        float y = Input.GetAxis("Mouse Y");

        dir += new Vector3(-y, x, 0);
        dir.x = Mathf.Clamp(dir.x, -80, 80);
        target.localEulerAngles = dir;

        CKObstacle();
    }

    void CKObstacle()
    {
        Ray playertocamRay = new Ray(target.position, -target.forward);
        float fwd = thisCam.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.z;

        if (Physics.Raycast(playertocamRay, out RaycastHit hit, fwd, whatIsObstacle)){
            thisCam.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.z
                = Vector3.Distance(target.position, hit.point);
            print(thisCam.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.z);
        }
        //else
        //{
        //    thisCam.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.z = followDistance;
        //}
    }
}
