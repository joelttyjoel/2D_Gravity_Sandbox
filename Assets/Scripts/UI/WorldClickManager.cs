using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class WorldClickManager : MonoBehaviour
{
    public Tilemap ground;
    public GameObject tileInteractMenu;
    public Text TurnOnOffText;

    private Vector3Int affectedTileID;

    private void Start()
    {
        tileInteractMenu.SetActive(false);
    }

    void Update()
    {
        //if leftclick a tile and not a ui element
        //if tile is interactable
        //show a thing to turn it on or off
        if (Input.GetMouseButtonDown(1))
        {
            //get basic stuff
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int pos = ground.WorldToCell(mouseWorldPos);

            //if click on empty return
            if (customGravity.instance.GetTile(ground.WorldToCell(mouseWorldPos)) == null) return;

            //ADD SHIT FOR IF CLICK UI AT SME TIME

            //if menu is already open and same block is clicked, disable menu
            if (tileInteractMenu.active == true && affectedTileID == pos)
            {
                tileInteractMenu.SetActive(false);
                return;
            }
            
            if (customGravity.instance.GetTile(ground.WorldToCell(mouseWorldPos)).isInteractable)
            {
                //show menu of buttons and set text to on or off depending on thing
                string onOrOff = "";
                if (customGravity.instance.GetTile(ground.WorldToCell(mouseWorldPos)).isTurnedOn) onOrOff = "OFF";
                else onOrOff = "ON";
                TurnOnOffText.text = "Turn " + onOrOff;
                //set position
                tileInteractMenu.transform.position = new Vector3(mouseWorldPos.x, mouseWorldPos.y, 0);
                //now show it
                tileInteractMenu.SetActive(true);
                //set affectedTile
                affectedTileID = pos;
            }
        }
    }

    public void OnOffAffectedTile()
    {
        //just flips the value, very effecient and sexy code
        customGravity.instance.GetTile(affectedTileID).SetIsTurnedOn(!customGravity.instance.GetTile(affectedTileID).isTurnedOn);
        //now update all nativearrays with new data
        customGravity.instance.UpdateNativeArrays();
        //then disable interaction menu
        tileInteractMenu.SetActive(false);
    }
}
