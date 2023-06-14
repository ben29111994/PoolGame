using UnityEngine;
using System.Collections;

public class LayerManager : MonoBehaviour 
{
	[SerializeField]
	private string layerName;
	private LayerMask cullingMask;
	[SerializeField] 
	private bool isGUICamera = false;
	void Awake () 
	{
		if(!MenuControllerGenerator.controller)
			return;
		if(GetComponent<Camera>())
		{
			cullingMask = 1 << LayerMask.NameToLayer(layerName);
			if(isGUICamera)
				GetComponent<Camera>().cullingMask = cullingMask;
			else
				GetComponent<Camera>().cullingMask = ~cullingMask;
		}
		else
		{
			gameObject.layer = LayerMask.NameToLayer(layerName);
		}
	}
}
