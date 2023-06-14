using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MasterServerPUN_UI : MonoBehaviour
{
	[SerializeField]
	private MasterServerGUI masterServerGUI;

	[SerializeField]
	private UnityEngine.UI.Button connectButton;
	[SerializeField]
	private UnityEngine.UI.Button disconnectButton;
	[SerializeField]
	private UnityEngine.UI.Button createRoomButton;
	[SerializeField]
	private UnityEngine.UI.Button leftRoomButton;
	[SerializeField]
	private UnityEngine.UI.Button playWithAIButton;
	[SerializeField]
	private UnityEngine.UI.Button hotseatButton;
	[SerializeField]
	private UnityEngine.UI.Button joinRoomButton;
	private List<UnityEngine.UI.Button> joinRoomButtons;
	[SerializeField]
	private RectTransform[] roomCreateGroup;
	[SerializeField]
	private RectTransform[] photonGroup;

	[SerializeField]
	private Text canNotCreateRoomText;
	[SerializeField]
	private InputField leftAvatarURlField;
	[SerializeField]
	private Image leftAvatarImage;
	[SerializeField]
	private Material leftAvatarMaterial;
	[SerializeField]
	private InputField roomNameField;
	[SerializeField]
	private InputField playerNameField;
	[SerializeField]
	private InputField aISkillField;
	[SerializeField]
	private InputField prizeField;
	[SerializeField]
	private Text coinsText;
	[SerializeField]
	private Text ipAdress;

	private bool conectedToPhoton = false;
	private bool canCreateRoom = true;
	private bool roomIsCreated = true;
	private bool joinededToRoom = true;
	private int roomListCount = -1;


	void Start () 
	{
		#if !PHOTON_PUN
		Destroy(gameObject);
		return;
		#else
		joinRoomButtons = new List<UnityEngine.UI.Button>(0);
		DownloadAvatar();
		leftAvatarURlField.text = MenuControllerGenerator.controller.avatarURL;
		playerNameField.text = masterServerGUI.gameName;
		Debug.Log(masterServerGUI.gameName);
		aISkillField.text = "3";
		MenuControllerGenerator.controller.OnLoadLevel += MenuControllerGenerator_controller_OnLoadLevel;
		coinsText.text = ServerController.serverController.coins.ToString();
		prizeField.text = ServerController.serverController.prize.ToString();
		joinRoomButton.gameObject.SetActive(false);
		#endif
	}
	#if PHOTON_PUN
	void MenuControllerGenerator_controller_OnLoadLevel (MenuController menuController)
	{
		if (!gameObject || !menuController)
		{
			Show();
			return;
		}
		Debug.Log( menuController.levelName );
		if (menuController.levelName == "Game")
		{
			Hide();
		}
		else if (menuController.levelName == "GameStart")
		{
			Show();
		}
	}


	public void OnSetAvatarURL(string url)
	{
		url = leftAvatarURlField.text;
		MenuControllerGenerator.controller.avatarURL = url;
	}
	public void OnSetPlayerName(string playerName)
	{
		playerName = playerNameField.text;
		if ( playerNameField.text.Contains(","))
		{
			playerNameField.text =  playerNameField.text.Replace(",", "");
		}
		masterServerGUI.gameName = playerNameField.text;
		Debug.Log(masterServerGUI.gameName);
	}
	public void OnSetPrize(string prize)
	{
		prize = prizeField.text;
		int tryPrize;
		if (int.TryParse(prize, out tryPrize))
		{
			ServerController.serverController.prize = Mathf.Clamp(tryPrize, ServerController.serverController.minCoins, 500);
		}
		else
		{
			prizeField.text = "10";
		}
	}
	public void OnSetAISkill(string skill)
	{
		skill = aISkillField.text;
		int skillInt = 3;
		if (int.TryParse(skill, out skillInt))
		{
			MenuControllerGenerator.controller.AISkill = Mathf.Clamp(skillInt, 1, 3);
		}
		else
		{
			aISkillField.text = "3";
		}
	}
	public void DownloadAvatar ()
	{
		StartCoroutine(WaitWhenDownloadAvatar());
	}
	IEnumerator WaitWhenDownloadAvatar()
	{
		yield return StartCoroutine(masterServerGUI.DownloadAvatar(MenuControllerGenerator.controller.avatarURL));
		if (MenuControllerGenerator.controller.avatarTexture)
		{
			leftAvatarImage.material = null;
			yield return new WaitForSeconds(0.1f);
			leftAvatarMaterial.mainTexture = MenuControllerGenerator.controller.avatarTexture;
			leftAvatarImage.material = leftAvatarMaterial;
		}
	}
	public void ConnectToPhoton ()
	{
		masterServerGUI.ConnectToPhoton();
	}
	public void DisconnectFromPhoton ()
	{
		masterServerGUI.DisconnetcFromPhoton();
	}
	public void CreateRoom ()
	{
		masterServerGUI.CreateRoom();
	}
	private void JoinToRoom (RoomInfo room, int index)
	{
		int rPrize = int.Parse(room.name.Remove(0, room.name.LastIndexOf(',') + 1));
		if (room.playerCount == (int)room.maxPlayers)
		{
			joinRoomButtons[index].GetComponentInChildren<Text>().text = "Is busy";
			//is busy;
		}
		else if (rPrize > ServerController.serverController.coins)
		{
			joinRoomButtons[index].GetComponentInChildren<Text>().text = "You have a " + ServerController.serverController.coins;
			//can not to join;
		}
		else
		{
			masterServerGUI.JoinRoom(room);
		}
		Debug.Log("JoinToRoom " + room.name);
	}
	public void LeftFromRoom ()
	{
		masterServerGUI.LeftRoom();
	}
	public void PlayWithAI ()
	{
		masterServerGUI.PlayWithAI();
		Hide();
	}
	public void Hotseat ()
	{
		masterServerGUI.Hotseat();
		Hide();
	}
	public void Training ()
	{
		masterServerGUI.Training();
		Hide();
	}
		
	public void Show ()
	{
		if (!coinsText ||!gameObject)
		{
			return;
		}
		if (!ServerController.serverController)
		{
			gameObject.SetActive(true);
			return;
		}
		coinsText.text = ServerController.serverController.coins.ToString();
		gameObject.SetActive(true);
	}
	public void Hide ()
	{
		gameObject.SetActive(false);
	}
	void Update () 
	{
		if (conectedToPhoton != masterServerGUI.conectedToPhoton)
		{
			conectedToPhoton = masterServerGUI.conectedToPhoton;
			ChangeConectedToPhoton();
			if (conectedToPhoton)
			{
				ChangeRoomIsCreatedOrJoinedToRoom();
				ChangeCanCreateRoom();
			}
		}
		if (roomIsCreated != masterServerGUI.roomIsCreated || joinededToRoom != masterServerGUI.joinededToRoom)
		{
			roomIsCreated = masterServerGUI.roomIsCreated;
			joinededToRoom = masterServerGUI.joinededToRoom;
			ChangeRoomIsCreatedOrJoinedToRoom();
		}
		if (canCreateRoom != masterServerGUI.canCreateRoom)
		{
			canCreateRoom = masterServerGUI.canCreateRoom;
			ChangeCanCreateRoom();
		}
		if (roomListCount != PhotonNetwork.GetRoomList().Length)
		{
			roomListCount = PhotonNetwork.GetRoomList().Length;
			ChangeRoomCount();
		}

	}
	void ChangeConectedToPhoton ()
	{
		connectButton.gameObject.SetActive(!conectedToPhoton);
		disconnectButton.gameObject.SetActive(conectedToPhoton);
		foreach (var item in photonGroup)
		{
			item.gameObject.SetActive(conectedToPhoton);
		}
	}
	void ChangeCanCreateRoom ()
	{
		canNotCreateRoomText.gameObject.SetActive(!canCreateRoom);
		createRoomButton.gameObject.SetActive(canCreateRoom);
		playWithAIButton.gameObject.SetActive(canCreateRoom);
		hotseatButton.gameObject.SetActive(canCreateRoom);
		if (!canCreateRoom)
		{
			canNotCreateRoomText.text = "Can not create room, you have a " + ServerController.serverController.coins + " coins, choose less prize";
		}
	}
	void ChangeRoomIsCreatedOrJoinedToRoom ()
	{
		foreach (var item in roomCreateGroup)
		{
			item.gameObject.SetActive(!joinededToRoom || !roomIsCreated);
		}
		leftRoomButton.gameObject.SetActive(roomIsCreated || joinededToRoom);
		createRoomButton.gameObject.SetActive(canCreateRoom && (!roomIsCreated || !joinededToRoom));
		ipAdress.gameObject.SetActive(roomIsCreated);
		if (roomIsCreated)
		{
			ipAdress.text = " Ip: " + PhotonNetwork.networkingPeer.ServerAddress;
		}
		else if (joinededToRoom)
		{
			ipAdress.text = "Wait for other players or disconnect";
		}
	}
	void ChangeRoomCount ()
	{
		if (joinRoomButtons != null)
		{
			foreach (var item in joinRoomButtons)
			{
				Destroy(item.gameObject);
			}
		}
		joinRoomButtons = new List<UnityEngine.UI.Button>(0);

		Vector3 localPosition = joinRoomButton.GetComponent<RectTransform>().localPosition;
		float y = localPosition.y;
		float deltaY = -60.0f;
		int i = 0;
		foreach (RoomInfo room in PhotonNetwork.GetRoomList())
		{
			int rPrize = int.Parse(room.name.Remove(0, room.name.LastIndexOf(',') + 1));
			string rName = "room with " + room.name.Remove(room.name.IndexOf(',')) + ", Prize " + rPrize;
			if (room.playerCount == (int)room.maxPlayers)
			{
				//is busy;
			}
			else if (rPrize > ServerController.serverController.coins)
			{
				//can not to join;
			}
			else
			{
				UnityEngine.UI.Button button = UnityEngine.UI.Button.Instantiate(joinRoomButton);
				joinRoomButtons.Add(button);
				button.transform.parent = joinRoomButton.transform.parent;
				button.GetComponent<RectTransform>().localPosition = new Vector3(localPosition.x, y + (float)i * deltaY, localPosition.z);
				button.gameObject.SetActive(true);
				Text buttonName = button.GetComponentInChildren<Text>();
				buttonName.text = "Join to " + rName;
				button.onClick.AddListener(() => JoinToRoom(room, i));
				i ++;
			}
		}
	}
	#endif
}
