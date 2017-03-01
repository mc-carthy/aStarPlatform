using UnityEngine;

public class TargetController : MonoBehaviour {

    private Rigidbody rb;
    private Camera viewCam;
	private float speed = 10f;
    private Vector3 velocity;

    private void Awake ()
    {
        rb = GetComponent<Rigidbody> ();
        viewCam = Camera.main;
    }

    private void Update ()
    {
        Vector3 mousePos = viewCam.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x,Input.mousePosition.y, viewCam.transform.position.y));
        transform.LookAt (mousePos + Vector3.up * transform.position.y);
    }

    private void FixedUpdate ()
    {
        float hMove = Input.GetAxisRaw ("Horizontal") * speed * Time.fixedDeltaTime;
        float vMove = Input.GetAxisRaw ("Vertical") * speed * Time.fixedDeltaTime;
        velocity = new Vector3 (hMove, 0, vMove).normalized * speed;
        rb.MovePosition (transform.position + velocity * Time.fixedDeltaTime);
    }

}
