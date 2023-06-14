using UnityEngine;
using System.Collections;

[AddComponentMenu("Menu/Button3D")]
public class Button3D : Button 
{
	public TextMesh text;
	
	[SerializeField]
	protected Renderer ClickedObject;
	[SerializeField]
	protected Renderer DefaultObject;
	
	new protected void Awake ()
	{
		base.Awake();
		if(text)
		text.text = ButtonName;
	}
	protected override void SetState (bool state)
	{
		if(ClickedObject)
		{
			DefaultObject.enabled = !state;
		}
		if(DefaultObject)
		{
			ClickedObject.enabled = state;
		}
	}
}
