using UnityEngine;
using System.Collections;

public class TargetsAI : MonoBehaviour
{
	public Target[] targets;
	void Awake ()
	{
		if(!MenuControllerGenerator.controller)
		{
			return;
		}
		if(!MenuControllerGenerator.controller.playWithAI)
		{
			Destroy(gameObject);
			return;
		}
		ServerController.logicAI.targetsAI = this;
	}
}
