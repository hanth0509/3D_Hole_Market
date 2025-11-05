using UnityEngine;
using UnityEngine.InputSystem;

public class Hole:MonoBehaviour
{
    public float speed = 10f;
    private Vector2 moveInput;
    private Move controls;

    private void Awake() {
        controls = new Move();
        controls.Hole.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Hole.Move.canceled += ctx => moveInput = Vector2.zero;
    }
    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    void Update()
    {
        Vector3 move = new Vector3(moveInput.x, moveInput.y, 0);
        transform.Translate(move * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("Object"))
        {
            other.gameObject.GetComponent<Rigidbody>().useGravity = true;
        }
  
    }
}
