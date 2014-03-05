using UnityEngine;
using System.Collections;

public class SpiderWalking : MonoBehaviour {

	public float walkSpeed = 10;
	public float turnRate = 5;

	public float knockbackThreshold = 20;

	public LayerMask layers;

	private Rigidbody r;
	private Transform t;

	void Awake() {
		t = transform;
		r = rigidbody;
	}

	void Start() {
		r.isKinematic = true;
		StartCoroutine("Walking");
	}

	//Add another function for transitioning into walking from freefloating
	//Possibly need to lerp into position (rather than "snapping") before giving control to player

	IEnumerator Walking() {
		RaycastHit leftHit;
		RaycastHit rightHit;

		while (true) {
			t.position += t.right * Input.GetAxis("Horizontal") * walkSpeed * Time.fixedDeltaTime;
			//Raycast down
			if (checkRaycastDown(out leftHit, out rightHit)) {
			//Position and rotate based on raycast
				movePlayer(leftHit, rightHit);
			}
			else {
				//Add code here for spinning to find a foothold
				break;
			}

			yield return new WaitForFixedUpdate();
		}
	}

	bool checkRaycastDown(out RaycastHit leftHitInfo, out RaycastHit rightHitInfo) {
		Ray leftRay = new Ray(t.position + .5f * t.right, -t.up);
		Ray rightRay = new Ray(t.position - .5f * t.right, -t.up);

		return Physics.Raycast(leftRay, out leftHitInfo, 2, layers) && Physics.Raycast(rightRay, out rightHitInfo, 2, layers);
	}

	void movePlayer(RaycastHit leftHitInfo, RaycastHit rightHitInfo) {
		//Sadly math is mostly found, could clean up quite a bit
		r.MoveRotation(Quaternion.Euler (0, 0, Quaternion.RotateTowards(t.rotation, Quaternion.FromToRotation(Vector3.up, (leftHitInfo.normal + rightHitInfo.normal) / 2), turnRate).eulerAngles.z));
		r.MovePosition(t.up + (leftHitInfo.point + rightHitInfo.point)/2);
	}
}
