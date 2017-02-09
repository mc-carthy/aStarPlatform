using UnityEngine;

public class TargetController : MonoBehaviour {

    private Rigidbody rb;
    private Camera viewCam;
	private float speed = 15f;
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
        
        float hMove = Input.GetAxis ("Horizontal") * speed * Time.fixedDeltaTime;
        float vMove = Input.GetAxis ("Vertical") * speed * Time.fixedDeltaTime;
        velocity = new Vector3 (hMove, 0, vMove).normalized * speed;
    }

    private void FixedUpdate ()
    {
        rb.MovePosition (transform.position + velocity * Time.fixedDeltaTime);
    }

}
