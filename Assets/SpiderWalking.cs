using UnityEngine;
using System.Collections;

public class SpiderWalking : MonoBehaviour {

	public float walkSpeed = 5;
	public float turnRate = 10;

	public float stepHeight = .2f;
	public float fallToleranceHeight = 4f;

	public float knockbackThreshold = 20;

	public LayerMask layers;

	private RaycastHit leftRayHit;
	private RaycastHit rightRayHit;
	private RaycastHit conditionalRayHit;

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
		float walkDirection;
		while (true) {
			walkDirection = Input.GetAxis("Horizontal");
			t.position += t.right * walkDirection * walkSpeed * Time.fixedDeltaTime;
			//Raycast down
			if (checkRaycastDown()) {
			//Position and rotate based on raycast
				if (walkDirection > 0) {
					if (Physics.Raycast(t.position + stepHeight * t.up, t.right, out conditionalRayHit, .51f, layers)) {
						rightRayHit = conditionalRayHit;
					}
				}
				else if (walkDirection < 0) {
					if (Physics.Raycast(t.position + stepHeight * t.up, -t.right, out conditionalRayHit, .51f, layers)) {
						print ("hit left");
						leftRayHit = conditionalRayHit;
					}
				}
				movePlayer();
			}
			else {
				//Add code here for spinning to find a foothold
				//break;
			}

			yield return new WaitForFixedUpdate();
		}
	}

	bool checkRaycastDown() {
		return Physics.Raycast(t.position + .5f * t.right + 2 * t.up, -t.up, out leftRayHit, fallToleranceHeight, layers)
			&& Physics.Raycast(t.position - .5f * t.right + 2 * t.up, -t.up, out rightRayHit, fallToleranceHeight, layers);
	}

	void movePlayer() {
		//Sadly math is mostly found, could clean up quite a bit
		r.MoveRotation(Quaternion.Euler(0, 0, Quaternion.RotateTowards(t.rotation, Quaternion.FromToRotation(Vector3.up, (leftRayHit.normal + rightRayHit.normal) / 2), turnRate).eulerAngles.z));
		r.MovePosition(Vector3.MoveTowards(t.position, (leftRayHit.point + rightRayHit.point)/2, walkSpeed * Time.fixedDeltaTime));
	}
}
