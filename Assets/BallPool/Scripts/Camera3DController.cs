using UnityEngine;
using System.Collections;

public class Camera3DController : MonoBehaviour 
{
	[SerializeField]
	private CircularSlider camera3dSlider;
	[SerializeField]
	private Transform rotator;
	[SerializeField]
	private float rotationSpeed = 5.0f;
	[SerializeField]
	private float minAngle = 10.0f;
	[SerializeField]
	private float maxAngle = 45.0f;

	private float rotation = 0.0f;

	[SerializeField]
	private CueController cueController;


	void Awake () 
	{
		//camera3dSlider.CircularSliderPress += Rotate3DCamera;
		rotation = rotator.localRotation.x;
	}

	void Rotate3DCamera (CircularSlider circularSlider)
	{
		MenuControllerGenerator.controller.canControlCue = false;
		transform.Rotate(Vector3.up, -rotationSpeed*camera3dSlider.displacementZ*Time.deltaTime);
		rotation -= 0.5f*rotationSpeed*camera3dSlider.displacementX*Time.deltaTime;
		rotation = Mathf.Clamp(rotation, minAngle, maxAngle);
		rotator.localRotation = Quaternion.Euler( rotation, 0.0f, 0.0f );
	}
}
