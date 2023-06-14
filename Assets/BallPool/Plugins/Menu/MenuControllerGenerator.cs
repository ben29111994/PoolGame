using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuControllerGenerator : MonoBehaviour 
{
	public static MenuController controller;
	[SerializeField]
	private MenuController controllerPrefab;
	
	void Awake ()
	{
		if(!controller)
		{
			bool checkLeyers = CheckLayers ();

			SetPhysics ();

			controller = MenuController.Instantiate(controllerPrefab) as MenuController;
			controller.name = "Controller";
			controller.isPaused = false;
			DontDestroyOnLoad(controller.gameObject);
			if(PlayerPrefs.GetInt("IsFirstTimeStarted") != 1)
			{
				PlayerPrefs.SetInt("IsFirstTimeStarted", 1);
				controller.IsFirstTimeStarted = true;
			}
			else
			controller.IsFirstTimeStarted = false;
			if(checkLeyers)
			{
#if !UNITY_EDITOR
				controller.isTouchScreen = Application.platform != RuntimePlatform.WindowsPlayer && Application.platform != RuntimePlatform.OSXPlayer && Application.platform != RuntimePlatform.LinuxPlayer;
				if(controller.isTouchScreen)
					Screen.sleepTimeout = SleepTimeout.NeverSleep;
#endif
                if(SceneManager.sceneCountInBuildSettings >= 3)
					controller.OnStart();
				else
					Debug.LogError("Please add the scenes (GameStart, Game and Loader) in the File/Build Settings" +
					               " as shown in the image  Assets/BallPool/TagAndLayers.png");
			}
			else
			{
				Debug.LogError("Please add the layers \n " +
				                 "Ball, Canvas, Wall, MainBall, Graund, GUI \n" +
				                 "as shown in the image ");

			}
		}
	}
	bool CheckLayers ()
	{
		return LayerMask.NameToLayer("Ball") > 0 && LayerMask.NameToLayer("Canvas") > 0 && LayerMask.NameToLayer("Wall") > 0
			&& LayerMask.NameToLayer("MainBall") > 0 && LayerMask.NameToLayer("Graund") > 0 && LayerMask.NameToLayer("GUI") > 0;
	}
	void SetPhysics ()
	{
		Physics.gravity = 9.81f*Vector3.down;
		Physics.bounceThreshold = 1.0f;
		Physics.sleepThreshold = 0.1f;
		Physics.defaultContactOffset = 0.01f;
		Physics.defaultSolverIterations = 10;
		Time.fixedDeltaTime = 0.01f;

		for (int i = 0; i < 32; i++)
		{
			string layerName_i = LayerMask.LayerToName(i);
			for (int j = 0; j < 32; j++)
			{
				string layerName_j = LayerMask.LayerToName(j);
				if(layerName_i == "Ball")
				{
					if(layerName_j == "Graund" || layerName_j == "MainBall" || layerName_j == "Wall" || layerName_j == "Ball")
						Physics.IgnoreLayerCollision(i,j, false);
					else
						Physics.IgnoreLayerCollision(i,j, true);
				}
				else
					if(layerName_i == "Canvas")
				{
					Physics.IgnoreLayerCollision(i,j, true);
				}
				else
				if(layerName_i == "Wall")
				{
					if(layerName_j == "MainBall" || layerName_j == "Ball")
						Physics.IgnoreLayerCollision(i,j, false);
					else
						Physics.IgnoreLayerCollision(i,j, true);
				}
				else
					if(layerName_i == "MainBall")
				{
					if(layerName_j == "Graund" || layerName_j == "Wall" || layerName_j == "Ball")
						Physics.IgnoreLayerCollision(i,j, false);
					else
						Physics.IgnoreLayerCollision(i,j, true);
				}
				if(layerName_i == "Graund")
				{
					if(layerName_j == "MainBall" || layerName_j == "Ball")
						Physics.IgnoreLayerCollision(i,j, false);
					else
						Physics.IgnoreLayerCollision(i,j, true);
				}
				else
					if(layerName_i == "GUI")
				{
					Physics.IgnoreLayerCollision(i,j, true);
				}
			}
		}

	}
}
