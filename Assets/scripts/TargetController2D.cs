using UnityEngine;

public class TargetController2D : MonoBehaviour {

    private Rigidbody2D rb;
    private Camera viewCam;
	private float speed = 10f;
    private Vector2 velocity;

    private void Awake ()
    {
        rb = GetComponent<Rigidbody2D> ();
        viewCam = Camera.main;
    }

    private void Update ()
    {
        Vector3 objectPos = Camera.main.WorldToScreenPoint(transform.position);
        Vector3 dir = Input.mousePosition - objectPos; 
        transform.rotation = Quaternion.Euler (new Vector3 (0, 0, Mathf.Atan2 (dir.y, dir.x) * Mathf.Rad2Deg));

        float hMove = Input.GetAxisRaw ("Horizontal") * speed * Time.deltaTime;
        float vMove = Input.GetAxisRaw ("Vertical") * speed * Time.deltaTime;
        velocity = new Vector2 (hMove, vMove).normalized * speed;
    }

    private void FixedUpdate ()
    {
        rb.MovePosition ((Vector2) transform.position + velocity * Time.fixedDeltaTime);
    }

}
