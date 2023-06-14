using UnityEngine;
using System.Collections;

public class Preloader : MonoBehaviour 
{
	[System.NonSerialized]
	public bool isDone = false;
	[SerializeField]
	private Renderer splash;
	[System.NonSerialized]
	public MenuController controller;
	public void SetState ( bool state )
	{
		isDone = !state;
        splash.enabled = state;
    }
	public void UpdateLoader (float time)
	{
	
	}
}
