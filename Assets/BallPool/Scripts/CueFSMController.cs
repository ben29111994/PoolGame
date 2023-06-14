using UnityEngine;
using System.Collections;

public class CueFSMController : MonoBehaviour 
{
	[SerializeField]
	private CueController cueController;

	public bool moveInTable = false;
	public bool moveBall = false;
	public bool inMove = false;

	public void setMoveInTable ()
	{
		moveInTable = true;
		moveBall = false;
		inMove = false;
	}
	public void setMoveBall ()
	{
		moveInTable = false;
		moveBall = true;
		inMove = false;
	}
	public void setInMove ()
	{
		moveInTable = false;
		moveBall = false;
		inMove = true;
	}
	void Awake ()
	{
		if (ServerController.serverController) 
		{
			enabled = false;
		}
	}
	void Update () 
	{
		if(!MenuControllerGenerator.controller)
		{
			return;
		}
        //if(MenuControllerGenerator.controller.playWithAI || MenuControllerGenerator.controller.hotseat)
        //{
        //	cueController.networkAllIsSleeping = true;
        //}
        if (moveInTable)
            cueController.MoveInTable();
        else
            if (moveBall)
            cueController.MoveBall();
        else
			if(inMove)
				cueController.InMove();
		if(cueController.allIsSleeping)
			cueController.OnControlCue();
	}
}
