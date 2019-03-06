using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This scripts is used for switching camera view in the demo scenes
/// </summary>
public class CameraRotate : MonoBehaviour
{
    //camera rotate speed
    public float rotateSpeed = 10.0f;
    //camera state array
    public Transform[] cameraStateArray;
    //current camera state index
    private int m_CameraIndex;
    private bool m_CameraRotateEnable;


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            this.SwitchCamera();
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            this.EnableCameraRotate();
        }

        if (m_CameraRotateEnable == true)
        {
            this.transform.Rotate(Vector3.up * Time.deltaTime * rotateSpeed);
        }
    }

    public void SwitchCamera()
    {
        if (cameraStateArray != null && cameraStateArray.Length != 0)
        {
            m_CameraIndex = m_CameraIndex % cameraStateArray.Length;

            Vector3 cameraLocalPos = cameraStateArray[m_CameraIndex].localPosition;
            Quaternion cameraLocalRot = cameraStateArray[m_CameraIndex].localRotation;

            Camera.main.transform.localPosition = cameraLocalPos;
            Camera.main.transform.localRotation = cameraLocalRot;

            m_CameraIndex++;
        }

    }

    public void EnableCameraRotate()
    {
        m_CameraRotateEnable = !m_CameraRotateEnable;
    }
}
