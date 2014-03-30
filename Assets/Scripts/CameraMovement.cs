using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour {

	public float maxCameraSpeed = 10f;

	Transform player;
	Transform t;

	void Awake() {
		player = GameObject.FindGameObjectWithTag("Player").transform;
		t = transform;
	}

	void Update () {
		t.position = Vector3.MoveTowards(t.position,new Vector3(player.position.x,player.position.y,t.position.z), maxCameraSpeed);
	}
}
