using UnityEngine;

[RequireComponent (typeof (Rigidbody))]
public class PlayerController2D : MonoBehaviour {

    private Rigidbody rb;
	private float moveSpeed = 500f;
    private Vector2 input;

    private void Awake ()
    {
        rb = GetComponent<Rigidbody> ();
    }

    private void Update ()
    {
        float h = Input.GetAxisRaw ("Horizontal");
        float v = Input.GetAxisRaw ("Vertical");

        input = new Vector2 (h, v);
    }

    private void FixedUpdate ()
    {
        rb.velocity = input.normalized * moveSpeed * Time.fixedDeltaTime;
    }

}
