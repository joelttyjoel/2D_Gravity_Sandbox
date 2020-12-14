using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    //settings movement
    //walking movement
    [Header("Walking")]
    public float oldraycastDownLengthJump;
    public float oldraycastDownLengthWalk;
    public float raycastDownLengthJump;
    public float raycastDownLengthWalk;
    public float jumpForce;
    public float stepForce;
    public float jumpCooldownTimer;
    public float walkCooldownTimer;
    //jetpack movement
    [Header("Jetpack")]
    public float jetpackPowerMax;
    public float jetpackPower;
    public float jetpackRegenPerFixedUpdate;
    public bool jetpackLowTimeout;
    public float jetpackLowTimeoutTime;
    public bool jetpackMode;
    public float jetpackThrustMultiplierUp;
    public float jetpackThrustMultiplierSides;
    public float jetpackThrustMultiplierRotation;
    //jetpack stuf
    [Header("Stuff")]
    public Slider jetpackPowerIndicator;
    public Image jetpackIcon;
    public Toggle movementMode;
    public Toggle drawSceneGravityRadiobutton;
    public Text scaleFactor;
    private int scaleFactorVal = 1;
    public Color okColor;
    public Color badColor;
    public int jetpackPowerOutBlinkTimes;
    public AudioSource jetpackSound;
    public AudioSource jumpSound;
    public AudioSource walkSound;
    public float cameraZoomSpeed;
    public Vector2Int cameraSizeMinMax;
    [Header("Anti fall")]
    public float antiFallForce = 10f;
    public Vector3 center;
    public Vector3[] oldavoidanceVectors = new Vector3[2];
    public Vector3[] avoidanceVectors = new Vector3[2];
    private Vector3[] avoidanceVectorsRotated = new Vector3[2];
    private Vector3 centerRotated;
    private Vector3 centerPosition;


    private Rigidbody2D thisRigidbody;
    //jetpack
    private bool somethingWasPressedJetpack;
    private bool jetpackIsPlaying;
    private float originalJetpackVolume;
    //walking
    private bool feetOnGround;
    private bool canJump;
    private bool canWalk;
    //camera zoom
    private float cameraZoomSizeValue;

    public static PlayerMovement instance;
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

    void Start()
    {
        thisRigidbody = GetComponent<Rigidbody2D>();
        jetpackPower = jetpackPowerMax;
        somethingWasPressedJetpack = false;
        jetpackIsPlaying = false;
        originalJetpackVolume = jetpackSound.volume;
        canJump = true;
        canWalk = true;

        //camera zoom value
        cameraZoomSizeValue = 5f;

        //anti fall
        for (int i = 0; i < avoidanceVectors.Length; i++)
        {
            avoidanceVectorsRotated[i] = avoidanceVectors[i];
        }

        for (int i = 0; i < avoidanceVectors.Length; i++)
        {
            oldavoidanceVectors[i] = avoidanceVectors[i];
        }
        oldraycastDownLengthJump = raycastDownLengthJump;
        oldraycastDownLengthWalk = raycastDownLengthWalk;
}

    private void Update()
    {
        //MOVE THIS OPTHER TIME
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            scaleFactorVal += 1;
            scaleFactor.text = "(arrowsupdown)ScaleFactor: " + scaleFactorVal.ToString();
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            scaleFactorVal -= 1;
            if (scaleFactorVal < 1) scaleFactorVal = 1;
            scaleFactor.text = "(arrowsupdown)ScaleFactor: " + scaleFactorVal.ToString();
        }

        //scale stuff
        transform.localScale = new Vector3(scaleFactorVal, scaleFactorVal, 1);
        for (int i = 0; i < avoidanceVectors.Length; i++)
        {
            avoidanceVectors[i] = oldavoidanceVectors[i] * scaleFactorVal;
        }
        raycastDownLengthJump = oldraycastDownLengthJump * scaleFactorVal;
        raycastDownLengthWalk = oldraycastDownLengthWalk * scaleFactorVal;


        if (!movementMode.isOn)
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
                RaycastHit2D[] hit = Physics2D.RaycastAll(centerPosition, avoidanceVectorsRotated[i], avoidanceVectorsRotated[i].magnitude);
                Color yesColor = Color.green;
                if (hit.Length > 1)
                {
                    yesColor = Color.red;
                    if(i == 0)
                    {
                        thisRigidbody.AddTorque(antiFallForce);
                    }
                    if (i == 1)
                    {
                        thisRigidbody.AddTorque(antiFallForce * -1);
                    }
                }

                Debug.DrawLine(centerPosition, centerPosition + avoidanceVectorsRotated[i], yesColor);
            }
        }

        //check if change jetpack or no
        //doing in updaet to not miss clicks
        if (Input.GetKeyDown(KeyCode.C))
        {
            movementMode.isOn = !movementMode.isOn;
        }
        //MOVE ALL THIS SHTI TO SEPERATE SHIT TO AVOID CONFUSION LATER GUT FOR NOW WHO CARES
        if (Input.GetKeyDown(KeyCode.V))
        {
            drawSceneGravityRadiobutton.isOn = !drawSceneGravityRadiobutton.isOn;
        }

        //zoom scroll on camera
        //is 1 or -1 for each click on mouse
        cameraZoomSizeValue += cameraZoomSpeed * -Input.mouseScrollDelta.y;
        if (cameraZoomSizeValue < cameraSizeMinMax.x) cameraZoomSizeValue = cameraSizeMinMax.x;
        else if (cameraZoomSizeValue > cameraSizeMinMax.y) cameraZoomSizeValue = cameraSizeMinMax.y;

        Camera.main.orthographicSize = cameraZoomSizeValue;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //do different movement depending on if jetpack or no
        somethingWasPressedJetpack = false;
        if (jetpackMode) DoJetpackMovement();
        else DoWalkingMovement();

        JetpackCooldownAndRegenStuff();
    }

    private void JetpackCooldownAndRegenStuff()
    {
        //if not enough power, start cooldown, when on cooldown, only regen, movement will auto return and not drain
        if (jetpackPower <= 0)
        {
            StartCoroutine(JetpackCooldown());
        }
        //only regen if nothing was pressed and not at full
        if (!somethingWasPressedJetpack && jetpackPower <= jetpackPowerMax) jetpackPower += jetpackRegenPerFixedUpdate;

        //stuff that was in update
        //if sound isn't playing, but button is pressed, start sound
        if (!jetpackSound.isPlaying && somethingWasPressedJetpack)
        {
            jetpackSound.volume = 0f;
            jetpackSound.Play();
            StartCoroutine(VolumeFade(jetpackSound, originalJetpackVolume, 0.2f));
        }
        //if sound is playing but button isn't pressed, turn of
        if (jetpackMode && jetpackSound.isPlaying && !somethingWasPressedJetpack)
        {
            StartCoroutine(VolumeFade(jetpackSound, 0f, 0.2f));
        }

        //update jetpack power
        jetpackPowerIndicator.value = jetpackPower / jetpackPowerMax;
    }

    private void DoJetpackMovement()
    {
        if (jetpackLowTimeout) return;
        //if button is pressed, add thurst that way
        //UP
        if(Input.GetKey(KeyCode.W))
        {
            thisRigidbody.AddForce(transform.up * jetpackThrustMultiplierUp);
            jetpackPower -= jetpackThrustMultiplierUp;
            somethingWasPressedJetpack = true;
        }
        //DOWN
        if (Input.GetKey(KeyCode.S))
        {
            thisRigidbody.AddForce(-transform.up * jetpackThrustMultiplierSides);
            jetpackPower -= jetpackThrustMultiplierSides;
            somethingWasPressedJetpack = true;
        }
        //LEFT
        if (Input.GetKey(KeyCode.A))
        {
            thisRigidbody.AddForce(transform.right * -1 * jetpackThrustMultiplierSides);
            jetpackPower -= jetpackThrustMultiplierSides;
            somethingWasPressedJetpack = true;
        }
        //RIGHT
        if (Input.GetKey(KeyCode.D))
        {
            thisRigidbody.AddForce(transform.right * jetpackThrustMultiplierSides);
            jetpackPower -= jetpackThrustMultiplierSides;
            somethingWasPressedJetpack = true;
        }
        //ROTATE COUNTER
        if (Input.GetKey(KeyCode.Q))
        {
            thisRigidbody.AddTorque(jetpackThrustMultiplierRotation);
            jetpackPower -= jetpackThrustMultiplierRotation;
            somethingWasPressedJetpack = true;
        }
        //ROTATE CLOCK
        if (Input.GetKey(KeyCode.E))
        {
            thisRigidbody.AddTorque(-jetpackThrustMultiplierRotation);
            jetpackPower -= jetpackThrustMultiplierRotation;
            somethingWasPressedJetpack = true;
        }
    }

    private void DoWalkingMovement()
    {
        //check if on ground, JUMP
        //Debug.DrawRay(transform.position, -transform.up * raycastDownLengthJump, Color.red);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, -transform.up, raycastDownLengthJump, 1 << 9);

        //if on ground, do movement stuff
        if(hit.collider != null)
        {
            //jump
            if (Input.GetKey(KeyCode.Space) && canJump)
            {
                thisRigidbody.AddForce(transform.up * jumpForce);
                jumpSound.Play();
                StartCoroutine(JumpCooldown());
            }
        }

        //check if on ground, WALK
        Debug.DrawRay(transform.position, -transform.up * raycastDownLengthWalk, Color.green);
        RaycastHit2D hit2 = Physics2D.Raycast(transform.position, -transform.up, raycastDownLengthWalk, 1 << 9);

        //if on ground, do movement stuff
        if (hit2.collider != null)
        {
            //Left
            if (Input.GetKey(KeyCode.A) && canWalk)
            {
                Debug.Log("LEFT");
                //create 45 degree vector to left
                thisRigidbody.AddForce((transform.up + -transform.right) * stepForce);
                walkSound.Play();
                StartCoroutine(WalkCoolDown());
            }
            //Left
            else if (Input.GetKey(KeyCode.D) && canWalk)
            {
                Debug.Log("RIGHT");
                //create 45 degree vector to right
                thisRigidbody.AddForce((transform.up + transform.right) * stepForce);
                walkSound.Play();
                StartCoroutine(WalkCoolDown());
            }
        }

        //can still rotate wtihout ground
        //doesent feel good without this come on
        //ROTATE COUNTER
        //if close to ground, add alot of spinny, if not, dont
        float spinnyMultiplier = 0f;
        //raycast both sides of player as if laying down
        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, -transform.right, 0.6f, 1 << 9);
        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, transform.right, 0.6f, 1 << 9);

        if (hitLeft || hitRight) spinnyMultiplier = 1.4f;
        else spinnyMultiplier = 0.5f;

        if (Input.GetKey(KeyCode.Q))
        {
            thisRigidbody.AddTorque(jetpackThrustMultiplierRotation * spinnyMultiplier);
        }
        //ROTATE CLOCK
        if (Input.GetKey(KeyCode.E))
        {
            thisRigidbody.AddTorque(-jetpackThrustMultiplierRotation * spinnyMultiplier);
        }
    }

    private IEnumerator JetpackCooldown()
    {
        jetpackLowTimeout = true;
        float waitTimeDivided = jetpackLowTimeoutTime / jetpackPowerOutBlinkTimes;
        bool blinkState = false;
        for (int i = 0; i < jetpackPowerOutBlinkTimes; i++)
        {
            blinkState = !blinkState;
            if (blinkState)
            {
                jetpackIcon.color = badColor;
            }
            else
            {
                jetpackIcon.color = okColor;
            }
            yield return new WaitForSeconds(waitTimeDivided);
        }
        jetpackLowTimeout = false;
    }

    private IEnumerator JumpCooldown()
    {
        canJump = false;
        yield return new WaitForSeconds(jumpCooldownTimer);
        canJump = true;
    }

    private IEnumerator WalkCoolDown()
    {
        canWalk = false;
        yield return new WaitForSeconds(walkCooldownTimer);
        canWalk = true;
    }

    IEnumerator VolumeFade(AudioSource _AudioSource, float _EndVolume, float _FadeLength)
    {
        float _StartVolume = _AudioSource.volume;

        float _StartTime = Time.time;

        while (Time.time < _StartTime + _FadeLength)
        {
            _AudioSource.volume = _StartVolume + ((_EndVolume - _StartVolume) * ((Time.time - _StartTime) / _FadeLength));
            yield return null;
        }

        if (_EndVolume <= 0) _AudioSource.Stop();

    }

    public void ToggleMovementMode()
    {
        jetpackMode = !jetpackMode;
    }

    public Vector3 Rotated(Vector3 vector, Quaternion rotation, Vector3 pivot)
    {
        return rotation * (vector - pivot) + pivot;
    }

}
