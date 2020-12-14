using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Assets.Scripts.TilemapStuff;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

public class customGravity : MonoBehaviour
{
    [Header("settings")]
    public Tilemap ground;
    public float gravityConstant = 0.0000000000667f;
    public float playerMass = 1f;
    public float timeBetweenUpdateGravity = 0.1f;

    private Vector3 gravityVector;
    private Vector3 playerPosition;
    private Rigidbody2D playerRigidbody;

    private Dictionary<Vector3Int, MyTile> allTiles = new Dictionary<Vector3Int, MyTile>();

    private bool drawSceneGravityVector = true;
    public LineRenderer sceneGravityVector;

    //native lists for jobs
    //create lists
    private NativeArray<Vector3> allCalculatedGravity;
    private NativeArray<Vector3> playerPositionArray;
    private NativeArray<float> playerMassArray;
    private NativeArray<float> gravityConstantArray;
    private NativeList<float> allTileMassArray;
    private NativeList<Vector3> allTilePosWorldArray;
    private NativeList<bool> allTileIsTurnedOnArray;
    //----

    public static customGravity instance;
    private void Awake()
    {
        // if the singleton hasn't been initialized yet
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;//Avoid doing anything else
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();

        //fill out dic
        //fill out hashtable
        foreach (var pos in ground.cellBounds.allPositionsWithin)
        {
            if (ground.HasTile(pos))
            {
                allTiles.Add(pos, CreateTileDataFromName(ground.GetTile(pos)));
            }
        }

        //start update gravity, here intead of fixed update
        StartCoroutine(UpdateGravity());
    }

    void FixedUpdate()
    {
        Debug.DrawRay(transform.position, gravityVector);
        //draw scene gravityvector or no
        DrawSceneGravityVector();

        playerRigidbody.AddForce(gravityVector);
    }

    private void DrawSceneGravityVector()
    {
        if (!drawSceneGravityVector)
        {
            sceneGravityVector.enabled = false;
            return;
        }

        sceneGravityVector.enabled = true;
        //first positino is always player position, this script is on player
        sceneGravityVector.SetPosition(0, transform.position);
        //second position is just gravityvector
        sceneGravityVector.SetPosition(1, transform.position + gravityVector);
    }

    private IEnumerator UpdateGravity()
    {
        //create lists
        allCalculatedGravity = new NativeArray<Vector3>(1, Allocator.Persistent);
        playerPositionArray = new NativeArray<Vector3>(1, Allocator.Persistent);
        playerMassArray = new NativeArray<float>(1, Allocator.Persistent);
        gravityConstantArray = new NativeArray<float>(1, Allocator.Persistent);
        //get count of tiles to gravity on
        int tileCount = 0;
        foreach (var pos in ground.cellBounds.allPositionsWithin)
        {
            if (ground.HasTile(pos))
            {
                tileCount++;
            }
        }
        allTileMassArray = new NativeList<float>(tileCount + 1, Allocator.Persistent);
        allTilePosWorldArray = new NativeList<Vector3>(tileCount + 1, Allocator.Persistent);
        allTileIsTurnedOnArray = new NativeList<bool>(tileCount + 1, Allocator.Persistent);
        //fill lists
        //-------- fill in all tile info into nativelists ------
        foreach (var pos in ground.cellBounds.allPositionsWithin)
        {
            if (ground.HasTile(pos))
            {
                MyTile currentTile = new MyTile();
                allTiles.TryGetValue(pos, out currentTile);
                allTileMassArray.Add(currentTile.mass);
                allTilePosWorldArray.Add(ground.CellToWorld(pos));
                allTileIsTurnedOnArray.Add(currentTile.isTurnedOn);
            }
        }

        //BASICLY UPDATE LOOP
        while (true)
        {
            //---------
            //fill in the rest
            allCalculatedGravity[0] = Vector3.zero;
            playerPositionArray[0] = transform.position;
            playerMassArray[0] = playerMass;
            gravityConstantArray[0] = gravityConstant;
            //create job
            UpdateGravityJob gravityJobNow = new UpdateGravityJob
            {
                allCalculatedGravityJob = allCalculatedGravity,
                playerPositionArrayJob = playerPositionArray,
                playerMassArrayJob = playerMassArray,
                gravityConstantArrayJob = gravityConstantArray,
                allTileMassArrayJob = allTileMassArray,
                allTilePosWorldArrayJob = allTilePosWorldArray,
                allTileIsTurnedOnArrayJob = allTileIsTurnedOnArray
            };
            //execute
            JobHandle jobHandle = gravityJobNow.Schedule(tileCount, 500);
            jobHandle.Complete();
            //copy stuff from lists into useful info
            gravityVector = allCalculatedGravity[0];
            //done, remove lists
            //allCalculatedGravity.Dispose();
            //playerPositionArray.Dispose();
            //playerMassArray.Dispose();
            //gravityConstantArray.Dispose();
            //allTileMassArray.Dispose();
            //allTilePosWorldArray.Dispose();
            //allTileIsTurnedOnArray.Dispose();

            //NOW WAIT FOR NEXT UPDATE
            yield return new WaitForSeconds(timeBetweenUpdateGravity);
        }
    }

    [BurstCompile]
    private struct UpdateGravityJob : IJobParallelFor
    {
        [NativeDisableParallelForRestriction] public NativeArray<Vector3> allCalculatedGravityJob;
        [ReadOnly] public NativeArray<Vector3> playerPositionArrayJob;
        [ReadOnly] public NativeArray<float> playerMassArrayJob;
        [ReadOnly] public NativeArray<float> gravityConstantArrayJob;
        [ReadOnly] public NativeList<float> allTileMassArrayJob;
        [ReadOnly] public NativeList<Vector3> allTilePosWorldArrayJob;
        [ReadOnly] public NativeList<bool> allTileIsTurnedOnArrayJob;

        public void Execute(int index)
        {
            if (allTileIsTurnedOnArrayJob[index] == false) return;

            Vector3 vectorPlayerTile;
            float distancePlayerTile;

            //get world variables
            vectorPlayerTile = allTilePosWorldArrayJob[index] - playerPositionArrayJob[0];
            distancePlayerTile = vectorPlayerTile.magnitude;
            vectorPlayerTile.Normalize();

            ////calculate vector scalar
            //float scalar = gravityConstant * ((playerMass * TilemapTileKnower.instance.GetTile(pos).mass) / distancePlayerTile);
            //Debug.DrawLine(transform.position, ground.CellToWorld(pos));
            //add force from this tile to total gravity
            allCalculatedGravityJob[0] += vectorPlayerTile * (gravityConstantArrayJob[0] * ((playerMassArrayJob[0] * allTileMassArrayJob[index]) / distancePlayerTile));
        }
    }

    private void OnApplicationQuit()
    {
        allCalculatedGravity.Dispose();
        playerPositionArray.Dispose();
        playerMassArray.Dispose();
        gravityConstantArray.Dispose();
        allTileMassArray.Dispose();
        allTilePosWorldArray.Dispose();
        allTileIsTurnedOnArray.Dispose();
    }

    private MyTile CreateTileDataFromName(TileBase inputTile)
    {
        MyTile thisTile = new MyTile();
        //i know but ok sh i cba to make custom class and everything
        string tileName = inputTile.name;
        switch (tileName)
        {
            case "moonrocktile":
                thisTile.isInteractable = false;
                thisTile.isDestructible = true;
                thisTile.isTurnedOn = true;
                thisTile.mass = 25000f;
                thisTile.hardness = 1f;
                break;
            case "halfblockmoonrocktile":
                thisTile.isInteractable = false;
                thisTile.isDestructible = true;
                thisTile.isTurnedOn = true;
                thisTile.mass = 12500f;
                thisTile.hardness = 1f;
                break;
            case "grasstile":
                thisTile.isInteractable = false;
                thisTile.isDestructible = true;
                thisTile.isTurnedOn = true;
                thisTile.mass = 20000f;
                thisTile.hardness = 0.5f;
                break;
            case "windowtile":
                thisTile.isInteractable = false;
                thisTile.isDestructible = true;
                thisTile.isTurnedOn = true;
                thisTile.mass = 5000f;
                thisTile.hardness = 0.5f;
                break;
            //special blocks
            case "gravitygeneratortile":
                thisTile.isInteractable = true;
                thisTile.isDestructible = true;
                thisTile.isTurnedOn = true;
                thisTile.mass = 5500000f;
                thisTile.hardness = 3f;
                break;
        }

        return thisTile;
    }

    public MyTile GetTile(Vector3Int inputGridPos)
    {
        MyTile currentTile = new MyTile();
        allTiles.TryGetValue(inputGridPos, out currentTile);
        return currentTile;
    }

    public void UpdateNativeArrays()
    {
        //first clear all
        allCalculatedGravity.Dispose();
        playerPositionArray.Dispose();
        playerMassArray.Dispose();
        gravityConstantArray.Dispose();
        allTileMassArray.Dispose();
        allTilePosWorldArray.Dispose();
        allTileIsTurnedOnArray.Dispose();

        //create lists
        allCalculatedGravity = new NativeArray<Vector3>(1, Allocator.Persistent);
        playerPositionArray = new NativeArray<Vector3>(1, Allocator.Persistent);
        playerMassArray = new NativeArray<float>(1, Allocator.Persistent);
        gravityConstantArray = new NativeArray<float>(1, Allocator.Persistent);
        //get count of tiles to gravity on
        int tileCount = 0;
        foreach (var pos in ground.cellBounds.allPositionsWithin)
        {
            if (ground.HasTile(pos))
            {
                tileCount++;
            }
        }
        allTileMassArray = new NativeList<float>(tileCount + 1, Allocator.Persistent);
        allTilePosWorldArray = new NativeList<Vector3>(tileCount + 1, Allocator.Persistent);
        allTileIsTurnedOnArray = new NativeList<bool>(tileCount + 1, Allocator.Persistent);
        //fill lists
        //-------- fill in all tile info into nativelists ------
        foreach (var pos in ground.cellBounds.allPositionsWithin)
        {
            if (ground.HasTile(pos))
            {
                MyTile currentTile = new MyTile();
                allTiles.TryGetValue(pos, out currentTile);
                allTileMassArray.Add(currentTile.mass);
                allTilePosWorldArray.Add(ground.CellToWorld(pos));
                allTileIsTurnedOnArray.Add(currentTile.isTurnedOn);
            }
        }
    }

    public void ToggleDrawSceneGravityVector()
    {
        drawSceneGravityVector = !drawSceneGravityVector;
    }
}
