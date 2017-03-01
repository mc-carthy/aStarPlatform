using UnityEngine;

public class CameraController : MonoBehaviour {

	[SerializeField]
	private Transform target;
	private float maxCamDistFromTarget = 15f;
	private float maxCamDistFromTargetFar = 20f;


	private void Update () {
		float mousePosX = Input.mousePosition.x - (Screen.width / 2f);
		float mousePosY = Input.mousePosition.y - (Screen.height / 2f);

		Vector3 dirToCursor = new Vector3 (mousePosX, transform.position.y, mousePosY) - target.transform.position;

		maxCamDistFromTarget = Mathf.Min (dirToCursor.magnitude, maxCamDistFromTarget);
		dirToCursor.y = 10f;

		transform.position = target.transform.position + dirToCursor.normalized * maxCamDistFromTarget;
	}
}
