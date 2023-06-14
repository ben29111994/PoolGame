using UnityEngine;
using System.Collections;


public class CircularSlider : MonoBehaviour 
{
	[SerializeField]
	private Menu menu;
	[SerializeField]
	private Transform touch;
	[SerializeField]
	public float radius = 1.0f;
	public float displacementZ;
	public float displacementX;
	[SerializeField]
	private Camera cueCamera;
	private Vector3 strPosition;
	private bool isSelected = false;

	public delegate void OnCircularSlider (CircularSlider circularSlider);
	public event OnCircularSlider CircularSliderPress;

    public CueController cueController;

    void Awake ()
	{
		touch.GetComponent<Renderer>().enabled = false;
	}
	void Start ()
	{
		strPosition = touch.position;
	}
	public bool OnButtonDown (RaycastHit curentHit)
	{
		if(curentHit.collider == GetComponent<Collider>()/* && Vector3.Distance(curentHit.point, strPosition) < radius*/)
		{
			//MenuControllerGenerator.controller.canRotateCue = false;
			isSelected = true;
			touch.GetComponent<Renderer>().enabled = true;
			return true;
		}
        else
        {
            this.gameObject.SetActive(false);
        }
		return false;
	}
	public bool OnButtonUp ()
	{
        MenuControllerGenerator.controller.canRotateCue = true;
        if (!isSelected)
			return false;

		isSelected = false;
		touch.position = strPosition;
		touch.GetComponent<Renderer>().enabled = false;
		displacementX = 0.0f;
		displacementZ = 0.0f;
		return true;
	}
	public bool OnButton (RaycastHit curentHit)
	{
		if(!isSelected)
			return false;

			if(curentHit.collider == GetComponent<Collider>() && Vector3.Distance(curentHit.point, strPosition) < radius)
			{
				touch.position = curentHit.point;
			}
			else
			{
				Vector3 displacement = curentHit.point - strPosition;
				
				touch.position = radius*(displacement - Vector3.Project(displacement, cueCamera.transform.forward)).normalized + strPosition;
			}

        displacementX = Mathf.Clamp((1.0f / radius) * VectorOperator.getLocalPosition(transform, touch.position).x, -1.0f, 1.0f);
        displacementZ = Mathf.Clamp((1.0f / radius) * VectorOperator.getLocalPosition(transform, touch.position).z, -1.0f, 1.0f);

        if ( CircularSliderPress != null)
				CircularSliderPress(this);

		return true;
	}
}
