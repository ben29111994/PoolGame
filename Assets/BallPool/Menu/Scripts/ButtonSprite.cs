using UnityEngine;
using System.Collections;

[AddComponentMenu("Menu/Button Sprite")]
public class ButtonSprite : Button 
{
	[SerializeField]
	private SpriteRenderer spriteRenderer;
	[SerializeField]
	private Sprite spriteUp;
	[SerializeField]
	private Sprite spriteDown;

	new protected void Awake ()
	{
		base.Awake();
	}

	protected override void SetState (bool state)
	{
		if(spriteRenderer)
		spriteRenderer.sprite = state? spriteDown : spriteUp;
	}
}
