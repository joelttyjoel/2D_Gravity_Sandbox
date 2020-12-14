using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Assets.Scripts.TilemapStuff;

//public class TilemapTileKnower : MonoBehaviour
//{
//    private Dictionary<Vector3Int, MyTile> allTiles = new Dictionary<Vector3Int, MyTile>();
//    private Tilemap ground;

//    public static TilemapTileKnower instance;
//    private void Awake()
//    {
//        // if the singleton hasn't been initialized yet
//        if (instance != null && instance != this)
//        {
//            Destroy(this.gameObject);
//            return;//Avoid doing anything else
//        }

//        instance = this;
//        DontDestroyOnLoad(this.gameObject);
//    }

//    private void Start()
//    {
//        ground = GetComponent<Tilemap>();

//        //fill out hashtable
//        foreach (var pos in ground.cellBounds.allPositionsWithin)
//        {
//            if (ground.HasTile(pos))
//            {
//                allTiles.Add(pos, CreateTileDataFromName(ground.GetTile(pos)));
//            }
//        }
//    }

//    public MyTile GetTile(Vector3Int inputGridPos)
//    {
//        MyTile currentTile = new MyTile();
//        allTiles.TryGetValue(inputGridPos, out currentTile);
//        return currentTile;
//    }

//    private MyTile CreateTileDataFromName(TileBase inputTile)
//    {
//        MyTile thisTile = new MyTile();
//        //i know but ok sh i cba to make custom class and everything
//        string tileName = inputTile.name;
//        switch (tileName)
//        {
//            case "moonrocktile":
//                thisTile.isInteractable = false;
//                thisTile.isDestructible = true;
//                thisTile.isTurnedOn = true;
//                thisTile.mass = 25000f;
//                thisTile.hardness = 1f;
//                break;
//            case "halfblockmoonrocktile":
//                thisTile.isInteractable = false;
//                thisTile.isDestructible = true;
//                thisTile.isTurnedOn = true;
//                thisTile.mass = 12500f;
//                thisTile.hardness = 1f;
//                break;
//            case "gravitygeneratortile":
//                thisTile.isInteractable = true;
//                thisTile.isDestructible = true;
//                thisTile.isTurnedOn = true;
//                thisTile.mass = 2500000f;
//                thisTile.hardness = 3f;
//                break;
//            case "grasstile":
//                thisTile.isInteractable = false;
//                thisTile.isDestructible = true;
//                thisTile.isTurnedOn = true;
//                thisTile.mass = 20000f;
//                thisTile.hardness = 0.5f;
//                break;
//            case "windowtile":
//                thisTile.isInteractable = false;
//                thisTile.isDestructible = true;
//                thisTile.isTurnedOn = true;
//                thisTile.mass = 5000f;
//                thisTile.hardness = 0.5f;
//                break;
//        }

//        return thisTile;
//    }
//}
