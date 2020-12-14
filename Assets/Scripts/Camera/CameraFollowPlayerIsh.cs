using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraFollowPlayerIsh : MonoBehaviour
{
    public Transform playerTransform;
    public float cameraTargetRadius;
    public float cameraMoveSpeed;
    public float rotationSpeed;
    public float differenceRotationDivider = 10f;
    public bool rotateWithPlayer = false;
    public Text rotateText;
    public GameObject testingText;

    // Update is called once per frame
    void Update()
    {
        //if distance to player > radius of target circle, move towards player
        Vector2 direction = new Vector2(playerTransform.position.x - transform.position.x, playerTransform.position.y - transform.position.y);
        float distance = direction.magnitude - cameraTargetRadius;
        if(distance > 0)
        {
            direction.Normalize();
            transform.position += new Vector3(direction.x * distance * cameraMoveSpeed, direction.y * distance * cameraMoveSpeed, 0);
        }
        //is negative if camera ray is inside ok circle

        //rotate along player
        if(rotateWithPlayer)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, playerTransform.rotation, rotationSpeed * Time.deltaTime * (Quaternion.Angle(transform.rotation, playerTransform.rotation)/ differenceRotationDivider + 1));
            //Debug.Log(Quaternion.Angle(transform.rotation, playerTransform.rotation) / differenceRotationDivider + 1);
        }
        else
        {
            transform.rotation = new Quaternion(0, 0, 0, 0);
        }

        if(Input.GetKeyDown(KeyCode.R))
        {
            rotateWithPlayer = !rotateWithPlayer;
            rotateText.text = "(R)RotateWithPlayer: " + rotateWithPlayer.ToString();
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            testingText.SetActive(!testingText.activeSelf);
        }
    }
}
