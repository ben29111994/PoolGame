using UnityEngine;
using System.Collections;

public class TextureSlider : MonoBehaviour 
{
	[SerializeField]
	private float value = 1.0f;
	[SerializeField]
	private Transform pivot;
	[SerializeField]
	private string propertyName = "_MainTex";
	[SerializeField]
	private float startValue = 1.0f;
	private float oldValue = 0.0f;
	[SerializeField]
	private MeshRenderer meshRender;
	
	void Start () 
	{
		value = startValue;
		UpdateTexture(value);
	}

	void Update () 
	{
		if(oldValue != value)
		{
			oldValue = value;
			UpdateTexture(value);
		}
	}
	void UpdateTexture (float value)
	{
		pivot.localScale = new Vector3(value, 1.0f, 1.0f);
		meshRender.sharedMaterial.SetTextureScale(propertyName, new Vector2(value, 1.0f));
	}
	public void Resset ()
	{
		value = startValue;
	}
	public void SetValue(float value)
	{
		this.value = value;
	}
}
