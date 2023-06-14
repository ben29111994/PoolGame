using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuController : MonoBehaviour 
{
    public bool useNetwork = true;
    public float shotTime = 30.0f;
    [System.NonSerialized]
    public bool playWithAI = false;
    [System.NonSerialized]
    public bool hotseat = false;
    [System.NonSerialized]
    public int AISkill = 1;
    [System.NonSerialized]
    public bool canRotateCue = true;
    public delegate void LoadLevelHandler(MenuController menuController);
    public event LoadLevelHandler OnLoadLevel;
    public GameObject masterServerGUI;
    [SerializeField]
    private GameObject masterServerGUIPrefab;
    public Preloader preloader;
	public string loader = "Loader";
	public string game = "Game";
	public Camera loaderCamera;
	public Camera guiCamera;
    [System.NonSerialized]
    public bool IsFirstTimeStarted = false;
    [System.NonSerialized]
    public bool isPaused = false;
    [System.NonSerialized]
	public string levelName = "";
	[System.NonSerialized]
	public int levelNumber = -1;
	
	[System.NonSerialized]
	public bool LoaderIsDoneUnload = false;

	[System.NonSerialized]
	public float progress;

	[System.NonSerialized]
	public bool canControlCue = true;

    [System.NonSerialized]
    public bool loadIsComplite = false;
    public bool isTouchScreen = false;
    public GameObject title;

    public void Pause(bool pause)
    {
        isPaused = pause;
        if (isPaused)
            Time.timeScale = 0.0f;
        else
            Time.timeScale = 1.0f;
    }

    public void OnGoBack()
    {
        MenuControllerGenerator.controller.LoadLevel("GameStart");
        StartCoroutine(WaitAndActivateMasterServerGUI());
    }
    IEnumerator WaitAndActivateMasterServerGUI()
    {
        while (!loadIsComplite)
        {
            yield return null;
        }
        masterServerGUI.SetActive(true);
    }
    public void OnStart()
    {
        LoadLevel(game);
    }
    public Menu CreatedMenuFromResources(string path)
    {
        Menu menuRes = Resources.Load(path, typeof(Menu)) as Menu;
        return CreatedMenu(menuRes);
    }
    public Menu CreatedMenu(Menu menuRes)
    {
        Menu menu = Menu.Instantiate(menuRes) as Menu;
        menu.guiCamera = guiCamera;
        if (guiCamera)
            menu.transform.parent = guiCamera.transform;
        return menu;
    }
    public void LoadLevel (string levelName)
	{
		this.levelName = levelName;
		this.levelNumber = -1;
		StartCoroutine( Load () );
	}
    public void LoadLevel(int levelNumber)
    {
        this.levelName = "";
        this.levelNumber = levelNumber;
        StartCoroutine(Load());
    }
    IEnumerator Load ()
	{
		preloader.SetState(true);

		if(OnLoadLevel != null)
			OnLoadLevel(this);
		loadIsComplite = false;
		if(loaderCamera)
		loaderCamera.enabled = true;
		
		if(guiCamera)
		guiCamera.enabled = false;
		
		LoaderIsDoneUnload = false;
		progress = 0.0f;
		preloader.UpdateLoader( 0.0f );

        SceneManager.LoadScene(loader);

        yield return StartCoroutine(UpdateLoader ());
		yield return new WaitForEndOfFrame();
		
		preloader.UpdateLoader(1.0f);
	
		yield return null;
		loadIsComplite = true;
        title.SetActive(false);
    }
	IEnumerator UpdateLoader ()	
	{
        while(SceneManager.GetActiveScene().name != loader)
		{
			yield return null;
		}
		if(levelName != "")
		{
            while(levelName != SceneManager.GetActiveScene().name)
			{
				if(LoaderIsDoneUnload)
				{
					progress = 0.8f;
					preloader.UpdateLoader( 0.8f );

				}
				yield return null;
			}
		}
		else
		{
            while(levelNumber != SceneManager.GetActiveScene().buildIndex)
			{
				if(LoaderIsDoneUnload)
				{
					progress = 0.8f;
					preloader.UpdateLoader( 0.8f );
				}
					yield return null;
			}
		}
        if(SceneManager.GetActiveScene().buildIndex != 0)
		{
			if(guiCamera)
				guiCamera.enabled = true;
			
			if(loaderCamera)
				loaderCamera.enabled = false;

			preloader.SetState(false);
		}

	}
}
