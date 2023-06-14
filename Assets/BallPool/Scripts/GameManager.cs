using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour 
{
	public CueController cueController;
	[SerializeField]
	private TextMesh gameInfo;
	[SerializeField]
	private TextMesh gameInfoError;
	[System.NonSerialized]
	public string gameInfoErrorText = "";
	[SerializeField]
	private Sprite firstAvatar;
	[SerializeField]
	private Sprite secondAvatar;
	[System.NonSerialized]
	public bool isFirstShot = true;
	[System.NonSerialized]
	public int firstShotHitCount = 0;
	public Profile profileLeft;
	public Profile profileRight;
	[System.NonSerialized]
	public Profile myProfile;
	[System.NonSerialized]
	public Profile otherProfile;
	[System.NonSerialized]
	public bool needToChangeQueue = true;
	[System.NonSerialized]
	public bool needToForceChangeQueue = false;
	[System.NonSerialized]
	public bool setMoveInTable = true;
	[System.NonSerialized]
	public bool tableIsOpened = true;
	[System.NonSerialized]
	public int ballType = 0;
	[SerializeField]
	private Menu menu;
	[System.NonSerialized]
	public List<BallController> firsBalls;
	[System.NonSerialized]
	public bool remainedBlackBall = false;
	[System.NonSerialized]
	public bool otherRemainedBlackBall = false;

	[System.NonSerialized]
	public bool afterRemainedBlackBall = false;
	[System.NonSerialized]
	public bool afterOtherRemainedBlackBall = false;

	[System.NonSerialized]
	public bool mainBallIsOut = false;
	[System.NonSerialized]
	public bool otherMainBallIsOut = false;

	[System.NonSerialized]
	public bool blackBallInHolle = false;
	[System.NonSerialized]
	public bool otherBlackBallInHolle = false;
	[System.NonSerialized]
	public BallController firstHitBall = null;
	[System.NonSerialized]
	public bool isWinner = false;
	[System.NonSerialized]
	public bool outOfTime = false;
	[SerializeField]
	private TextMesh prize;
	private float shotTime = 30.0f;
	[System.NonSerialized]
	public float shotCurrentTime = 0.0f;
    [System.NonSerialized]
    public float otherShotCurrentTime = 0.0f;
	[System.NonSerialized]
	public bool calculateShotTime = false;
	[System.NonSerialized]
	public bool ballsInMove = false;

	public int maxHighScore = 10;
	public AnimationCurve highScoreCurve;
	[System.NonSerialized]
	public bool menuButtonsIsActive;

	void Awake ()
	{
		if(!MenuControllerGenerator.controller)
			return;
		shotTime = MenuControllerGenerator.controller.shotTime;
		if(!ServerController.serverController && !MenuControllerGenerator.controller.playWithAI && !MenuControllerGenerator.controller.hotseat)
		{
			Destroy(menu.gameObject);
			Destroy(gameObject);
			return;
		}
		firsBalls = new List<BallController>(0);
		ActivateMenuButtons(false);
		myProfile = profileLeft;
		myProfile.isMain = true;
		otherProfile = profileRight;
		otherProfile.isMain = false;
		myProfile.gameManager = this;
		otherProfile.gameManager = this;
		myProfile.OnAwakeGameManager();
		otherProfile.OnAwakeGameManager();

		myProfile.playerName.text = ServerController.serverController.myName;
		otherProfile.playerName.text = ServerController.serverController.otherName;

		if(ServerController.serverController.isFirstPlayer || MenuControllerGenerator.controller.playWithAI || MenuControllerGenerator.controller.hotseat)
		{
		    prize.text = ServerController.serverController.prize.ToString();
			if(!MenuControllerGenerator.controller.playWithAI && !MenuControllerGenerator.controller.hotseat && !MenuControllerGenerator.controller.hotseat)
			{
				//ServerController.serverController.SendRPCToServer("SetPrizeToOther", ServerController.serverController.otherNetworkPlayer, ServerController.serverController.prize);
			}
		}
		myProfile.coins.text = ServerController.serverController.coins.ToString();
		if(!MenuControllerGenerator.controller.playWithAI && !MenuControllerGenerator.controller.hotseat)
		{
			//Debug.Log("avatarURL " + MenuControllerGenerator.controller.avatarURL);
			//ServerController.serverController.SendRPCToServer("SetCoinsToOther", ServerController.serverController.otherNetworkPlayer, ServerController.serverController.coins);
			//ServerController.serverController.SendRPCToServer("SetAvatarToOther", ServerController.serverController.otherNetworkPlayer, MenuControllerGenerator.controller.avatarURL);
		} else
		{
			SetCoinsToOther(ServerController.serverController.otherCoins);
		}

		int highScore = Profile.GetUserDate(ServerController.serverController.myName + "_High_Score");
		ServerController.serverController.highScore = highScore;
		myProfile.highScore.text = ServerController.serverController.highScore.ToString();
		if(!MenuControllerGenerator.controller.playWithAI && !MenuControllerGenerator.controller.hotseat)
		{
			//ServerController.serverController.SendRPCToServer("SetHighScoreToOther", ServerController.serverController.otherNetworkPlayer, ServerController.serverController.highScore);
		}
		HideGameInfoError();

		StartCalculateShotTime();
		myProfile.timeSlider.SetValue(0.0f);
		otherProfile.timeSlider.SetValue(0.0f);

		ServerController.serverController.OnChangeQueueEvent += OnChangeCalculateShotTime;

		//if (MenuControllerGenerator.controller.avatarTexture)
		//{
		//	StartCoroutine(myProfile.SetAvatar(MenuControllerGenerator.controller.avatarTexture));
		//}
		//if (MenuControllerGenerator.controller.otherAvatarTexture)
		//{
		//	StartCoroutine(otherProfile.SetAvatar(MenuControllerGenerator.controller.otherAvatarTexture));
		//}

		//if(ServerController.serverController.isFirstPlayer)
		//{
		//	if (!MenuControllerGenerator.controller.avatarTexture)
		//	{
		//		StartCoroutine(myProfile.SetAvatar(secondAvatar));
		//	}
		//	if (!MenuControllerGenerator.controller.otherAvatarTexture)
		//	{
		//		StartCoroutine(otherProfile.SetAvatar(firstAvatar));
		//	}
		//}
		//else
		//{
		//	if (!MenuControllerGenerator.controller.otherAvatarTexture)
		//	{
		//		StartCoroutine(otherProfile.SetAvatar(secondAvatar));
		//	}
		//	if (!MenuControllerGenerator.controller.avatarTexture)
		//	{
		//		StartCoroutine(myProfile.SetAvatar(firstAvatar));
		//	}
		//}
	}
	void FixedUpdate ()
	{
		if(calculateShotTime)
		{
			if(cueController.enabled)
			{
				if (ServerController.serverController.isServerClientArchitecture && !MenuControllerGenerator.controller.playWithAI && !MenuControllerGenerator.controller.hotseat)
				{
					if (otherShotCurrentTime > 0.0f)
					{ 
						shotCurrentTime = otherShotCurrentTime;
					}
				}
				else
				{
					if (ServerController.serverController.isMyQueue || MenuControllerGenerator.controller.playWithAI || MenuControllerGenerator.controller.hotseat)
					{
						shotCurrentTime -= Time.fixedDeltaTime;
						if (!MenuControllerGenerator.controller.playWithAI && !MenuControllerGenerator.controller.hotseat)
						{
							//ServerController.serverController.SendRPCToServer("SendShotCurrentTime", ServerController.serverController.otherNetworkPlayer, shotCurrentTime);
						}
					}
					else if (!MenuControllerGenerator.controller.playWithAI && !MenuControllerGenerator.controller.hotseat && otherShotCurrentTime > 0.0f)
					{ 
						shotCurrentTime = otherShotCurrentTime;
					}
				}
			}
           
			if(shotCurrentTime <= 0.0f)
			{
				StopCalculateShotTime ();
				gameInfoErrorText = "ran out of time";

				if(ServerController.serverController.isMyQueue)
				{
					string myGameInfoErrorText = "You " +  gameInfoErrorText + "\n" + ServerController.serverController.otherName + " has ball in hand";
					string otherGameInfoErrorText = ServerController.serverController.myName + "  " + gameInfoErrorText + "\nYou have ball in hand";

					ShowGameInfoError(myGameInfoErrorText, 5.0f);
					//ServerController.serverController.SendRPCToServer("SetErrorText", ServerController.serverController.otherNetworkPlayer, otherGameInfoErrorText);
					//ServerController.serverController.SendRPCToServer("SetMoveInTable", ServerController.serverController.otherNetworkPlayer);

				    ServerController.serverController.ChangeQueue(false);
				} else if(MenuControllerGenerator.controller.playWithAI || MenuControllerGenerator.controller.hotseat)
				{
					string otherGameInfoErrorText = ServerController.serverController.otherName + " " +  gameInfoErrorText + "\nYou have ball in hand";
					ShowGameInfoError(otherGameInfoErrorText, 5.0f);
					cueController.cueFSMController.setMoveInTable();
					ServerController.serverController.ChangeQueue(true);
				}
			}
			if(ServerController.serverController.isMyQueue)
			{
				myProfile.timeSlider.SetValue(shotCurrentTime/shotTime);
				otherProfile.timeSlider.SetValue(0.0f);
			}
			else
			{
				myProfile.timeSlider.SetValue(0.0f);
				otherProfile.timeSlider.SetValue(shotCurrentTime/shotTime);
			}
		}
	}
	void OnDestroy ()
	{
		if(ServerController.serverController)
		{
			ServerController.serverController.OnChangeQueueEvent -= OnChangeCalculateShotTime;
		}
	}
	void OnChangeCalculateShotTime (bool myTurn)
	{
		cueController.RessetShotOptions();
		StartCalculateShotTime();
	}
	public void StartCalculateShotTime ()
	{
		shotCurrentTime = shotTime;
		calculateShotTime = true;
	}
	public void StopCalculateShotTime ()
	{
		calculateShotTime = false;
		shotCurrentTime = 0.0f;
	}
	public void SetPrizeToOther(int otherPrize)
	{
		ServerController.serverController.prize = otherPrize;
		prize.text = otherPrize.ToString();
	}
	public void SetHighScoreToOther(int otherHighScore)
	{
		ServerController.serverController.otherHighScore = otherHighScore;
		otherProfile.highScore.text = otherHighScore.ToString();
	}
	public void SetCoinsToOther(int otherCoins)
	{
		ServerController.serverController.otherCoins = otherCoins;
		otherProfile.coins.text = otherCoins.ToString();
	}
	public void ActivateMenuButtons (bool value)
	{
		menuButtonsIsActive = value;
		if(!value)
		{
		    menu.gameObject.SetActive(value);
			if (ServerController.serverController.isServerClientArchitecture && ServerController.serverController.isMyQueue && !MenuControllerGenerator.controller.playWithAI && !MenuControllerGenerator.controller.hotseat)
			{
				if (ServerController.serverController.myNetworkPlayer != 0)
				{
					StartCoroutine(ServerController.serverController.serverMessenger.WaitAndSendOnClientStartPlay(ServerController.serverController.isMyQueue));
				}
			}
		}
		ServerController.serverController.menuButtonsIsActive = value;
		if(value)
		{
			cueController.enabled = false;
			cueController.cueFSMController.enabled = false;

			if(!afterRemainedBlackBall && !afterOtherRemainedBlackBall)
			{
				isWinner = !ServerController.serverController.isMyQueue;
			}
			else
			{
				if(ServerController.serverController.isMyQueue)
				{
					isWinner = (afterRemainedBlackBall && !mainBallIsOut);

					if(MenuControllerGenerator.controller.playWithAI && MenuControllerGenerator.controller.hotseat)
					{
						if(!mainBallIsOut && firstHitBall && !firstHitBall.isBlack && afterRemainedBlackBall && ballType != firstHitBall.ballType)
						{
							isWinner = false;
						}
					}
				}
				else
				{
					isWinner = !(afterOtherRemainedBlackBall && !otherMainBallIsOut);

					if(MenuControllerGenerator.controller.playWithAI && MenuControllerGenerator.controller.hotseat)
					{
						if(!otherMainBallIsOut && firstHitBall && !firstHitBall.isBlack && afterOtherRemainedBlackBall && ballType == firstHitBall.ballType)
						{
							isWinner = true;
						}
					}
				}
			}
			ServerController.serverController.isMyQueue = isWinner;

			if (isWinner && ServerController.serverController.isServerClientArchitecture && !MenuControllerGenerator.controller.playWithAI && !MenuControllerGenerator.controller.hotseat)
			{
				if (ServerController.serverController.myNetworkPlayer != 0)
				{
					ServerController.serverController.SendRPCToServer("OnClientWon", ServerController.serverController.myNetworkPlayer);
				}
			}



			ServerController.serverController.coins += isWinner?ServerController.serverController.prize:-ServerController.serverController.prize;
			ServerController.serverController.coins = Mathf.Clamp(ServerController.serverController.coins, ServerController.serverController.minCoins, ServerController.serverController.maxCoins);
			Profile.SetUserDate(ServerController.serverController.myName + "_Coins", ServerController.serverController.coins);
			myProfile.coins.text = ServerController.serverController.coins.ToString();

			ServerController.serverController.SendRPCToServer("SetCoinsToPlayerClient", ServerController.serverController.myNetworkPlayer, ServerController.serverController.coins);

			ServerController.serverController.otherCoins += isWinner?-ServerController.serverController.prize:ServerController.serverController.prize;
			ServerController.serverController.otherCoins = Mathf.Clamp(ServerController.serverController.otherCoins, ServerController.serverController.minCoins, ServerController.serverController.maxCoins);
			if(MenuControllerGenerator.controller.playWithAI || MenuControllerGenerator.controller.hotseat)
			{
				Profile.SetUserDate(ServerController.serverController.otherName + "_Coins", ServerController.serverController.otherCoins);
			}
			otherProfile.coins.text = ServerController.serverController.otherCoins.ToString();

			int highScore = (int)((float)maxHighScore*(highScoreCurve.Evaluate((float)ServerController.serverController.coins/(float)ServerController.serverController.maxCoins)));
			if(ServerController.serverController.highScore < highScore)
			{
				ServerController.serverController.highScore = highScore;
			}
			int otherHighScore = (int)((float)maxHighScore*(highScoreCurve.Evaluate((float)ServerController.serverController.otherCoins/(float)ServerController.serverController.maxCoins)));

			if(ServerController.serverController.otherHighScore < otherHighScore)
			{
				ServerController.serverController.otherHighScore = otherHighScore;
			}
			Profile.SetUserDate(ServerController.serverController.myName + "_High_Score", ServerController.serverController.highScore);
			if(MenuControllerGenerator.controller.playWithAI || MenuControllerGenerator.controller.hotseat)
			{
				Profile.SetUserDate(ServerController.serverController.otherName + "_High_Score", ServerController.serverController.otherHighScore);
			}
			myProfile.highScore.text = ServerController.serverController.highScore.ToString();
			otherProfile.highScore.text = ServerController.serverController.otherHighScore.ToString();

			if(isWinner)
			{
				myProfile.winner.GetComponent<Renderer>().enabled = true;
			}
			else
			{
				otherProfile.winner.GetComponent<Renderer>().enabled = true;
			}
			if( ServerController.serverController.coins < ServerController.serverController.prize )
			{
				if(!MenuControllerGenerator.controller.playWithAI && !MenuControllerGenerator.controller.hotseat)
				{
				    StartCoroutine(WaitAndDisconnect());
				} 
				else
				{
					MenuControllerGenerator.controller.playWithAI = false;
					MenuControllerGenerator.controller.hotseat = false;
					MenuControllerGenerator.controller.OnGoBack();
				}
			}
			else
			{
				menu.gameObject.SetActive(value);
			}
			if(MenuControllerGenerator.controller.playWithAI || MenuControllerGenerator.controller.hotseat)
			{
				cueController.otherWantToPlayAgain = true;
				otherProfile.WantToPlayAgain.gameObject.SetActive(true);
			}
		}
	}
	IEnumerator WaitAndDisconnect()
	{
		yield return new WaitForSeconds(1.5f);
		MasterServerGUI.Disconnect();
	}
	public void ShowGameInfo (string info)
	{
		gameInfo.GetComponent<Renderer>().enabled = true;
		gameInfo.text = info;
	}
	public void ShowGameInfo (string info, float visibleTime)
	{
		gameInfo.GetComponent<Renderer>().enabled = true;
		gameInfo.text = info;
		Invoke("HideGameInfo", visibleTime);
	}
	public void ShowGameInfoError (string info, float visibleTime)
	{
		if(MenuControllerGenerator.controller.playWithAI || MenuControllerGenerator.controller.hotseat)
		{
		    cueController.cueFSMController.setMoveInTable();
		}
		gameInfoError.GetComponent<Renderer>().enabled = true;
		gameInfoError.text = info;
		Invoke("HideGameInfoError", visibleTime);
	}
	public void HideGameInfoError ()
	{
		gameInfoError.GetComponent<Renderer>().enabled = false;
	}
	public void HideGameInfo ()
	{
		gameInfo.GetComponent<Renderer>().enabled = false;
	}
	public void SetBallType(int ballType)
	{
		this.ballType = ballType;
		if(ballType == 1)
		{
			for (int i = 1; i <= 7; i++)
			{
				myProfile.AddGuiBall(i, cueController.ballTextures[i]);
		    }
			for (int i = 9; i <= 15; i++)
			{
				otherProfile.AddGuiBall(i, cueController.ballTextures[i]);
			}
		}
		else if(ballType == -1)
		{
			for (int i = 1; i <= 7; i++)
			{
				otherProfile.AddGuiBall(i, cueController.ballTextures[i]);
			}
			for (int i = 9; i <= 15; i++)
			{
				myProfile.AddGuiBall(i, cueController.ballTextures[i]);
			}
		}
	}
	void GetMenu ()
	{
		if(MenuControllerGenerator.controller.playWithAI || MenuControllerGenerator.controller.hotseat)
		{
			MenuControllerGenerator.controller.playWithAI = false;
			MenuControllerGenerator.controller.hotseat = false;
			MenuControllerGenerator.controller.OnGoBack();
		}else
		{
			MasterServerGUI.Disconnect();
		}

	}
	void PlayAgain ()
	{
		myProfile.WantToPlayAgain.gameObject.SetActive(true);
		ServerController.serverController.SendRPCToServer("WantToPlayAgain", ServerController.serverController.otherNetworkPlayer);
		ActivateMenuButtons(false);
		ShowGameInfo("Waiting for your opponent");
		StartCoroutine(WaitWhenOtherWantToPlayAgain ());
	}
	IEnumerator WaitWhenOtherWantToPlayAgain ()
	{
		while(!cueController.otherWantToPlayAgain)
		{
			yield return null;
		}
		yield return new WaitForSeconds(1.5f);
        MenuControllerGenerator.controller.LoadLevel(SceneManager.GetActiveScene().buildIndex);
	}
}
