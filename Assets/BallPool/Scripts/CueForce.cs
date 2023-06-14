using UnityEngine;
using System.Collections;

public class CueForce : MonoBehaviour 
{
	public SliderSprite slider;
	[SerializeField]
	private MeshRenderer meshRender;
	public float cueForceValue = 0.0f;
	[SerializeField]
	private float startValue = 1.0f;
	[SerializeField]
	private CueController cueController;
	[SerializeField]
	private bool disableIfNotTouchScreen = true;

	void Awake ()
	{
		if(MenuControllerGenerator.controller)
		{
			//if(disableIfNotTouchScreen && !MenuControllerGenerator.controller.isTouchScreen)
			//{
			//	transform.parent.gameObject.SetActive(false);
			//	return;
			//}
			//else
			//{
				slider.MoveSlider += MoveForceSlider;
				slider.CheckSlider += MoveForceSlider;
			//}
		}
	}
	void Start ()
	{
		slider.Value = startValue;
		MoveForceSlider (slider);
		cueController.inTouchForceSlider = false;
		cueController.cueForceisActive = false;
	}

	void MoveForceSlider (MySlider slider)
	{
		if(!cueController.allIsSleeping)
			return;
        if (ServerController.serverController && !(ServerController.serverController.isMyQueue || MenuControllerGenerator.controller.hotseat))
            return;
        if (!disableIfNotTouchScreen) 
		{
			MenuControllerGenerator.controller.canControlCue = false;
		} else
		{
			MenuControllerGenerator.controller.canRotateCue = false;
			cueController.inTouchForceSlider = true;
			cueController.cueForceisActive = true;

			cueController.cueDisplacement = cueController.cueMaxDisplacement * cueForceValue;
		}
		cueForceValue = slider.Value;
		transform.localScale = new Vector3(slider.Value/slider.maxValue, 1.0f, 1.0f);
		meshRender.sharedMaterial.SetTextureScale("_MainTex", new Vector2(1.0f, slider.Value/slider.maxValue));
        cueController.tutorial2.SetActive(false);
	}

	public void Resset ()
	{
        try
        {
            //StartCoroutine(WaitAndRessetValue());
        }
        catch { }
	}
	IEnumerator WaitAndRessetValue ()
	{
		yield return new WaitForEndOfFrame();
		slider.Value = startValue;
		slider.Resset();
		transform.localScale = new Vector3(slider.Value/slider.maxValue, 1.0f, 1.0f);
		meshRender.sharedMaterial.SetTextureScale("_MainTex", new Vector2(1.0f, slider.Value/slider.maxValue));
	}
}
