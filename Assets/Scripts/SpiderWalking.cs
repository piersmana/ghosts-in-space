using UnityEngine;
using System.Collections;

public class SpiderWalking : MonoBehaviour {

	public float walkSpeed = 5;
	public float turnRate = 10;

	public float stepHeight = -.8f;
	public float stepLength = 1f;
	public float startSlopeAngle = 10;
	public float fallToleranceHeight = 3f;

	public float attachSearchDistance = 1.5f;

	public float knockbackThreshold = 20;

	public LayerMask layers;

	private RaycastHit leftRayHit;
	private RaycastHit rightRayHit;
	private RaycastHit conditionalRayHit;
	private RaycastHit surfaceTarget;

	public bool currentlyWalking = false;
	public bool currentlyRepositioning = false;

	private Rigidbody r;
	private Transform t;

	void Awake() {
		t = transform;
		r = rigidbody;
	}

	public bool StartSpiderWalk() {
		if (!currentlyWalking && RaycastFromBody() && checkDoubleRaycast(-surfaceTarget.normal) != 0) { //TODO: add smart raycasting to object to stand near object edges
			r.isKinematic = true;
			StartCoroutine("SpiderWalk");
			return true;
		}

		return false;
	}

	bool RaycastFromBody() {
		Vector3 lowerorigin = t.position - .5f * t.up;
		Vector3 upperorigin = t.position + .5f * t.up;
		return Physics.Raycast(lowerorigin, -t.up * attachSearchDistance,				 	out surfaceTarget, 1f, layers)
			|| Physics.Raycast(lowerorigin,  (-t.up/2 + t.right/2) * attachSearchDistance, 	out surfaceTarget, 1f, layers)
			|| Physics.Raycast(lowerorigin,  (-t.up/2 - t.right/2) * attachSearchDistance, 	out surfaceTarget, 1f, layers)
			|| Physics.Raycast(lowerorigin,  t.right * attachSearchDistance, 				out surfaceTarget, 1f, layers)
			|| Physics.Raycast(lowerorigin, -t.right * attachSearchDistance, 				out surfaceTarget, 1f, layers)
			|| Physics.Raycast(t.position,   t.right * attachSearchDistance, 				out surfaceTarget, 1f, layers)
			|| Physics.Raycast(t.position,  -t.right * attachSearchDistance, 				out surfaceTarget, 1f, layers)
			|| Physics.Raycast(upperorigin,  t.right * attachSearchDistance, 				out surfaceTarget, 1f, layers)
			|| Physics.Raycast(upperorigin, -t.right * attachSearchDistance, 				out surfaceTarget, 1f, layers)
			|| Physics.Raycast(upperorigin,  (t.up/2 + t.right/2) * attachSearchDistance, 	out surfaceTarget, 1f, layers)
			|| Physics.Raycast(upperorigin,  (t.up/2 - t.right/2) * attachSearchDistance, 	out surfaceTarget, 1f, layers)
			|| Physics.Raycast(upperorigin,  t.up * attachSearchDistance, 					out surfaceTarget, 1f, layers);
	}

	public void StopSpiderWalk() {
		currentlyWalking = false;
		r.isKinematic = false;
		//StopCoroutine("Walking");
	}

	IEnumerator SpiderWalk() {
		float walkDirection;
		Vector3 newPosition;
		currentlyWalking = true;

		yield return StartCoroutine("Reposition");

		while (currentlyWalking) {
			walkDirection = Input.GetAxis("Horizontal");
			newPosition = t.position + t.right * walkDirection * walkSpeed * Time.fixedDeltaTime;
			if (isValidMove(newPosition)) {
				t.position = newPosition; //TODO: fix jittering caused by this positioning step, ideally eliminate it
				//Raycast down
				if (checkDoubleRaycast(-t.up) != 0) {
				//Position and rotate based on raycast
					if (walkDirection != 0) {
						if (Physics.Raycast(t.position + stepHeight * t.up, walkDirection > 0 ? t.right : -t.right, out conditionalRayHit, stepLength, layers)) {
							if (Vector3.Angle(conditionalRayHit.normal,t.up) >= startSlopeAngle) {
								checkDoubleRaycast(-conditionalRayHit.normal);
								yield return StartCoroutine("Reposition");
							}
						}
					}
					movePlayer(getTargetRotation(), getTargetPosition());
				}
				else {
					//print ("Not surface");
					//Add code here for spinning to find a foothold
					//break;
				}
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

	int checkDoubleRaycast(Vector3 direction) {
		Vector3 perpVector = .5f * Vector3.Cross(direction,t.forward);

		if (Physics.Raycast(t.position + perpVector, direction, out leftRayHit, fallToleranceHeight, layers)
				&& Physics.Raycast(t.position - perpVector, direction, out rightRayHit, fallToleranceHeight, layers))
			return 2;
		else if (Physics.Raycast(t.position, direction, out leftRayHit, fallToleranceHeight, layers)
			    && Physics.Raycast(t.position - perpVector, direction, out rightRayHit, fallToleranceHeight, layers))
			return 1;
		else if (Physics.Raycast(t.position + perpVector, direction, out leftRayHit, fallToleranceHeight, layers)
			    && Physics.Raycast(t.position, direction, out rightRayHit, fallToleranceHeight, layers))
			return -1;
		else 
			return 0;
	}

	bool movePlayer(Quaternion targetRotation, Vector3 targetPosition) {
		if (Mathf.Approximately(t.rotation.eulerAngles.z, targetRotation.eulerAngles.z) && t.position == targetPosition) {
			return true;
		}

		if (!Mathf.Approximately(t.rotation.eulerAngles.z,targetRotation.eulerAngles.z)) {
			//print (Quaternion.RotateTowards(t.rotation, targetRotation, turnRate * Time.fixedDeltaTime).eulerAngles.z);
			if (Mathf.Abs(t.rotation.eulerAngles.z - targetRotation.eulerAngles.z) < .001f)
				t.rotation = Quaternion.Euler(t.rotation.eulerAngles.x, t.rotation.eulerAngles.y, targetRotation.eulerAngles.z);
			else
				r.MoveRotation(Quaternion.RotateTowards(t.rotation, targetRotation, turnRate * Time.fixedDeltaTime));
		}
		if (t.position != targetPosition)
			r.MovePosition(Vector3.MoveTowards(t.position, targetPosition, walkSpeed * Time.fixedDeltaTime));

		return false;
	}


	//Utilities
	Quaternion getTargetRotation() {
		return Quaternion.Euler(t.rotation.x, t.rotation.y, Quaternion.FromToRotation(Vector3.up, (leftRayHit.normal + rightRayHit.normal) / 2).eulerAngles.z);
	}

	Vector3 getTargetPosition() {
		Vector3 direction = (leftRayHit.normal + rightRayHit.normal) / 2;
		Vector3 target = direction + (leftRayHit.point + rightRayHit.point)/2;
		if (!isValidMove(target)) {
			RaycastHit newTarget;
			Physics.SphereCast(target + direction, .1f, -direction, out newTarget, 2f, layers);
			target = newTarget.point;
		}
		return target;
	}

	bool isValidMove(Vector3 newPos) {
		return !Physics.CheckCapsule(newPos + t.up * .49f, newPos - t.up * .49f, .5f, layers);
	}

	//Gizmos

}
