using UnityEngine;
using System.Collections;

public class Debuger : MonoBehaviour {

	public static string debugSTR;

	void OnGUI () {
		GUILayout.Label ("");
		GUILayout.Label ("");
		GUILayout.Label ("");
		GUILayout.Label ("");
		GUILayout.Label ("");
		GUILayout.Label ("");

		GUILayout.Label (debugSTR);
	}
	public static void DebugOnScreen (string str)
	{
		debugSTR += "\n" + str;
	}
}
