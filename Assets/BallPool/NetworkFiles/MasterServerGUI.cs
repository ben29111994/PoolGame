using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;


public class MasterServerGUI : MonoBehaviour
{
	public string gameName = "Player Name";

	private GUIStyle customButton;
	private GUIStyle customText;
	private GUIStyle customLabel;



	#if UNITY_NETWORK
	private string skill = "3";
	[SerializeField]
	private int serverPort = 25013;
	[SerializeField]
	private float lastHostListRequest = -1000.0f;
	[SerializeField]
	private float hostListRefreshTimeout = 10.0f;
	
	private ConnectionTesterStatus natCapable = ConnectionTesterStatus.Undetermined;
	private bool filterNATHosts = false;
	private bool probingPublicIP = false;
	private bool doneTesting = false;
	private float timer = 0.0f;
	private string serverPortStr = "2016";
	private string serverIp = "127.0.0.1";

	private string testMessage = "Undetermined NAT capabilities";
	private bool useNat = false;
	private Vector2 scrollPosition = Vector2.zero;
	private Vector2 listScrollPosition = Vector2.zero;
	
	[System.NonSerialized]
	public NetworkPlayer[] players;
	private List<NetworkPlayer> playersList;
	
#elif PHOTON_PUN
	[System.NonSerialized]
	public bool conectedToPhoton = false;
	[System.NonSerialized]
	public bool canCreateRoom = false;
	private string oldGameName = "Player Name";
	[System.NonSerialized]
	public string roomName = "myRoom";
	private bool connectFailed = false;
	[System.NonSerialized]
	public bool roomIsCreated = false;
	[System.NonSerialized]
	public bool joinededToRoom = false;

	private PhotonPlayer otherPlayer;
	private PhotonView photonView;

    
















	#elif PHOTON_NETWORK
   //
#endif
	void OnGUI()
	{
		GUI.contentColor = Color.Lerp(Color.white, Color.yellow, 0.15f);
		customButton = new GUIStyle("button");
		customButton.fontSize = 32;
		customText = new GUIStyle("textfield");
		customText.fontSize = 32;
		customLabel = new GUIStyle("label");
		customLabel.fontSize = 32;
     
		ShowServerGUI();
	}

	public float sendRate
	{

		#if UNITY_NETWORK
		get{ return (float)Network.sendRate; }
		#elif PHOTON_PUN
		get{ return (float)PhotonNetwork.sendRate; }
		#elif PHOTON_NETWORK
		get{ return 30.0f;}
		#else
		get{ return 30; }
		#endif
	}

	void Awake()
	{
		name = "MasterServerGUI";
		//StartCoroutine(DownloadAvatar(MenuControllerGenerator.controller.avatarURL));

#if UNITY_NETWORK
		Network.sendRate = 30;
		NetworkView nv = gameObject.AddComponent<NetworkView>();
		nv.stateSynchronization = NetworkStateSynchronization.Off;
		nv.observed = this;
#elif PHOTON_PUN
		PhotonNetwork.sendRate = 30;
		PhotonNetwork.sendRateOnSerialize = 30;
		photonView = gameObject.AddComponent<PhotonView>();
		photonView.ObservedComponents = new List<Component>(0);
		photonView.ObservedComponents.Add(this);
		photonView.viewID = 1;

		// this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
		PhotonNetwork.automaticallySyncScene = true;
		
		// the following line checks if this client was just created (and not yet online). if so, we connect
		if (PhotonNetwork.connectionStateDetailed == ClientState.PeerCreated)
		{
			// Connect to the photon master-server. We use the settings saved in PhotonServerSettings (a .asset file in this project)
			PhotonNetwork.ConnectUsingSettings("0.1");
		}
		

#elif PHOTON_NETWORK
		//
#endif
	}

	void OnEnable()
	{
		ServerController.serverController = gameObject.GetComponent<ServerController>();
		ServerController.logicAI = gameObject.GetComponent<LogicAI>();

		string savedMyName = PlayerPrefs.GetString("BallPoolMultyplayerServerTemplateDemoPlayerName");
		if (savedMyName != "")
		{
			gameName = savedMyName;

		}

		name = "MasterServerGUI";
		ServerController.serverController.myName = gameName;
		int coins = Profile.GetUserDate(ServerController.serverController.myName + "_Coins");
		if (coins > 0)
		{
			ServerController.serverController.coins = coins;
		}
#if UNITY_NETWORK
		ServerController.serverController.isServerClientArchitecture = true;
		natCapable = Network.TestConnection();
		// What kind of IP does this machine have? TestConnection also indicates this in the
		// test results
		//if (Network.HavePublicAddress())
		//Debug.Log("This machine has a public IP address");
		//else
		//Debug.Log("This machine has a private IP address");
#elif PHOTON_PUN
		ServerController.serverController.isServerClientArchitecture = false;
		oldGameName = gameName;
		PhotonNetwork.playerName = gameName;
		roomName = "Room" + ", Prize - " + ServerController.serverController.prize;
#elif PHOTON_NETWORK
    ServerController.serverController.isServerClientArchitecture = true;
#endif
	}

	void Update()
	{
#if UNITY_NETWORK
		if (Network.isServer && ServerController.serverController.players != null)
		{
			foreach (KeyValuePair<int, ServerController.Player> playerDictionary in ServerController.serverController.players)
			{
				ServerController.Player player = playerDictionary.Value;
				if (player.myTurn)
				{
					if (player.time > 0.0f)
					{
						player.time -= Time.deltaTime;
						if (player.time < 0.0f)
						{
							player.time = 0.0f;
							ServerController.serverController.SendRPCToNetworkPlayer("ForceSetShotCurrentTimeClient", player.networkPlayer, player.time);
							ServerController.serverController.SendRPCToNetworkPlayer("ForceSetShotCurrentTimeClient", player.otherPlayer.networkPlayer, player.time);
						}
						else
						{
							ServerController.serverController.SendRPCToNetworkPlayer("SendShotCurrentTimeClient", player.networkPlayer, player.time);
							ServerController.serverController.SendRPCToNetworkPlayer("SendShotCurrentTimeClient", player.otherPlayer.networkPlayer, player.time);
						}
					}
					
					//MenuControllerGenerator.controller.shotTime
				}
			}
		}
		// If test is undetermined, keep running
		if (!doneTesting)
		{
			TestConnection();
		}
		
#endif
	}

	public static void Disconnect()
	{
#if UNITY_NETWORK
		Network.Disconnect();
#elif PHOTON_PUN
		PhotonNetwork.LeaveRoom();
#elif PHOTON_NETWORK

#endif
	}

	#if UNITY_NETWORK
	void TestConnection()
	{
		// Start/Poll the connection test, report the results in a label and react to the results accordingly
		natCapable = Network.TestConnection();

		switch (natCapable)
		{
			case ConnectionTesterStatus.Error: 
				testMessage = "Problem determining NAT capabilities";
				doneTesting = true;
				break;
			
			case ConnectionTesterStatus.Undetermined: 
				testMessage = "Undetermined NAT capabilities";
				doneTesting = false;
				break;
			case ConnectionTesterStatus.LimitedNATPunchthroughPortRestricted:
				testMessage = "Limited NAT punchthrough capabilities. Cannot " +
				"connect to all types of NAT servers. Running a server " +
				"is ill advised as not everyone can connect.";
				useNat = true;
				doneTesting = true;
				break;
			
			case ConnectionTesterStatus.LimitedNATPunchthroughSymmetric:
				testMessage = "Limited NAT punchthrough capabilities. Cannot " +
				"connect to all types of NAT servers. Running a server " +
				"is ill advised as not everyone can connect.";
				useNat = true;
				doneTesting = true;
				break;
			
			case ConnectionTesterStatus.NATpunchthroughAddressRestrictedCone:
			case ConnectionTesterStatus.NATpunchthroughFullCone:
				testMessage = "NAT punchthrough capable. Can connect to all " +
				"servers and receive connections from all clients. Enabling " +
				"NAT punchthrough functionality.";
				useNat = true;
				filterNATHosts = true;
				doneTesting = true;
				break;
			case ConnectionTesterStatus.PublicIPIsConnectable:
				testMessage = "Directly connectable public IP address.";
				useNat = false;
				doneTesting = true;
				break;
			
		// This case is a bit special as we now need to check if we can 
		// use the blocking by using NAT punchthrough
			case ConnectionTesterStatus.PublicIPPortBlocked:
				testMessage = "Non-connectble public IP address (port " + serverPort + " blocked),"
				+ " running a server is impossible.";
				useNat = false;
			// If no NAT punchthrough test has been performed on this public IP, force a test
				if (!probingPublicIP)
				{
					//Debug.Log("Testing if firewall can be circumnvented");
					natCapable = Network.TestConnectionNAT();
					probingPublicIP = true;
					timer = Time.time + 10;
				}
			// NAT punchthrough test was performed but we still get blocked
			else if (Time.time > timer)
				{
					probingPublicIP = false; // reset
					useNat = true;
					doneTesting = true;
				}
				break;
			case ConnectionTesterStatus.PublicIPNoServerStarted:
				testMessage = "Public IP address but server not initialized,"
				+ "it must be started to check server accessibility. Restart connection test when ready.";
				break;
			default: 
				testMessage = "Error in test routine, got " + natCapable;
				break;
		}

	}
	#endif

	void ShowGUI()
	{
		if (!ServerController.serverController)
		{
			return;
		}
		#if !PHOTON_PUN
		gameName = GUI.TextField(new Rect(0.5f * Screen.width - 100, 0.5f * Screen.height - 50, 200, 60), gameName, customText);
		if (gameName.Contains(","))
		{
			gameName = gameName.Replace(",", "");
		}

		if (GUI.Button(new Rect(0.5f * Screen.width - 130, 50 + 0.5f * Screen.height - 15, 260, 70), "Play with AI", customButton))
		{
			PlayWithAI();
		}
		if (GUI.Button(new Rect(0.5f * Screen.width - 130, 230 + 0.5f * Screen.height, 260, 70), "Hotseat mode", customButton))
		{
			Hotseat();
		}
		
		GUI.Label(new Rect(0.5f * Screen.width + 170, 0.5f * Screen.height - 170, 210, 70), "AI Skill", customLabel);
		//skill = GUI.TextField(new Rect(0.5f * Screen.width + 300, 0.5f * Screen.height - 170, 100, 50), skill, customText);
		//int skillInt = 3;
		//if (int.TryParse(skill, out skillInt))
		//{
		//	MenuControllerGenerator.controller.AISkill = Mathf.Clamp(skillInt, 1, 3);
		//	skill = MenuControllerGenerator.controller.AISkill.ToString();
		//}
		//else
		//{
		//	skill = skillInt.ToString();
		//}

		//GUI.Label(new Rect(0.5f * Screen.width + 170, 0.5f * Screen.height - 50, 200, 70), "Avatar URL", customLabel);
		//MenuControllerGenerator.controller.avatarURL = GUI.TextField(new Rect(0.5f * Screen.width + 170, 0.5f * Screen.height, 300, 50), MenuControllerGenerator.controller.avatarURL, customText);

		//if (GUI.Button(new Rect(0.5f * Screen.width + 170, 0.5f * Screen.height - 100, 300, 50), "Download Avatar", customButton))
		//{
		//	StartCoroutine(DownloadAvatar(MenuControllerGenerator.controller.avatarURL));
		//}
		
		//if (MenuControllerGenerator.controller.avatarTexture != null)
		//{
		//	GUI.DrawTexture(new Rect(0.5f * Screen.width + 170, 0.5f * Screen.height + 80, 220, 220), MenuControllerGenerator.controller.avatarTexture);
		//}

		if (GUI.Button(new Rect(0.5f * Screen.width - 100, 120 + 0.5f * Screen.height + 10, 200, 70), "Training", customButton))
		{
			Training ();
		}


		if (!ServerController.serverController)
		{
			return;
		}
		GUI.Label(new Rect(470, 20, 100, 70), " Prize ", customLabel);

		string prize = GUI.TextField(new Rect(570, 20, 120, 70), ServerController.serverController.prize.ToString(), customText);
		GUI.Label(new Rect(700, 20, 200, 60), "Coins - " + ServerController.serverController.coins.ToString(), customLabel);
		int tryPrize;
		if (int.TryParse(prize, out tryPrize))
		{
			ServerController.serverController.prize = Mathf.Clamp(tryPrize, ServerController.serverController.minCoins, 500);
		}
		#endif
	}

	//public IEnumerator DownloadAvatar(string avatarURL)
	//{
	//	Debug.Log(avatarURL);
	//	WWW www = new WWW(avatarURL);
	//	yield return www;
	//	if (!string.IsNullOrEmpty(www.error))
	//	{
	//		MenuControllerGenerator.controller.avatarURL = www.error;
	//		Debug.Log(www.error);
	//	}
	//	else
	//	{
	//		MenuControllerGenerator.controller.avatarTexture = www.texture;
	//		PlayerPrefs.SetString("PoolGameAvatarURL", MenuControllerGenerator.controller.avatarURL);
	//	}
	//}


	public void PlayWithAI()
	{
		ServerController.serverController.myName = gameName;
		PlayerPrefs.SetString("BallPoolMultyplayerServerTemplateDemoPlayerName", gameName);
		ServerController.serverController.isMyQueue = Random.Range(0, 2) == 0;
		MenuControllerGenerator.controller.playWithAI = true;
		ServerController.serverController.otherName = "Player AI";
		int otherCoins = Profile.GetUserDate(ServerController.serverController.otherName + "_Coins");
		if (otherCoins > 0)
		{
			ServerController.serverController.otherCoins = otherCoins;
		}
		MenuControllerGenerator.controller.LoadLevel(MenuControllerGenerator.controller.game);
	}

	public void Hotseat()
	{
		gameName = "Player 1";
		ServerController.serverController.myName = gameName;
		PlayerPrefs.SetString("BallPoolMultyplayerServerTemplateDemoPlayerName", gameName);
		ServerController.serverController.isMyQueue = Random.Range(0, 2) == 0;
		MenuControllerGenerator.controller.hotseat = true;
		ServerController.serverController.otherName = "Player 2";
		int otherCoins = Profile.GetUserDate(ServerController.serverController.otherName + "_Coins");
		if (otherCoins > 0)
		{
			ServerController.serverController.otherCoins = otherCoins;
		}
		MenuControllerGenerator.controller.LoadLevel(MenuControllerGenerator.controller.game);
	}

	public void Training()
	{
		ServerController.serverController.myName = gameName;
		PlayerPrefs.SetString("BallPoolMultyplayerServerTemplateDemoPlayerName", gameName);
		MenuControllerGenerator.controller.LoadLevel(MenuControllerGenerator.controller.game);
		ServerController.serverController = null;
		gameObject.SetActive(false);
	}

	void ShowServerGUI()
	{
		if (MenuControllerGenerator.controller.playWithAI || MenuControllerGenerator.controller.hotseat)
		{
			return;
		}
#if UNITY_NETWORK

		if (Network.peerType == NetworkPeerType.Disconnected)
		{
			if (!ServerController.serverController)
			{
				return;
			}
			ShowGUI();
			if (GUI.Button(new Rect(10, 90, 200, 70), "Connect", customButton))
			{
				if (gameName == "")
				{
					Debug.LogWarning("Empty game name given during host registration");
				}
				else
				{
					Network.Connect(serverIp, serverPort, "VaghoPoolGame");
				}
			}
			if (Network.peerType == NetworkPeerType.Disconnected)
			{
				//Debug.Log("Test message: " + testMessage);
			}
			// Start a new server

			if (Network.peerType != NetworkPeerType.Server)
			{
				GUI.Label(new Rect(210, 20, 90, 50), " Server Port ", customLabel);
				serverPortStr = GUI.TextField(new Rect(300, 10, 120, 50), serverPortStr, customText);
			}




			GUI.Label(new Rect(210, 45, 90, 50), " Connection IP ", customLabel);
			serverIp = GUI.TextField(new Rect(300, 70, 220, 50), serverIp, customText);

			if (!int.TryParse(serverPortStr, out serverPort))
				return;
			if (GUI.Button(new Rect(10, 10, 230, 70), "Create Server", customButton))
			{
				if (gameName == "")
				{
					Debug.LogWarning("Empty game name given during host registration");
				}
				else
				{
					Network.InitializeServer(32, serverPort + ServerController.serverController.prize, useNat);
					Network.incomingPassword = "VaghoPoolGame";
					MasterServer.updateRate = 3;
					MasterServer.RegisterHost("BallPoolMultyplayerServerTemplateDemo", gameName + " , Prize: " + ServerController.serverController.prize, "testing the 8 ball pool multiplayer game template");
				}
			}
			
			// Refresh hosts
			if (GUI.Button(new Rect(10, 170, 170, 70), "Refresh", customButton)
			    || Time.realtimeSinceStartup > lastHostListRequest + hostListRefreshTimeout)
			{
				MasterServer.ClearHostList();
				MasterServer.RequestHostList("BallPoolMultyplayerServerTemplateDemo");
				lastHostListRequest = Time.realtimeSinceStartup;
				//Debug.Log("Refresh Click");
			}
			
			HostData[] data = MasterServer.PollHostList();
			
			int _cnt = 0;
			GUI.Label(new Rect(20, 240, 400, 55), "Available servers ", customLabel);
			listScrollPosition = GUI.BeginScrollView(new Rect(20, 250, 270, 310), listScrollPosition, new Rect(20, 250, data.Length == 0 ? 270 : 500, 75 * data.Length));

			foreach (HostData element in data)
			{
				// Do not display NAT enabled games if we cannot do NAT punchthrough
				if (!(filterNATHosts && element.useNat))
				{
					//string name = element.gameName + " " + element.connectedPlayers + " / " + element.playerLimit;
					string hostInfo;
					hostInfo = "[";
					// Here we display all IP addresses, there can be multiple in cases where
					// internal LAN connections are being attempted. In the GUI we could just display
					// the first one in order not confuse the end user, but internally Unity will
					// do a connection check on all IP addresses in the element.ip list, and connect to the
					// first valid one.
					foreach (string host in element.ip)
					{
						hostInfo = hostInfo + host + ":" + element.port + " ";
					}
					hostInfo = hostInfo + "]";
					
					if (GUI.Button(new Rect(20, 300 + (_cnt * 70), 400, 55), element.gameName, customButton))
					{
						if (gameName == "")
						{
							//Debug.LogError("Empty game name given during host registration");
						}
						else
						{
							// Enable NAT functionality based on what the hosts if configured to do
							useNat = element.useNat;
							Network.Connect(element.ip, element.port, "VaghoPoolGame");	

						}
						
					}
					_cnt++;
				}
			}
			GUI.EndScrollView();
		}
		else if (Network.isServer)
		{
			GUI.Label(new Rect(270, 10, 450, 70), " Ip: " + Network.player.ipAddress, customLabel);
			GUI.Label(new Rect(270, 70, 450, 150), " Connections: " + Network.connections.Length + "\n Prize:            " + ServerController.serverController.prize, customLabel);

			if (GUI.Button(new Rect(10, 10, 250, 70), "Kill Server", customButton))
			{
				Network.Disconnect();
				MasterServer.UnregisterHost();
				//Debug.Log("Disconnect");
			}
			GUI.Label(new Rect(10, 100, 250, 120), "Server:" + gameName + ",\n Players ", customLabel);
			scrollPosition = GUI.BeginScrollView(new Rect(10, 210, 0.9f * Screen.width, 0.5f * Screen.height), scrollPosition, new Rect(10, 210, 2100, 0.5f * Screen.height + 110 * ServerController.serverController.players.Count));
			int i = 0;
			foreach (KeyValuePair<int, ServerController.Player> playerDictionary in ServerController.serverController.players)
			{
				i++;
				ServerController.Player player = playerDictionary.Value;
				string usersData = "   [ Id: " + player.networkPlayer.ToString() + ": " + player.name + ", Coins: " + player.coins + "]";
				if (i % 2 == 0)
				{
					
					string otherPlayerName = player.otherPlayer == null ? "No player" : player.otherPlayer.name;
					string otherCoins = player.otherPlayer == null ? "" : "Coins: " + player.otherPlayer.coins;
					string time = (player.myTurn ? (player.time == 0.0f ? "" : player.name + " Is playing: Time: " + player.time.ToString("0.0")) : (player.otherPlayer.time == 0.0f ? "" : player.otherPlayer.name + " Is playing: Time: " + player.otherPlayer.time.ToString("0.0")).ToString());
					string isWinner = player.isWinner ? ", " + player.name + " winner" : player.otherPlayer.isWinner ? ", " + player.otherPlayer.name + " Has win" : "";
					usersData = "   [ Id: " + player.networkPlayer.ToString() + ": " + player.name + ", Coins: " + player.coins + "  ]  [  Id: " + (player.otherPlayer == null ? "" : player.otherPlayer.networkPlayer.ToString()) + ": " + otherPlayerName + ", " + otherCoins + "  ]" + ", " + time + isWinner;

					GUI.Label(new Rect(10, 210 + 30 * (i - 1), 2000, 100), "\n" + usersData, customLabel);
				}
				else if (player.otherPlayer == null)
				{
					GUI.Label(new Rect(10, 210 + 30 * (i - 1), 2000, 100), "\n" + usersData, customLabel);
				}
			}
			GUI.EndScrollView();
		}
		else if (Network.isClient && SceneManager.GetActiveScene().buildIndex == 0)
		{
			GUI.Label(new Rect(170, 120, 250, 60), "Coins: " + ServerController.serverController.coins, customLabel);
			GUI.Label(new Rect(170, 170, 650, 60), "Wait for other players or disconnect", customLabel);
            
			if (GUI.Button(new Rect(10, 10, 350, 70), "Disconnect from server", customButton))
			{
				Network.Disconnect();
				//Debug.Log("Disconnect");
			}
		}
#elif PHOTON_PUN

		if (!PhotonNetwork.connected)
		{
			if (PhotonNetwork.connecting)
			{
				GUILayout.Label("Connecting to: " + PhotonNetwork.ServerAddress, customLabel);
			}
			else
			{
				GUILayout.Label("Not connected. Check console output. Detailed connection state: " + PhotonNetwork.connectionStateDetailed + " Server: " + PhotonNetwork.ServerAddress, customLabel);
			}
			
			if (this.connectFailed)
			{
				conectedToPhoton = false;
			}
			ShowGUI();
			return;
		}
		else
		{
			if (!roomIsCreated && !joinededToRoom)
			{
				conectedToPhoton = true;
			}
		}
		if (!roomIsCreated && !joinededToRoom)
		{
			if (!ServerController.serverController)
			{
				return;
			}
			ShowGUI();

			if (oldGameName != gameName)
			{
				oldGameName = gameName;
				ServerController.serverController.myName = gameName;
				int coins = Profile.GetUserDate(ServerController.serverController.myName + "_Coins");
				if (coins > 0)
				{
					ServerController.serverController.coins = coins;
				}

				PlayerPrefs.SetString("BallPoolMultyplayerServerTemplateDemoPlayerName", gameName);
			}
			if (!ServerController.serverController)
			{
				return;
			}
			roomName = gameName + ',' + ServerController.serverController.prize;

			canCreateRoom = ServerController.serverController.coins >= ServerController.serverController.prize;
		}
		#elif PHOTON_NETWORK

#else
		ShowGUI ();
#endif

	}

	public void SendRPCToServer(string message, params object[] args)
	{
		if (MenuControllerGenerator.controller.playWithAI || MenuControllerGenerator.controller.hotseat)
			return;
#if UNITY_NETWORK
		if (!ServerController.serverController.GetComponent<NetworkView>())
			return;

		ServerController.serverController.GetComponent<NetworkView>().RPC(message, RPCMode.Server, args);
#elif PHOTON_PUN
		if (!photonView)
			return;
		System.Reflection.MethodInfo methodInfo = ServerController.serverController.GetType().GetMethod(message);
		methodInfo.Invoke(ServerController.serverController, args);
#elif PHOTON_NETWORK

#endif
	}
	//Send message thru the network for some network  player
	public void SendRPCToNetworkPlayer(string message, int player, params object[] args)
	{
		if (MenuControllerGenerator.controller.playWithAI || MenuControllerGenerator.controller.hotseat)
			return;
#if UNITY_NETWORK
		if (!ServerController.serverController.GetComponent<NetworkView>())
			return;
		ServerController.serverController.GetComponent<NetworkView>().RPC(message, players[player], args);
#elif PHOTON_PUN
		if (!photonView)
			return;
		photonView.RPC(message, otherPlayer, args);
#elif PHOTON_NETWORK


#endif
	}

	public void SetCoinsToPlayer(int networkPlayer, int coins)
	{
#if UNITY_NETWORK
		ServerController.serverController.FindPlayer(networkPlayer).coins = coins;
#elif PHOTON_PUN

#elif PHOTON_NETWORK
		ServerController.serverController.FindPlayer(networkPlayer).coins = coins;
#endif
	}


	#if UNITY_NETWORK
	

	void OnFailedToConnectToMasterServer(NetworkConnectionError info)
	{
		Debug.Log(info);
	}

	void OnFailedToConnect(NetworkConnectionError info)
	{
		Debug.Log(info);
	}

	void OnDisconnectedFromServer(NetworkDisconnection info)
	{
		if (Network.isServer)
			Debug.Log("Local server connection disconnected");
		else if (info == NetworkDisconnection.LostConnection)
			Debug.Log("Lost connection to the server");
		else
			Debug.Log("Successfully diconnected from the server");
		ServerController.serverController.isMyQueue = true;
		MenuControllerGenerator.controller.LoadLevel("GameStart");
		ServerController.serverController.isFirstPlayer = false;
		ServerController.serverController.myNetworkPlayer = 0;
		ServerController.serverController.otherNetworkPlayer = 0;

	}

	void OnPlayerDisconnected(NetworkPlayer player)
	{
		Debug.Log("Clean up after player " + player);
		ServerController.serverController.DeletePlayer(int.Parse(player.ToString()));

		Network.RemoveRPCs(player);
		Network.DestroyPlayerObjects(player);
	}

	void OnServerInitialized()
	{
		//Debug.Log("Server initialized and ready");
		ServerController.serverController.players = new Dictionary<int, ServerController.Player>();
		playersList = new List<NetworkPlayer>(0);
		ServerController.serverController.myNetworkPlayer = 0;
		ServerController.serverController.otherNetworkPlayer = 0;
		playersList.Add(Network.player);
		players = playersList.ToArray();
	}

	void OnConnectedToServer()
	{
		//Debug.Log("Connected to server");
	}

	void OnPlayerConnected(NetworkPlayer player)
	{
		Debug.Log("Player " + player + " connected from " + player.ipAddress + ":" + player.port);
		playersList.Add(player);
		players = playersList.ToArray();
		int playerId = int.Parse(player.ToString());
		ServerController.serverController.SendRPCToNetworkPlayer("SetMyNetworkPlayerClient", playerId, playerId);

	}
	
#elif PHOTON_PUN
	void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
	{
		Debug.Log("OnPhotonPlayerConnected " + newPlayer.ID);
		otherPlayer = newPlayer;
		ServerController.serverController.otherNetworkPlayer = otherPlayer.ID;
		ServerController.serverController.myName = gameName;
		ServerController.serverController.SendRPCToServer("SendFirstPlayerName", ServerController.serverController.otherNetworkPlayer, ServerController.serverController.myName);
		ServerController.serverController.StartGame();
	}

	void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
	{
		PhotonNetwork.LeaveRoom();
		PhotonNetwork.RemoveRPCs(otherPlayer);
		PhotonNetwork.DestroyPlayerObjects(otherPlayer);
	}

	public void ConnectToPhoton()
	{
		this.connectFailed = false;
		PhotonNetwork.ConnectUsingSettings("0.1");
	}

	public void DisconnetcFromPhoton()
	{
		this.connectFailed = true;
		PhotonNetwork.Disconnect();
	}

	public void CreateRoom()
	{
		ServerController.serverController.isMyQueue = true;
		ServerController.serverController.isFirstPlayer = true;
		PhotonNetwork.CreateRoom(this.roomName, new RoomOptions() { MaxPlayers = 2 }, null);
	}
	public void JoinRoom(RoomInfo room)
	{
		int rPrize = int.Parse(room.name.Remove(0, room.name.LastIndexOf(',') + 1));
		ServerController.serverController.isMyQueue = false;
		ServerController.serverController.prize = rPrize;
		PhotonNetwork.JoinRoom(room.name);
	}
	public void LeftRoom()
	{
		PhotonNetwork.LeaveRoom();
	}

	public void OnCreatedRoom()
	{
		roomIsCreated = true;
		Debug.Log("OnCreatedRoom");
	}

	public void OnJoinedRoom()
	{
		ServerController.serverController.otherNetworkPlayer = 0;
		if (!roomIsCreated)
		{
			otherPlayer = PhotonNetwork.masterClient;
			ServerController.serverController.otherNetworkPlayer = otherPlayer.ID;
		}
		joinededToRoom = true;
		Debug.Log("OnJoinedRoom");
		ServerController.serverController.myName = gameName;
		int coins = Profile.GetUserDate(ServerController.serverController.myName + "_Coins");
		if (coins > 0)
		{
			ServerController.serverController.coins = coins;
		}
		PlayerPrefs.SetString("BallPoolMultyplayerServerTemplateDemoPlayerName", gameName);

		if (!roomIsCreated)
		{
			ServerController.serverController.SendRPCToServer("SendFirstPlayerName", ServerController.serverController.otherNetworkPlayer, ServerController.serverController.myName);
			ServerController.serverController.StartGame();
		}
	}

	void OnLeftRoom()
	{
		roomIsCreated = false;
		joinededToRoom = false;
		ServerController.serverController.isMyQueue = true;
		MenuControllerGenerator.controller.LoadLevel("GameStart");
		ServerController.serverController.isFirstPlayer = false;
	}

	public void OnFailedToConnectToPhoton(object parameters)
	{
		this.connectFailed = true;
		Debug.Log("OnFailedToConnectToPhoton. StatusCode: " + parameters + " ServerAddress: " + PhotonNetwork.networkingPeer.ServerAddress);
	}
















	#elif PHOTON_NETWORK
    
#endif
	
}
