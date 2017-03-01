using UnityEngine;

public class CameraController : MonoBehaviour {

	[SerializeField]
	private Transform target;
	private Vector3 startPos;

	private void Start () {
		startPos = transform.position;
	}

	private void Update () {
		Vector3 temp = startPos;
		temp.x = target.position.x;
		temp.z = target.position.z;
		transform.position = temp;
	}
}
