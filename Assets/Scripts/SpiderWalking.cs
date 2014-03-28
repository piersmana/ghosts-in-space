using UnityEngine;
using System.Collections;

public class SpiderWalking : MonoBehaviour {

	public float walkSpeed = 5;
	public float turnRate = 10;

	public float stepHeight = -.8f;
	public float stepLength = 1f;
	public float startSlopeAngle = 40;
	public float fallToleranceHeight = 3f;

	public float knockbackThreshold = 20;

	public LayerMask layers;

	private RaycastHit leftRayHit;
	private RaycastHit rightRayHit;
	private RaycastHit conditionalRayHit;

	public bool currentlyWalking = false;
	public bool currentlyRepositioning = false;

	private Rigidbody r;
	private Transform t;
	private RaycastHit surfaceTarget;

	void Awake() {
		t = transform;
		r = rigidbody;
	}

	public bool StartSpiderWalk() {
		if (!currentlyWalking && RaycastFromBody() && checkDoubleRaycast(-surfaceTarget.normal)) {
			r.isKinematic = true;
			StartCoroutine("SpiderWalk");
			return true;
		}

		return false;
	}

	bool RaycastFromBody() {
		Vector3 lowerorigin = t.position - .5f * t.up;
		Vector3 upperorigin = t.position + .5f * t.up;
		return Physics.Raycast(lowerorigin, -t.up, 					out surfaceTarget, 1f, layers)
			|| Physics.Raycast(lowerorigin, -t.up/2 + t.right/2, 	out surfaceTarget, 1f, layers)
			|| Physics.Raycast(lowerorigin, -t.up/2 - t.right/2, 	out surfaceTarget, 1f, layers)
			|| Physics.Raycast(lowerorigin,  t.right, 				out surfaceTarget, 1f, layers)
			|| Physics.Raycast(lowerorigin, -t.right, 				out surfaceTarget, 1f, layers)
			|| Physics.Raycast(t.position,   t.right, 				out surfaceTarget, 1f, layers)
			|| Physics.Raycast(t.position,  -t.right, 				out surfaceTarget, 1f, layers)
			|| Physics.Raycast(upperorigin,  t.right, 				out surfaceTarget, 1f, layers)
			|| Physics.Raycast(upperorigin, -t.right, 				out surfaceTarget, 1f, layers)
			|| Physics.Raycast(upperorigin,  t.up/2 + t.right/2, 	out surfaceTarget, 1f, layers)
			|| Physics.Raycast(upperorigin,  t.up/2 - t.right/2, 	out surfaceTarget, 1f, layers)
			|| Physics.Raycast(upperorigin, t.up, 					out surfaceTarget, 1f, layers);
	}

	public void StopSpiderWalk() {
		currentlyWalking = false;
		r.isKinematic = false;
		//StopCoroutine("Walking");
	}

	//Add another function for transitioning into walking from freefloating
	//Possibly need to lerp into position (rather than "snapping") before giving control to player

	IEnumerator SpiderWalk() {
		float walkDirection;
		currentlyWalking = true;

		yield return StartCoroutine("Reposition");

		while (currentlyWalking) {
			walkDirection = Input.GetAxis("Horizontal");
			t.position += t.right * walkDirection * walkSpeed * Time.fixedDeltaTime;
			//Raycast down
			if (checkDoubleRaycast(-t.up)) {
			//Position and rotate based on raycast
				if (walkDirection != 0) {
					if (Physics.Raycast(t.position + stepHeight * t.up, walkDirection > 0 ? t.right : -t.right, out conditionalRayHit, stepLength, layers)) {
						if (Vector3.Angle(conditionalRayHit.normal,t.up) >= startSlopeAngle) {
							checkDoubleRaycast(-conditionalRayHit.normal);
							yield return StartCoroutine("Reposition");
						}
					}
				}
				//yield return StartCoroutine("Reposition");
				movePlayer(getTargetRotation(), getTargetPosition());
			}
			else {
				//Add code here for spinning to find a foothold
				//break;
			}

			yield return new WaitForFixedUpdate();
		}

		currentlyWalking = false;
		yield return new WaitForFixedUpdate();
		r.AddForce(t.up, ForceMode.Impulse);
	}
	
	IEnumerator Reposition() {
		Quaternion targetRot = getTargetRotation();
		Vector3 targetPos = getTargetPosition();

		currentlyRepositioning = true;
		renderer.material.color = Color.red;
		
		while (currentlyRepositioning && currentlyWalking && !movePlayer(targetRot, targetPos)) {
			yield return new WaitForFixedUpdate();
		}

		currentlyRepositioning = false;
		renderer.material.color = Color.white;
	}

	bool checkDoubleRaycast(Vector3 direction) {
		Vector3 perpVector = .5f * Vector3.Cross(direction,Vector3.forward);

		return Physics.Raycast(t.position + perpVector, direction, out leftRayHit, fallToleranceHeight, layers)
			&& Physics.Raycast(t.position - perpVector, direction, out rightRayHit, fallToleranceHeight, layers);
	}

	bool movePlayer(Quaternion targetRotation, Vector3 targetPosition) {
		if (Mathf.Approximately(t.rotation.eulerAngles.z, targetRotation.eulerAngles.z) && t.position == targetPosition) {
			return true;
		}

		if (!Mathf.Approximately(t.rotation.eulerAngles.z,targetRotation.eulerAngles.z)) {
			if (t.rotation.eulerAngles.z - targetRotation.eulerAngles.z < .001f)
				t.rotation = Quaternion.Euler(t.rotation.eulerAngles.x, t.rotation.eulerAngles.y, Quaternion.RotateTowards(t.rotation, targetRotation, turnRate).eulerAngles.z);
			else
			    r.MoveRotation(Quaternion.Euler(t.rotation.eulerAngles.x, t.rotation.eulerAngles.y, Quaternion.RotateTowards(t.rotation, targetRotation, turnRate).eulerAngles.z));
		}
		if (t.position != targetPosition)
			r.MovePosition(Vector3.MoveTowards(t.position, targetPosition, walkSpeed * Time.fixedDeltaTime));

		return false;
	}


	//Utilities
	Quaternion getTargetRotation() {
		return Quaternion.FromToRotation(Vector3.up, (leftRayHit.normal + rightRayHit.normal) / 2);
	}

	Vector3 getTargetPosition() {
		Vector3 target = (leftRayHit.normal + rightRayHit.normal) / 2 + (leftRayHit.point + rightRayHit.point)/2;
		return target;
	}
}
