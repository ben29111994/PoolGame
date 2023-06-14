using UnityEngine;
using System.Collections;

public class GuiObjects : MonoBehaviour
{
	[SerializeField]
	private float editorScreenWidth = 960;
	[SerializeField]
	private float editorScreenHeight = 640;
	
	void Awake () 
	{
        //editorScreenHeight = Display.main.systemHeight;
        //editorScreenWidth = Display.main.systemWidth;
        //Debug.Log(Display.main.systemWidth + " + " + Display.main.systemHeight);
        if (!MenuControllerGenerator.controller)
			return;
		float k = ((float)Screen.width/(float)Screen.height)/(editorScreenWidth/editorScreenHeight);
		
		for (int i = 0; i < transform.childCount; i++) 
		{
			Transform child = transform.GetChild(i);
			Vector3 localPosition = child.localPosition;
		    child.localPosition = new Vector3(k*localPosition.x, localPosition.y, localPosition.z);
		}
		foreach (Transform item in GetComponentsInChildren<Transform>())
		{
			item.gameObject.layer = LayerMask.NameToLayer("GUI");
		}
	}
	
	void Update () 
	{
	
	}
}
