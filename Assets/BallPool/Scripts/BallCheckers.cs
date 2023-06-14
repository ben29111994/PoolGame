using UnityEngine;
using System.Collections;

public class BallCheckers : MonoBehaviour 
{
	[SerializeField]
	private CueController cueController;

	void Awake ()
	{
		GetComponent<Renderer>().enabled = false;
	}
	void Start ()
	{
		InvokeRepeating ("CheckBalls", 0.0f, 0.3f);
	}
	void OnDestroy ()
	{
		CancelInvoke( "CheckBalls");
	}
	void CheckBalls () 
	{
		if (!cueController) 
		{
			return;
		}
		foreach (BallController ballController in cueController.ballControllers) 
		{
			if(!ballController.ballIsOut)
			{
				if(VectorOperator.sphereInCube(ballController.transform.position, -cueController.ballRadius, transform))
				{
                    ballController.transform.position += 1.01f * (cueController.ballRadius - VectorOperator.getLocalPosition(transform, ballController.transform.position).z + 0.5f * transform.lossyScale.z) * transform.forward;
				}
			}
		}
	}
}
