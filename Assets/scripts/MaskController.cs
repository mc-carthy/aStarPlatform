using UnityEngine;

public class MaskController : MonoBehaviour {

    private float nonFunctionalMaskHeight = 20f;
    private float functionalMaskHeight = 50f;
    private bool isMaskFunctional;

	private void Update ()
    {
        if (Input.GetKeyDown (KeyCode.Space))
        {
            isMaskFunctional = !isMaskFunctional;
            Vector3 newPos = transform.position;
            newPos.y = (isMaskFunctional) ? functionalMaskHeight : nonFunctionalMaskHeight;
            transform.position = newPos;
        }
    }

}
