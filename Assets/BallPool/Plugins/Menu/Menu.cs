using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


public abstract class Menu : MonoBehaviour 
{
	public Button[] buttons;
	public MySlider[] sliders;
	public MenuEnum[] enums;
	public CircularSlider[] circularSliders;
	
	
	public Camera guiCamera; 
	[SerializeField]
	private Collider wall;
	private Vector3 mousePosition = Vector3.zero;
	private Vector3 mouseOldPosition = Vector3.zero;
	private Vector3 mouseSpeed = Vector3.zero;
	private Vector3 mouseScreenOldPosition = Vector3.zero;
	private Vector3 mouseScreenSpeed = Vector3.zero;

	private bool mouseIsMove = false;
	private Collider curentCollider = null;
	private RaycastHit curentHit;
	
	public Vector3 MouseWorldPosition
	{
		get
		{
			return mousePosition;
		}
	}
	public Vector3 MouseWorldSpeed
	{
		get
		{
			return mouseSpeed;
		}
	}
	public Vector3 MouseScreenSpeed
	{
		get
		{
			return mouseScreenSpeed;
		}
	}
	public Vector3 GetMouseLocalSpeed (Vector3 position, Vector3 direction)
	{
		if(!guiCamera)
			return Vector2.zero;
		
		Vector3 speed = guiCamera.orthographic? MouseWorldSpeed :
		(Vector3.Distance(guiCamera.transform.position, position)/guiCamera.nearClipPlane)*MouseWorldSpeed;
		
		float tangents = Mathf.Abs( Vector3.Dot(speed.normalized, direction.normalized) );
		
		return Vector3.Project(speed, direction)/Mathf.Clamp( tangents, 0.01f, float.MaxValue );
	}
	public bool MouseIsMove
	{
		get
		{
			return mouseIsMove;
		}
	}
	public Button button(string name)
	{
		Button returnButtn = null;
		foreach(Button btn in buttons)
		{
			if(btn && btn.ButtonName == name)
			{
			returnButtn = btn;
			break;
			}
		}
		
		if(returnButtn == null)
		Debug.LogWarning("there is no button named " + name);
		return returnButtn;
	}	
	
	public abstract bool GetButton ();
	public abstract bool GetButtonDown ();
	public abstract bool GetButtonUp ();
	public abstract Vector3 GetPosition ();
	public abstract Vector3 GetScreenPoint ();

    void Awake()
    {
#if UNITY_EDITOR
        if (!MenuControllerGenerator.controller)
        {
            if (SceneManager.sceneCountInBuildSettings >= 3)
                SceneManager.LoadScene("GameStart");
            else
                Debug.LogError("Please add the scenes (GameStart, Game and Loader) in the File/Build Settings" +
                                   " as shown in the image  Assets/BallPool/TagAndLayers.png");
        }
#endif

    }

    void Start ()
	{
		
	}
	void Update ()
	{
		if(MenuControllerGenerator.controller && MenuControllerGenerator.controller.isPaused)
		{
			return ;
		}
		
		if(GetButtonDown ())
		{
			CheckButtonDown ();
			mouseScreenOldPosition = GetScreenPoint();
		    mouseOldPosition = GetPosition();
		}
		
		if(GetButtonUp ())
		{
			CheckButtonUp ();
			mouseIsMove = false;
		}
		if( GetButton () )
		{
			CheckButton ();
				
			mousePosition = GetPosition();
			mouseSpeed = MouseWorldPosition - mouseOldPosition;
			mouseOldPosition = mousePosition;
			Vector3 mouseScreenPosition = GetScreenPoint();
			mouseScreenSpeed = mouseScreenPosition - mouseScreenOldPosition;
			mouseScreenOldPosition = mouseScreenPosition;

			mouseIsMove = mouseSpeed != Vector3.zero;
		}
		else
		{
			mouseIsMove = false;
		}
	}
	void CheckButtonDown ()
	{
		curentCollider = FindCollider ();
			
		if(!curentCollider || curentCollider == wall)
		return;

		for (int i = 0; i < circularSliders.Length; i++) 
		{
			if(circularSliders[i].OnButtonDown (curentHit))
			{
				return;
			}
		}

		for (int i = 0; i < buttons.Length; i++) 
		{
			Button btn = buttons[i];
			
			if((btn.enabled && btn.HasButtonDown || btn.HasButtonClick) && CheckCollider(btn))
			{
				btn.hit = curentHit;
				if(btn.HasButtonDown)
				btn.OnButtonDown();
				if(btn.HasButtonClick)
				btn.OnButtonClick();
				
				return;
			}
		}	
		
		
		for (int i = 0; i < sliders.Length; i++) 
		{
			Button btn = sliders[i].button;
			
			if((btn.enabled && btn.HasButtonDown || btn.HasButtonClick) && CheckCollider(btn))
			{
				btn.hit = curentHit;
				if(btn.HasButtonDown)
				btn.OnButtonDown();
				if(btn.HasButtonClick)
				btn.OnButtonClick();
				
				return;
			}
		}	
		
		for (int i = 0; i < enums.Length; i++) 
		{
			Button btn = enums[i].enumButton;
			
			if((btn.enabled && btn.HasButtonDown || btn.HasButtonClick) && CheckCollider(btn))
			{
				btn.hit = curentHit;
				if(btn.HasButtonDown)
				btn.OnButtonDown();
				if(btn.HasButtonClick)
				btn.OnButtonClick();
				
				return;
			}
			
			if(enums[i].isOpened)
			{
				for (int j = 0; j < enums[i].buttons.Length; j++) 
		        {
					Button btnj = enums[i].buttons[j];
			
					if((btnj.enabled && btnj.HasButtonDown || btnj.HasButtonClick) && CheckCollider(btnj))
					{
						btnj.hit = curentHit;
						if(btnj.HasButtonDown)
						btnj.OnButtonDown();
						if(btnj.HasButtonClick)
						btnj.OnButtonClick();
						
						return;
					}
				}
			}
		}	

	}
	IEnumerator Resset ()
	{
		yield return new WaitForEndOfFrame ();
		
		for (int i = 0; i < buttons.Length; i++) 
		{
			buttons[i].Resset();
		}
		for (int i = 0; i < sliders.Length; i++) 
		{
			sliders[i].button.Resset();
		}
		for (int i = 0; i < enums.Length; i++) 
		{
			enums[i].enumButton.Resset();
			if(enums[i].isOpened)
			{
				for (int j = 0; j < enums[i].buttons.Length; j++) 
		        {
					enums[i].buttons[j].Resset();
				}
				
			}
		}
	}
	void CheckButtonUp ()
	{
		curentCollider = FindCollider ();
			
		StartCoroutine(Resset ());
		
		if(!curentCollider || curentCollider == wall)
		return;

		for (int i = 0; i < circularSliders.Length; i++) 
		{
			if(circularSliders[i].OnButtonUp ())
			{
				return;
			}
		}

		for (int i = 0; i < buttons.Length; i++) 
		{
			Button btn = buttons[i];
			
			if((btn.enabled &&  btn.HasButtonUp || btn.HasButtonClick) && CheckCollider(btn))
			{
				btn.hit = curentHit;
				if(btn.HasButtonUp)
				btn.OnButtonUp();
				if(btn.HasButtonClick)
				btn.OnButtonClick();
				
				return;
			}
		}
		
		
		
		for (int i = 0; i < sliders.Length; i++) 
		{
			Button btn = sliders[i].button;
			
			if((btn.enabled &&  btn.HasButtonUp || btn.HasButtonClick) && CheckCollider(btn))
			{
				btn.hit = curentHit;
				if(btn.HasButtonUp)
				btn.OnButtonUp();
				if(btn.HasButtonClick)
				btn.OnButtonClick();
				
				return;
			}
		}
		
		for (int i = 0; i < enums.Length; i++) 
		{
			Button btn = enums[i].enumButton;
			
			if((btn.enabled &&  btn.HasButtonUp || btn.HasButtonClick) && CheckCollider(btn))
			{
				btn.hit = curentHit;
				if(btn.HasButtonUp)
				btn.OnButtonUp();
				if(btn.HasButtonClick)
				btn.OnButtonClick();
				
				return;
			}
			
			if(enums[i].isOpened)
			{
				for (int j = 0; j < enums[i].buttons.Length; j++) 
		        {
					Button btnj = enums[i].buttons[j];
			
					if((btnj.enabled && btnj.HasButtonDown || btnj.HasButtonClick) && CheckCollider(btnj))
					{
						btnj.hit = curentHit;
						if(btnj.HasButtonUp)
						btnj.OnButtonUp();
						if(btnj.HasButtonClick)
						btnj.OnButtonClick();
						
						return;
					}
				}
			}
		}

	}
	void CheckButton ()
	{
		curentCollider = FindCollider ();
			
		if(!curentCollider || curentCollider == wall)
		return;

		for (int i = 0; i < circularSliders.Length; i++) 
		{
			if(circularSliders[i].OnButton (curentHit))
			{
				return;
			}
		}

		for (int i = 0; i < buttons.Length; i++) 
		{
			Button btn = buttons[i];
			
			if(btn.enabled && CheckCollider(btn))
			{
				btn.hit = curentHit;
				if(btn.HasButtonPressed)
				btn.OnButtonPressed();
				if(!btn.isFlipFlop)
				btn.state = true;
				
				return;
			}
			else
			{
			if(!btn.isFlipFlop)
			btn.state = false;
			}
		}
		
		for (int i = 0; i < sliders.Length; i++) 
		{
			Button btn = sliders[i].button;
			
			if(btn.enabled && CheckCollider(btn))
			{
				btn.hit = curentHit;
				if(btn.HasButtonPressed)
				btn.OnButtonPressed();
				if(!btn.isFlipFlop)
				btn.state = true;	
				
				return;
			}
			else
			{
			if(!btn.isFlipFlop)
			btn.state = false;
			}
		}
		
		for (int i = 0; i < enums.Length; i++) 
		{
			Button btn = enums[i].enumButton;
			
			if(btn.enabled && CheckCollider(btn))
			{
				btn.hit = curentHit;
				if(btn.HasButtonPressed)
				btn.OnButtonPressed();
				if(!btn.isFlipFlop)
				btn.state = true;	
				
				return;
			}
			else
			{
			if(!btn.isFlipFlop)
			btn.state = false;
			}
			
			if(enums[i].isOpened)
			{
				for (int j = 0; j < enums[i].buttons.Length; j++) 
		        {
					Button btnj = enums[i].buttons[j];
			
					if(btnj.enabled && CheckCollider(btnj))
					{
						btnj.hit = curentHit;
						if(btnj.HasButtonPressed)
						btnj.OnButtonPressed();
						if(!btnj.isFlipFlop)
						btnj.state = true;	
						
						return;
					}
					else
					{
					if(!btnj.isFlipFlop)
					btnj.state = false;
					}
				}
			}
		}

	}
	
	private bool CheckCollider (Button btn)
	{
		if(curentCollider == null || curentCollider == wall)
		return false;
		Collider[] colliders = btn.GetComponentsInChildren<Collider>();
		for(int i = 0; i < colliders.Length; i ++)
		{
			if(curentCollider == colliders[i])
			return true;
		}
		return false;
	}
	private Collider FindCollider ()
	{
		Vector3 _mousePosition = GetScreenPoint ();
		Ray ray = guiCamera.ScreenPointToRay(_mousePosition);
		RaycastHit hit;
		
		if(Physics.Raycast(ray, out hit, guiCamera.farClipPlane, guiCamera.cullingMask))
		{
		curentHit = hit;
		return curentHit.collider;
		}
		return null;
	}
}
