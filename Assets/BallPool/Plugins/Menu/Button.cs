using UnityEngine;
using System;
using System.Collections;

public enum ButtonOption {Click, Down, Up, Pressed};

public abstract class Button : MonoBehaviour 
{
	[System.Serializable]
	public class MessengerObjects
	{
		public Component MessageObject;
		public string Message;
		public ButtonOption Option;
		public bool SendOnStart;
		
		public MessengerObjects (Component MessageObject, string Message, ButtonOption Option, bool SendOnStart)
		{
			this.MessageObject = MessageObject;
			this.Message = Message;
			this.Option = Option;
			this.SendOnStart = SendOnStart;
		}
	}
	[System.NonSerialized]
	public RaycastHit hit;
	public Menu menu;
	public string ButtonName = "New button";
	public string PlayerPrefsId = "New id";
	public MessengerObjects[] Messengers;
	public bool SendOnStart = false;
	private bool isDown = false;
	
	public delegate void OnButton(Button btton);
	public event OnButton ButtonClick;
	public event OnButton ButtonDown;
	public event OnButton ButtonUp;
	public event OnButton ButtonPressed;
	public bool state = false;
	private bool oldState = false;
	
	
	public bool isFlipFlop = false;
	public float SendTime = 0.3f;
	
	[SerializeField]
	private AudioClip down;
	[SerializeField]
	private AudioClip up;
	[SerializeField]
	private AudioClip pressed;
	
	private AudioSource Down;
	private AudioSource Up;
	private AudioSource Pressed;
	
	
	private AudioSource sound;
	
	public bool HasButtonDown
	{
		get{return ButtonDown != null || (Messengers.Length != 0 && CheckOption(ButtonOption.Down));}
	}
	public bool HasButtonUp
	{
		get{return ButtonUp != null || (Messengers.Length != 0 && CheckOption(ButtonOption.Up));}
	}
	public bool HasButtonClick
	{
		get{return ButtonClick != null || (Messengers.Length != 0 && CheckOption(ButtonOption.Click));}
	}
	public bool HasButtonPressed
	{
		get{ return ButtonPressed != null || (Messengers.Length != 0 && CheckOption(ButtonOption.Pressed));}
	}
	protected void Awake ()
	{		
		PlayerPrefsId = "PlayerPrefs" + transform.position.ToString();
		SetAudio(ref Down, down);
		SetAudio(ref Up, up);
		SetAudio(ref Pressed, pressed);
		
		if(MenuControllerGenerator.controller && !MenuControllerGenerator.controller.IsFirstTimeStarted)
		state = isFlipFlop ? PlayerPrefs.GetInt(PlayerPrefsId) == 1 : false;
		else
		if(!isFlipFlop)
		state = false;
			
		SetState (state);
		
		SendTime = Mathf.Clamp(SendTime, 0.1f, float.MaxValue);
				
		ChackSound(Down);
		ChackSound(Up);
	    ChackSound(Pressed);
	}
	IEnumerator Start ()
	{
		if(isFlipFlop)
		SendMessagesAll ();
		
		if(SendOnStart)
		yield return new WaitForEndOfFrame ();
		else
		yield break;
		
		if(ButtonDown != null)
		ButtonDown(this);
		if(ButtonUp != null)
		ButtonUp(this);
		if(ButtonClick != null)
		ButtonClick(this);
		if(ButtonPressed != null)
		ButtonPressed(this);
	}
	void Update ()
	{
		if(oldState != state)
		Convert ();
	}
	public void Convert ()
	{
		oldState = state;
		SetState(state);
		if(isFlipFlop)
		PlayerPrefs.SetInt(PlayerPrefsId, state?1:0 );
	}
	void SetAudio (ref AudioSource sourse, AudioClip clip)
	{
		if(!clip)
		return;
		sourse = gameObject.AddComponent<AudioSource>();
		sourse.clip = clip;
	}
	public void Resset ()
	{
		isDown = false;
		if(!isFlipFlop)
		{
		state = false;
		}
	}
	private bool CheckOption (ButtonOption option)
	{
		for (int i = 0; i < Messengers.Length; i++)
		{
			if(Messengers[i].Option == option)
			return true;
		}
		return false;
	}
	private void SendMessagesAll (ButtonOption option)
	{
		for (int i = 0; i < Messengers.Length; i++)
		{
		    if(Messengers[i].MessageObject && Messengers[i].Message != "" && Messengers[i].Option == option)
		    Messengers[i].MessageObject.SendMessage(Messengers[i].Message, this);
		}
	}
	private void SendMessagesAll ()
	{
		for (int i = 0; i < Messengers.Length; i++)
		{
		    if(Messengers[i].MessageObject && Messengers[i].Message != "" && Messengers[i].SendOnStart)
		    Messengers[i].MessageObject.SendMessage(Messengers[i].Message, this);
		}
	}
	public void OnButtonClick ()
	{
		if(!isDown)
		{
		isDown = true;
		}
		else
		{
		PlaySound(Up);
		if(isFlipFlop)
		state = !state;
	
		if(!IsInvoking("SendOnButtonClick"))
		Invoke("SendOnButtonClick", SendTime);
		}
	}
	private void SendOnButtonClick ()
	{
		SendMessagesAll(ButtonOption.Click);
		if(ButtonClick != null)
		ButtonClick(this);
	}
	public void OnButtonDown ()
	{
		PlaySound(Down);
		if(isFlipFlop)
		state = !state;
		
		if(!IsInvoking("SendOnButtonDown"))
		Invoke("SendOnButtonDown", SendTime);
	}
	private void SendOnButtonDown ()
	{
		SendMessagesAll(ButtonOption.Down);
		if(ButtonDown != null)
		ButtonDown(this);
	}
	public void OnButtonUp ()
	{
		PlaySound(Up);
		if(isFlipFlop)
		state = !state;
				
		if(!IsInvoking("SendOnButtonUp"))
		Invoke("SendOnButtonUp", SendTime);
	}
	private void SendOnButtonUp ()
	{
		SendMessagesAll(ButtonOption.Up);
		if(ButtonUp != null)
		ButtonUp(this);
	}
	public void OnButtonPressed ()
	{	
		if(!IsInvoking("SendOnButtonPresset"))
		Invoke("SendOnButtonPresset", SendTime);
	}
	private void SendOnButtonPresset ()
	{
		PlaySound(Pressed);
		if(isFlipFlop)
		state = !state;
		
		SendMessagesAll(ButtonOption.Pressed);
		if(ButtonPressed != null)
		ButtonPressed(this);
	}
	void ChackSound(AudioSource sound)
	{
		if(sound)
		{
		sound.playOnAwake  = false;
		sound.Stop();
		sound.loop = false;
		}
	}
	void PlaySound (AudioSource sound)
	{
		if(sound && !sound.isPlaying)
		sound.Play();
	}
	protected abstract void SetState (bool state);
}
