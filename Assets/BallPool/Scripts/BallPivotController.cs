using UnityEngine;
using System.Collections;

public class BallPivotController : MonoBehaviour 
{
	[SerializeField]
	private CircularSlider circularSlider;
	public float radius = 0.75f;
	private Vector3 strPosition = Vector3.zero;
	[SerializeField]
	private CueController cueController;

	

	void Start ()
	{
		circularSlider.CircularSliderPress += SlideBallPivot;
		strPosition = transform.position;
	}
	
	void SlideBallPivot (CircularSlider circularSlider)
	{
		//if(ServerController.serverController && !(ServerController.serverController.isMyQueue || MenuControllerGenerator.controller.hotseat))
		//	return;

		MenuControllerGenerator.controller.canControlCue = false;
		transform.localPosition = new Vector3(-circularSlider.displacementZ, circularSlider.displacementX, 0.0f);
		float distance = Vector3.Distance(transform.position, strPosition);
		if(distance > radius)
		{
			transform.position -= (distance - radius)*(transform.position - strPosition).normalized;
		}
	}
	public void SetPosition (Vector3 localPosition)
	{
        float x = localPosition.x;
        float y = localPosition.y;
        if (Mathf.Abs(x) < 0.15f)
        {
            x = 0.0f;
        }
        if (Mathf.Abs(y) < 0.15f)
        {
            y = 0.0f;
        }
        transform.localPosition = radius*(new Vector3(x,y, localPosition.z));
	}
	public void Reset ()
	{
		transform.position = strPosition;
	}
}
