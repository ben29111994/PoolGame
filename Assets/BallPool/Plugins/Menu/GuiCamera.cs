using UnityEngine;
using System.Collections;

public class GuiCamera : MonoBehaviour 
{
	void Awake () 
	{
		if(MenuControllerGenerator.controller)
		MenuControllerGenerator.controller.guiCamera = GetComponent<Camera>();
	}
}
