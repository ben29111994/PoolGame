using UnityEngine;
using System.Collections;

public class ReflactionChecker : MonoBehaviour
{

	void OnTriggerEnter(Collider other) 
	{
		BallController ballController = other.GetComponent<BallController>();
	}
	void OnTriggerExit(Collider other) 
	{
		BallController ballController = other.GetComponent<BallController>();
	}
}
