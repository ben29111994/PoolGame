using UnityEngine;
using System.Collections;

public class VectorOperator
{
	public static float getRayDistance (Ray ray, Vector3 point)
	{
		return getRayNormal(ray, point).magnitude;
	}
	public static Vector3 getRayPoint (Ray ray, Vector3 point)
	{
		return ray.origin + Vector3.Project(point - ray.origin, ray.direction);
	}
	public static Vector3 getRayNormal (Ray ray, Vector3 point)
	{
		Vector3 origin = ray.origin;
		Vector3 direction = ray.direction;
		Vector3 delta = point - origin;
		Vector3 project = Vector3.Project(delta, direction);
		Vector3 normal = delta- project;
		return normal;
	}
	public static Vector3 getPerpendicularToVector(Vector3 direction, Vector3 origin)
	{
		return origin - Vector3.Project(origin, direction);
	}
	public static Vector3 getPerpendicularXZ(Vector3 normal)
	{
		return new Vector3(-normal.z, 0.0f, normal.x);
	}
	public static Vector3 getProjectXZ (Vector3 currentVector, bool saveLength)
	{
		Vector3 projectXZ = new Vector3(currentVector.x, 0.0f, currentVector.z);
		return saveLength? currentVector.magnitude*projectXZ.normalized: projectXZ;
	}
	public static Vector3 getBallVelocity (float radius, Vector3 currentVelocity, Vector3 currentPosition, LayerMask hitCheckMask, float maxDistance, 
	                                       ref Vector3 hitBallVelocity, ref Collider hitCollider, float rotationDisplacementX)
	{
		Vector3 newVelocity = Vector3.zero;

		Ray ray = new Ray(currentPosition - radius*currentVelocity.normalized, currentVelocity.normalized);
		RaycastHit hit;
		hitCollider = null;
		if(Physics.SphereCast(ray, radius, out hit, maxDistance, hitCheckMask))
		{
		    hitCollider = hit.collider;

            if(hitCollider.GetComponent<BallController>())
			{
				Vector3 perependicular = getPerpendicularToVector(hit.normal, currentVelocity);
				newVelocity = Vector3.Project(currentVelocity, perependicular);
				hitBallVelocity = -( newVelocity - currentVelocity ).magnitude*hit.normal;
				hitBallVelocity = getProjectXZ( hitBallVelocity , true);
			}
			else
			{			
				newVelocity = currentVelocity - 2.0f*Vector3.Project(currentVelocity, -hit.normal);
				hitBallVelocity = Vector3.zero;	

				Vector3 perependicular = getPerpendicularXZ(hit.normal);
				
				float tangets = Vector3.Dot(perependicular, currentVelocity.normalized);
				if(tangets < 0.0f)
				{
					perependicular = -perependicular;
				}
				float xFactor =  rotationDisplacementX >= 0.0f? (tangets >= 0.0? 0.35f: -1.0f):(tangets >= 0.0? 1.0f: -0.35f);

				newVelocity = newVelocity.magnitude * (xFactor * rotationDisplacementX *(perependicular.normalized - newVelocity.normalized ) + newVelocity.normalized).normalized;
			}

			string collisionName = LayerMask.LayerToName(hitCollider.gameObject.layer);
			if(collisionName == "Wall")
			newVelocity = getProjectXZ( newVelocity , true );
		}

		return newVelocity;
	}
	public static Vector3 getBallVelocity (Vector3 currentVelocity, Vector3 hitNormal)
	{
		Vector3 perependicular = getPerpendicularToVector(hitNormal, currentVelocity);
		return Vector3.Project(currentVelocity, perependicular);
	}
	public static Vector3 getBallWallVelocity (float radius, Vector3 currentVelocity, Vector3 currentPosition, LayerMask hitCheckMask, float maxDistance, Vector3 angularVelocityY)
	{
		Vector3 newVelocity = Vector3.zero;
		
		Ray ray = new Ray(currentPosition, currentVelocity.normalized);
		RaycastHit hit;
		if(Physics.SphereCast(ray, radius, out hit, maxDistance, hitCheckMask))
		{
			Vector3 project = Vector3.Project(currentVelocity, -hit.normal);
			newVelocity = currentVelocity - 2.0f*project;
			newVelocity = getProjectXZ( newVelocity , true );

			if(angularVelocityY.magnitude != 0.0f)
			{
				Vector3 forceDirection = -Vector3.Cross(angularVelocityY.normalized , VectorOperator.getProjectXZ( -hit.normal, false));
				newVelocity = newVelocity.magnitude*(newVelocity + 0.009f * project.magnitude*angularVelocityY.magnitude*forceDirection.normalized).normalized;
			}
		}
		
		return newVelocity;
	}
	public static void KeepInCube (Transform cube, float radius, Transform sphere)
	{
		if(!sphereInCube(sphere.position, radius, cube))
		{
			Vector3 _ballLocalPosition = VectorOperator.getLocalPosition(cube, sphere.position);
			float displacementX1 = _ballLocalPosition.x + radius - 0.5f*cube.localScale.x;
			float displacementX2 = _ballLocalPosition.x - radius + 0.5f*cube.localScale.x;

			if( displacementX1 > 0.0f )
			{
				sphere.position -= displacementX1*cube.right;
			}
			else
				if( displacementX2 < 0.0f )
			{
				sphere.position -= displacementX2*cube.right;
			}

			float displacementZ1 = _ballLocalPosition.z + radius - 0.5f*cube.localScale.z;
			float displacementZ2 = _ballLocalPosition.z - radius + 0.5f*cube.localScale.z;

			if( displacementZ1 > 0.0f )
			{
				sphere.position -= displacementZ1*cube.forward;
			}
			else
				if( displacementZ2 < 0.0f )
			{
				sphere.position -= displacementZ2*cube.forward;
			}
		}
	}
	public static void MoveBallInQuad (Transform cube, float radius, Vector3 hitPoint, ref Vector3 position)
	{
		if(sphereInCube(hitPoint, radius, cube))
		{
			position = hitPoint;
		}
		else
		{
			Vector3 _ballMoveLocalPosition = VectorOperator.getLocalPosition(cube, hitPoint);
			
			if( !(Mathf.Abs( _ballMoveLocalPosition.x ) <= -radius + 0.5f*cube.localScale.x) )
			{
				if(Mathf.Abs( _ballMoveLocalPosition.z ) <= -radius + 0.5f*cube.localScale.z)
				{
					Vector3 ballMovePositionX = _ballMoveLocalPosition.x > 0.0f? VectorOperator.getWordPosition(cube, (-radius + 0.5f*cube.localScale.x)*Vector3.right ):
						VectorOperator.getWordPosition(cube, (radius - 0.5f*cube.localScale.x)*Vector3.right);
					position = new Vector3(ballMovePositionX.x, position.y, hitPoint.z);
				}
			}
			else
			{
				if(Mathf.Abs( _ballMoveLocalPosition.x ) <= -radius + 0.5f*cube.localScale.x)
				{
					Vector3 ballMovePositionZ = _ballMoveLocalPosition.z > 0.0f? VectorOperator.getWordPosition(cube, (-radius + 0.5f*cube.localScale.z)*Vector3.forward ):
						VectorOperator.getWordPosition(cube, (radius - 0.5f*cube.localScale.z)*Vector3.forward);
					position = new Vector3(hitPoint.x, position.y, ballMovePositionZ.z);
				}
			}
		}
		
		
	}
	public static Vector3 getLocalPosition(Transform parent, Vector3 wordPosition)
	{
		Vector3 localPosition = wordPosition - parent.position;
		return new Vector3( Vector3.Dot(localPosition, parent.right),
		                    Vector3.Dot(localPosition, parent.up),
		                    Vector3.Dot(localPosition, parent.forward)
		                   );
	}
	public static Vector3 getWordPosition(Transform parent, Vector3 localPosition)
	{
		return parent.position + localPosition.x*parent.right + localPosition.y*parent.up + localPosition.z*parent.forward;
	}
	public static void keepToFace(Transform objectTokeep, Transform keeper, ref Vector3 normal, ref Vector3 point, Vector3 wallNormal, Vector3 wallPoint, Vector3 wordUp)
	{
		keeper.position = point;
		keeper.LookAt(point + normal, wordUp);
		Transform parent = objectTokeep.parent;
		objectTokeep.parent = keeper;
		keeper.position = wallPoint;
		keeper.LookAt(wallPoint - wallNormal, wordUp);
		objectTokeep.parent = parent;
		point = wallPoint;
		normal = -wallNormal;
	}
	public static float localCylindricalAngle(Transform Parent, Vector3 Child)
	{
		Vector3 localPosition = Child - Parent.position;
		float dot = Vector3.Dot(localPosition.normalized, Parent.forward);
		float angle;
		if(Vector3.Dot(localPosition.normalized, Parent.up) >= 0)
		angle = (180/Mathf.PI)* Mathf.Acos( dot );
		else
		angle = -(180/Mathf.PI)* Mathf.Acos( dot );
		return angle;
	}
	public static Vector3 localCylindricalPosition(Transform Parent, Vector3 Child)
	{
		Vector3 localPosition = Child - Parent.position;
		float radius = Mathf.Sqrt( Mathf.Pow( Vector3.Dot(localPosition, Parent.right) , 2) + Mathf.Pow( Vector3.Dot(localPosition, Parent.forward) , 2));
		float angle = localCylindricalAngle(Parent, Child);
		float height = Vector3.Dot(localPosition, Parent.up);
		return new Vector3( radius, angle, height );
	}
	public static bool pointInAngle(Vector3 point, Transform cylindr, float min_angle, float max_angle)
	{
		float localCylindricalAng = localCylindricalAngle(cylindr, point);
		bool in_angle = localCylindricalAng >= min_angle && localCylindricalAng <= max_angle;
		return in_angle;
	}
	
	public static bool sphereInCube (Vector3 point, float radius,  Transform cube)
	{
		Vector3 localPos = getLocalPosition(cube, point);
		 Vector3 cubeSqale = cube.lossyScale;
		return Mathf.Abs(localPos.x) + radius <= 0.5f*cubeSqale.x && Mathf.Abs(localPos.y) + radius <= 0.5f*cubeSqale.y && Mathf.Abs(localPos.z) + radius <= 0.5f*cubeSqale.z;
	}
	public static bool sphereInCylindr (Vector3 point, float radius,  Transform cylindr, float min_angle, float max_angle)
	{
		Vector3 localCylindricalPos = localCylindricalPosition(cylindr, point);
		bool in_radius = Mathf.Abs(localCylindricalPos.x) - radius <= 0.5f*cylindr.lossyScale.x;
		bool in_angle = localCylindricalPos.y >= min_angle && localCylindricalPos.y <= max_angle;
		bool in_height = Mathf.Abs(localCylindricalPos.z) - radius <= cylindr.lossyScale.y;
		return in_radius && in_angle && in_height;
	}
	public static bool cubeInCube (Transform cube1, Transform cube2)
	{
		bool check;
		
		Vector3 pos1 = cube1.position;
		Vector3 pos2 = cube2.position;
				
		Vector3 localCube1Pos1 = 0.5f*cube1.lossyScale.x * cube1.right + 0.5f*cube1.lossyScale.y*cube1.up + 0.5f*cube1.lossyScale.x*cube1.forward;
		Vector3 localCube2Pos1 = 0.5f*cube2.lossyScale.x * cube2.right + 0.5f*cube2.lossyScale.y*cube2.up + 0.5f*cube2.lossyScale.x*cube2.forward;
		
		float localCube1Scale = localCube1Pos1.magnitude;
		float localCube2Scale = localCube2Pos1.magnitude;
		
		if(Vector3.Distance(pos1, pos2) > localCube1Scale + localCube2Scale)
		return false;
		
		Vector3 localCube1Pos2 = -0.5f*cube1.lossyScale.x * cube1.right + 0.5f*cube1.lossyScale.y*cube1.up + 0.5f*cube1.lossyScale.x*cube1.forward;
		Vector3 localCube1Pos3 = 0.5f*cube1.lossyScale.x * cube1.right - 0.5f*cube1.lossyScale.y*cube1.up + 0.5f*cube1.lossyScale.x*cube1.forward;
		Vector3 localCube1Pos4 = -0.5f*cube1.lossyScale.x * cube1.right - 0.5f*cube1.lossyScale.y*cube1.up + 0.5f*cube1.lossyScale.x*cube1.forward;
		
		Vector3 localCube2Pos2 = -0.5f*cube2.lossyScale.x * cube2.right + 0.5f*cube2.lossyScale.y*cube2.up + 0.5f*cube2.lossyScale.x*cube2.forward;
		Vector3 localCube2Pos3 = 0.5f*cube2.lossyScale.x * cube2.right - 0.5f*cube2.lossyScale.y*cube2.up + 0.5f*cube2.lossyScale.x*cube2.forward;
		Vector3 localCube2Pos4 = -0.5f*cube2.lossyScale.x * cube2.right - 0.5f*cube2.lossyScale.y*cube2.up + 0.5f*cube2.lossyScale.x*cube2.forward;
		
		
		Vector3 cube1Pos1 = pos1 + localCube1Pos1;
		Vector3 cube1Pos2 = pos1 - localCube1Pos1;
		Vector3 cube1Pos3 = pos1 + localCube1Pos2;
		Vector3 cube1Pos4 = pos1 - localCube1Pos2;
		Vector3 cube1Pos5 = pos1 + localCube1Pos3;
		Vector3 cube1Pos6 = pos1 - localCube1Pos3;
		Vector3 cube1Pos7 = pos1 + localCube1Pos4;
		Vector3 cube1Pos8 = pos1 - localCube1Pos4;
		
		Vector3 cube2Pos1 = pos2 + localCube2Pos1;
		Vector3 cube2Pos2 = pos2 - localCube2Pos1;
		Vector3 cube2Pos3 = pos2 + localCube2Pos2;
		Vector3 cube2Pos4 = pos2 - localCube2Pos2;
		Vector3 cube2Pos5 = pos2 + localCube2Pos3;
		Vector3 cube2Pos6 = pos2 - localCube2Pos3;
		Vector3 cube2Pos7 = pos2 + localCube2Pos4;
		Vector3 cube2Pos8 = pos2 - localCube2Pos4;
		
		check = sphereInCube(cube1Pos1, 0 , cube2) ||
			    sphereInCube(cube1Pos2, 0 , cube2) ||
				sphereInCube(cube1Pos3, 0 , cube2) ||
				sphereInCube(cube1Pos4, 0 , cube2) ||
				sphereInCube(cube1Pos5, 0 , cube2) ||
				sphereInCube(cube1Pos6, 0 , cube2) ||
				sphereInCube(cube1Pos7, 0 , cube2) ||
				sphereInCube(cube1Pos8, 0 , cube2) ||
				
				sphereInCube(cube2Pos1, 0 , cube1) ||
				sphereInCube(cube2Pos2, 0 , cube1) ||
				sphereInCube(cube2Pos3, 0 , cube1) ||
				sphereInCube(cube2Pos4, 0 , cube1) ||
				sphereInCube(cube2Pos5, 0 , cube1) ||
				sphereInCube(cube2Pos6, 0 , cube1) ||
				sphereInCube(cube2Pos7, 0 , cube1) ||
				sphereInCube(cube2Pos8, 0 , cube1);
		
		return check;
	}
}
