using UnityEngine;
using System.Collections;



[AddComponentMenu("Menu/Enum3D")]
public class Enum3D : MenuEnum 
{
	[SerializeField]
	private Vector3 step = -0.7f*Vector3.up;
	[SerializeField]
	private Vector3 move = new Vector3(1.3f, 0.7f, 0.0f);
	private Vector3[] startSqales;
	
	new protected void Awake ()
	{
		startSqales = new Vector3[buttons.Length];
		for(int i = 0; i < buttons.Length; i++)
		{
			Button btn = buttons[i];
			startSqales[i] = btn.transform.localScale;
			btn.transform.localScale = Vector3.zero;
		}
		base.Awake();
	}
	new protected IEnumerator Start ()
	{
		StartCoroutine(base.Start());
		yield return new WaitForEndOfFrame();
		SetText ();
	}
	
	protected override IEnumerator Open ()
	{
		for(int i = 0; i < buttons.Length; i++)
		{
		Button btn = buttons[i];
		btn.enabled = true;
		yield return new WaitForSeconds(OpenCloseTime/buttons.Length);
		btn.transform.localPosition += (i+1)*step + move;
		btn.transform.localScale = startSqales[i];
		
		if(closeWhenSelected)
		btn.ButtonDown += CloseWhenSelected;
		}
		inProcess = false;
	}
	protected override IEnumerator Close ()
	{
		for(int i = 0; i < buttons.Length; i++)
		{
		Button btn = buttons[i];
		if(closeWhenSelected)
		btn.ButtonDown -= CloseWhenSelected;
		btn.enabled = false;
		}
		
		for(int i = buttons.Length - 1; i >=0 ; i--)
		{
		Button btn = buttons[i];
		yield return new WaitForSeconds(OpenCloseTime/buttons.Length);
		btn.transform.localPosition -= (i+1)*step + move;
		btn.transform.localScale = Vector3.zero;
		}
		inProcess = false;
	}
	
	protected override void CloseWhenSelected (Button button)
	{
		base.CloseWhenSelected (button);
		SetText ();
	}
	void SetText ()
	{
		if(curentButton)
		{
			Button3D cb = curentButton.gameObject.GetComponent<Button3D>() as Button3D;
			Button3D btn = enumButton.gameObject.GetComponent<Button3D>() as Button3D;
			btn.text.text = cb.text.text;
		}
	}
}
