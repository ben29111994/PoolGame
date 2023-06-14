using UnityEngine;
using System.Collections;

[AddComponentMenu("Menu/MenuTouch")]
public class MenuTouch : Menu 
{
    public override bool GetButton ()
	{
		return Input.GetMouseButton(0);
	}
	public override bool GetButtonDown ()
	{
		return Input.GetMouseButtonDown(0);
	}
	public override bool GetButtonUp ()
	{
		return Input.GetMouseButtonUp(0);
	}
	public override Vector3 GetPosition ()
	{
		Ray ray = guiCamera.ScreenPointToRay(GetScreenPoint ());
		return ray.origin;
	}
	public override Vector3 GetScreenPoint ()
	{
		return Input.mousePosition;
	}
}
