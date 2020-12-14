using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhenEnabledAlignWithPlayerRotation : MonoBehaviour
{
    public GameObject playerGameObject;

    // Update is called once per frame
    void Update()
    {
        transform.rotation = playerGameObject.transform.rotation;
    }
}
