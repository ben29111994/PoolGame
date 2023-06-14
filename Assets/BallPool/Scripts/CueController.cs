using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleInputNamespace;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
//using UnityStandardAssets.ImageEffects;
using System.Linq;

public class CueController : MonoBehaviour 
{
#region Parametrs
	//Distance of balls in the start, (correctly when it is a zero),
	[SerializeField]
	private float ballsDistance = 0.0f;
	//Maximum displacement of the cue during shot,
	public float cueMaxDisplacement = 9.0f;
	//Maximum velocity of the ball during shot, with maximum displacement of the cue,
	public float ballMaxVelocity = 85.0f;
	//Maximum length of the line, which shows the direction of the velocity after hitting balls,
	[SerializeField]
	private float ballLineLength = 7.0f;

	//Defines the angular velocity of the ball at impact, depending on the impact force,
	public AnimationCurve ballAngularVelocityCurve;
	//When the ball collides with another ball, he loses the linear velocity,but, because it also has a angular velocity, he can ride forward after collision
	//depending on the increased the force shot , the  ball gets a less angular velocity because no time to roll on the ground,
	public AnimationCurve ballVelocityCurve;
	//Control  sensitivity of cue for the mobile devices.
    public float touchSensitivity = 0.075f;
#endregion
	[SerializeField]
	private Menu menu;
	[SerializeField]
	private Camera guiCamera;
	[SerializeField]
	private Camera camera2D;
	[SerializeField]
	private Camera camera3D;
	[SerializeField]
	private Texture2D collisionBall;
	[SerializeField]
	private Texture2D collisionBallRed;
	private bool checkMyBall = false;
	private bool oldCheckMyBall = false;

	public CueFSMController cueFSMController;
	
	[SerializeField]
	private Transform mainBallPoint;
	[SerializeField]
	private Transform firstBallPoint;
	public Transform centerPoint;
    public List<BallController> ballControllers;
    public List<BallController> startBallControllers;

    public Texture2D[] ballTextures;
	[SerializeField]
	private Vector2[] deltaPositions;
	[System.NonSerialized]
	public int ballsCount = 16;

	[SerializeField]
	private Transform ballsParent;
	[SerializeField]
	private Transform ballsReflactionParent;
	[SerializeField]
	private Renderer lights;


	[SerializeField]
	private BallController ballControllerPrefab;
	[System.NonSerialized]
	public BallController ballController;

	[System.NonSerialized]
	public BallController currentSelectedBallController;

	private Camera currentCamera;
	public Transform cuePivot; 

	public Transform cueRotation;
	public Transform verticalControl;
	private Vector3 checkCuePosition = Vector3.zero;

	public BallPivotController cueBallPivot;
	public float ballRadius = 0.35f;
	[System.NonSerialized]
	public float cueDisplacement = 0.0f;


	[System.NonSerialized]
	public bool shotingInProgress = false;
	[System.NonSerialized]
	public bool thenInshoting = true;

	[System.NonSerialized]
	public bool allIsSleeping = true;
	[System.NonSerialized]
	public bool othersSleeping = true;
	private bool checkAllIsSleeping = true;
	private bool checkOthersIsSleeping = true;
	private bool checkInProgress = false;
	[System.NonSerialized]
	public bool inMove = false;
	[System.NonSerialized]
	public bool canMoveBall = true;
	[System.NonSerialized]
	public bool isFirsTime = true;
	[System.NonSerialized]
	public bool ballIsOut = false;

	private bool oldAllIsSleeping = true;

	[SerializeField]
	private GameObject cameraCircularSlider;
	public Transform StartCube;
	public Transform MoveCube;
	public Transform collisionSphere;
	[SerializeField]
	private LineRenderer firstCollisionLine;
	[SerializeField]
	private LineRenderer secondCollisionLine;
	[SerializeField]
	private LineRenderer ballCollisionLine;

	private Vector3 ballSelectPosition = Vector3.zero;

	private Vector3 ballCurrentPosition = Vector3.zero;
	[System.NonSerialized]
	public Vector3 ballShotVelocity = Vector3.zero;
	[System.NonSerialized]
	public Vector3 ballShotAngularVelocity = Vector3.zero;

	[System.NonSerialized]
	public Vector3 ballVelocityOrient = Vector3.forward;
	[System.NonSerialized]
	public Vector3 OutBallFirstVelocityOrient;
	[System.NonSerialized]
	public Vector3 OutBallSecondVelocityOrient;
	[System.NonSerialized]
	public Vector3 secondVelocity = Vector3.zero;
	
	[System.NonSerialized]
	public Vector2 rotationDisplacement = Vector2.zero;
	[System.NonSerialized]
	public Vector3 hitBallVelocity = Vector3.zero;
	[System.NonSerialized]
	public BallController currentHitBallController = null;
	[System.NonSerialized]
	public Collider hitCollider = null;

	[System.NonSerialized]
	public bool haveFirstCollision = false;
	[System.NonSerialized]
	public bool haveSecondCollision = false;
	[System.NonSerialized]
	public bool haveThrthCollision = false;
		

	[System.NonSerialized]
	 public LayerMask mainBallMask;
	[System.NonSerialized]
	public LayerMask ballMask;
	[System.NonSerialized]
	public LayerMask canvasMask;
    [System.NonSerialized]
    public LayerMask clothMask;
	[System.NonSerialized]
	public LayerMask wallMask;
	[System.NonSerialized]
	public LayerMask wallAndBallMask;
	private LayerMask canvasAndBallMask;
	private bool hitCanvas = false;
	
	[System.NonSerialized]
	public bool is3D = true;

	public Camera3DController camera3DController;
	private bool canDrawLinesAndSphere = false;
	[System.NonSerialized]
	public int ballsAudioPlayingCount = 0;
	private Vector3 cueRotationStrLocalPosition = Vector3.zero;


	private Vector3 cue2DStartPosition = Vector3.zero;

	[System.NonSerialized]
	public float cueForceValue = 1.0f;
	[System.NonSerialized]
	public bool inTouchForceSlider = false;
	[System.NonSerialized]
	public bool cueForceisActive = false;
	[System.NonSerialized]
	public float timeAfterShot = 0.0f;
	[System.NonSerialized]
	public bool networkAllIsSleeping = true;

	[System.NonSerialized]
	public GameManager gameManager;

	[System.NonSerialized]
	public Vector3 cueRotationLocalPosition = Vector3.zero;
	private Quaternion cuePivotLocalRotation = Quaternion.identity;
    private Quaternion cuePivotVerticalRotation = Quaternion.identity;
	[System.NonSerialized]
	public Vector3 cueBallPivotLocalPosition = Vector3.zero;
	[System.NonSerialized]
	public Vector3 ballMovePosition = Vector3.zero;
	private float sendTime = 0.0f;
	[System.NonSerialized]
	public bool ballsIsCreated = false;

	[System.NonSerialized]
	public bool otherWantToPlayAgain = false;
	private float touchRotateAngle;
    public CueForce cueVerticalControll;

    public GameObject ballParent;
    public List<GameObject> listBallParent = new List<GameObject>();
    public List<Transform> ballList = new List<Transform>();
    public int ballsLeft;
    public int ballsLeftMemory;
    public Transform mousePoint;
    Vector3 startMousePos;
    Vector3 currentMousePos;
    Vector3 lastMousePos;
    float force;
    bool isForce = false;
    int delay = 0;
    public Transform start;
    public Transform last;
    Vector3 dir;
    float angle;
    public GameObject levelText;
    public GameObject nextLevelText;
    public GameObject cueMain;
    Quaternion originalPos;
    Vector3 line;
    public GameObject cueCircle;
    public Slider levelBar;
    public bool isCircle = false;
    public int strike = 1;
    public int combo = 1;
    public int score;
    public GameObject plus;
    public GameObject strikeText;
    public GameObject comboText;
    public GameObject scoreText;
    public GameObject scoreInGameText;
    public GameObject strikeInGameText;
    public GameObject resultCanvas;
    public GameObject failedCanvas;
    public List<Color> listTableColor = new List<Color>();
    public MeshRenderer tableColor;
    public GameObject tutorialCanvas;
    public GameObject tutorial1;
    public GameObject tutorial2;
    public GameObject swipe1;
    public GameObject swipe2;
    public Canvas canvas;
    public GameObject comboPopup;
    public float comboPopupOriginPosX;
    public GameObject nothingPocketed;
    bool checkTurn = true;
    //bool isFever = false;
    public GameObject holeEffect;
    public GameObject ballEffect;
    Color originColor;
    public List<GameObject> listBonusPoint = new List<GameObject>();
    public List<GameObject> listHoles = new List<GameObject>();
    List<GameObject> tempList = new List<GameObject>();
    public GameObject trail;
    public int ballCountStart;
    public GameObject ceiling;
    public MeshRenderer cueRenderer;
    bool isFailed = false;
    public List<GameObject> mapList = new List<GameObject>();
    int currentMap;

    void Awake ()
	{
        strike = 1;
        combo = 1;
        comboPopupOriginPosX = comboPopup.transform.position.x;
        originColor = listTableColor[Random.Range(0, listTableColor.Count)];
        tableColor.material.color = originColor;
        tableColor.material.SetColor("_EmissionColor", originColor * 0.25f);
        var editorScreenHeight = Display.main.systemHeight;
        var editorScreenWidth = Display.main.systemWidth;
        var aspectRatio = /*editorScreenWidth / editorScreenHeight*/Camera.main.aspect;
        if (aspectRatio > 0.4f && aspectRatio < 0.5f)
        {
            //camera2D.transform.position = new Vector3(camera2D.transform.position.x, camera2D.transform.position.y + 14, camera2D.transform.position.z);
            camera2D.fieldOfView = 46;
        }
        Application.targetFrameRate = 60;
        currentMap = PlayerPrefs.GetInt("CurrentMap");
        levelText.GetComponent<Text>().text = currentMap.ToString();
        nextLevelText.GetComponent<Text>().text = (currentMap + 1).ToString();
        ballParent = listBallParent[currentMap];
        if (currentMap > listBallParent.Count - 1)
        {
            currentMap = 0;
        }
        PlayerPrefs.SetInt("CurrentMap", currentMap);
        ballParent.SetActive(true);
        ballsCount = ballParent.transform.childCount;
        for (int i = 0; i < ballsCount; i++)
        {
            if (ballParent.transform.GetChild(i).tag == "Hole")
            {
                ballsCount = i;
                break;
            }
            startBallControllers.Add(ballParent.transform.GetChild(i).GetComponent<BallController>());
            ballControllers.Add(ballParent.transform.GetChild(i).GetComponent<BallController>());
            ballList.Add(ballParent.transform.GetChild(i));
            ballParent.transform.GetChild(i).tag = "Ball";
        }
        ballsLeft = ballsCount - 1;
        ballCountStart = ballsLeft;
        levelBar.maxValue = ballsLeft;
        levelBar.value = 0;
        score = PlayerPrefs.GetInt("score");
        scoreInGameText.GetComponent<Text>().text = "SCORE  " + score.ToString();
        strikeInGameText.GetComponent<Text>().text = "STRIKE  " + strike.ToString();

        var randomMap = Random.Range(0, mapList.Count);
        mapList[randomMap].SetActive(true);

        if (!MenuControllerGenerator.controller)
			return;


		if(ServerController.serverController)
		{
			gameManager = GameManager.FindObjectOfType<GameManager>();
			gameManager.ShowGameInfo("Waiting for your opponent");
			enabled = false;
		}
		is3D = PlayerPrefs.GetInt("Current Camera") == 1;
		//lights.enabled = is3D;
		currentCamera = is3D? camera3D : camera2D;
		currentCamera.enabled = true;
		
		
        SortBalls ();
		ballsIsCreated = true;

		foreach (BallController item in ballControllers)
		{
			item.cueController = this;
			item.OnStart();
		}
		
		mainBallMask = 1 << LayerMask.NameToLayer("MainBall");
		ballMask = 1 << LayerMask.NameToLayer("Ball");
		canvasMask = 1 << LayerMask.NameToLayer("Canvas");
        clothMask = 1 << LayerMask.NameToLayer("Graund");
		wallMask = 1 << LayerMask.NameToLayer("Wall");
		wallAndBallMask = wallMask | ballMask;
		canvasAndBallMask = canvasMask | ballMask;
		
		//camera3D.enabled = false;
		camera2D.enabled = false;	

		collisionSphere.GetComponent<Renderer>().sharedMaterial.mainTexture = collisionBall;

        Dictionary<float, int> sortList = new Dictionary<float, int>();
        foreach (var item in listBonusPoint)
        {
            float totalDis = 0;
            var holdId = int.Parse(item.name);
            foreach (var ball in ballList)
            {
                totalDis += Vector3.Distance(listHoles[holdId].transform.position, ball.transform.position);
                //Debug.Log(item.name + ": " + totalDis);
            }
            sortList.Add(totalDis, holdId);
            //var value = (int)(totalDis / 30);
            //if(isFever)
            //{
            //    value *= 2;
            //    item.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 1).SetLoops(-1, LoopType.Yoyo);
            //}
            //else
            //{
            //    item.transform.DOKill();
            //}
        }
        var result = sortList.Keys.ToList();
        result.Sort();
        result.Reverse();
        var count = 5;
        foreach (var item in result)
        {
            var value = count * 2;
            if (value <= 0)
            {
                value = 1;
            }
            //Debug.Log(listHoles[sortList[item]]);
            listBonusPoint[int.Parse(listHoles[sortList[item]].tag)].GetComponent<Text>().text = "x" + value;
            listHoles[sortList[item]].name = value.ToString();
            count--;
        }
    }
	void OnEnable ()
	{
		if(ServerController.serverController)
		{
			if(gameManager)
			gameManager.HideGameInfo();
		}
	}
	IEnumerator Start ()
	{
        if (MenuControllerGenerator.controller)
        {
            FirstStart();
            cueFSMController.setMoveInTable();

            MenuControllerGenerator.controller.canRotateCue = true;
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            Ray ray = new Ray(centerPoint.position, -Vector3.up);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1000.0f, clothMask))
            {
                centerPoint.position = new Vector3(centerPoint.position.x, hit.point.y + ballRadius, centerPoint.position.z);
                foreach (var item in ballControllers)
                {
                    item.bodyInitializer.DeActivate(new Vector3(item.transform.position.x, centerPoint.position.y, item.transform.position.z));
                }
            }
        }
        cueCircle.SetActive(false);
    }
    void Update ()
	{
		if(shotingInProgress)
		{
			UpdateShotCue ();
		}
		if(!allIsSleeping)
		timeAfterShot += Time.fixedDeltaTime;
	}

#region FSMController
	void FirstStart ()
	{
		cueRotationStrLocalPosition = cueRotation.localPosition;
		OnShowLineAndSphereFirstTime ();
	}
	//When  player can move the cue ball in  table
	public void MoveInTable ()
	{
		if(oldAllIsSleeping != allIsSleeping)
		{
			SetOnChengSleeping ();
			cueFSMController.setInMove();
			if(allIsSleeping && ServerController.serverController)
			{
				gameManager.StartCalculateShotTime();
			}
			return;
		}
		if((!ServerController.serverController || ((ServerController.serverController.isMyQueue || MenuControllerGenerator.controller.hotseat) && networkAllIsSleeping)) && menu.GetButtonDown())
		{
			Ray ray = currentCamera.ScreenPointToRay(menu.GetScreenPoint());
			RaycastHit hit;
			
			if(Physics.SphereCast(ray, 5.0f * ballRadius, out hit, 1000.0f, mainBallMask))
			{
				OnSelectBall(ballController.transform.position);

				if(ServerController.serverController && (ServerController.serverController.isMyQueue || MenuControllerGenerator.controller.hotseat))
				{
					ServerController.serverController.SendRPCToServer("OnSelectBall", ServerController.serverController.otherNetworkPlayer, ballController.transform.position);
				}
			}
		}
		if(allIsSleeping && canDrawLinesAndSphere && !shotingInProgress/* && !ballIsOut*/)
		DrawLinesAndSphere ();

		canDrawLinesAndSphere = true;
	}

	public void MoveBall ()
	{
		if(ballIsOut)
		{
			OnBallIsOut(false);
		}

		if(ballController.ballIsSelected)
		{
			if((!ServerController.serverController || ServerController.serverController.isMyQueue || MenuControllerGenerator.controller.hotseat) && menu.MouseIsMove )
                //OnMoveBall();
            if (ServerController.serverController && !(ServerController.serverController.isMyQueue || MenuControllerGenerator.controller.hotseat))
			{
				ballController.transform.position = Vector3.Lerp(ballController.transform.position, ballMovePosition, 10.0f*Time.deltaTime);
			}
		}
		if(ballController.ballIsSelected && menu.GetButtonUp())
		{
			OnUnselectBall();
			if(ServerController.serverController && (ServerController.serverController.isMyQueue || MenuControllerGenerator.controller.hotseat))
			{
				ServerController.serverController.SendRPCToServer("OnUnselectBall",ServerController.serverController.otherNetworkPlayer);
			}
		}

	}
	public void InMove ()
	{
		CheckAllIsSleeping ();

		if(oldAllIsSleeping != allIsSleeping)
		{
			SetOnChengSleeping ();
			if(allIsSleeping)
			{
				StopCoroutine("WaitAndShowLineAndSphere");
				StartCoroutine("WaitAndShowLineAndSphere", false);
				if(ServerController.serverController)
				{
					gameManager.StartCalculateShotTime();
				}
			}
			else
			{
				HideLineAndSphere ();
			}

			foreach (BallController item in ballControllers)
			{
				item.inMove = !allIsSleeping;
			}
		}
		if(allIsSleeping && !shotingInProgress && !ballIsOut)
		DrawLinesAndSphere ();
	}
#endregion
    private float ballYPos;
	//When player select the ball
	public void OnSelectBall (Vector3 ballPosition)
	{
        ballYPos = ballController.transform.position.y;
		HideLineAndSphere ();
		MenuControllerGenerator.controller.canControlCue = false;
		ballMovePosition = ballPosition;
		ballSelectPosition = ballPosition;
		ballCurrentPosition = ballSelectPosition;

		ballController.ballIsSelected = true;



		canMoveBall = true;	
		cueFSMController.setMoveBall();
	}
	//When  player move the cue ball in table
	void OnMoveBall ()
	{
		Transform StartOrMoveCube = isFirsTime? StartCube : MoveCube;
		Ray ray = currentCamera.ScreenPointToRay(menu.GetScreenPoint());
		RaycastHit hit;
		
		if(Physics.SphereCast(ray, 1.0f*ballRadius , out hit, 1000.0f, canvasAndBallMask))
		{
			VectorOperator.MoveBallInQuad(StartOrMoveCube, ballRadius, hit.point, ref ballCurrentPosition);
			ballController.transform.position = ballCurrentPosition + 1.5f*ballRadius*Vector3.up;
			if(ServerController.serverController && (ServerController.serverController.isMyQueue || MenuControllerGenerator.controller.hotseat))
			{
				sendTime += Time.deltaTime;
				if(sendTime > 1.0f/10.0f)
				{
					sendTime = 0.0f;
					ServerController.serverController.SendRPCToServer("SetOnMoveBall", ServerController.serverController.otherNetworkPlayer, ballController.transform.position);
				}
			}
		}
	}
	//When player unselect the ball
	public void OnUnselectBall ()
	{
		MenuControllerGenerator.controller.canControlCue = true;

		Ray ray = new Ray(ballController.transform.position + 3.0f*ballRadius*Vector3.up, -Vector3.up);
		RaycastHit hit;
		
		if(Physics.SphereCast(ray, 1.0f*ballRadius , out hit, 1000.0f, canvasAndBallMask))
		{

			hitCanvas = hit.collider.gameObject.layer == LayerMask.NameToLayer("Canvas");
		}
		else
		{
			hitCanvas = false;
		}

		ballController.ballIsSelected = false;
		canMoveBall = false;
		cueFSMController.setMoveInTable();


		if(hitCanvas)
		{
			transform.position = ballController.transform.position;
		}
		else
		{
			ballController.RessetPosition( ballSelectPosition, true );
		}

        ballController.transform.position = new Vector3(ballController.transform.position.x, ballYPos, ballController.transform.position.z);


        ballController.bodyInitializer.position = ballController.transform.position;

		ballController.GetComponent<Collider>().enabled = true;
		

		if(ServerController.serverController && ServerController.serverController.isMyQueue && !MenuControllerGenerator.controller.playWithAI && !MenuControllerGenerator.controller.hotseat)
		{
            ServerController.serverController.SendRPCToServer("ForceSetBallMove", ServerController.serverController.otherNetworkPlayer, ballController.id, ballController.transform.position);
		}
		StopCoroutine("WaitAndShowLineAndSphere");
		StartCoroutine("WaitAndShowLineAndSphere", true);
	}


	void CheckAllIsSleeping ()
	{
		if(checkInProgress)
			return;
		checkOthersIsSleeping = thenInshoting;
		checkAllIsSleeping = thenInshoting;

		if(thenInshoting)
		{
			foreach (BallController ballC in ballControllers)
			{
				if(ballC != ballController)
				checkOthersIsSleeping = checkOthersIsSleeping && ballC.IsSleeping();
			}
			checkAllIsSleeping = checkOthersIsSleeping && (ballController.IsSleeping() || ballController.ballIsOut);
			StartCoroutine(WaitAndCheckAllIsSleeping ());
		}

		if(!checkAllIsSleeping)
		{
			ballsAudioPlayingCount = 0;
			foreach (BallController item in ballControllers)
			{
				if(item.GetComponent<AudioSource>().isPlaying)
					ballsAudioPlayingCount ++;
			}
		}
	}
	IEnumerator WaitAndCheckAllIsSleeping ()
	{
		checkInProgress = true;
		yield return new WaitForSeconds(1.0f);
	
		bool _allIsSleeping = thenInshoting;
		bool _othersSleeping = thenInshoting;
		foreach (BallController ballC in ballControllers)
		{

			if(ballC != ballController)
			{
				_othersSleeping = _othersSleeping && ballC.IsSleeping();
				_allIsSleeping = _allIsSleeping && ballC.IsSleeping();

			}
			else
			{
				_allIsSleeping = _allIsSleeping && (ballController.IsSleeping() || ballController.ballIsOut);
			}
		}
		if(_allIsSleeping == checkAllIsSleeping)
		{
			allIsSleeping = _allIsSleeping;
		}
		if(_othersSleeping == checkOthersIsSleeping)
		{
			othersSleeping = _othersSleeping;
		}
		checkInProgress = false;
	}
    public void CreateAndSortBalls ()
    {
        ballsParent = ballList[0].parent;
        Vector3 ballsParentPosition = ballList[0].transform.position;
      
        Transform Parent = ballsParent.parent;
        DestroyImmediate(ballsParent.gameObject);
        ballsParent = (new GameObject("Balls")).transform;
        ballsParent.position = ballsParentPosition;
        ballsParent.parent = Parent;

        ballRadius = 0.5f*ballControllerPrefab.transform.lossyScale.x;

        float newBallRadius = ballRadius + ballsDistance;
        ballsCount = ballTextures.Length;

        ballControllers = new List<BallController>(0);
        //startBallControllers = new BallController[ballsCount];
        for (int i = 0; i < ballsCount; i++)
        {
            BallController bc = null;
            float deltaX = deltaPositions[i].x;
            float deltaZ = deltaPositions[i].y;
            Vector3 position = i == 0 ? mainBallPoint.position : firstBallPoint.position +
                               new Vector3(deltaX * Mathf.Sqrt(Mathf.Pow(2.0f * newBallRadius, 2.0f) - Mathf.Pow(newBallRadius, 2.0f)), 0.0f, deltaZ * newBallRadius);


            bc = BallController.Instantiate(ballControllerPrefab) as BallController;

            bc.transform.position = position;
            bc.transform.parent = ballsParent;

            BodyInitializer bodyInitializer = bc.gameObject.AddComponent<BodyInitializer>();
        
            bodyInitializer.InitializeBodies();

            ballControllers.Add(bc);
            startBallControllers[i] = bc;
        }
    }
	void SortBalls ()
	{
		for (int i = 0; i < ballsCount; i++) 
		{
            BallController bc = startBallControllers[i];
            bc.name = "Ball_" + i;

			bc.isMain = i == 0;
			if(i == 0)
			{
				bc.ballType = 0;
				ballController = bc;
				ballController.gameObject.layer = LayerMask.NameToLayer("MainBall");
			}
			else
			{
				bc.gameObject.layer = LayerMask.NameToLayer("Ball");
				if(i == 8)
				{
					bc.isBlack = true;
					bc.ballType = 2;
				}
				else if(i >= 1 && i <= 7)
				{
					bc.ballType = 1;
				}
				else
				{
					bc.ballType = -1;
				}

			}
			bc.id = i;
			bc.cueController = this;
            bc.GetComponent<Renderer>().material.SetTexture("_DiffuseMap", ballTextures[i]);
            bc.name = i.ToString();
        }
       
	}


	IEnumerator WaitAndShowLineAndSphere (bool isFirst)
	{
		yield return new WaitForEndOfFrame();
		while(ballIsOut)
		{
			yield return null;
		}
		if(ServerController.serverController)
		{
			while(!networkAllIsSleeping)
			{
				yield return null;
			}
		}
		if(allIsSleeping)
		ShowLineAndSphere (isFirst);
	}
	public void OnBallIsOut (bool isOut)
	{
		ballIsOut = isOut;
		ballController.ballIsOut = ballIsOut;
	}

	void SetOnChengSleeping ()
	{
		//Set Some On Cheng Sleeping
		oldAllIsSleeping = allIsSleeping;
	}

	void DrawLinesAndSphere ()
	{
		if(!ballController.ballIsSelected && allIsSleeping && (!ServerController.serverController || networkAllIsSleeping || MenuControllerGenerator.controller.playWithAI || MenuControllerGenerator.controller.hotseat))
		OnDrawLinesAndSphere ();
	}
	IEnumerator WaitAndUncheckForceSlider ()
	{
		yield return new WaitForSeconds(0.3f);
	
		if(!cueForceisActive)
		inTouchForceSlider = false;
	}
	IEnumerator WaitAndRotateCue (Vector3 hitPoint)
	{
		yield return new WaitForSeconds (0.1f);
		if(MenuControllerGenerator.controller.canRotateCue)
			cuePivot.LookAt(hitPoint + ballRadius*Vector3.up);
	}
	void ControleCueThenDraw ()
	{
		if(MenuControllerGenerator.controller.canControlCue)
		{
            if (menu.GetButtonDown() && allIsSleeping)
            {
                Ray ray = currentCamera.ScreenPointToRay(menu.GetScreenPoint());
                RaycastHit hit;
                ceiling.SetActive(false);
                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    //startMousePos = new Vector3(hit.point.x, 0, hit.point.z);
                    if (hit.collider.tag == "Ball")
                    {
                        Debug.Log("Hit!");
                        cuePivot.LookAt(hit.point + ballRadius * Vector3.up);
                    }
                    inTouchForceSlider = true;
                    StartCoroutine(WaitAndUncheckForceSlider());
                    //mousePoint.gameObject.SetActive(true);
                    //mousePoint.transform.position = new Vector3(hit.point.x, 4f, hit.point.z);
                }
            }

            if (menu.GetButton() && !inTouchForceSlider)
            {
                //if (MenuControllerGenerator.controller.isTouchScreen)
                //{
                    Ray ray = currentCamera.ScreenPointToRay(menu.GetScreenPoint());
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, canvasMask))
                    {
                        //currentMousePos = new Vector3(hit.point.x, 0, hit.point.z);
                        Vector3 cuePivotScreenPoint = currentCamera.WorldToScreenPoint(cuePivot.position);
                        float orientY = menu.GetScreenPoint().y - cuePivotScreenPoint.y > 0.0f ? 1f : -1f;
                        float orientX = menu.GetScreenPoint().x - cuePivotScreenPoint.x > 0.0f ? 1f : -1f;
                        float speed = orientY * menu.MouseScreenSpeed.x - orientX * menu.MouseScreenSpeed.y;
                        touchRotateAngle = Mathf.Lerp(touchRotateAngle,
                                                      touchSensitivity * speed * Mathf.Abs(speed) * 0.5f * Time.deltaTime, 30 * Time.deltaTime);
                        //var checkForce = Vector3.Distance(currentMousePos, new Vector3(start.position.x, 0, start.position.z));
                        //if (hit.collider.gameObject.tag == "Cue" || isForce)
                        //{
                        //    Debug.Log("hit!");
                        //    isForce = true;
                        //    mousePoint.gameObject.SetActive(false);
                        //    //startMousePos = hit.point;
                        //    cueDisplacement = 0.0f;
                        //    lastMousePos = currentMousePos;
                        //    force = Vector3.Distance(lastMousePos, new Vector3(cuePivot.transform.position.x, 0, cuePivot.transform.position.z));
                        //    force /= 6;
                        //    cueDisplacement = Mathf.Clamp(force * cueMaxDisplacement, 0, 6);
                        //    if (cueDisplacement < 1f)
                        //    {
                        //        cueDisplacement = 0;
                        //    }
                        //    cueForceValue = 3f;
                        //}
                        //else
                        //{
                        //    Debug.Log("out hit!");
                        //    isForce = false;
                        //    cuePivot.Rotate(Vector3.up, touchRotateAngle);
                        //}
                        //Vector3 from = currentMousePos - lastMousePos;
                        //Vector3 to = start.position - last.position;
                        //float angle = Vector3.Angle(from, to);
                        //Debug.Log(angle);
                        //if ((hit.collider.gameObject.tag == "Cue" || isForce) && angle < 20f)
                        //{
                        //    if (!isForce)
                        //    {
                        //        isForce = true;
                        //        startMousePos = lastMousePos;
                        //    }
                        //    force = Vector3.Distance(currentMousePos, startMousePos);
                        //}
                        //else
                        //{
                        //    isForce = false;
                        cuePivot.Rotate(Vector3.up, touchRotateAngle);
                        cueDisplacement = 0.0f;
                        //}
                        //force /= 6;
                        //cueDisplacement = Mathf.Clamp(force * cueMaxDisplacement, 0, 6);
                        //if (cueDisplacement < 1f)
                        //{
                        //    cueDisplacement = 0;
                        //}
                        //cueForceValue = 3f;
                        //if (currentMousePos != lastMousePos && !isForce)
                        //    lastMousePos = currentMousePos;
                    }
                //}
                if (menu.GetButton())
                {
                    //if (!MenuControllerGenerator.controller.isTouchScreen)
                    //    cueDisplacement = cueMaxDisplacement * Mathf.Clamp01(Vector3.Dot(hit.point - cue2DStartPosition, (cuePivot.position - ballRadius * Vector3.up - cue2DStartPosition).normalized) / cueMaxDisplacement);
                    cueForceValue = 1.0f;
                }
            }

            if (menu.GetButtonUp())
			{
                ceiling.SetActive(true);
                MenuControllerGenerator.controller.canRotateCue = true;
				CheckShotCue ();
			}
		}
		else
		{
			cueDisplacement = 0.0f;
		}
		
		if(menu.GetButtonUp())
		{
			MenuControllerGenerator.controller.canControlCue = true;
		}
	}

	public void CheckShotCue ()
	{
		cueForceValue = Mathf.Clamp(cueDisplacement/cueMaxDisplacement, 0.005f, 1.0f);
		inTouchForceSlider = false;
		cueForceisActive = false;
		if(cueForceValue > 0.011f)
		{
			ShotCue ();
            checkTurn = false;
		}
		else
		{
			cueForceValue = 1.0f;
			cueRotation.localPosition = cueRotationStrLocalPosition;
		}
		if(MenuControllerGenerator.controller.isTouchScreen)
		{
			(CueForce.FindObjectOfType(typeof(CueForce)) as CueForce).Resset();
		}
	}

	void SendCueControlToNetwork ()
	{
		sendTime += Time.deltaTime;
		if(sendTime > 1.0f/10.0f/*Network.sendRate*/)
		{
			sendTime = 0.0f;
            if(cueRotationLocalPosition != cueRotation.localPosition || cuePivotLocalRotation != cuePivot.localRotation || cuePivotVerticalRotation != verticalControl.localRotation)
			{
				cueRotationLocalPosition = cueRotation.localPosition; 
				cuePivotLocalRotation = cuePivot.localRotation;
                ServerController.serverController.SendRPCToServer("SendCueControl", ServerController.serverController.otherNetworkPlayer, cuePivot.localRotation, cueRotation.localPosition, new Vector3( rotationDisplacement.x,rotationDisplacement.y, 0.0f) , cueVerticalControll.cueForceValue);
			}
		}
	}
    public void SetCueControlFromNetwork ( Quaternion localRotation, Vector3 localPosition, Vector2 displacement, float verticalRotation)
	{
		cuePivotLocalRotation = localRotation;
		cueRotationLocalPosition = localPosition;
        cuePivotVerticalRotation = Quaternion.Euler(verticalRotation, 0.0f, 0.0f);
		cueBallPivotLocalPosition = new Vector3(displacement.x*cueBallPivot.radius, displacement.y*cueBallPivot.radius, cueBallPivot.transform.localPosition.z);
        cueVerticalControll.slider.Value = verticalRotation;
        cueVerticalControll.cueForceValue = verticalRotation;
        rotationDisplacement = displacement;
	}
    void SendCueControl ( Quaternion localRotation, Vector3 localPosition, Quaternion cueVerticalRotation)
	{
		if (!cuePivot)
		{
			return;
		}
		cuePivot.localRotation = Quaternion.Lerp(cuePivot.localRotation, localRotation, 10.0f*Time.deltaTime);
		cueRotation.localPosition = Vector3.Lerp(cueRotation.localPosition, localPosition, 10.0f*Time.deltaTime);
		cueBallPivot.transform.localPosition = Vector3.Lerp(cueBallPivot.transform.localPosition, cueBallPivotLocalPosition, 10.0f*Time.deltaTime);
        verticalControl.localRotation = Quaternion.Lerp(verticalControl.localRotation, cueVerticalRotation, 10.0f*Time.deltaTime);
	}
	public void OnDrawLinesAndSphere ()
	{
		if (!transform)
		{
			return;
		}
		if(!ServerController.serverController || ((ServerController.serverController.isMyQueue || MenuControllerGenerator.controller.hotseat) && networkAllIsSleeping))
		{
            if (!checkTurn && !isFailed)
            {
                cueRenderer.enabled = true;
                checkTurn = true;
                if (ballsLeftMemory == ballsLeft)
                {
                    strike++;
                    nothingPocketed.SetActive(true);
                    scoreInGameText.GetComponent<Text>().text = "SCORE  " + score.ToString();
                    strikeInGameText.GetComponent<Text>().text = "STRIKE  " + strike.ToString();
                    ballCountStart = ballsLeft;
                    StartCoroutine(delayDisable(nothingPocketed, 2));
                    strikeText.GetComponent<Text>().text = strike.ToString();
                    //isFever = false;
                }
                Dictionary<float, int> sortList = new Dictionary<float, int>();
                foreach (var item in listBonusPoint)
                {
                    float totalDis = 0;
                    var holdId = int.Parse(item.name);
                    foreach (var ball in ballList)
                    {
                        totalDis += Vector3.Distance(listHoles[holdId].transform.position, ball.transform.position);
                        //Debug.Log(item.name + ": " + totalDis);
                    }
                    sortList.Add(totalDis, holdId);
                    //var value = (int)(totalDis / 30);
                    //if(isFever)
                    //{
                    //    value *= 2;
                    //    item.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 1).SetLoops(-1, LoopType.Yoyo);
                    //}
                    //else
                    //{
                    //    item.transform.DOKill();
                    //}
                }
                var result = sortList.Keys.ToList();
                result.Sort();
                result.Reverse();
                var count = 5;
                foreach (var item in result)
                {
                    var value = count * 2;
                    if (value <= 0)
                    {
                        value = 1;
                    }
                    //Debug.Log(listHoles[sortList[item]]);
                    listBonusPoint[int.Parse(listHoles[sortList[item]].tag)].GetComponent<Text>().text = "x" + value;
                    listHoles[sortList[item]].name = value.ToString();
                    count--;
                }
            }
			ControleCueThenDraw ();
            ballsLeftMemory = ballsLeft;
		}
		if(ServerController.serverController)
		{
			if(!(MenuControllerGenerator.controller.playWithAI || MenuControllerGenerator.controller.hotseat))
			{
				if(ServerController.serverController.isMyQueue)
					SendCueControlToNetwork ();
				else
                    SendCueControl(cuePivotLocalRotation, cueRotationLocalPosition, cuePivotVerticalRotation);
			}
			checkMyBall = false;
		}

		currentHitBallController = null;
		if (!transform)
		{
			return;
		}
		if(allIsSleeping)
		transform.position = ballController.transform.position;
        float displacementFactor = 0.0f;// 0.075f;
		ballVelocityOrient = (VectorOperator.getProjectXZ( cuePivot.forward, true) - displacementFactor * rotationDisplacement.x * cuePivot.right).normalized;  


		float lostEnergy = 1.0f - Mathf.Sqrt( (Mathf.Pow(rotationDisplacement.y,2.0f) + Mathf.Pow(rotationDisplacement.x,2.0f)) );
		lostEnergy = Mathf.Clamp(lostEnergy, 0.75f, 1.0f);

	
		ballShotVelocity = ballMaxVelocity*cueForceValue*lostEnergy*ballVelocityOrient;
		ballShotAngularVelocity = ballAngularVelocityCurve.Evaluate(cueForceValue)*Mathf.Pow( cueForceValue, 2.0f )*350.0f*(Mathf.Clamp(rotationDisplacement.y, -1.0f,1.0f)*cuePivot.right - rotationDisplacement.x*cuePivot.up);;
        //if (rotationDisplacement.y > -0.05f)
        //{
        //    float forwardFactor = 30.0f;
        //    ballShotAngularVelocity -= cueForceValue * forwardFactor * Mathf.Abs(Vector3.Dot(verticalControl.forward, Vector3.up)) * rotationDisplacement.x * cuePivot.forward;
        //}
		Ray firstRey = new Ray(cuePivot.position, ballVelocityOrient);
		RaycastHit firstHit;

		if(Physics.SphereCast(firstRey, 0.99f*ballRadius, out firstHit, 1000.0f, wallAndBallMask))
		{
			collisionSphere.position = firstHit.point + ballRadius*firstHit.normal;

		    Vector3	outVelocity = VectorOperator.getProjectXZ(ballShotVelocity, true);

			secondVelocity = VectorOperator.getBallVelocity(ballRadius, outVelocity, collisionSphere.position, 
			                                                wallAndBallMask, 20.0f*ballRadius, ref hitBallVelocity, ref hitCollider, rotationDisplacement.x);

			currentHitBallController = hitCollider.GetComponent<BallController>();
			ballCollisionLine.enabled = currentHitBallController && allIsSleeping;

			firstCollisionLine.SetVertexCount(2);
			firstCollisionLine.SetPosition(0, ballController.transform.position);
			firstCollisionLine.SetPosition(1, collisionSphere.position);
			
			secondCollisionLine.SetVertexCount(2);
			secondCollisionLine.SetPosition(0, collisionSphere.position);
			float angle1 = secondVelocity.magnitude/ballMaxVelocity;
			secondCollisionLine.SetPosition(1, collisionSphere.position + (angle1*ballLineLength + 0.7f*ballRadius)*secondVelocity.normalized);
			
			if(currentHitBallController)
			{
				ballCollisionLine.SetVertexCount(2);
                ballCollisionLine.SetPosition(0, currentHitBallController.transform.position);
                Vector3 hbvOrient = (currentHitBallController.transform.position - collisionSphere.position).normalized;
				float angle2 = Mathf.Abs( Vector3.Dot(hbvOrient, cuePivot.forward) );
                ballCollisionLine.SetPosition(1, currentHitBallController.transform.position + (angle2*ballLineLength + 0.99f*ballRadius)*hbvOrient);
			}
			else
			{
				ballCollisionLine.SetVertexCount(0);
			}
		}

		if(ServerController.serverController)
		{
			if((gameManager.tableIsOpened && (!currentHitBallController || !currentHitBallController.isBlack)) || !currentHitBallController || 
				(currentHitBallController.isBlack && gameManager.ballType != 0 && ((ServerController.serverController.isMyQueue || MenuControllerGenerator.controller.hotseat)? (gameManager.remainedBlackBall || (!ServerController.serverController.isMyQueue && gameManager.otherRemainedBlackBall && MenuControllerGenerator.controller.hotseat)): gameManager.otherRemainedBlackBall)))
			{
				checkMyBall = true;
			}
			else
			{
				checkMyBall = !currentHitBallController.isBlack && 
					((ServerController.serverController.isMyQueue/* || MenuControllerGenerator.controller.hotseat*/)? currentHitBallController.ballType == gameManager.ballType :
					 currentHitBallController.ballType != gameManager.ballType);
			}

			if(oldCheckMyBall != checkMyBall)
			{
				oldCheckMyBall = checkMyBall;
				collisionSphere.GetComponent<Renderer>().sharedMaterial.mainTexture = checkMyBall? collisionBall:collisionBallRed;
			}
		}
	}
	void HideLineAndSphere ()
	{
		OnHideLineAndSphere ();
	}
	public void OnHideLineAndSphere ()
	{
		collisionSphere.GetComponent<Renderer>().enabled = false;
		firstCollisionLine.enabled = false;
		secondCollisionLine.enabled = false;
		ballCollisionLine.enabled = false;
	}

	void ShowLineAndSphere (bool isFirst)
	{
		if(ServerController.serverController && (ServerController.serverController.isMyQueue || MenuControllerGenerator.controller.hotseat))
		{
			ServerController.serverController.SendRPCToServer("SendCueControl", ServerController.serverController.otherNetworkPlayer, cuePivot.localRotation, cueRotationStrLocalPosition, Vector3.zero, 0.0f);
		}
		if(isFirst)
			OnShowLineAndSphereFirstTime();
		else
		    OnShowLineAndSphere ();
	}
	public void OnShowLineAndSphereFirstTime ()
	{
		MenuControllerGenerator.controller.canControlCue = true;
		cueForceValue = 1.0f;
		collisionSphere.GetComponent<Renderer>().enabled = true;
		firstCollisionLine.enabled = true;
		secondCollisionLine.enabled = true;
		ballCollisionLine.enabled = true;
	}
	public void OnShowLineAndSphere ()
	{
		foreach (BallController item in ballControllers) 
		{
            item.bodyInitializer.DeActivate();
//			if(item.isMain)
//				StartCoroutine(WaitWhenResse (item));
//			else
//			item.GetComponent<Rigidbody>().Sleep();
		}
       
		OnShowLineAndSphereFirstTime ();


	}

	IEnumerator WaitWhenResse (BallController item)
	{
		yield return new WaitForSeconds(0.5f);
		if(allIsSleeping)
		item.GetComponent<Rigidbody>().Sleep();
	}
	public void OnControlCue ()
	{
		if(ServerController.serverController && !(ServerController.serverController.isMyQueue || MenuControllerGenerator.controller.hotseat) && !MenuControllerGenerator.controller.playWithAI && !MenuControllerGenerator.controller.hotseat)
			return;
		if (!cueBallPivot)
		{
			return;
		}
		//verticalControl.localRotation = Quaternion.Euler (cueVerticalControll.cueForceValue, 0.0f, 0.0f);
		float x = ballRadius*cueBallPivot.transform.localPosition.x/cueBallPivot.radius;
		float y = ballRadius*cueBallPivot.transform.localPosition.y/cueBallPivot.radius;
        if (Mathf.Abs(x) < 0.15f)
        {
            x = 0.0f;
        }
        if (Mathf.Abs(y) < 0.15f)
        {
            y = 0.0f;
        }

		rotationDisplacement = (1.0f/(ballRadius))*(new Vector2(x, y));

		float z = - Mathf.Sqrt(Mathf.Clamp(  Mathf.Pow( ballRadius, 2.0f ) - ( Mathf.Pow( x, 2.0f) +  Mathf.Pow( y, 2.0f )), 0.0f, Mathf.Pow( ballRadius, 2.0f ) ));

		if(!shotingInProgress)
		{
			checkCuePosition = new Vector3(x, y, z);
		}


		cueRotation.localPosition = checkCuePosition - cueDisplacement*Vector3.forward;
	}

	void ShotCue ()
	{
        tutorialCanvas.SetActive(false);
        scoreInGameText.SetActive(true);
        tutorial1.SetActive(false);
        tutorial2.SetActive(false);
        swipe1.SetActive(false);
        swipe2.SetActive(false);
        if (!allIsSleeping)
            return;
		if (ServerController.serverController && ServerController.serverController.isMyQueue && !MenuControllerGenerator.controller.playWithAI && !MenuControllerGenerator.controller.hotseat)
        {
            ServerController.serverController.SendRPCToServer("OnShotCue", ServerController.serverController.otherNetworkPlayer);
        }
        OnShotCue();
	}

	private float shotTime = 0.0f;
	//Player is shot (for cue)
	public void OnShotCue ()
	{
		if(!allIsSleeping)
			return;
		timeAfterShot = 0.0f;

		shotTime = 0.0f;
		isFirsTime = false;

		HideLineAndSphere();
		allIsSleeping = false;
		othersSleeping = false;
		inMove = true;

		shotingInProgress = true;
		thenInshoting = false;
		OnBallIsOut(false);

        StopCoroutine("WaitWhenAllIsSleeping");
		StartCoroutine("WaitWhenAllIsSleeping");

	}

	//When all balls is sleeping 
	IEnumerator WaitWhenAllIsSleeping ()
	{
		if(ServerController.serverController)
		{
			if(!(MenuControllerGenerator.controller.playWithAI || MenuControllerGenerator.controller.hotseat))
			{
			    networkAllIsSleeping = false;
			}
			gameManager.ballsInMove = true;
		}
        yield return new WaitForSeconds(0.2f);
        cueRenderer.enabled = false;
        //Time.timeScale = 1.5f;
        while (ballController.ballIsSelected || !allIsSleeping || ballIsOut || ballController.ballIsOut)
		{
			yield return null;
		}
        //Time.timeScale = 1;
		if (!ServerController.serverController)
		{
			cueBallPivot.Reset ();
            cueVerticalControll.Resset();
            cueVerticalControll.slider.Value = cueVerticalControll.slider.minValue;
            cueVerticalControll.cueForceValue = cueVerticalControll.slider.minValue;
            verticalControl.localRotation = Quaternion.Euler(cueVerticalControll.slider.minValue, 0.0f, 0.0f);
		}
		else
		{
			//if(!(MenuControllerGenerator.controller.playWithAI || MenuControllerGenerator.controller.hotseat))
			//{
			//	ServerController.serverController.SendRPCToServer("OnChanghAllIsSleeping", ServerController.serverController.otherNetworkPlayer);
			//}
			bool isMyQueue = ServerController.serverController.isMyQueue;

			if(gameManager.needToChangeQueue)
			{
				ServerController.serverController.isMyQueue = false;
			}

			//if(MenuControllerGenerator.controller.playWithAI || MenuControllerGenerator.controller.hotseat)
			//{
			//	networkAllIsSleeping = true;
			//}
			//if(isMyQueue || MenuControllerGenerator.controller.playWithAI || MenuControllerGenerator.controller.hotseat)
			//{
			//	if(!(MenuControllerGenerator.controller.playWithAI || MenuControllerGenerator.controller.hotseat))
			//	{
   //                 foreach (var item in ballControllers)
   //                 {
   //                     item.bodyInitializer.DeActivate(item.transform.position);
   //                 }
   //                 string ballsPositions = GetAllBallsPositions();
   //                 ServerController.serverController.SendRPCToServer("SetBodyesPositions", ServerController.serverController.otherNetworkPlayer, ballsPositions);
			//	}

			//	if(gameManager.needToForceChangeQueue || gameManager.needToChangeQueue)
			//	{

			//		while(!networkAllIsSleeping)
			//		{
			//			yield return null;
			//		}
			//		if((gameManager.isFirstShot && gameManager.firstShotHitCount < 4) || gameManager.setMoveInTable)
			//		{
			//			if(isMyQueue)
			//			{
			//				ServerController.serverController.SendRPCToServer("SetMoveInTable", ServerController.serverController.otherNetworkPlayer);
			//			}
			//			else
			//			{
			//				cueFSMController.setMoveInTable();
			//			}
					
			//			string gameInfoErrorText = "";
			//			string  otherGameInfoErrorText = "";

			//			if(gameManager.gameInfoErrorText == "")
			//			{
			//				if(gameManager.isFirstShot && gameManager.firstShotHitCount < 4)
			//				{
			//					gameInfoErrorText = "You made an illegal break\n" + ServerController.serverController.otherName + " has ball in hand";
			//					otherGameInfoErrorText = ((MenuControllerGenerator.controller.playWithAI || MenuControllerGenerator.controller.hotseat)? ServerController.serverController.otherName :ServerController.serverController.myName) + 
			//						" made an illegal break" + "\nYou have ball in hand";
			//				}
			//				else if(!gameManager.isFirstShot && gameManager.setMoveInTable)
			//				{
			//					if(gameManager.tableIsOpened)
			//					{
			//						gameInfoErrorText = "You need to hit either a solid or striped ball" + "\n" + ServerController.serverController.otherName + " has ball in hand";
			//						otherGameInfoErrorText = ((MenuControllerGenerator.controller.playWithAI || MenuControllerGenerator.controller.hotseat)? ServerController.serverController.otherName :ServerController.serverController.myName)
			//							+ " need to hit either a solid or striped ball" + "\nYou have ball in hand";
			//					}
			//					else
			//					{
			//						gameInfoErrorText = "The cue ball did not strike another ball" + "\n" + ServerController.serverController.otherName + " has ball in hand";
			//						otherGameInfoErrorText = "The cue ball did not strike another ball" + "\nYou have ball in hand";
			//					}
			//				}
			//			}
			//			else
			//			{
			//				gameInfoErrorText = "You " +  gameManager.gameInfoErrorText + "\n" + ServerController.serverController.otherName + " has ball in hand";
			//				otherGameInfoErrorText = ((MenuControllerGenerator.controller.playWithAI || MenuControllerGenerator.controller.hotseat)? ServerController.serverController.otherName :ServerController.serverController.myName)
			//					+ "  " + gameManager.gameInfoErrorText + "\nYou have ball in hand";
			//			}
			//			if(isMyQueue)
			//			{
			//			    gameManager.ShowGameInfoError(gameInfoErrorText, 5.0f);
			//			} else
			//			{
			//				gameManager.ShowGameInfoError(otherGameInfoErrorText, 5.0f);
			//			}
			//			if(!MenuControllerGenerator.controller.playWithAI && !MenuControllerGenerator.controller.hotseat)
			//			{
			//				ServerController.serverController.SendRPCToServer("SetErrorText", ServerController.serverController.otherNetworkPlayer, otherGameInfoErrorText);
			//			}

			//		}
			//		if(!(gameManager.blackBallInHolle || gameManager.otherBlackBallInHolle))
			//		{
			//			if((MenuControllerGenerator.controller.playWithAI || MenuControllerGenerator.controller.hotseat) && !isMyQueue)
			//			    ServerController.serverController.ChangeQueue(true);
			//			else
			//				ServerController.serverController.ChangeQueue(false);
			//		}
			//	}
			//}
			//if(gameManager.needToForceChangeQueue || gameManager.needToChangeQueue)
			//{

			//} else if (ServerController.serverController.isServerClientArchitecture && ServerController.serverController.isMyQueue && !MenuControllerGenerator.controller.playWithAI && !MenuControllerGenerator.controller.hotseat)
			//{
			//	if (ServerController.serverController.myNetworkPlayer != 0)
			//	{
			//		StopCoroutine("WaitAndSendOnClientStartPlay");
			//		StartCoroutine("WaitAndSendOnClientStartPlay", ServerController.serverController.isMyQueue);

			//	}
			//}

			if(gameManager.blackBallInHolle || gameManager.otherBlackBallInHolle)
			{
				if((isMyQueue || MenuControllerGenerator.controller.playWithAI || MenuControllerGenerator.controller.hotseat))
				{
					if(gameManager.mainBallIsOut)
					{
                        if (isMyQueue)
						{
							gameManager.ShowGameInfoError("you potted the cue ball with black ball", 5.0f);
						} else
						{
							gameManager.ShowGameInfoError(ServerController.serverController.otherName + " potted the cue ball with black ball", 5.0f);
						}
						
						if(!MenuControllerGenerator.controller.playWithAI && !MenuControllerGenerator.controller.hotseat)
						{
							ServerController.serverController.SendRPCToServer("SetErrorText", ServerController.serverController.otherNetworkPlayer, ServerController.serverController.myName + " potted the cue ball with black ball");
						}
					}else
					{
						if(isMyQueue)
						{
							if(gameManager.firstHitBall && !gameManager.firstHitBall.isBlack &&  gameManager.afterRemainedBlackBall && gameManager.ballType != gameManager.firstHitBall.ballType)
							{
								gameManager.HideGameInfoError();
								gameManager.ShowGameInfoError("need to hit a " + (gameManager.ballType == 1? "solid":"striped") +  " or black ball", 5.0f);
								if(!MenuControllerGenerator.controller.playWithAI && !MenuControllerGenerator.controller.hotseat)
								{
									ServerController.serverController.SendRPCToServer("SetErrorText", ServerController.serverController.otherNetworkPlayer, ServerController.serverController.myName + " need to hit a " + (gameManager.ballType == 1? "solid":"striped") +  " or black ball");
								}
							}
						} else
						{
							if(gameManager.firstHitBall && !gameManager.firstHitBall.isBlack &&  gameManager.afterOtherRemainedBlackBall && gameManager.ballType == gameManager.firstHitBall.ballType)
							{
								gameManager.HideGameInfoError();
								gameManager.ShowGameInfoError(ServerController.serverController.otherNetworkPlayer + " need to hit a " + (gameManager.ballType == -1? "solid":"striped") +  " or black ball", 5.0f);
							}
						}
						
					}
				}
				if (!ServerController.serverController || MenuControllerGenerator.controller.playWithAI || MenuControllerGenerator.controller.hotseat || ServerController.serverController.isMyQueue)
				{
					gameManager.ActivateMenuButtons(true);
					if (ServerController.serverController && !MenuControllerGenerator.controller.playWithAI && !MenuControllerGenerator.controller.hotseat)
					{
						ServerController.serverController.SendRPCToServer("ActivateMenuButtons", ServerController.serverController.otherNetworkPlayer);
					}
				}
			}


			if(gameManager.remainedBlackBall)
			{
				gameManager.afterRemainedBlackBall = true;
			}
			if(gameManager.otherRemainedBlackBall)
			{
				gameManager.afterOtherRemainedBlackBall = true;
			}



			gameManager.isFirstShot = false;
			gameManager.setMoveInTable = true;
			gameManager.needToChangeQueue = true;
			gameManager.needToForceChangeQueue = false;
			gameManager.firstHitBall = null;
			gameManager.gameInfoErrorText = "";
			gameManager.ballsInMove = false;

			gameManager.mainBallIsOut = false;
			gameManager.otherMainBallIsOut = false;

			RessetShotOptions();

			if(MenuControllerGenerator.controller.playWithAI && !isMyQueue)
			{
				ServerController.serverController.serverMessenger.ShotWithAI ();
			}


        }
    }

	IEnumerator WaitAndSendOnClientStartPlay(bool myTurn)
	{
		while (!MenuControllerGenerator.controller.canControlCue || !allIsSleeping || !cueFSMController.inMove || !gameManager.calculateShotTime ||!networkAllIsSleeping)
		{
			yield return null;
		}
		StartCoroutine(ServerController.serverController.serverMessenger.WaitAndSendOnClientStartPlay(myTurn));
	}
	public void OnPlayBallSound (float volume)
	{
		ballController.GetComponent<AudioSource>().volume = volume;
		ballController.GetComponent<AudioSource>().Play();
	}
	public void OnPlayCueSound (float volume)
	{
		GetComponent<AudioSource>().volume = volume;
		GetComponent<AudioSource>().Play();
	}

	void UpdateShotCue ()
	{
		shotTime += Time.deltaTime;
		if(shotingInProgress && Vector3.Distance(cueRotation.localPosition, checkCuePosition + cueRotationStrLocalPosition) < 0.1f*ballRadius)
		{
			cueRotation.localPosition = checkCuePosition + cueRotationStrLocalPosition;
			shotingInProgress = false;

			cueDisplacement = 0.0f;

			foreach (BallController item in ballControllers)
			{
				item.inMove = !allIsSleeping;
			}
			StartCoroutine(WaitAndSetThenInshoting ());
			if(!ServerController.serverController || ServerController.serverController.isMyQueue || MenuControllerGenerator.controller.playWithAI || MenuControllerGenerator.controller.hotseat)
			{
				if (ServerController.serverController && !MenuControllerGenerator.controller.playWithAI && !MenuControllerGenerator.controller.hotseat)
                {
                    string ballsPositions = GetAllBallsPositions();
                   
                    ServerController.serverController.SendRPCToServer("ShotBall", ServerController.serverController.otherNetworkPlayer, ballShotVelocity, hitBallVelocity, secondVelocity, ballShotAngularVelocity, ballsPositions);
					if (ServerController.serverController.isServerClientArchitecture && !MenuControllerGenerator.controller.playWithAI && !MenuControllerGenerator.controller.hotseat)
					{
						if (ServerController.serverController.myNetworkPlayer != 0)
						{
							ServerController.serverController.SendRPCToServer("OnClientShot", ServerController.serverController.myNetworkPlayer);
						}
					}
                }
                ballController.ShotBall();
			}
		}
		else
		{
			cueRotation.localPosition = Vector3.Lerp(cueRotation.localPosition, checkCuePosition + cueRotationStrLocalPosition, 100.0f*( 0.9f*cueForceValue + 0.1f)*Time.deltaTime);
		}
	}

    public string GetAllBallsPositions ()
    {
        string ballsPositions = "";
        foreach (var item in ballControllers)
        {
            ballsPositions += item.id.ToString() + "(" + item.transform.position.x + "," + item.transform.position.y + "," + item.transform.position.z + ")";
        }
        return  ballsPositions;
    }

	IEnumerator WaitAndSetThenInshoting ()
	{
		yield return new WaitForSeconds(3.0f);
		thenInshoting = true;
	}
	void SetCamera (Button btn)
	{
		is3D = btn.state;
		//lights.enabled = is3D;
		currentCamera = is3D? camera3D : camera2D;
		//camera3D.enabled = false;
		camera2D.enabled = false;
		currentCamera.enabled = true;
		cameraCircularSlider.SetActive( btn.state );
		PlayerPrefs.SetInt("Current Camera", btn.state? 1:0);
	}
	public void RessetShotOptions ()
	{
		cueBallPivot.Reset();
        cueVerticalControll.slider.Value = cueVerticalControll.slider.minValue;
        cueVerticalControll.cueForceValue = cueVerticalControll.slider.minValue;
        verticalControl.localRotation = Quaternion.Euler(cueVerticalControll.slider.minValue, 0.0f, 0.0f);
		if(ballController.ballIsSelected)
		{
			OnUnselectBall();
		}
	}

    public void ButtonCueCircle()
    {
        if (!cueCircle.activeSelf)
        {
            isCircle = true;
            cueCircle.SetActive(true);
        }
        else
        {
            isCircle = false;
            cueCircle.SetActive(false);
        }
    }

    public void Result()
    {
        allIsSleeping = false;
        resultCanvas.SetActive(true);
        PlayerPrefs.SetInt("score", score);
        scoreText.GetComponent<Text>().text = score.ToString();
        strikeText.GetComponent<Text>().text = strike.ToString();
        comboText.GetComponent<Text>().text = combo.ToString();
        currentMap++;
        PlayerPrefs.SetInt("CurrentMap", currentMap);
    }

    public void Failed()
    {
        isFailed = true;
        allIsSleeping = false;
        failedCanvas.SetActive(true);
    }

    public void Swipe1Tutorial()
    {
        tutorialCanvas.SetActive(false);
        scoreInGameText.transform.parent.gameObject.SetActive(true);
        strikeInGameText.transform.parent.gameObject.SetActive(true);
        tutorial1.SetActive(false);
        tutorial2.SetActive(true);
        swipe1.SetActive(false);
        swipe2.SetActive(false);
    }

    public void Swipe2Tutorial()
    {
        tutorial2.SetActive(false);
        swipe2.SetActive(false);
    }

    IEnumerator delayDisable(GameObject target, float time)
    {
        yield return new WaitForSeconds(time);
        target.SetActive(false);
    }

    public void Scoring(Vector3 pos, int plusScore, float offset)
    {
        score += plusScore;
        scoreInGameText.GetComponent<Text>().text = "SCORE  " + score.ToString();
        var plusVar = Instantiate(plus);
        plusVar.transform.parent = canvas.transform;
        plusVar.transform.localScale = new Vector3(1, 1, 1);
        plusVar.transform.position = worldToUISpace(canvas, pos);
        plusVar.transform.DOMoveX(plusVar.transform.position.x + offset, 0);
        plusVar.transform.DOMoveY(plusVar.transform.position.y + 25, 2);
        plusVar.GetComponent<Text>().text = "+" + plusScore.ToString();
        StartCoroutine(delayDisable(plusVar, 2));
        scoreText.GetComponent<Text>().text = score.ToString();
    }

    public Vector3 worldToUISpace(Canvas parentCanvas, Vector3 worldPos)
    {
        //Convert the world for screen point so that it can be used with ScreenPointToLocalPointInRectangle function
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        Vector2 movePos;

        //Convert the screenpoint to ui rectangle local point
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentCanvas.transform as RectTransform, screenPos, parentCanvas.worldCamera, out movePos);
        //Convert the local point to world point
        return parentCanvas.transform.TransformPoint(movePos);
    }

    public void LoadScene()
    {
        SceneManager.LoadScene(2);
    }

    //public void Fever()
    //{
    //    camera2D.GetComponent<BloomOptimized>().enabled = true;
    //    tableColor.material.color = Color.black;
    //    tableColor.material.SetColor("_EmissionColor", Color.black * 0.25f);
    //    foreach (var item in ballList)
    //    {
    //        var effect = Instantiate(ballEffect);
    //        effect.transform.parent = item.transform;
    //        effect.transform.localPosition = Vector3.zero;
    //        effect.transform.localScale = new Vector3(3, 3, 3);

    //        var trailEffect = Instantiate(trail);
    //        trailEffect.transform.parent = item.transform;
    //        trailEffect.transform.localPosition = Vector3.zero;
    //        trailEffect.transform.localScale = new Vector3(1, 1, 1);
    //        trailEffect.SetActive(true);
    //    }
    //    foreach(var item in listHoles)
    //    {
    //        var effect = Instantiate(holeEffect);
    //        effect.transform.position = item.transform.GetChild(0).transform.position;
    //        effect.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
    //        effect.GetComponent<ParticleSystem>().loop = true;
    //        tempList.Add(effect);
    //    }
    //    StartCoroutine(feverTime());
    //}

    //IEnumerator feverTime()
    //{
    //    while (isFever)
    //    {
    //        yield return null;
    //    }
    //    camera2D.GetComponent<BloomOptimized>().enabled = false;
    //    tableColor.material.color = originColor;
    //    tableColor.material.SetColor("_EmissionColor", originColor * 0.25f);
    //    foreach (var item in ballList)
    //    {
    //        Destroy(item.transform.GetChild(0).gameObject);
    //        Destroy(item.transform.GetChild(1).gameObject);
    //    }
    //    foreach (var item in tempList)
    //    {
    //        Destroy(item);
    //    }
    //    foreach (var item in listBonusPoint)
    //    {
    //        item.transform.DOKill();
    //    }
    //}
}
