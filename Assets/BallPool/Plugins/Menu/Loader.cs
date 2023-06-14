using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Loader : MonoBehaviour 
{
	private AsyncOperation unloadUnusedAssets;

	IEnumerator Start ()
	{
		if(MenuControllerGenerator.controller)
		{
			unloadUnusedAssets = Resources.UnloadUnusedAssets();
			StartCoroutine("UpdateLoader");
			yield return unloadUnusedAssets;
			StopCoroutine("UpdateLoader");
			MenuControllerGenerator.controller.LoaderIsDoneUnload = true;
			MenuControllerGenerator.controller.progress = 0.2f;
			yield return new WaitForEndOfFrame();

			if(MenuControllerGenerator.controller.levelName != "")
                SceneManager.LoadScene(MenuControllerGenerator.controller.levelName);
			else
                SceneManager.LoadScene(MenuControllerGenerator.controller.levelNumber);
		}
		else
		{
			yield return null;
#if UNITY_EDITOR
            if(SceneManager.sceneCountInBuildSettings >= 3)
                SceneManager.LoadScene("GameStart");
			else
				Debug.LogError("Please add the scenes (GameStart, Game and Loader) in the File/Build Settings" +
				               " as shown in the image  Assets/BallPool/TagAndLayers.png");
#endif
		}
	}
	
	IEnumerator UpdateLoader ()	
	{
		if(MenuControllerGenerator.controller.levelName != "")
		{
			while(true)
			{
				MenuControllerGenerator.controller.preloader.UpdateLoader( 0.2f*unloadUnusedAssets.progress );
				yield return null;
			}
		}
		else
		{
			while(true)
			{
				MenuControllerGenerator.controller.preloader.UpdateLoader( 0.2f*unloadUnusedAssets.progress );
				yield return null;
			}
		}
	}
}
