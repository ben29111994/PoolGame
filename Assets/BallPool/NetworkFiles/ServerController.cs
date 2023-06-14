using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_NETWORK
using SomeRPC = UnityEngine.RPC;



#elif PHOTON_PUN
using SomeRPC = PunRPC;





#elif PHOTON_NETWORK
using SomeRPC = //PHOTON RPC





#else
public class SomeRPC : System.Attribute
{

}
#endif



public class ServerController : MonoBehaviour
{
	[System.NonSerialized]
	public bool _isMyQueue = true;


	public bool isMyQueue
	{
		get{ return _isMyQueue; }
		set{ _isMyQueue = value; }
	}

	public int myNetworkPlayer;
	public int otherNetworkPlayer;

	public class Player
	{
		public string name;
		public int coins;
		public int prize;
		public int networkPlayer;
		public Player otherPlayer;
		public float time;
		public bool myTurn;
		public bool isWinner;
		public string avatarURL;

		public Player(string name, int coins, int prize, int networkPlayer, Player otherPlayer, string avatarURL)
		{
			this.name = name;
			this.coins = coins;
			this.prize = prize;
			this.networkPlayer = networkPlayer;
			this.otherPlayer = otherPlayer;
			this.time = MenuControllerGenerator.controller.shotTime;
			this.myTurn = false;
			this.isWinner = false;
			this.avatarURL = avatarURL;
		}
	}

	public IDictionary<int, Player> players;

	public delegate void ChangeQueueHandler(bool myTurn);

	public event ChangeQueueHandler OnChangeQueueEvent;

	public ServerMessenger serverMessenger;
	[System.NonSerialized]
	public MasterServerGUI
		masterServerGUI;
	public static ServerController serverController;
	public static LogicAI logicAI;
	[System.NonSerialized]
	public bool
		isFirstPlayer = false;
   
	[System.NonSerialized]
	public string
		myName = "Player Name";
	[System.NonSerialized]
	public string
		otherName = "Player Name";
	[System.NonSerialized]
	public int
		prize = 20;
	public int coins = 500;
	public int otherCoins = 500;
	public int highScore = 0;
	public int otherHighScore = 0;
	public int minCoins = 10;
	public int maxCoins = 1000000;
	[System.NonSerialized]
	public bool
		menuButtonsIsActive = false;
	public bool isServerClientArchitecture;

	void Awake()
	{
		masterServerGUI = GetComponent<MasterServerGUI>();
	}

	public void ChangeQueueEvent(bool myTurn)
	{
		if (serverController.OnChangeQueueEvent != null)
		{
			serverController.OnChangeQueueEvent(myTurn);
		}
	}
	//Change the queue for shot
	public void ChangeQueue(bool myTurn)
	{
		_isMyQueue = myTurn;
		if (OnChangeQueueEvent != null)
		{
			OnChangeQueueEvent(myTurn);
		}
		if (!MenuControllerGenerator.controller.playWithAI && !MenuControllerGenerator.controller.hotseat)
		{
			SendRPCToServer("OnChangeQueue", otherNetworkPlayer, !myTurn);
		}
		else if (MenuControllerGenerator.controller.playWithAI && !isMyQueue)
		{
			serverMessenger.ShotWithAI();
		} 

	}
	//Send message thru the network
	public void SendRPCToServer(string message, params object[] args)
	{
		masterServerGUI.SendRPCToServer(message, args);
	}
	//Send message thru the network for some network  player
	public void SendRPCToNetworkPlayer(string message, int player, params object[] args)
	{
		masterServerGUI.SendRPCToNetworkPlayer(message, player, args);
	}

	public ServerController.Player FindPlayer(int networkPlayer)
	{
		ServerController.Player player;
		if (serverController.players.TryGetValue(networkPlayer, out player))
		{
			return player;
		}
		return null;
	}

	//Client start play and server will be calculate his shot time;
	[SomeRPC]
	void OnClientStartPlay(int networkPlayer, bool myTurn)
	{
		Debug.Log("OnClientStartPlay " + networkPlayer);
		Player plyer;
		if (ServerController.serverController.players.TryGetValue(networkPlayer, out plyer))
		{
			plyer.myTurn = myTurn;
			plyer.otherPlayer.myTurn = !myTurn;
			plyer.isWinner = false;
			plyer.otherPlayer.isWinner = false;
			if (myTurn)
			{
				plyer.time = MenuControllerGenerator.controller.shotTime;
				plyer.otherPlayer.time = 0.0f;
			}
			else
			{
				plyer.time = 0.0f;
				plyer.otherPlayer.time = MenuControllerGenerator.controller.shotTime;
			}
			ServerController.serverController.SendRPCToNetworkPlayer("OnSendCoinsToOtherFromServerClient", plyer.networkPlayer, plyer.otherPlayer.coins);
			ServerController.serverController.SendRPCToNetworkPlayer("OnSendCoinsToOtherFromServerClient", plyer.otherPlayer.networkPlayer, plyer.coins);

		}
	}
	//Client have shot, and server will be stop calculate shot time;
	[SomeRPC]
	void OnClientShot(int networkPlayer)
	{
		Debug.Log("OnClientShot " + networkPlayer);
		Player plyer;
		if (ServerController.serverController.players.TryGetValue(networkPlayer, out plyer))
		{
			plyer.myTurn = false;
			plyer.otherPlayer.myTurn = false;

			plyer.isWinner = false;
			plyer.otherPlayer.isWinner = false;
			
			plyer.time = 0.0f;
			plyer.otherPlayer.time = 0.0f;
		}
	}

	//Client have shot, and server will be stop calculate shot time;
	[SomeRPC]
	void OnClientWon(int networkPlayer)
	{
		Debug.Log("OnClientWon " + networkPlayer);
		Player plyer;
		if (ServerController.serverController.players.TryGetValue(networkPlayer, out plyer))
		{
			plyer.myTurn = false;
			plyer.otherPlayer.myTurn = false;
			plyer.isWinner = true;
			plyer.otherPlayer.isWinner = false;

			plyer.time = 0.0f;
			plyer.otherPlayer.time = 0.0f;
		}
	}
	//Send coins from server
	[SomeRPC]
	void OnSendCoinsToOtherFromServerClient(int otherCoinsFromServer)
	{
		serverMessenger.SetCoinsToOther(otherCoinsFromServer);
	}

	//Server set coins ti player
	[SomeRPC]
	public void SetCoinsToPlayerClient(int networkPlayer, int coins)
	{
		masterServerGUI.SetCoinsToPlayer(networkPlayer, coins);
	}
	//Server checks players

	[SomeRPC]
	void CheckPlayers(string name, int coins, int networkPlayer, string avatarURL)
	{
		Debug.Log("CheckPlayers: name " + name + " coins  " + coins + "  networkPlayer " + networkPlayer);
		if (serverController.prize > coins)
		{
			serverController.SendRPCToNetworkPlayer("DeletePlayerClient", networkPlayer);
			return;
		}
		foreach (KeyValuePair<int, ServerController.Player> playerDictionary in ServerController.serverController.players)
		{
			ServerController.Player player = playerDictionary.Value;
			if (player.otherPlayer == null)
			{
				player.otherPlayer = new ServerController.Player(name, coins, serverController.prize, networkPlayer, player, avatarURL);
				serverController.players.Add(player.otherPlayer.networkPlayer, player.otherPlayer);
				serverController.isMyQueue = Random.Range(0, 2) == 0;
				serverController.SendRPCToNetworkPlayer("SetPlayerParameters", player.networkPlayer, true, serverController.isMyQueue, player.otherPlayer.networkPlayer, serverController.prize,
					player.coins, player.otherPlayer.coins, player.name, player.otherPlayer.name, player.avatarURL, player.otherPlayer.avatarURL);
				serverController.SendRPCToNetworkPlayer("SetPlayerParameters", player.otherPlayer.networkPlayer, false, serverController.isMyQueue, player.networkPlayer, serverController.prize, 
					player.otherPlayer.coins, player.coins, player.otherPlayer.name, player.name, player.otherPlayer.avatarURL, player.avatarURL);
				

				//serverController.SendRPCToNetworkPlayer("SendFirstPlayerNameClient", player.networkPlayer, player.otherPlayer.name);
				//serverController.SendRPCToNetworkPlayer("SendFirstPlayerNameClient", player.otherPlayer.networkPlayer, player.name);
                				
				serverController.SendRPCToNetworkPlayer("StartGame", player.networkPlayer);
				serverController.SendRPCToNetworkPlayer("StartGame", player.otherPlayer.networkPlayer);
				player.myTurn = true;
				player.otherPlayer.myTurn = false;
				player.isWinner = false;
				player.otherPlayer.isWinner = false;
				return;
			}
		}
		ServerController.Player firstPlayer = new ServerController.Player(name, coins, serverController.prize, networkPlayer, null, avatarURL);
		serverController.players.Add(firstPlayer.networkPlayer, firstPlayer);
	}

	[SomeRPC]
	void SetMyNetworkPlayerClient(int player)
	{
		ServerController.serverController.myNetworkPlayer = player;

		ServerController.serverController.myName = masterServerGUI.gameName;
		int coins = Profile.GetUserDate(ServerController.serverController.myName + "_Coins");
		if (coins > 0)
		{
			ServerController.serverController.coins = coins;
		}
		PlayerPrefs.SetString("BallPoolMultyplayerServerTemplateDemoPlayerName", masterServerGUI.gameName);
		//ServerController.serverController.SendRPCToServer("CheckPlayers", ServerController.serverController.myName, ServerController.serverController.coins, ServerController.serverController.myNetworkPlayer, MenuControllerGenerator.controller.avatarURL);

	}
	//Send to Client
	[SomeRPC]
	void SetPlayerParameters(bool isFirstPlayer, bool isMyQueue, int player, int serverPrize, int myCoins, int otherCoins, string myName, string otherName, string avatarURL, string otherAvatarURL)
	{
		serverController.otherNetworkPlayer = player;
		serverController.prize = serverPrize;

		if (isFirstPlayer)
		{
			serverController.isMyQueue = isMyQueue;
		}
		else
		{
			serverController.isMyQueue = !isMyQueue;
		}
		
		serverController.isFirstPlayer = isFirstPlayer;
		serverController.myName = myName;
		serverController.otherName = otherName;
		serverController.coins = myCoins;
		serverController.otherCoins = otherCoins;
		//MenuControllerGenerator.controller.avatarURL = avatarURL;
		//MenuControllerGenerator.controller.otherAvatarURL = otherAvatarURL;
		//StartCoroutine(DownloadMyAvatar());
		//StartCoroutine(DownloadOtherAvatar());
	}

	//IEnumerator DownloadMyAvatar()
	//{
	//	WWW www = new WWW(MenuControllerGenerator.controller.avatarURL);
	//	yield return www;
	//	if (!string.IsNullOrEmpty(www.error))
	//	{
	//		Debug.Log(www.error);
	//	}
	//	else
	//	{
	//		MenuControllerGenerator.controller.avatarTexture = www.texture;
	//		PlayerPrefs.SetString("PoolGameAvatarURL", MenuControllerGenerator.controller.avatarURL);
	//		CueController cueController = CueController.FindObjectOfType<CueController>();
	//		while (!cueController)
	//		{
	//			cueController = CueController.FindObjectOfType<CueController>();
	//			yield return null;
	//		}
	//		yield return new WaitForFixedUpdate();
	//		yield return new WaitForEndOfFrame();
	//		StartCoroutine(cueController.gameManager.myProfile.SetAvatar(MenuControllerGenerator.controller.avatarTexture));
	//	}
	//}

	//IEnumerator DownloadOtherAvatar()
	//{
	//	WWW www = new WWW(MenuControllerGenerator.controller.otherAvatarURL);
	//	yield return www;
	//	if (!string.IsNullOrEmpty(www.error))
	//	{
	//		Debug.Log(www.error);
	//	}
	//	else
	//	{
	//		MenuControllerGenerator.controller.otherAvatarTexture = www.texture;
	//		CueController cueController = CueController.FindObjectOfType<CueController>();
	//		while (!cueController)
	//		{
	//			cueController = CueController.FindObjectOfType<CueController>();
	//			yield return null;
	//		}
	//		yield return new WaitForFixedUpdate();
	//		yield return new WaitForEndOfFrame();
	//		Debug.Log("otherAvatarURL " + MenuControllerGenerator.controller.otherAvatarURL);
	//		StartCoroutine(cueController.gameManager.otherProfile.SetAvatar(MenuControllerGenerator.controller.otherAvatarTexture));
	//	}
	//}

	public void DeletePlayer(int networkPlayer)
	{
		ServerController.Player player = null;
		foreach (KeyValuePair<int, ServerController.Player> playerDictionary in ServerController.serverController.players)
		{
			ServerController.Player item = playerDictionary.Value;
			if (item.networkPlayer == networkPlayer)
			{
				player = item;
				if (player.otherPlayer != null)
				{
					serverController.SendRPCToNetworkPlayer("DeletePlayerClient", player.otherPlayer.networkPlayer);
					serverController.players.Remove(player.otherPlayer.networkPlayer); 
					player.otherPlayer.otherPlayer = null;
				}
				serverController.players.Remove(player.networkPlayer);
				player = null;
				return;
			}
		}
	}

	[SomeRPC]
	void DeletePlayerClient()
	{
		MasterServerGUI.Disconnect();
	}
	//Clients started the game...
	[SomeRPC]
	public void StartGame()
	{
		OnChangeQueueClient(serverController.isMyQueue);
		MenuControllerGenerator.controller.LoadLevel(MenuControllerGenerator.controller.game);
	}
	//Increase the coins, when other player has disconnected in game time
	[SomeRPC]
	public void OnOtherForceDisconnected(int otherPlayer)
	{
		serverController.SendRPCToNetworkPlayer("OnOtherForceDisconnectedClient", otherPlayer);
	}

	[SomeRPC]
	void OnOtherForceDisconnectedClient()
	{
		serverController.coins += serverController.prize;
		serverController.coins = Mathf.Clamp(serverController.coins, serverController.minCoins, serverController.maxCoins);
		Profile.SetUserDate(serverController.myName + "_Coins", serverController.coins);
	}
	
	//Set prize to other player
	[SomeRPC]
	public void SetPrizeToOther(int otherPlayer, int otherPrize)
	{
		serverController.SendRPCToNetworkPlayer("SetPrizeToOtherClient", otherPlayer, otherPrize);
	}

	[SomeRPC]
	void SetPrizeToOtherClient(int otherPrize)
	{
		serverController.serverMessenger.SetPrizeToOther(otherPrize);
	}




	//Activate Menu Buttons
	[SomeRPC]
	public void ActivateMenuButtons(int otherPlayer)
	{
		serverController.SendRPCToNetworkPlayer("ActivateMenuButtonsClient", otherPlayer);
	}

	[SomeRPC]
	void ActivateMenuButtonsClient()
	{
		GameManager gameManager = GameManager.FindObjectOfType<GameManager>();
		if (gameManager)
		{
			gameManager.ActivateMenuButtons(true);
		}
	}



	//Set "High Score" to other player
	[SomeRPC]
	public void SetHighScoreToOther(int otherPlayer, int otherHighScore)
	{
		serverController.SendRPCToNetworkPlayer("SetHighScoreToOtherClient", otherPlayer, otherHighScore);
	}

	[SomeRPC]
	void SetHighScoreToOtherClient(int otherHighScore)
	{
		serverController.serverMessenger.SetHighScoreToOther(otherHighScore);
	}
	//Set coins to other player
	[SomeRPC]
	public void SetCoinsToOther(int otherPlayer, int otherCoins)
	{
		serverController.SendRPCToNetworkPlayer("SetCoinsToOtherClient", otherPlayer, otherCoins);
	}

	[SomeRPC]
	void SetCoinsToOtherClient(int otherCoins)
	{
		serverController.serverMessenger.SetCoinsToOther(otherCoins);
	}



	//Set avatar to other player
	[SomeRPC]
	public void SetAvatarToOther(int otherPlayer, string avatarURL)
	{
		serverController.SendRPCToNetworkPlayer("SetAvatarToOtherClient", otherPlayer, avatarURL);
	}

	//[SomeRPC]
	//void SetAvatarToOtherClient(string otherAvatarURL)
	//{
	//	MenuControllerGenerator.controller.otherAvatarURL = otherAvatarURL;
	//	StartCoroutine(serverController.DownloadOtherAvatar());
	//}


	//Show message the other player
	[SomeRPC]
	public void ShowOtherMessage(int otherPlayer, string message)
	{
		serverController.SendRPCToNetworkPlayer("ShowOtherMessageClient", otherPlayer, message);
	}

	[SomeRPC]
	void ShowOtherMessageClient(string message)
	{
		serverController.serverMessenger.ShowOtherMessage(message);
	}
	//Set the error text, example when the player potted the cue ball
	[SomeRPC]
	public void SetErrorText(int otherPlayer, string errorText)
	{
		serverController.SendRPCToNetworkPlayer("SetErrorTextClient", otherPlayer, errorText);
	}

	[SomeRPC]
	void SetErrorTextClient(string errorText)
	{
		serverController.serverMessenger.SetErrorText(errorText);
	}



	//Send player name
	[SomeRPC]
	public void SendFirstPlayerName(int otherPlayer, string otherPlayerName)
	{
		serverController.SendRPCToNetworkPlayer("SendFirstPlayerNameClient", otherPlayer, otherPlayerName);
	}

	[SomeRPC]
	void SendFirstPlayerNameClient(string otherPlayerName)
	{
		serverMessenger.SendFirstPlayerName(otherPlayerName);
	}

	//Send player name
	[SomeRPC]
	public void SendSecondPlayerName(int otherPlayer, string otherPlayerName)
	{
		serverController.SendRPCToNetworkPlayer("SendSecondPlayerNameClient", otherPlayer, otherPlayerName);
	}

	[SomeRPC]
	void SendSecondPlayerNameClient(string otherPlayerName)
	{
		serverMessenger.SendSecondPlayerName(otherPlayerName);
	}


	//When  player can move the cue ball in  table
	[SomeRPC]
	public void SetMoveInTable(int otherPlayer)
	{

		serverController.SendRPCToNetworkPlayer("SetMoveInTableClient", otherPlayer);
	}

	[SomeRPC]
	void SetMoveInTableClient()
	{
		serverController.serverMessenger.SetMoveInTable();
	}
	//When all balls is sleeping
	[SomeRPC]
	public void OnChanghAllIsSleeping(int otherPlayer)
	{
		serverController.SendRPCToNetworkPlayer("OnChanghAllIsSleepingClient", otherPlayer);
	}

	[SomeRPC]
	void OnChanghAllIsSleepingClient()
	{
		serverController.serverMessenger.OnChanghAllIsSleeping();
	}
	//When  player want  to play again
	[SomeRPC]
	public void WantToPlayAgain(int otherPlayer)
	{
		serverController.SendRPCToNetworkPlayer("WantToPlayAgainClient", otherPlayer);
	}

	[SomeRPC]
	void WantToPlayAgainClient()
	{
		serverController.serverMessenger.OnWantToPlayAgain();
	}
	//When player is ready to play
	[SomeRPC]
	public void ReadyToPlay(int otherPlayer)
	{
		Debuger.DebugOnScreen("ReadyToPlay");
		serverController.SendRPCToNetworkPlayer("ReadyToPlayClient", otherPlayer);
	}

	[SomeRPC]
	void ReadyToPlayClient()
	{
		Debuger.DebugOnScreen("ReadyToPlayClient");
		serverController.serverMessenger.OnReadyToPlay();
	}
	//Send Change the queue for shot to other player
	[SomeRPC]
	public void OnChangeQueue(int otherPlayer, bool myTurn)
	{
		serverController.SendRPCToNetworkPlayer("OnChangeQueueClient", otherPlayer, myTurn);
	}

	[SomeRPC]
	void OnChangeQueueClient(bool myTurn)
	{
		serverController._isMyQueue = myTurn;
		serverController.ChangeQueueEvent(myTurn);
		serverController.serverMessenger.OnChangeQueue(myTurn);
	}
	//When player select the ball
	[SomeRPC]
	public void OnSelectBall(int otherPlayer, Vector3 position)
	{
		serverController.SendRPCToNetworkPlayer("OnSelectBallClient", otherPlayer, position);
	}

	[SomeRPC]
	void OnSelectBallClient(Vector3 position)
	{
		serverController.serverMessenger.OnSelectBall(position);
	}
	//When player unselect the ball
	[SomeRPC]
	public void OnUnselectBall(int otherPlayer)
	{
		serverController.SendRPCToNetworkPlayer("OnUnselectBallClient", otherPlayer);
	}

	[SomeRPC]
	void OnUnselectBallClient()
	{
		serverController.serverMessenger.OnUnselectBall();
	}
	//When  player move the cue ball in table
	[SomeRPC]
	public void SetOnMoveBall(int otherPlayer, Vector3 positin)
	{
		serverController.SendRPCToNetworkPlayer("SetOnMoveBallClient", otherPlayer, positin);
	}

	[SomeRPC]
	void SetOnMoveBallClient(Vector3 positin)
	{
		serverController.serverMessenger.SetOnMoveBall(positin);
	}
	

	//Sent shot time
	[SomeRPC]
	public void ForceSetShotCurrentTimeClient(float shotCurrentTime)
	{
		GameManager gameManager = GameManager.FindObjectOfType<GameManager>();
		if (gameManager && gameManager.cueController && gameManager.cueController.allIsSleeping)
		{
			gameManager.shotCurrentTime = shotCurrentTime;
			gameManager.otherShotCurrentTime = shotCurrentTime;
		}
	}

	[SomeRPC]
	void SendShotCurrentTimeClient(float shotCurrentTime)
	{
		serverController.serverMessenger.SendShotCurrentTime(shotCurrentTime);
	}

	public void SendShotCurrentTime(int otherPlayer, float shotCurrentTime)
	{
		serverController.SendRPCToNetworkPlayer("SendShotCurrentTimeClient", otherPlayer, shotCurrentTime);
	}


	//Set positin, velocity and  angular velocity for the ball
	[SomeRPC]
	public void ForceSetBallMove(int otherPlayer, int id, Vector3 positin)
	{
		serverController.SendRPCToNetworkPlayer("ForceSetBallMoveClient", otherPlayer, id, positin);
	}

	[SomeRPC]
	void ForceSetBallMoveClient(int id, Vector3 positin)
	{
		serverController.serverMessenger.ForceSetBallMove(id, positin);
	}
	//Send when the ball audio is playing
	[SomeRPC]
	public void OnPlayBallAudio(int otherPlayer, int id, float audioVolume)
	{
		serverController.SendRPCToNetworkPlayer("OnPlayBallAudioClient", otherPlayer, id, audioVolume);
	}

	[SomeRPC]
	void OnPlayBallAudioClient(int id, float audioVolume)
	{
		serverController.serverMessenger.OnPlayBallAudio(id, audioVolume);
	}
	//Send when the ball is sleeping
	[SomeRPC]
	public void SetBallSleeping(int otherPlayer, int id, Vector3 positin)
	{
		serverController.SendRPCToNetworkPlayer("SetBallSleepingClient", otherPlayer, id, positin);
	}

	[SomeRPC]
	void SetBallSleepingClient(int id, Vector3 positin)
	{
		serverController.serverMessenger.SetBallSleeping(id, positin);
	}
	//Send the control for the cue
	[SomeRPC]
	public void SendCueControl(int otherPlayer, Quaternion localRotation, Vector3 localPosition, Vector3 rotationDisplacement, float verticalRotation)
	{
		serverController.SendRPCToNetworkPlayer("SendCueControlClient", otherPlayer, localRotation, localPosition, rotationDisplacement, verticalRotation);
	}

	[SomeRPC]
	void SendCueControlClient(Quaternion localRotation, Vector3 localPosition, Vector3 rotationDisplacement, float verticalRotation)
	{
		serverController.serverMessenger.SendCueControl(localRotation, localPosition, rotationDisplacement, verticalRotation);
	}



	//Send when player is shot (for cue)
	[SomeRPC]
	public void SetBodyesPositions(int otherPlayer, string ballsPositions)
	{
		serverController.SendRPCToNetworkPlayer("SetBodyesPositionsClient", otherPlayer, ballsPositions);
	}

	[SomeRPC]
	void SetBodyesPositionsClient(string ballsPositions)
	{
		serverController.serverMessenger.SetBodyesPositions(ballsPositions);
	}






	//Set Network Parameters
	[SomeRPC]
	public void ForceSetNetworkParameters(int otherPlayer, int id, float networkSpeed, Vector3 networkPosition, Vector3 networkAngularVelocity)
	{
		serverController.SendRPCToNetworkPlayer("ForceSetNetworkParametersClient", otherPlayer, id, networkSpeed, networkPosition, networkAngularVelocity);
	}

	[SomeRPC]
	void ForceSetNetworkParametersClient(int id, float networkSpeed, Vector3 networkPosition, Vector3 networkAngularVelocity)
	{
		serverController.serverMessenger.ForceSetNetworkParameters(id, networkSpeed, networkPosition, networkAngularVelocity);
	}


	//Set Network Parameters
	[SomeRPC]
	public void SetNetworkParameters(int otherPlayer, int id, float networkSpeed, Vector3 networkPosition, Vector3 networkAngularVelocity)
	{
		serverController.SendRPCToNetworkPlayer("SetNetworkParametersClient", otherPlayer, id, networkSpeed, networkPosition, networkAngularVelocity);
	}

	[SomeRPC]
	void SetNetworkParametersClient(int id, float networkSpeed, Vector3 networkPosition, Vector3 networkAngularVelocity)
	{
		serverController.serverMessenger.SetNetworkParameters(id, networkSpeed, networkPosition, networkAngularVelocity);
	}



	//Send when player is shot (for cue)
	[SomeRPC]
	public void OnShotCue(int otherPlayer)
	{
		serverController.SendRPCToNetworkPlayer("OnShotCueClient", otherPlayer);
	}

	[SomeRPC]
	void OnShotCueClient()
	{
		serverController.serverMessenger.OnShotCue();
	}
	//Send when player is shot (for ball)
	[SomeRPC]
	public void ShotBall(int otherPlayer, Vector3 ballShotVelocity, Vector3 hitBallVelocity, Vector3 secondVelocity, Vector3 ballShotAngularVelocity, string ballsPositions)
	{
		serverController.SendRPCToNetworkPlayer("ShotBallClient", otherPlayer, ballShotVelocity, hitBallVelocity, secondVelocity, ballShotAngularVelocity, ballsPositions);
	}

	[SomeRPC]
	void ShotBallClient(Vector3 ballShotVelocity, Vector3 hitBallVelocity, Vector3 secondVelocity, Vector3 ballShotAngularVelocity, string ballsPositions)
	{
		serverController.serverMessenger.ShotBall(ballShotVelocity, hitBallVelocity, secondVelocity, ballShotAngularVelocity, ballsPositions);
	}
	//Send when ball in pocket
	[SomeRPC]
	public void SendOnTriggerEnter(int otherPlayer, int ballId, float audioVolume, float currentLungth, int holleId)
	{
		serverController.SendRPCToNetworkPlayer("SendOnTriggerEnterClient", otherPlayer, ballId, audioVolume, currentLungth, holleId);
	}

	[SomeRPC]
	void SendOnTriggerEnterClient(int ballId, float audioVolume, float currentLungth, int holleId)
	{
		serverController.serverMessenger.SendOnTriggerEnter(ballId, audioVolume, currentLungth, holleId);
	}
}
