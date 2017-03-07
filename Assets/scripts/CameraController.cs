using UnityEngine;

public class CameraController : MonoBehaviour {

	[SerializeField]
	private Transform target;
	private float maxCamDistFromTarget = 10f;
	private float maxCamDistFromTargetFar = 20f;
	private float cameraHeight = 25f;


	private void Update () {
		if (Input.GetKey (KeyCode.LeftShift))
		{
			FollowCursor ();
		}
		else
		{
			FollowTarget ();
		}
	}

	private void FollowCursor ()
	{
		float mousePosX = Input.mousePosition.x - (Screen.width / 2f);
		float mousePosY = Input.mousePosition.y - (Screen.height / 2f);

		Vector3 dirToCursor = new Vector3 (mousePosX, transform.position.y, mousePosY) - target.transform.position;

		maxCamDistFromTarget = Mathf.Min (dirToCursor.magnitude, maxCamDistFromTarget);
		dirToCursor.y = 10f;

		Vector3 camPos = target.transform.position + dirToCursor.normalized * maxCamDistFromTarget;
		camPos.y = cameraHeight;
		transform.position = camPos;
	}

	private void FollowTarget ()
	{
		Vector3 camPos = target.transform.position;
		camPos.y = cameraHeight;
		transform.position = camPos;
	}
}
