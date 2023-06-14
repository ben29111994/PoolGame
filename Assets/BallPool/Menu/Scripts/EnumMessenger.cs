using UnityEngine;
using System.Collections;

[AddComponentMenu("Menu/Enum Messenger")]
public class EnumMessenger : MenuEnum 
{
	[SerializeField]
	private GameObject MessengerWall;
	[SerializeField]
	private ProfileMessenger profileMessenger;

	new protected void Awake ()
	{
		MessengerWall.gameObject.SetActive(false);
		base.Awake();
	}
	new protected IEnumerator Start ()
	{
		StartCoroutine(base.Start());
		yield return new WaitForEndOfFrame();
	}
	protected override IEnumerator Open ()
	{
		//enabled = false;
		//collider.enabled = false;
		MessengerWall.gameObject.SetActive(true);
		yield return null;
		for(int i = 0; i < buttons.Length; i++)
		{
			Button btn = buttons[i];
			btn.enabled = true;
			btn.state = false;
			btn.ButtonDown += CloseWhenSelected;
		}
		inProcess = false;
	}
	protected override IEnumerator Close ()
	{
		MessengerWall.gameObject.SetActive(false);
		//enabled = true;
		//collider.enabled = true;
		for(int i = 0; i < buttons.Length; i++)
		{
			Button btn = buttons[i];
			btn.enabled = false;
			btn.ButtonDown -= CloseWhenSelected;
		}
		yield return new WaitForSeconds(0.3f);
		inProcess = false;
		isOpened = false;
	}

	//protected override void CloseWhenSelected (Button button)
	//{
	//	profileMessenger.ShowAndSendMessage(button.GetComponent<TextMesh>().text);
	//	StartCoroutine(Close ());
	//}
}
