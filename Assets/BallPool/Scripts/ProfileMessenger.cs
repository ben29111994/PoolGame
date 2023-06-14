using UnityEngine;
using System.Collections;

public class ProfileMessenger : MonoBehaviour 
{
	[SerializeField]
	private TextMesh myMessage;
	[SerializeField]
	private TextMesh otherMessage;
	[SerializeField]
	private float visibleTime = 1.5f;

	void Awake ()
	{
		myMessage.GetComponent<Renderer>().enabled = false;
		otherMessage.GetComponent<Renderer>().enabled = false;
	}
	//Show message the player
	//public void ShowAndSendMessage (string message)
	//{
	//	StartCoroutine(StartShowMessage (myMessage, message));
	//	ServerController.serverController.SendRPCToServer("ShowOtherMessage", ServerController.serverController.otherNetworkPlayer, message);
	//}
	//Show message the other player
	public void ShowOtherMessage (string message)
	{
		StartCoroutine(StartShowMessage (otherMessage, message));
	}
	IEnumerator StartShowMessage (TextMesh textMesh, string message)
	{
		textMesh.text = message;
		textMesh.GetComponent<Renderer>().enabled = true;
		yield return new WaitForSeconds(visibleTime);
		textMesh.GetComponent<Renderer>().enabled = false;
	}
}
