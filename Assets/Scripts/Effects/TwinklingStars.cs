using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwinklingStars : MonoBehaviour
{
    public float maxOpacity;
    public float minOpacity;
    public float minTimeUpDown;
    public float maxTimeUpDown;

    private SpriteRenderer starImage;
    private float minOpacityThis;
    private float maxOpacityThis;
    private float timeUpDown;
    //between 0 and 1 up down depending on time
    private float currentProgress;
    //true = up, false = down
    private bool direction;
    //stuff
    private float lastTime;
    

    // Start is called before the first frame update
    void Start()
    {
        //get image
        starImage = GetComponent<SpriteRenderer>();

        //between min opacity and halfway to max
        minOpacityThis = Random.Range(minOpacity, minOpacity + ((maxOpacity - minOpacity) / 4));
        maxOpacityThis = Random.Range(maxOpacity, maxOpacity - ((maxOpacity - minOpacity) / 4));

        timeUpDown = Random.Range(minTimeUpDown, maxTimeUpDown);

        //set starting stuff
        currentProgress = Random.Range(0f, 1f);
        lastTime = Time.time;
        //starts at random progress thing so that all dont just start at lowest state, therefor add progress to start
        lastTime -= timeUpDown * currentProgress;

        direction = false;
        if (Random.Range(0f, 1f) < 0.5f) direction = true;

        //ADD LATER, CHANGE BETWEEEN COLORS
        //ADD LATER, SIZE CHANGE
    }

    // Update is called once per frame
    void Update()
    {
        //first set direction of where to make progressif(direction)
        //enough time has passed, flip direction
        if(Time.time > lastTime + timeUpDown)
        {
            direction = !direction;
            lastTime = Time.time;
        }
        //then make progress
        float timeSinceLast = Time.time - lastTime;
        float currentProgressTime = timeSinceLast / timeUpDown;
        //if direction, going from 0 to 1
        if (direction) currentProgress = currentProgressTime;
        else currentProgress = 1 - currentProgressTime;

        //now do stuff depending on progress
        float opacityProgress = (maxOpacityThis - minOpacityThis) * currentProgress;
        float currentOpacityValue = minOpacityThis + opacityProgress;
        starImage.color = new Color(starImage.color.r, starImage.color.g, starImage.color.b, currentOpacityValue);

        //opacity value is also used for scale, why not
        transform.localScale = new Vector3(currentOpacityValue, currentOpacityValue, 1);
    }
}
