using UnityEngine;
using UnityEngine.InputSystem;

public class Hole : MonoBehaviour
{
    public float speed = 10f;
    private Vector2 moveInput;
    private Move controls;

    private void Awake()
    {
        controls = new Move();
        controls.Hole.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Hole.Move.canceled += ctx => moveInput = Vector2.zero;
    }
    private void OnEnable()
    {
        if (controls != null)
        {
            controls.Enable();
        }
    }

    private void OnDisable()
    {
        if (controls != null)
        {
            controls.Disable();
        }
    }

    void Update()
    {
        // Vector3 move = new Vector3(moveInput.x, moveInput.y, 0);
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
        transform.Translate(move * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Object"))
        {
            other.gameObject.GetComponent<Rigidbody>().useGravity = true;
        }

    }
    private void OnDestroy()
    {
        if (controls != null)
        {
            controls.Dispose(); // Giải phóng tài nguyên Input System
        }
    }
}
