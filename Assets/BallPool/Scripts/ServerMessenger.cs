using UnityEngine;
using System.Collections;

public class ServerMessenger : MonoBehaviour 
{
	public CueController cueController;
	void Awake ()
	{
		Application.runInBackground = true;
	}

	void OnEnable () 
	{
		MenuControllerGenerator.controller.OnLoadLevel += FindCueController;
	}

	void FindCueController (MenuController menuController)
	{
        StopCoroutine("WaitAndFindCueController");
		StopCoroutine("WaitAndEnabledCueController");

		StartCoroutine("WaitAndFindCueController", menuController.preloader);
        StopCoroutine("WaitAndForceEnabled");
        StartCoroutine("WaitAndForceEnabled");
	}
    public void SendFirstPlayerName(string otherName)
    {
        ServerController.serverController.otherName = otherName;
        ServerController.serverController.myName = ServerController.serverController.masterServerGUI.gameName;
        //ServerController.serverController.SendRPCToServer("SendSecondPlayerName", ServerController.serverController.otherNetworkPlayer, ServerController.serverController.myName);
    }
    public void SendSecondPlayerName(string otherName)
    {
        ServerController.serverController.otherName = otherName;
        GameManager gameManager = GameManager.FindObjectOfType<GameManager>();
        if (gameManager)
        {
            gameManager.otherProfile.playerName.text = ServerController.serverController.otherName;
        }
    }
	void OnDisable ()
	{
		MenuControllerGenerator.controller.OnLoadLevel -= FindCueController;
	}
    IEnumerator WaitAndForceEnabled ()
    {
        yield return new WaitForSeconds(5.0f);
		if (cueController && ServerController.serverController && !MenuControllerGenerator.controller.playWithAI && !MenuControllerGenerator.controller.hotseat)
        {
            if (!cueController.enabled || !cueController.cueFSMController.enabled)
            {
                cueController.enabled = true;
                cueController.cueFSMController.enabled = true;
                string info = ServerController.serverController.isMyQueue ? "You are breaking\n Good luck!" : "Your opponent is breaking\n Good luck!";
                cueController.gameManager.ShowGameInfo(info, 2.5f);
                Debuger.DebugOnScreen("EndEnabledCueController");
            }
        }
    }
	IEnumerator WaitAndFindCueController (Preloader preloader)
	{
		cueController = null;
        float maxTime = 0.0f;
        while(!preloader.isDone && maxTime < 3.0f)
		{
            maxTime += Time.deltaTime;
			yield return null;
		}
		while(!cueController)
		{
			cueController = CueController.FindObjectOfType<CueController>();
			yield return null;
		}
		Debuger.DebugOnScreen("CueController " + (cueController != null).ToString());
		OnFindCueController();
	}
	public void ShotWithAI ()
	{
		ServerController.logicAI.ShotCue(cueController);
	}

	void OnFindCueController()
	{
		if(MenuControllerGenerator.controller.playWithAI || MenuControllerGenerator.controller.hotseat)
		{
			OnReadyToPlay ();
			if(!ServerController.serverController.isMyQueue)
			{
				ShotWithAI();
			}
		}
		else
		{
			Debuger.DebugOnScreen("SendRPCToServer ReadyToPlay");
			ServerController.serverController.SendRPCToServer("ReadyToPlay", ServerController.serverController.otherNetworkPlayer);
		}
	}
	public void ShowOtherMessage (string message)
	{
		ProfileMessenger.FindObjectOfType<ProfileMessenger>().ShowOtherMessage(message);
	}
	public void SetMoveInTable ()
	{
		if(!cueController)
			return;
		cueController.cueFSMController.setMoveInTable();
	}
	public IEnumerator WaitAndSendOnClientStartPlay(bool myTurn)
	{
		CueController cueController = CueController.FindObjectOfType<CueController>();
		if (cueController)
		{
			while (!cueController.enabled || !cueController.networkAllIsSleeping)
			{
				yield return null;
			}

			yield return new WaitForSeconds(0.2f + Time.fixedDeltaTime);
			if (!cueController.gameManager.menuButtonsIsActive)
			{
				ServerController.serverController.SendRPCToServer("OnClientStartPlay", ServerController.serverController.myNetworkPlayer, myTurn);
			}
		}
	}
	public void OnChangeQueue (bool myTurn)
	{
		if (ServerController.serverController.isServerClientArchitecture && ServerController.serverController.isMyQueue && !MenuControllerGenerator.controller.playWithAI && !MenuControllerGenerator.controller.hotseat)
		{
			if (ServerController.serverController.myNetworkPlayer != 0)
			{
				StartCoroutine(WaitAndSendOnClientStartPlay(myTurn));
			}
		}
	}
	public void OnChanghAllIsSleeping ()
	{
		if(!cueController)
			return;
		cueController.networkAllIsSleeping = true;
	}
	public void OnWantToPlayAgain ()
	{
		StartCoroutine(WaitForOtherWantToPlayAgain());
	}
	IEnumerator WaitForOtherWantToPlayAgain ()
	{
		while(!cueController)
		{
			yield return null;
		}
		yield return new WaitForEndOfFrame(); 
		cueController.otherWantToPlayAgain = true;
		cueController.gameManager.otherProfile.WantToPlayAgain.gameObject.SetActive(true);
	}
	public void SetPrizeToOther (int otherPrize)
	{
		StartCoroutine(WaitAndSetPrizeToOther (otherPrize));
	}
	IEnumerator WaitAndSetPrizeToOther (int  otherPrize)
	{
		while(!cueController)
		{
			yield return null;
		}
		yield return new WaitForEndOfFrame();
		GameManager gameManager = GameManager.FindObjectOfType<GameManager>();
		gameManager.SetPrizeToOther(otherPrize);
	}
	public void SetHighScoreToOther (int otherHighScore)
	{
		StartCoroutine(WaitAndSetHighScoreToOther (otherHighScore));
	}
	IEnumerator WaitAndSetHighScoreToOther (int otherHighScore)
	{
		while(!cueController)
		{
			yield return null;
		}
		yield return new WaitForEndOfFrame();
		GameManager gameManager = GameManager.FindObjectOfType<GameManager>();
		gameManager.SetHighScoreToOther(otherHighScore);
	}
	public void SetCoinsToOther(int otherCoins)
	{
		StartCoroutine(WaitAndSetCoinsToOther (otherCoins));
	}
	IEnumerator WaitAndSetCoinsToOther (int otherCoins)
	{
		while(!cueController)
		{
			yield return null;
		}
		yield return new WaitForEndOfFrame();
		yield return new WaitForFixedUpdate();
		GameManager gameManager = GameManager.FindObjectOfType<GameManager>();
		gameManager.SetCoinsToOther(otherCoins);
	}
	public void SetErrorText (string errorText)
	{
		if(!cueController)
			return;
		cueController.gameManager.ShowGameInfoError(errorText, 5.0f);
	}
	public void OnReadyToPlay ()
	{
		Debuger.DebugOnScreen("OnReadyToPlay");
		StartCoroutine("WaitAndEnabledCueController");
	}
	IEnumerator WaitAndEnabledCueController ()
	{
		Debuger.DebugOnScreen("StartEnabledCueController");
		Debuger.DebugOnScreen((cueController != null).ToString());
		Debuger.DebugOnScreen((ServerController.serverController.menuButtonsIsActive).ToString());

		Debuger.DebugOnScreen((ServerController.serverController.otherNetworkPlayer < 1).ToString());
		Debuger.DebugOnScreen((!MenuControllerGenerator.controller.playWithAI).ToString());

        while (!cueController)
        {
            cueController = CueController.FindObjectOfType<CueController>();
            yield return null;
        }
		while(ServerController.serverController.menuButtonsIsActive || (ServerController.serverController.otherNetworkPlayer < 1 && !MenuControllerGenerator.controller.playWithAI && !MenuControllerGenerator.controller.hotseat))
		{
			yield return null;
		}
		Debuger.DebugOnScreen("WaitForEndOfFrame");
        yield return new WaitForEndOfFrame();
        cueController.enabled = true;
		cueController.cueFSMController.enabled = true;
		string info = ServerController.serverController.isMyQueue? "You are breaking\n Good luck!":"Your opponent is breaking\n Good luck!";
		cueController.gameManager.ShowGameInfo(info, 2.5f);
		Debuger.DebugOnScreen("EndEnabledCueController");
	}
	public void OnSelectBall (Vector3 position)
	{
		if(!cueController)
			return;
		cueController.OnSelectBall(position);
	}
	public void OnUnselectBall ()
	{
		if(!cueController)
			return;
		cueController.OnUnselectBall();
	}
	public void SetOnMoveBall(Vector3 positin )
	{
		if(!cueController)
			return;
		cueController.ballMovePosition = positin;
	}
	public void ForceSetBallMove (int id, Vector3 positin )
	{
		if(!cueController)
			return;
		BallController ballController = cueController.startBallControllers[id];
		if(ServerController.serverController.isMyQueue || ballController.ballIsOut)
			return;
		ballController.ForceSetMove(positin);
	}
	public void OnPlayBallAudio (int id, float audioVolume)
	{
		if(!cueController)
			return;
		BallController ballController = cueController.startBallControllers[id];
		if(ServerController.serverController.isMyQueue || ballController.ballIsOut)
			return;
		ballController.OnPlayBallAudio(audioVolume);
	}

    public void SendShotCurrentTime (float shotCurrentTime)
    {
		if (cueController)
		{
			cueController.gameManager.otherShotCurrentTime = shotCurrentTime;
		}
    }

	public void SetBallSleeping(int id, Vector3 positin)
	{
		if(!cueController)
			return;
		BallController ballController = cueController.startBallControllers[id];
		if(ServerController.serverController.isMyQueue || ballController.ballIsOut)
			return;

        ballController.transform.position = positin;
	
		
	}
	
	private BallController FindBallById(int id)
	{
		return null;
	}
    public void SendCueControl( Quaternion localRotation, Vector3 localPosition, Vector3 rotationDisplacement, float verticalRotation)
	{
		if(!cueController)
			return;
        cueController.SetCueControlFromNetwork(localRotation, localPosition, new Vector2(rotationDisplacement.x, rotationDisplacement.y), verticalRotation);
	}
	public void OnShotCue()
	{
		if(!cueController)
			return;
		cueController.OnShotCue();
	}
    public void SetBodyesPositions(string ballsPositions)
    {
        string data = "";
        int balId = -1;
        float x = 0.0f;
        float y = 0.0f;
        float z = 0.0f;
        int valueNumber = 0;
        bool haveBall = false;

        foreach (char item in ballsPositions)
        {
            if (haveBall)
            {
                haveBall = false;
                BallController ball = cueController.startBallControllers[balId];
                Vector3 position = new Vector3(x, y, z);
                ball.transform.position = position;
                ball.bodyInitializer.DeActivate(position);
            }
            if (item == '(')
            {
                balId = int.Parse(data);
                data = "";
                valueNumber = 1;
                continue;
            }
            if (item == ',')
            {
                if (valueNumber == 1)
                {
                    x = float.Parse(data);
                    data = "";
                    valueNumber = 2;
                    continue;
                }
                else if (valueNumber == 2)
                {
                    y = float.Parse(data);
                    data = "";
                    valueNumber = 3;
                    continue;
                }
            }
            if (item == ')')
            {
                z = float.Parse(data);
                data = "";
                haveBall = true;
                continue;
            }
            data += item;
        }
    }

    public void ForceSetNetworkParameters(int id, float networkSpeed, Vector3 networkPosition, Vector3 networkAngularVelocity)
    {
        cueController.startBallControllers[id].ForceSetNetworkParameters(networkSpeed, networkPosition, networkAngularVelocity);
    }

    public void SetNetworkParameters(int id, float networkSpeed, Vector3 networkPosition, Vector3 networkAngularVelocity)
    {
        if (cueController)
        {
            cueController.startBallControllers[id].SetNetworkParameters(networkSpeed, networkPosition, networkAngularVelocity);
        }
    }
	//Player is shot (for ball)
    public void ShotBall(Vector3 ballShotVelocity, Vector3 hitBallVelocity, Vector3 secondVelocity, Vector3 ballShotAngularVelocity, string ballsPositions)
	{
		if(!cueController)
			return;
		cueController.ballShotVelocity = ballShotVelocity;
		cueController.hitBallVelocity = hitBallVelocity;
		cueController.secondVelocity = secondVelocity;
		cueController.ballShotAngularVelocity = ballShotAngularVelocity;

        SetBodyesPositions(ballsPositions);

		cueController.ballController.ShotBall();
	}
	public void SendOnTriggerEnter (int ballId, float audioVolume, float currentLungth, int holleId)
	{
		HolleController holleController = HolleController.FindeHoleById(holleId);
		holleController.SendOnTriggerEnter(ballId, audioVolume, currentLungth, holleId);
	}
}
