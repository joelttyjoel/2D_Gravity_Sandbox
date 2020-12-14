using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicFloatingAroundEnemy : MonoBehaviour
{
    public float sideForce = 1f;
    public float sideTime = 1f;
    public float sideChance2X = 0.4f;
    public float forwardForce = 1f;
    public float forwardTime = 1f;
    public float forwardChance = 0.9f;
    public float avoidanceForce = 10f;
    public Vector3 center;
    public Vector3[] avoidanceVectors = new Vector3[4];
    
    private Rigidbody2D rigidBody;
    private Vector3 forwardForceVector;
    private float sideTorqueForce;
    private float forwardTimer = 0f;
    private float sideTimer = 0f;

    private Vector3[] avoidanceVectorsRotated = new Vector3[4];
    private Vector3 centerRotated;
    private Vector3 centerPosition;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        forwardTimer = forwardTime;
        sideTimer = sideTime;
        
        for(int i = 0; i < avoidanceVectors.Length; i++)
        {
            avoidanceVectorsRotated[i] = avoidanceVectors[i];
        }
    }

    // Update is called once per frame
    void Update()
    {
        //avoidance stuff
        centerRotated = center.x * transform.right + center.y * transform.up;
        centerPosition = transform.position + centerRotated;
        //rotate avoidance vectors
        for (int i = 0; i < avoidanceVectors.Length; i++)
        {
            avoidanceVectorsRotated[i] = Rotated(avoidanceVectors[i], transform.rotation, new Vector3(0, 0, 0));
        }

        for (int i = 0; i < avoidanceVectors.Length; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(centerPosition, avoidanceVectorsRotated[i], avoidanceVectorsRotated[i].magnitude);
            Color yesColor = Color.green;
            if (hit.collider != null)
            {
                yesColor = Color.red;
                rigidBody.AddForceAtPosition(-avoidanceVectorsRotated[i] * avoidanceForce, centerPosition);
            }

            Debug.DrawLine(centerPosition, centerPosition + avoidanceVectorsRotated[i], yesColor);
        }

        //basic shit ai just waddle around
        //rotation
        if (sideTimer < 0)
        {
            sideTimer = sideTime;

            if(Random.Range(0f, 1f) < sideChance2X)
            {
                if(Random.Range(0f, 1f) > 0.5f)
                {
                    sideTorqueForce = sideForce * -1;
                }
                else
                {
                    sideTorqueForce = sideForce * 1;
                }
            }
            else
            {
                sideTorqueForce = 0f;
            }

            rigidBody.AddTorque(sideTorqueForce);
        }
        sideTimer -= Time.deltaTime;

        //forward
        if (forwardTimer < 0)
        {
            forwardTimer = forwardTime;

            if (Random.Range(0f, 1f) < forwardChance)
            {
                forwardForceVector = forwardForce * transform.right;
            }
            else
            {
                forwardForceVector = new Vector3(0, 0, 0);
            }

            rigidBody.AddForce(forwardForceVector);
        }
        forwardTimer -= Time.deltaTime;
    }

    //private void OnDrawGizmos()
    //{
    //    //if (avoidanceVectorsRotated == null) return;
    //    for (int i = 0; i < avoidanceVectors.Length; i++)
    //    {
    //        Gizmos.DrawLine(transform.position + centerRotated, transform.position + centerRotated + avoidanceVectorsRotated[i]);
    //    }
    //}

    public Vector3 Rotated(Vector3 vector, Quaternion rotation, Vector3 pivot)
    {
        return rotation * (vector - pivot) + pivot;
    }
}
