using UnityEngine;
using System.Collections;

public class ShootingControl : MonoBehaviour {

	public float fireForce = 2.0f;
	public float fireCooldown = 0.125f;
	public GameObject bulletType;

	private bool onCooldown = false;

	private Transform t;
	private Rigidbody r;

	void Awake() {
		t = transform;
		r = rigidbody;
	}

	public bool Shoot(Vector3 shootVector) {
		if (onCooldown)
			return false;

		StartCoroutine("DoShoot",shootVector);
		return true;
	}

	IEnumerator DoShoot(Vector3 shootVector) {
		onCooldown = true;

		r.AddForce((shootVector - t.position).normalized * fireForce,ForceMode.Impulse);
		
		//GameObject bullet = Instantiate(bulletType,t.position,Quaternion.identity) as GameObject;
		//Physics.IgnoreCollision(collider, bullet.collider);
		//bullet.rigidbody.AddForce((shootVector - t.position).normalized * fireForce,ForceMode.Impulse);
		
		yield return new WaitForSeconds(fireCooldown);

		onCooldown = false;
	}
}
