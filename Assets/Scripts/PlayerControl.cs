using UnityEngine;
using System.Collections;

public class PlayerControl : MonoBehaviour {

	private bool canShoot = true;

	private ShootingControl shootControl;
	private GravityWalking gravityWalking;
	private SpiderWalking spiderWalking;

	void Awake() {
		shootControl = GetComponent<ShootingControl>();
		gravityWalking = GetComponent<GravityWalking>();
		spiderWalking = GetComponent<SpiderWalking>();
	}

	void Start() {
		StartCoroutine("ShootInput");
		StartCoroutine("SurfaceAttachInput");
	}

	IEnumerator ShootInput() {
		while (true) {
			if (Input.GetAxis("Fire1") > 0 && canShoot) {
				shootControl.Shoot(Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.z)));
			}
			yield return null;
		}
	}

	IEnumerator SurfaceAttachInput() {
		while (true) {
			if (Input.GetKeyDown(KeyCode.E)) {
				if (!spiderWalking.currentlyWalking) {
					spiderWalking.StartSpiderWalk();
				}
				else {
					spiderWalking.StopSpiderWalk();
				}
			}
			yield return null;
		}
	}
}
