using UnityEngine;
using System.Collections;

public abstract class MenuEnum : MonoBehaviour
{
	public Menu menu;
	public Button enumButton;
	public Button[] buttons;
	[System.NonSerialized]
	public Button curentButton = null;

	
	[SerializeField]
	protected float OpenCloseTime = 0.3f;
	
	[System.NonSerialized]
	public bool isOpened = false;
	[SerializeField]
	protected bool closeWhenSelected = true;
	[SerializeField]
	private bool isFlipFlop = false;

	
	protected bool inProcess = false;
	
	protected void Awake ()
	{
		enumButton.ButtonDown += OnDown;
		enumButton.isFlipFlop = true;
		
		
		for(int i = 0; i < buttons.Length; i++)
		{
			Button btn = buttons[i];
			
			if(i == 0 && MenuControllerGenerator.controller && MenuControllerGenerator.controller.IsFirstTimeStarted)
			btn.state = true;

			btn.isFlipFlop = isFlipFlop;
		}
	}
	protected IEnumerator Start ()
	{
		yield return new WaitForEndOfFrame();
		for(int i = 0; i < buttons.Length; i++)
		{
			Button btn = buttons[i];
			if(btn.state)
			curentButton = btn;
			btn.enabled = false;
		}
		enumButton.state = !isOpened;
		enumButton.Convert();
	}
	void OnDown (Button button)
	{
	    if(!inProcess)
		OpenCloseList(isOpened);
	}
	void OpenCloseList (bool isOpen)
	{
		if(!inProcess)
		{
		inProcess = true;
	    isOpened = !isOpen;
		enumButton.state = !isOpened;
		enumButton.Convert();
		if(isOpened)
		StartCoroutine(Open ());	
		else
		StartCoroutine(Close ());
		}
	}
	
	protected virtual void CloseWhenSelected (Button button)
	{
		if(!inProcess && isOpened)
		{
			OpenCloseList(isOpened);
			curentButton = button;
			foreach(Button btn in this.buttons)
			{
				btn.state = btn == curentButton;
				btn.Convert();
			}
		}
	}
	
	protected virtual IEnumerator Open ()
	{
		yield return null;
	}
	protected virtual IEnumerator Close ()
	{
		yield return null;
	}
}
