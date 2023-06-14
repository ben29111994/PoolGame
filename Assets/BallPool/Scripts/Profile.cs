using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Profile : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer avatarRender;
	[SerializeField]
	private MeshRenderer avatarMeshRenderer;
	public List<GuiBall> guiBalls;
	private Transform balls;
	[System.NonSerialized]
	public GameManager gameManager; 
	public TextMesh playerName;
	public TextMesh WantToPlayAgain;
	[SerializeField]
	private SpriteRenderer back;
	public Sprite activeBack;
	public Sprite inactiveBack;
	public TextMesh winner;
	public TextMesh coins;
	public TextMesh highScore;

	public TextureSlider timeSlider;
	[System.NonSerialized]
	public bool isMain;


	public void OnAwakeGameManager ()
	{
		winner.GetComponent<Renderer>().enabled = false;
		timeSlider.Resset();
		WantToPlayAgain.gameObject.SetActive(false);
		guiBalls = new List<GuiBall>(0);
		balls = transform.Find("Balls");
		for (int i = 0; i < balls.childCount; i++) 
		{
			GuiBall guiBall = balls.Find("Ball_" + (i + 1)).GetComponent<GuiBall>();
			guiBall.GetComponent<Renderer>().enabled = false;
		}
		SetActive(isMain? ServerController.serverController.isMyQueue:! ServerController.serverController.isMyQueue);
		ServerController.serverController.OnChangeQueueEvent += ChangeBack;
	}

	void ChangeBack (bool myTurn)
	{
		SetActive(isMain? myTurn: !myTurn);
	}
	void OnDestroy ()
	{
		if(ServerController.serverController)
		{
			ServerController.serverController.OnChangeQueueEvent -= ChangeBack;
		}
	}

	public void SetActive (bool value)
	{
		back.sprite = value?activeBack:inactiveBack;
	}
	public void AddGuiBall (int id, Texture2D ballTexture)
	{
		GuiBall guiBall = balls.Find("Ball_" + (guiBalls.Count + 1)).GetComponent<GuiBall>();
		guiBalls.Add(guiBall);
		guiBall.id = id;
		guiBall.GetComponent<Renderer>().material.mainTexture = ballTexture;
		guiBall.GetComponent<Renderer>().enabled = true;
	}
	public void RemoveGuiBall (int id)
	{
		if(guiBalls.Count == 1 && (isMain? !gameManager.remainedBlackBall:!gameManager.otherRemainedBlackBall))
		{
			GuiBall guiBall = guiBalls[0];
			guiBall.GetComponent<Renderer>().material.mainTexture = gameManager.cueController.ballTextures[8];
			guiBall.id = 8;
			if(isMain)
			{
				gameManager.remainedBlackBall = true;
			}
			else
			{
				gameManager.otherRemainedBlackBall = true;
			}
		}
		else
		{
			GuiBall lastGuiBall = guiBalls[guiBalls.Count - 1];
			if(id != lastGuiBall.id)
			{
				GuiBall guiBall = FaindGuiBallById(id);
				guiBall.GetComponent<Renderer>().material.mainTexture = lastGuiBall.GetComponent<Renderer>().material.mainTexture;
				guiBall.id = lastGuiBall.id;
			}
			guiBalls.Remove(lastGuiBall);
			guiBalls.TrimExcess();
			Destroy(lastGuiBall.gameObject);
		}
	}

	public GuiBall FaindGuiBallById (int id)
	{
		GuiBall guiBall = null;
		foreach (GuiBall item in guiBalls) 
		{
			if(item.id == id)
			{
				guiBall = item;
				return guiBall;
			}
		}
		return guiBall;
	}
	public IEnumerator SetAvatar (Sprite avatar) 
	{
		Debug.Log(name);
		avatarRender.sprite = avatar;
		avatarRender.enabled = true;
		avatarMeshRenderer.enabled = false;
		yield return null;
	}

	public IEnumerator SetAvatar (Texture2D avatar) 
	{
		Debug.Log(name);
		avatarRender.enabled = false;
		avatarMeshRenderer.enabled = true;
		avatarMeshRenderer.sharedMaterial.mainTexture = avatar;
		yield return null;
	}

	public static void SetUserDate (string key, int value)
	{
		PlayerPrefs.SetInt(key, value);
	}
	public static int GetUserDate (string key)
	{
		return PlayerPrefs.GetInt(key);
	}
	void Update () {
	
	}
}
