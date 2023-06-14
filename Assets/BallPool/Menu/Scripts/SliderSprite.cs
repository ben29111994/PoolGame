using UnityEngine;
using System.Collections;

[AddComponentMenu("Menu/Slider Sprite")]
[RequireComponent(typeof(ButtonSprite))]
public class SliderSprite : MySlider 
{
	private Vector3 strPosition;
	[SerializeField]
	private bool hideSliderObjectThenUp = false;
	[SerializeField]
	private bool ressetValue = false;
	[SerializeField]
	private float valueToResset = 0.0f;

	new void Awake ()
	{
		base.Awake();
		strPosition = slideObject.localPosition;
        if (hideSliderObjectThenUp)
            slideObject.GetComponent<Renderer>().enabled = canMove;
    }
	new void Update ()
	{
		base.Update();
		if(hideSliderObjectThenUp)
            slideObject.GetComponent<Renderer>().enabled = canMove;
        if (menu.GetButtonUp())
		{
			if(ressetValue)
			{
				slideObject.position = strPosition;
				Value = valueToResset;
			}
		}
	}
}
