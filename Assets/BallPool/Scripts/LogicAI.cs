using UnityEngine;
using System.Collections;

public class LogicAI : MonoBehaviour 
{
	public TargetsAI targetsAI;
	private Vector3 targetPoint;
	private BallController targetBallController;
	private Vector3 cueBallStartPosition;

	private float holleDistance = 0.0f;
	private float ballDistance = 0.0f;
	private float allDistance = 0.0f;
	private float realHitAngle = 0.0f;

	private bool haveBallTarget = false;
	private bool haveWallTarget = false;
	private Quaternion cueRotation;
	private bool shotIsStarted = false;


	void Update ()
	{
		if(shotIsStarted && ServerController.serverController.isMyQueue)
		{
			shotIsStarted = false;
			StopCoroutine("WaitAndShotCue");
		}
	}

	public void ShotCue (CueController cueController)
	{
		StopCoroutine("WaitAndShotCue");
		if(!ServerController.serverController.isMyQueue)
		{
			StartCoroutine("WaitAndShotCue", cueController);
		}
	}

	IEnumerator WaitAndShotCue(CueController cueController)
	{
		shotIsStarted = true;

		yield return new WaitForSeconds(0.5f);

		targetPoint = Vector3.zero;
		targetBallController = null;
		haveBallTarget = false;
		haveWallTarget = false;
		float ballHitAngle = 0.0f;
		float wallHitAngle = 0.0f;

		holleDistance = 0.0f;
		ballDistance = 0.0f;
		allDistance = 40.0f;
		realHitAngle = 0.1f;
		bool haveFirsHit = false;
		if (targetsAI && targetsAI.targets != null)
		{
			foreach (Target target in targetsAI.targets) {
				foreach (BallController ballController in cueController.ballControllers) {
					if (ballController == cueController.ballController) {
						continue;
					}
					if (ballController.isBlack && !cueController.gameManager.afterOtherRemainedBlackBall) {
						continue;
					} 
					if (!cueController.gameManager.tableIsOpened && cueController.gameManager.ballType == ballController.ballType) {
						continue;
					}

					Vector3 direction = (target.transform.position - ballController.transform.position).normalized;
					Vector3 origin = ballController.transform.position + 2.0f * cueController.ballRadius * direction;
					Vector3 checkPoint = ballController.transform.position - 1.99f * cueController.ballRadius * direction;
					if (!haveBallTarget && !haveWallTarget) {
						targetPoint = checkPoint;
					}

					RaycastHit targetHit;

					if (Physics.SphereCast (origin, 0.99f * cueController.ballRadius, direction, out targetHit, cueController.wallAndBallMask | cueController.mainBallMask)) {
						if (targetHit.collider.GetComponent<BallController> ()) {
							continue;
						}

						if (targetHit.collider.GetComponent<HolleController> ()) {
							cueBallStartPosition = cueController.ballController.transform.position;

							Vector3 cueBallMoveOrient = (checkPoint - ballController.transform.position).normalized;
							Ray cueBallMoveRay = new Ray (checkPoint + 0.04f * cueController.ballRadius * cueBallMoveOrient, cueBallMoveOrient);
							Ray checkOtherBallRay = new Ray (checkPoint + 2.0f * cueController.ballRadius * Vector3.up, -Vector3.up);

							Transform StartOrMoveCube = cueController.isFirsTime ? cueController.StartCube : cueController.MoveCube;
							Vector3 point = checkPoint + cueController.ballRadius * cueBallMoveOrient;
							if (VectorOperator.sphereInCube (point, cueController.ballRadius, StartOrMoveCube) && cueController.cueFSMController.moveInTable
								&& !Physics.SphereCast (cueBallMoveRay, 0.99f * cueController.ballRadius, 2.0f * cueController.ballRadius, cueController.wallAndBallMask)
								&& !Physics.SphereCast (checkOtherBallRay, 1.1f * cueController.ballRadius, 4.0f * cueController.ballRadius, cueController.ballMask)) {
								//Can move the main (cue ) ball
								Debug.Log ("cueBallStartPosition " + cueBallStartPosition);
								haveFirsHit = true;
								cueBallStartPosition = point;
								targetBallController = ballController;
								Vector3 cueBallOrient = (checkPoint - cueBallStartPosition).normalized;
								float distance = Vector3.Distance (cueBallStartPosition, checkPoint);
								float currentHitAngle = Vector3.Dot (cueBallOrient, direction);

								CheckSetCueBallPivot (cueController, 0.5f * currentHitAngle * Vector3.down);
								holleDistance = Vector3.Distance (ballController.transform.position, target.transform.position);
								ballDistance = distance;
								allDistance = holleDistance + ballDistance;
								ballHitAngle = currentHitAngle;
								realHitAngle = ballHitAngle;
								targetPoint = checkPoint;
								haveBallTarget = true;
								targetBallController = ballController;
								break;

							} else {
					
								Vector3 cueBallOrient = (checkPoint - cueBallStartPosition).normalized;

								float distance = Vector3.Distance (cueBallStartPosition, checkPoint);

								Ray ray = new Ray (cueBallStartPosition, cueBallOrient);
								if (!Physics.SphereCast (ray, 0.99f * cueController.ballRadius, distance - 0.02f * cueController.ballRadius, cueController.wallAndBallMask)) {
									float currentHitAngle = Vector3.Dot (cueBallOrient, direction);
									if (currentHitAngle > ballHitAngle) {
										//Can throw the target ball
										CheckSetCueBallPivot (cueController, 0.5f * currentHitAngle * Vector3.down);
										holleDistance = Vector3.Distance (ballController.transform.position, target.transform.position);
										ballDistance = distance;
										allDistance = holleDistance + ballDistance;
										ballHitAngle = currentHitAngle;
										realHitAngle = ballHitAngle;
										targetPoint = checkPoint;
										haveBallTarget = true;
										targetBallController = ballController;
									} 
								} else
								if (MenuControllerGenerator.controller.AISkill == 3 && !haveBallTarget && CheckAllWalls (cueController, cueController.ballRadius, cueBallStartPosition, checkPoint, ballController, ref targetPoint, ref wallHitAngle)) {
									//Can throw the target ball using the walls
									//Debug.LogWarning("have Wall Target");
								}
							}
						}
					}
					if (MenuControllerGenerator.controller.AISkill == 1) {
						cueRotation.SetLookRotation ((targetPoint + 0.2f * cueController.ballRadius * Random.onUnitSphere - cueController.cuePivot.position).normalized);
					} else {
						cueRotation.SetLookRotation ((targetPoint - cueController.cuePivot.position).normalized);
					}
				}

				if (!cueController.gameManager.isFirstShot) {
					cueController.cuePivot.rotation = cueRotation;
				} else {
                    //0.675f
                    float y = Random.Range(0.0f, 0.675f);
                    float y2 = Random.Range(0, 5) == 0 ? 0.0f : y;
                    cueController.cuePivot.localRotation = Quaternion.Euler (0.093f,  y2, 0.0f);
				}
				if (haveFirsHit) {
					break;
				}
		
				yield return null;

			}

			if (cueController.cueFSMController.moveInTable && targetBallController) {
                cueController.ballController.transform.position += 3.0f * cueController.ballRadius * Vector3.up;
				yield return new WaitForFixedUpdate ();
                cueController.ballController.transform.position = cueBallStartPosition + 3.0f * cueController.ballRadius * Vector3.up;
				yield return new WaitForFixedUpdate ();
				cueController.ballController.transform.position = cueBallStartPosition;
				yield return new WaitForSeconds (0.5f);
				if (!cueController.gameManager.isFirstShot) {
					cueController.cuePivot.LookAt (targetPoint);
				}			
				yield return new WaitForFixedUpdate ();
			}
			yield return StartCoroutine (StretchCue (cueController));


			cueController.cueForceValue = Mathf.Clamp01 ((allDistance / 40.0f) * (1.0f / Mathf.Clamp (realHitAngle, 0.3f, 1.0f)));
			if (!cueController.gameManager.isFirstShot) {
				cueController.cueForceValue *= 0.85f;
			}
			cueController.OnDrawLinesAndSphere ();
			cueController.CheckShotCue ();
			shotIsStarted = false;
		}
	}
	IEnumerator StretchCue (CueController cueController)
	{
		float stretchTime = 1.3f;
		float time = stretchTime;
		cueController.cueDisplacement = 0.0f;
		Vector3  ballPivotLocalPosition = Random.Range(0, 2) == 0? Vector3.down:Vector3.up;
		while(time > 0.0f)
		{
			if(targetBallController)
			{
				CheckCueBallHit(cueController, ballPivotLocalPosition);
			}
			time -= stretchTime*Time.fixedDeltaTime;
			cueController.cueDisplacement = Mathf.Clamp01 ((stretchTime - time)/stretchTime)*cueController.cueMaxDisplacement;
			cueController.cueDisplacement *= Mathf.Clamp01( (allDistance/40.0f)*(1.0f/Mathf.Clamp(realHitAngle, 0.3f, 1.0f)));
			cueController.OnControlCue ();
			cueController.cueForceValue = 1.0f;
            yield return new WaitForFixedUpdate ();
        }
    }
	private void CheckSetCueBallPivot (CueController cueController, Vector3 localPosition )
	{
		if(MenuControllerGenerator.controller.AISkill == 1 || !targetBallController)
		{
			return;
		}
		Vector3 shotBack = (cueBallStartPosition - targetBallController.transform.position).normalized;
		Ray ray = new Ray(cueBallStartPosition, shotBack);
		RaycastHit hit;
		if(Physics.SphereCast(ray, 0.1f*cueController.ballRadius, out hit, 1000.0f, cueController.wallAndBallMask))
		{
			if(!hit.collider.GetComponent<HolleController>())
			{
				cueController.cueBallPivot.SetPosition(localPosition);
			}

		}
	}
	public void CheckCueBallHit (CueController cueController, Vector3 localPosition)
	{
		Ray ray = new Ray(cueController.collisionSphere.position, cueController.secondVelocity.normalized);
		RaycastHit hit;
		if(Physics.SphereCast(ray, 0.1f*cueController.ballRadius, out hit, 1000.0f, cueController.wallAndBallMask))
		{
			if(hit.collider.GetComponent<HolleController>())
			{
				CheckSetCueBallPivot(cueController, localPosition);
			}
		}
	}
	bool CheckAllWalls(CueController cueController, float ballRadius, Vector3 cueBallPosition, Vector3 targetBallHitPoint, BallController targetBall, ref Vector3 checkPoint, ref float hitAngle)
	{
		return
			CheckWall(Vector3.right, cueController, ballRadius, cueBallPosition, targetBallHitPoint, targetBall, ref checkPoint, ref hitAngle) |
				CheckWall(Vector3.left, cueController, ballRadius, cueBallPosition, targetBallHitPoint, targetBall, ref checkPoint, ref hitAngle) |
				CheckWall(Vector3.forward, cueController, ballRadius, cueBallPosition, targetBallHitPoint, targetBall, ref checkPoint, ref hitAngle) |
				CheckWall(Vector3.back, cueController, ballRadius, cueBallPosition, targetBallHitPoint, targetBall, ref checkPoint, ref hitAngle);
		
	}
	bool CheckWall(Vector3 direction, CueController cueController, float ballRadius, Vector3 cueBallPosition, Vector3 targetBallHitPoint, BallController targetBall, ref Vector3 checkPoint, ref float hitAngle)
	{
		Ray ray = new Ray(cueBallPosition, direction);
		RaycastHit hit;
		if(Physics.Raycast(ray, out hit, 1000.0f, cueController.wallMask))
		{
			float height = Vector3.Distance(hit.point, cueBallPosition) - 0.99f*cueController.ballRadius;
			float deltaHeight = Vector3.Dot( targetBallHitPoint - cueBallPosition, direction);
			Vector3 orient = VectorOperator.getPerpendicularToVector(direction, targetBallHitPoint - cueBallPosition).normalized;
			float distance = Vector3.Project(targetBallHitPoint - cueBallPosition, orient).magnitude;

			Vector3 needPoint = cueBallPosition + height*direction + (distance*height/(2.0f*height - deltaHeight))*orient;


			Vector3 checkDirection1 = (needPoint - cueBallPosition).normalized;
			Vector3 checkDirection2 = (targetBallHitPoint - needPoint).normalized;


			Ray checkPointRay = new Ray(cueBallPosition, checkDirection1);
			RaycastHit checkPointHit;
			if(Physics.SphereCast(checkPointRay, 0.99f*cueController.ballRadius, out checkPointHit, 1000.0f, cueController.wallAndBallMask))
			{
				if(checkPointHit.collider.gameObject.layer == LayerMask.NameToLayer("Wall"))
				{

					if(Vector3.Dot(checkPointHit.normal, -direction) > 0.9f)
					{

						Ray targetRay = new Ray(needPoint + 0.02f*cueController.ballRadius*checkDirection2, checkDirection2);
						RaycastHit targetHit;
						if(Physics.SphereCast(targetRay, 0.99f*cueController.ballRadius, out targetHit, 1000.0f, cueController.wallAndBallMask))
						{
							BallController currentTargetBall = targetHit.collider.GetComponent<BallController>();
							if(currentTargetBall && currentTargetBall == targetBall)
							{
								float currentHitAngle = Vector3.Dot(checkDirection2, (targetBall.transform.position - targetBallHitPoint).normalized);
								ballDistance = Vector3.Distance(cueBallPosition, needPoint) + Vector3.Distance(needPoint, targetBallHitPoint); 
								if(currentHitAngle > hitAngle )
								{
									checkPoint = needPoint;
									hitAngle = currentHitAngle;
									realHitAngle = hitAngle;
									haveWallTarget = true;
									targetBallController = targetBall;
									return true;
								}
							}
						}
					}
				}
			}
		}
		return false;
	}
}
