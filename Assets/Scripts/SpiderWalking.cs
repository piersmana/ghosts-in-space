using UnityEngine;
using System.Collections;

public class SpiderWalking : MonoBehaviour {

	public float walkSpeed = 5;
	public float turnRate = 10;

	public float stepHeight = -.8f;
	public float startSlopeAngle = 40;
	public float fallToleranceHeight = 3f;

	public float knockbackThreshold = 20;

	public LayerMask layers;

	private RaycastHit leftRayHit;
	private RaycastHit rightRayHit;
	private RaycastHit conditionalRayHit;

	public bool currentlyWalking = false;

	private Rigidbody r;
	private Transform t;

	void Awake() {
		t = transform;
		r = rigidbody;
	}

	public bool StartSpiderWalk(Vector3 attachDirection) {
		if (!currentlyWalking && checkDoubleRaycast(attachDirection)) {
			currentlyWalking = true;
			r.isKinematic = true;
			StartCoroutine("AttachToSurface");
			return true;
		}

		return false;
	}

	IEnumerator AttachToSurface() {
		yield return StartCoroutine("Reposition");
		StartCoroutine("Walking");
	}

	public void StopSpiderWalk() {
		currentlyWalking = false;
		r.isKinematic = false;
		StopCoroutine("Walking");
		r.AddForce(t.up, ForceMode.Impulse);
	}

	//Add another function for transitioning into walking from freefloating
	//Possibly need to lerp into position (rather than "snapping") before giving control to player

	IEnumerator Walking() {
		float walkDirection;
		while (true) {
			walkDirection = Input.GetAxis("Horizontal");
			t.position += t.right * walkDirection * walkSpeed * Time.fixedDeltaTime;
			//Raycast down
			if (checkDoubleRaycast(-t.up)) {
			//Position and rotate based on raycast
				if (walkDirection != 0) {
					if (Physics.Raycast(t.position + stepHeight * t.up, walkDirection > 0 ? t.right : -t.right, out conditionalRayHit, .75f, layers)) {
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
	}
	
	IEnumerator Reposition() {
		Quaternion targetRot = getTargetRotation();
		Vector3 targetPos = getTargetPosition();
		
		renderer.material.color = Color.red;
		
		while (!movePlayer(targetRot, targetPos)) {
			yield return new WaitForFixedUpdate();
		}
		
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
