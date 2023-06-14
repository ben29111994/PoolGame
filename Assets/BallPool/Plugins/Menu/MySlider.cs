using UnityEngine;
using System.Collections;
using System;

public enum Orient { right, up, forward};

public abstract class MySlider : MonoBehaviour 
{
	public delegate void OnMoveSlider (MySlider slider);
	public event OnMoveSlider MoveSlider;
	public event OnMoveSlider CheckSlider;

	public Menu menu;
	public Button button;
	public Transform slideObject;
	public string PlayerPrefsId = "New id";
	public float minValue = 0.0f;
	public float maxValue = 1.0f;
	public float Value = 0.0f;
	private float curentValue = 0.0f;
	[SerializeField]
	protected string format = "0.0";
	[SerializeField]
	private bool sendOnStart = false;
	
	[SerializeField]
	private AudioClip moveClip;
	private AudioSource moveSound;
	[SerializeField]
	private bool keepWithAudio = true;
	
	[SerializeField]
	private float minMove = 0.0f;
	[SerializeField]
	private float maxMove = 1.0f;
	private float ValueMove = 0.0f;
	[SerializeField]
	private Orient orient;
	private Vector3 localMoveOrient = Vector3.right;
	
	protected bool canMove = false;
	private Vector3 moveTo = Vector3.right;
	private float a;
	private float b;

	private Vector3 startPosition = Vector3.zero;

	
	
	protected void Awake () 
	{
	PlayerPrefsId = "PlayerPrefs" + transform.position.ToString();
		startPosition = slideObject.localPosition;
	 if(moveClip)
	 {
	 moveSound = gameObject.AddComponent<AudioSource>();
	 moveSound.clip = moveClip;
	 }
	 button.SendTime = 0.1f;
	 button.isFlipFlop = false;
	 button.Messengers = new Button.MessengerObjects[1];
	 button.Messengers[0] = new Button.MessengerObjects(this, "OnDown", ButtonOption.Down, false);
		
	 if(moveSound)
	 {
	 moveSound.playOnAwake = false;
	 moveSound.Stop();
	 moveSound.loop = false;
	 }
	 if(minMove == maxMove)
	 a = b = 0;	
	 else
	 {
	 a = (maxValue - minValue)/(maxMove - minMove);
	 b = (maxMove*minValue - minMove*maxValue)/(maxMove - minMove);
	 }
	 Value = a*ValueMove + b;
		
	if(MenuControllerGenerator.controller && !MenuControllerGenerator.controller.IsFirstTimeStarted)
	{
	CheckOrient ();
		
	if(sendOnStart)
	slideObject.localPosition = new Vector3(PlayerPrefs.GetFloat(PlayerPrefsId + "_X"), PlayerPrefs.GetFloat(PlayerPrefsId + "_Y"), PlayerPrefs.GetFloat(PlayerPrefsId + "_Z"));
			
	Move();
	}
	}

	public void Resset ()
	{
		slideObject.localPosition = startPosition;
	}
	void CheckOrient ()
	{
		switch (orient) 
		{
		case Orient.right:
		moveTo = transform.right;
		localMoveOrient = Vector3.right;
		break;
		case Orient.up:
		moveTo = transform.up;
		localMoveOrient = Vector3.up;
		break;
		case Orient.forward:
		moveTo = transform.forward;
		localMoveOrient = Vector3.forward;
		break;
		}
	}
	void OnDown (Button btn)
	{
		if(CheckSlider != null)
			CheckSlider(this);

		CheckOrient ();
				
		canMove = true;
		btn.SendTime = 0.0f;
		
		int moveOrient = Vector3.Dot( btn.hit.point - slideObject.position, moveTo) >=0.0f? 1: -1;
        slideObject.position += (moveOrient*Vector3.Distance(btn.hit.point, slideObject.position))*moveTo;
	}
	
	void CalculateAB ()
	{
		if(minMove == maxMove)
		a = b = 0;	
		else
		{
			a = (maxValue - minValue)/(maxMove - minMove);
			b = (maxMove*minValue - minMove*maxValue)/(maxMove - minMove);
		}
	}
	private void Move ()
	{
		Vector3 localSpeed = menu.GetMouseLocalSpeed(slideObject.position, moveTo);
        slideObject.position += localSpeed;
		
		CalculateAB ();
		
		ValueMove = Vector3.Dot(slideObject.localPosition, localMoveOrient);
		
		Value = a*ValueMove + b;
					
		if(ValueMove < minMove)
		{
		slideObject.localPosition = minMove*localMoveOrient;
		ValueMove = minMove;
		Value = minValue;
		}
		else
		if(ValueMove > maxMove)
		{
		slideObject.localPosition = maxMove*localMoveOrient;
		ValueMove = maxMove;
		Value = maxValue;
		}
		
		
		if(moveSound && curentValue != Value)
		{
		curentValue = Value;
		if(keepWithAudio)
		moveSound.volume = maxValue == minValue? 0: (Value - minValue)/maxValue-minValue;
		if(!moveSound.isPlaying)
		moveSound.Play();
		}

		OnMoveSliderEvent ();
	}
	public void OnMoveSliderEvent ()
	{
		if(MoveSlider != null)
			MoveSlider(this);
	}
	protected void Update ()
	{
		if(canMove && menu.MouseIsMove)
		Move ();
		
		if(menu.GetButtonUp())
		{
		     canMove = false;
			if(sendOnStart)
			{
				PlayerPrefs.SetFloat(PlayerPrefsId, ValueMove);
				PlayerPrefs.SetFloat(PlayerPrefsId + "_X", slideObject.localPosition.x);
				PlayerPrefs.SetFloat(PlayerPrefsId + "_Y", slideObject.localPosition.y);
				PlayerPrefs.SetFloat(PlayerPrefsId + "_Z", slideObject.localPosition.z);
			}
		}
	}
}
