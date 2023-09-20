using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ShoulderViewCamera : MonoBehaviour
{
    public Vector3 pivotOffset = new Vector3(.0f, 1.0f, .0f);
    public Vector3 directOffet = new Vector3(.4f, .5f, -2.0f);

    public float smooth = 10f; // 카메라 반응속도
    public float horizontalAimingSpd = 6.0f; // 수평
    public float verticalAimingSpd = 6.0f; // 수직 회전속도

    public float verticalAngleMax = 30.0f;
    public float verticalAngleMin = -60.0f;
    public float recoilAngleBounce = 5.0f;

    private float horizontalAngle = .0f; // 마우스 이동에 따른 카메라 이동 값
    public float GetHorizontal => horizontalAngle;
    private float verticalAngle = .0f;


    public Transform playerTransform; //
    private Transform cameraTransform; //
    private Camera myCamera; // 필요한 클래스 캐싱

    private Vector3 realCameraPosition; // 카메라와
    private float realCameraPositionMag; // 플레이어 사이를 이중체크할것

    private Vector3 smoothPivotOffset;
    private Vector3 smoothCameraOffset;

    private Vector3 targetPivotOffset; //
    private Vector3 targetDirectOffset; //카메라가 따라가는것

    private float defaultFOV; // 기본시야각과
    private float targetFOV; // 런닝뷰

    private float targetMaxVerticalAngle; // 최대각
    private float recoilAngle = 0f; // 반동각

    private void Awake()
    {
        cameraTransform = transform; //
        myCamera = cameraTransform.GetComponent<Camera>(); // 캐싱때에 주의점 : 컴포의 널 체크; requireComponent(typeof(
        cameraTransform.position = playerTransform.position // 카메라 높이 : 플레이어 높이 + 피봇 높이
            + Quaternion.identity * pivotOffset //
            + Quaternion.identity * directOffet; //
        cameraTransform.rotation = Quaternion.identity; //

        realCameraPosition = cameraTransform.position - playerTransform.position; // p1 - p2 = p3 = p1서 p2를 보는 방향 및 거리
        realCameraPositionMag = realCameraPosition.magnitude - .5f; //

        smoothPivotOffset = pivotOffset;
        smoothCameraOffset = directOffet;
        defaultFOV = myCamera.fieldOfView;
        horizontalAngle = playerTransform.eulerAngles.y;

        ResetTargetOffsets();
        ResetFOV();
        ResetMaxVerticalAngle();
    }

    private void Update()
    {
        float mouseX = Mathf.Clamp(Input.GetAxis("Mouse X"), -1f, 1f);
        horizontalAngle += mouseX * horizontalAimingSpd;

        float mouseY = Mathf.Clamp(Input.GetAxis("Mouse Y"), -1f, 1f);
        verticalAngle += mouseY * verticalAimingSpd;

        verticalAngle = Mathf.Clamp(verticalAngle, verticalAngleMin, verticalAngleMax);

        verticalAngle = Mathf.LerpAngle(verticalAngle, verticalAngle + recoilAngle, 10.0f * Time.deltaTime); // 바운스

        Quaternion camYRotation = Quaternion.Euler(.0f, horizontalAngle, .0f);

        Quaternion aimRotation = Quaternion.Euler(-verticalAngle, horizontalAngle, .0f);

        cameraTransform.rotation = aimRotation;

        myCamera.fieldOfView = Mathf.Lerp(myCamera.fieldOfView, targetFOV, Time.deltaTime);

        Vector3 baseTempPosition = playerTransform.position + camYRotation * targetPivotOffset;

        Vector3 noCollisionOffset = targetDirectOffset;

        for (float zOffset = targetDirectOffset.z; zOffset <= .0f; zOffset += .5f)
        {
            noCollisionOffset.z = zOffset;
            Vector3 vecCkPos = baseTempPosition + aimRotation * noCollisionOffset;
            if (DoubleViewingPosCK(vecCkPos, Mathf.Abs(zOffset)) || zOffset == .0f)
                break;
        }

        smoothPivotOffset = Vector3.Lerp(smoothPivotOffset, targetPivotOffset, smooth * Time.deltaTime);

        smoothCameraOffset = Vector3.Lerp(smoothCameraOffset, noCollisionOffset, smooth * Time.deltaTime);

        cameraTransform.position = playerTransform.position + camYRotation * smoothPivotOffset + aimRotation * smoothCameraOffset; // 장애물 충돌시 이동

        if (recoilAngle > .0f)
        {
            recoilAngle -= recoilAngleBounce * Time.deltaTime;
        }
        else if (recoilAngle < .0f)
        {
            recoilAngle += recoilAngleBounce * Time.deltaTime;
        }// 사격후 반동 주기
    }

    public float GetNowPivotMagnitude(Vector3 finalPivotOffset)
    {
        return Mathf.Abs((finalPivotOffset - smoothPivotOffset).magnitude);
    }


    public void ResetTargetOffsets()
    {
        targetPivotOffset = pivotOffset;
        targetDirectOffset = directOffet;
    }

    public void ResetFOV()
    {
        targetFOV = defaultFOV;
    }

    public void ResetMaxVerticalAngle()
    {
        targetMaxVerticalAngle = verticalAngleMax; // 반동이후 최대값 리셋
    }

    public void BounceVertical(float degree)
    {
        recoilAngle = degree; // 각 준 만큼 바운스
    }

    public void SetTargetOffset(Vector3 newPivotOffset, Vector3 newDirectOffset)
    {
        targetPivotOffset = newPivotOffset;
        targetDirectOffset = newDirectOffset;
    }

    public void SetFOV(float customFOV)
    {
        targetFOV = customFOV;
    }

    bool ViewingPosCK(Vector3 ckPosition, float deltaPlayerHeight) // 카메라 -> 플레이어
    {
        Vector3 target = playerTransform.position + (Vector3.up * deltaPlayerHeight);

        if (Physics.SphereCast(ckPosition, .2f, target - ckPosition, out RaycastHit hit, realCameraPositionMag))
        {
            if (hit.transform != playerTransform && !hit.transform.GetComponent<Collider>().isTrigger)
            {
                return false;
            }
        }
        return true;
    }

    bool ReverseViewingPosCK(Vector3 ckPosition, float deltaPlayerHeight, float maxDistance) // 
    {
        Vector3 origin = playerTransform.position + (Vector3.up * deltaPlayerHeight);

        if (Physics.SphereCast(origin, .2f, ckPosition - origin, out RaycastHit hit, maxDistance))
        {
            if (hit.transform != playerTransform && hit.transform != transform && !hit.transform.GetComponent<Collider>().isTrigger)
            {
                return false;
            }
        }
        return true;
    }

    bool DoubleViewingPosCK(Vector3 ckPosition, float offset)
    {
        float playerFucusHeight = playerTransform.GetComponent<CapsuleCollider>().height * .75f;
        return ViewingPosCK(ckPosition, playerFucusHeight) && ReverseViewingPosCK(ckPosition, playerFucusHeight, offset);
    }
}
