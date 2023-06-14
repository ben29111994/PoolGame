using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using MoreMountains.NiceVibrations;
using UnityEngine.UI;
using DG.Tweening;

public class BallController : MonoBehaviour
{
    public bool isMain = false;
    public bool isBlack = false;
    public int ballType = 1;
    public int id = 0;
    [System.NonSerialized]
    public CueController cueController;
    [System.NonSerialized]
    public bool ballIsSelected = false;
    [System.NonSerialized]
    public bool ballIsOut = false;
    [System.NonSerialized]
    public bool inMove = false;
	
    [System.NonSerialized]
    public float step = 0.0f;
    [System.NonSerialized]
    public float speed = 0.0f;

    private float velocityNormalized = 0.0f;

    [System.NonSerialized]
    public AnimationSpline holeSpline;
    [System.NonSerialized]
    public HolleController holleController;
    [System.NonSerialized]
    public float holeSplineLungth = 0.0f;
    private Vector3 ballVeolociyInHole;
    private Vector3 checkVelocity = Vector3.zero;
    private Vector3 checkAngularVelocity = Vector3.zero;
	
    private BallController inBall = null;
	
    private bool firstHitIsChecked = false;
	
    private bool hasFirstHit = false;
    [System.NonSerialized]
    public bool inForceMove = false;

    public Rigidbody body
    {
        get
        {
            if (!bodyInitializer)
            {
                bodyInitializer = GetComponent<BodyInitializer>();
            }
            return bodyInitializer.body;
        }
    }

    public BodyInitializer bodyInitializer;
    private Vector3 networkPosition;
    private Vector3 networkAngularVelocity;
    private Vector3 oldNetworkAngularVelocity;
    private float sendTime;

    private float networkSpeed;
    [System.NonSerialized]
    public bool animate;
    [System.NonSerialized]
    public bool firsSend = false;
    Coroutine disableDelay;

	
    void Awake()
    {
        ballIsOut = false;
        bodyInitializer = GetComponent<BodyInitializer>();
    }

    public void ForceSetMove(Vector3 position)
    {
        transform.position = position;
    }

    public void OnStart()
    {
        if (MenuControllerGenerator.controller)
        {
            if (isMain)
            {
                InvokeRepeating("CheckInBall", 0.0f, 1.0f);
            }
        }
    }

    void OnDestroy()
    {
        CancelInvoke("CheckInBall");
    }

    void CheckInBall()
    {

        if (body && !cueController.allIsSleeping)
        {
            inBall = null;
            foreach (BallController item in cueController.ballControllers)
            {
                if (item != this)
                {
                    float inBallDistance = Vector3.Distance(transform.position, item.transform.position);
                    if (inBallDistance < 1.99f * cueController.ballRadius)
                    {
                        inBall = item;
                        if (inBall)
                        {
                            Vector3 normal = (transform.position - inBall.transform.position).normalized;
                            float dist = 2.0f * cueController.ballRadius - inBallDistance;

                            body.position += 0.5f * dist * normal;
                            inBall.body.position += -0.5f * dist * normal;
                        }
                        break;
                    }
                }
            }

        }
    }

    public bool IsSleeping()
    {
        return !body || (body.velocity.magnitude < (Physics.sleepThreshold + 10f) && body.angularVelocity.magnitude * cueController.ballRadius < (Physics.sleepThreshold + 1));
    }

    public void OnSetHoleSpline(float lenght, int holleId)
    {
        HolleController holleController = HolleController.FindeHoleById(holleId);
        holeSpline = holleController.ballSpline;
        holeSplineLungth = lenght;
        this.holleController = holleController;
        if (!isMain && cueController.ballController.ballIsOut && (this.holleController == cueController.ballController.holleController || this.holleController.haveNeighbors(cueController.ballController.holleController)))
        {
            cueController.ballController.holeSplineLungth = holeSplineLungth - 2.0f * cueController.ballRadius;
            if (cueController.ballController.step >= cueController.ballController.holeSplineLungth)
            {
                cueController.ballController.body.position = cueController.ballController.holeSpline.Evaluate(holeSplineLungth);
            }
        }
    }

    public void RessetPosition(Vector3 position, bool forceResset)
    {
        Vector3 newStrPosition = position;

        if (!forceResset)
        {
            ballIsOut = false;
            cueController.ballIsOut = false;

            if (body)
            {
                body.useGravity = false;
                body.isKinematic = true;
                body.GetComponent<Collider>().enabled = false;
            }


            Ray ray = new Ray(position + (5.0f * cueController.ballRadius) * Vector3.up, -Vector3.up);
            RaycastHit hit;
            int tryCunt = 0;
            while (tryCunt < 7 && Physics.SphereCast(ray, (1.05f * cueController.ballRadius), out hit, 10.0f * cueController.ballRadius, cueController.ballMask))
            {
                tryCunt++;
                ray = new Ray(newStrPosition + (2.5f * cueController.ballRadius) * Vector3.up, -Vector3.up);
                newStrPosition += (3.0f * cueController.ballRadius) * Vector3.right;
            }
            cueController.cueFSMController.setMoveInTable();
        }

        transform.position = newStrPosition;

        transform.GetComponent<Collider>().enabled = true;

    }

    public void ShotBall()
    {
        foreach (var item in cueController.ballControllers)
        {
            item.networkPosition = transform.position;
            item.networkSpeed = 0.0f;
            item.networkAngularVelocity = Vector3.zero;
            item.oldNetworkAngularVelocity = Vector3.zero;
            item.sendTime = 0.0f;
            item.firsSend = false;
        }

        if (ServerController.serverController && !ServerController.serverController.isMyQueue && !MenuControllerGenerator.controller.playWithAI && !MenuControllerGenerator.controller.hotseat)
        {
            foreach (var item in cueController.ballControllers)
            {
                item.animate = true;
            }
        }
        else
        {
            
            bodyInitializer.OnActivate += OnActivate;
            foreach (var item in cueController.ballControllers)
            {
                try
                {
                    item.bodyInitializer.Activate(item.transform.position);
                }
                catch { }
            }
            foreach (var item in cueController.ballControllers)
            {
                item.animate = false;
            }
        }		
    }

    public IEnumerator StartAnimate()
    {
        yield return new WaitForFixedUpdate();
        while (!body)
        {
            yield return new WaitForFixedUpdate();
        }
        while (body && !ballIsOut)
        {
            ServerController.serverController.SendRPCToServer("SetNetworkParameters", ServerController.serverController.otherNetworkPlayer, id, body.velocity.magnitude, body.position, body.angularVelocity);
            yield return new WaitForSeconds(1.0f / ServerController.serverController.masterServerGUI.sendRate);
        }
    }

    public void ForceSetNetworkParameters(float networkSpeed, Vector3 networkPosition, Vector3 networkAngularVelocity)
    {
        sendTime = 0.0f;
        firsSend = true;
        this.networkSpeed = networkSpeed;
        this.networkPosition = networkPosition;
        this.networkAngularVelocity = networkAngularVelocity;
        StartCoroutine(ForceSetNetworkParametersUpdate());
    }

    IEnumerator ForceSetNetworkParametersUpdate()
    {
        while (!cueController.networkAllIsSleeping && !cueController.allIsSleeping && !body && !ballIsOut && Vector3.Distance(transform.position, networkPosition) > 0.1f * cueController.ballRadius)
        {
            float networkFixedDeltaTime = 1.0f / ServerController.serverController.masterServerGUI.sendRate;
            transform.position = Vector3.Lerp(transform.position, networkPosition, 15.0f * networkFixedDeltaTime);
            yield return new WaitForSeconds(networkFixedDeltaTime);
        }
    }

    public void SetNetworkParameters(float networkSpeed, Vector3 networkPosition, Vector3 networkAngularVelocity)
    {
        sendTime = 0.0f;
        firsSend = true;
        ;
        this.networkSpeed = networkSpeed;
        this.networkPosition = networkPosition;
        this.networkAngularVelocity = networkAngularVelocity;
    }

    void OnActivate(BodyInitializer bodyInitializer)
    {
        if (ServerController.serverController)
        {
            cueController.gameManager.StopCalculateShotTime();
        }
        cueController.OnPlayCueSound(cueController.ballShotVelocity.magnitude / cueController.ballMaxVelocity);
        if (!body.isKinematic)
        {
            body.velocity = Vector3.ClampMagnitude(cueController.ballShotVelocity, cueController.ballMaxVelocity);
            body.angularVelocity = cueController.ballShotAngularVelocity;
        }
        checkVelocity = body.velocity;
        checkAngularVelocity = cueController.ballShotAngularVelocity;
        body.AddTorque(body.mass * cueController.ballShotAngularVelocity, ForceMode.Impulse);
        firstHitIsChecked = false;

        cueController.cueBallPivotLocalPosition = Vector3.zero;
        cueController.cueRotationLocalPosition = Vector3.zero;
        if (ServerController.serverController && ServerController.serverController.isMyQueue && !MenuControllerGenerator.controller.playWithAI && !MenuControllerGenerator.controller.hotseat)
        {
            foreach (var item in cueController.ballControllers)
            {
                StartCoroutine(item.StartAnimate());
            }
        }
        bodyInitializer.OnActivate -= OnActivate;
    }

    public void OnBallIsOut(float _holeSplineLungth)
    {
        if (body)
        {
            body.useGravity = false;
            body.isKinematic = true;
        }
        GetComponent<Collider>().enabled = false;

        transform.position = holeSpline.Evaluate(_holeSplineLungth);

        if (!isMain)
        {
            enabled = false;
        }
    }

    void FixedUpdate()
    {
        if (!cueController || !cueController.ballsIsCreated)
            return;
                        
        if (ballIsOut)
        {
            if ((!body || !body.isKinematic) && step < holeSplineLungth)
            {
                holeSpline.AnimationSlider(transform, Mathf.Clamp(0.2f * cueController.ballMaxVelocity, 0.01f, 20.0f), ref step, out ballVeolociyInHole, 1, false);

                if (body)
                {
                    body.velocity = ballVeolociyInHole;
                }
            }
            else
            {
                if (GetComponent<Collider>().enabled)
                    OnBallIsOut(holeSplineLungth);
            }
        }

        if (!body)
        {
            
            if (firsSend && animate && !ballIsOut && ServerController.serverController && !ServerController.serverController.isMyQueue && !MenuControllerGenerator.controller.playWithAI && !MenuControllerGenerator.controller.hotseat)
            {
                
                if (networkSpeed < Physics.sleepThreshold)
                {
                    transform.position = Vector3.Lerp(transform.position, networkPosition, 3.0f * Time.fixedDeltaTime);
                }
                else
                {
                    if (Vector3.Distance(networkPosition, transform.position) > 2.0f * cueController.ballRadius)
                    {
                        transform.position += 1.1f * (networkPosition - transform.position).normalized * Time.fixedDeltaTime * networkSpeed;
                    }
                    else
                    {
                        transform.position = Vector3.Lerp(transform.position, networkPosition, 5.0f * Time.fixedDeltaTime);
                    }
                }
                    
                if (oldNetworkAngularVelocity != networkAngularVelocity || sendTime < 1.0f / ServerController.serverController.masterServerGUI.sendRate)
                {
                    oldNetworkAngularVelocity = networkAngularVelocity;
                    transform.Rotate(networkAngularVelocity.normalized, (180.0f / Mathf.PI) * networkAngularVelocity.magnitude * Time.fixedDeltaTime, Space.World);
                }
                sendTime += Time.fixedDeltaTime; 
            }
            return;
        }


        if (!ballIsOut)
        {
            if (!ballIsSelected && inMove && !body.isKinematic)
            {
                if (!ServerController.serverController || ServerController.serverController.isMyQueue)
                    velocityNormalized = body.velocity.magnitude / cueController.ballMaxVelocity;

                if (velocityNormalized < 0.01f)
                {
                    body.velocity = Vector3.Lerp(body.velocity, Vector3.zero, 5.0f * Time.fixedDeltaTime);
                    body.angularVelocity = Vector3.Lerp(body.angularVelocity, Vector3.zero, 1.5f * Time.fixedDeltaTime);
                }

            }
			
        }

        checkVelocity = Vector3.Lerp(checkVelocity, body.velocity, 10.0f * Time.fixedDeltaTime);
        checkAngularVelocity = Vector3.Lerp(checkAngularVelocity, body.angularVelocity, 10.0f * Time.fixedDeltaTime);


    }


    public void OnCheckHolle(int bonus, float offset)
    {
        MMVibrationManager.Vibrate();
        cueController.ballControllers.Remove(this);
        cueController.ballControllers.TrimExcess();
        cueController.ballsLeft--;
        cueController.levelBar.value++;
        var point = int.Parse(name);
        point *= bonus;
        cueController.Scoring(transform.position, point, offset);
        if(cueController.ballsLeft <= 0)
        {
            cueController.Result();
        }
        var tempCombo = cueController.ballsLeftMemory - cueController.ballsLeft;
        if (tempCombo >= 2)
        {
            if(disableDelay != null)
                StopCoroutine(disableDelay);
            cueController.comboPopup.SetActive(true);
            cueController.comboPopup.GetComponent<Text>().text = "x" + tempCombo;
            cueController.comboPopup.transform.DOLocalMoveX(-2, 0);
            cueController.comboPopup.transform.DOLocalMoveX(cueController.comboPopup.transform.position.x + 4, 2);
            disableDelay = StartCoroutine(delayDisable(cueController.comboPopup, 2));
            //if(tempCombo >= 1)
            //{
            //    if (!isFever)
            //    {
            //        isFever = true;
            //        Fever();
            //    }
            //}
        }
        if (cueController.ballCountStart - cueController.ballsLeft >= 2 && tempCombo == 1)
        {
            tempCombo += cueController.ballCountStart - cueController.ballsLeft - (cueController.ballsLeftMemory - cueController.ballsLeft);
            if (disableDelay != null)
                StopCoroutine(disableDelay);
            cueController.comboPopup.SetActive(true);
            cueController.comboPopup.GetComponent<Text>().text = "x" + tempCombo;
            cueController.comboPopup.transform.DOLocalMoveX(-2, 0);
            cueController.comboPopup.transform.DOLocalMoveX(cueController.comboPopup.transform.position.x + 4, 2);
            disableDelay = StartCoroutine(delayDisable(cueController.comboPopup, 2));
            //if (!isFever)
            //{
            //    isFever = true;
            //    Fever();
            //}
        }
        if (tempCombo > cueController.combo)
        {
            cueController.combo = tempCombo;
        }
        cueController.comboText.GetComponent<Text>().text = cueController.combo.ToString();
    }

    IEnumerator delayDisable(GameObject target, float time)
    {
        yield return new WaitForSeconds(time);
        target.SetActive(false);
    }

    public void OnPlayBallAudio(float audioVolume)
    {
        GetComponent<AudioSource>().volume = audioVolume;
        GetComponent<AudioSource>().Play();
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if(other.tag == "Hole")
    //    {
    //        Debug.Log("Hole!");
    //        GetComponent<SphereCollider>().isTrigger = true;
    //    }
    //}

    void OnCollisionExit(Collision collision)
    {
        if (!ballIsOut && ServerController.serverController && ServerController.serverController.isMyQueue && !MenuControllerGenerator.controller.playWithAI && !MenuControllerGenerator.controller.hotseat)
        {
            ServerController.serverController.SendRPCToServer("ForceSetNetworkParameters", ServerController.serverController.otherNetworkPlayer, id, body.velocity.magnitude, body.position, body.angularVelocity);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        MMVibrationManager.Vibrate();
        string layerName = LayerMask.LayerToName(collision.collider.gameObject.layer);

        if (layerName != "Wall" && layerName != "Ball")
            return;

        if (ServerController.serverController && !ServerController.serverController.isMyQueue && !MenuControllerGenerator.controller.playWithAI && !MenuControllerGenerator.controller.hotseat)
        {
			
        }
        else if (ServerController.serverController && (ServerController.serverController.isMyQueue || MenuControllerGenerator.controller.playWithAI || MenuControllerGenerator.controller.hotseat))
        {
            if (layerName == "Wall")
            {
                if (cueController.gameManager.isFirstShot && !hasFirstHit && !isMain && !isBlack)
                {
                    hasFirstHit = true;
                    cueController.gameManager.firstShotHitCount++;
                    try
                    {
                        transform.GetChild(1).gameObject.SetActive(false);
                    }
                    catch
                    {

                    }
                }
            }
            else if (isMain && layerName == "Ball")
            {
                BallController firstHitBall = collision.collider.GetComponent<BallController>();
                if (!cueController.gameManager.firstHitBall && firstHitBall)
                {
                    cueController.gameManager.firstHitBall = firstHitBall;

                    if (ServerController.serverController.isMyQueue)
                    {
                        if ((!cueController.gameManager.tableIsOpened &&
                        (cueController.gameManager.firstHitBall.ballType == cueController.gameManager.ballType ||
                        (cueController.gameManager.firstHitBall.isBlack && cueController.gameManager.afterRemainedBlackBall)
                        )
                        ) ||
                        (!cueController.gameManager.isFirstShot && cueController.gameManager.tableIsOpened && !cueController.gameManager.firstHitBall.isBlack))
                        {
                            cueController.gameManager.setMoveInTable = false;
                        }
                        else if (cueController.gameManager.gameInfoErrorText == "")
                        {
                            if (!cueController.gameManager.tableIsOpened && cueController.gameManager.firstHitBall.ballType != cueController.gameManager.ballType)
                            {
                                cueController.gameManager.gameInfoErrorText = "need to hit a " + (cueController.gameManager.ballType == 1 ? "solid" : "striped") + " ball";
                            }
                        }
                    }
                    else
                    {
                        if ((!cueController.gameManager.tableIsOpened &&
                        (cueController.gameManager.firstHitBall.ballType == -cueController.gameManager.ballType ||
                        (cueController.gameManager.firstHitBall.isBlack && cueController.gameManager.afterOtherRemainedBlackBall)
                        )) ||
                        (!cueController.gameManager.isFirstShot && cueController.gameManager.tableIsOpened && !cueController.gameManager.firstHitBall.isBlack))
                        {
                            cueController.gameManager.setMoveInTable = false;
                        }
                        else if (cueController.gameManager.gameInfoErrorText == "")
                        {
                            if (!cueController.gameManager.tableIsOpened && cueController.gameManager.firstHitBall.ballType == cueController.gameManager.ballType)
                            {
                                cueController.gameManager.gameInfoErrorText = "need to hit a " + (cueController.gameManager.ballType == -1 ? "solid" : "striped") + " ball";
                            }
                        }
                    }
                }

            }
        }
        if (cueController.ballsAudioPlayingCount < 3)
        {
            float audioVolume = Mathf.Clamp01(collision.relativeVelocity.magnitude / cueController.ballMaxVelocity);
            if (!ServerController.serverController || ServerController.serverController.isMyQueue || MenuControllerGenerator.controller.playWithAI || MenuControllerGenerator.controller.hotseat)
            {
                OnPlayBallAudio(audioVolume);
                if (ServerController.serverController && !MenuControllerGenerator.controller.playWithAI && !MenuControllerGenerator.controller.hotseat)
                {
                    ServerController.serverController.SendRPCToServer("OnPlayBallAudio", ServerController.serverController.otherNetworkPlayer, id, audioVolume);
                }
            }
        }
        if (isMain)
        {
            if (inMove && cueController && collision.collider.GetComponent<Rigidbody>())
            {
                firstHitIsChecked = true;
                if (cueController.currentHitBallController && cueController.currentHitBallController.body == collision.collider.GetComponent<Rigidbody>())
                    CheckCurrentHitBall(collision.relativeVelocity.magnitude, cueController.ballShotVelocity.magnitude);
                else
                    cueController.currentHitBallController = null;
            }
            else if (layerName == "Wall" && !body.isKinematic && !ballIsOut)
            {
                if (!firstHitIsChecked)
                {
                    firstHitIsChecked = true;
                    float decreaseSpeed = checkVelocity.magnitude / cueController.ballShotVelocity.magnitude;
                    body.velocity = 0.9f * decreaseSpeed * cueController.secondVelocity;
                }
                else
                    SetBallVelocity(checkVelocity);
            }
        }

    }

    void SetBallVelocity(Vector3 velocity)
    {
        if (!ServerController.serverController || ServerController.serverController.isMyQueue || MenuControllerGenerator.controller.playWithAI || MenuControllerGenerator.controller.hotseat)
        {
            body.velocity = Vector3.ClampMagnitude(0.9f * VectorOperator.getBallWallVelocity(0.99f * cueController.ballRadius, velocity, body.position, 
                    cueController.wallMask, 20.0f * cueController.ballRadius, Vector3.Project(checkAngularVelocity, Vector3.up)), 1000.0f);
        }
        checkVelocity = body.velocity;
        cueController.currentHitBallController = null;
    }

    public void CheckCurrentHitBall(float velocityMagnitude, float ballShotSpeed)
    {
        if (cueController.cueVerticalControll.cueForceValue >= 1.2f * cueController.cueVerticalControll.slider.minValue)
        {
            Debug.Log("return");
            return;
        }

        if (!ServerController.serverController || ServerController.serverController.isMyQueue || MenuControllerGenerator.controller.playWithAI || MenuControllerGenerator.controller.hotseat)
        {
            float decreaseSpeed = velocityMagnitude / ballShotSpeed;
            cueController.currentHitBallController.body.velocity = decreaseSpeed * cueController.hitBallVelocity;
            Vector3 addVelocity = cueController.ballRadius * VectorOperator.getPerpendicularXZ(VectorOperator.getProjectXZ(checkAngularVelocity, false));
            body.velocity = Vector3.ClampMagnitude(decreaseSpeed * (cueController.secondVelocity + cueController.ballVelocityCurve.Evaluate(cueController.cueForceValue) * addVelocity), body.velocity.magnitude);
        }
        StartCoroutine(WaitAndUncheck());
    }

    IEnumerator WaitAndUncheck()
    {
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        cueController.currentHitBallController = null;
    }
}
