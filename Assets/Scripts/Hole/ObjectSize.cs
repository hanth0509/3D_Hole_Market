using UnityEngine;

public class ObjectSize : MonoBehaviour
{
    [Header("Object Size Settings")]
    public string size = "SMALL"; // SMALL, MEDIUM, LARGE, SPECIAL
    public int points = 10;
    
    [Header("Visual Settings")]
    public Color objectColor = Color.white;
    public float scaleMultiplier = 1f;

    void Start()
    {
        SetupObjectAppearance();
        SetupPhysics();
    }

    void SetupObjectAppearance()
    {
        // SETUP MÀU SẮC & KÍCH THƯỚC THEO LOẠI
        switch (size)
        {
            case "SMALL":
                objectColor = Color.green;
                scaleMultiplier = 0.5f;
                points = 10;
                break;
            case "MEDIUM":
                objectColor = Color.yellow;
                scaleMultiplier = 0.8f;
                points = 30;
                break;
            case "LARGE":
                objectColor = Color.red;
                scaleMultiplier = 1.5f;
                points = 100;
                break;
            case "SPECIAL":
                objectColor = Color.magenta;
                scaleMultiplier = 1.2f;
                points = 200;
                break;
        }

        // ÁP DỤNG MÀU SẮC & SCALE
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = objectColor;
        }
        
        transform.localScale = Vector3.one * scaleMultiplier;
    }

    void SetupPhysics()
    {
        // THÊM COLLIDER & RIGIDBODY ĐỂ TƯƠNG TÁC
        if (GetComponent<Collider>() == null)
        {
            gameObject.AddComponent<SphereCollider>();
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false; // Vật thể lơ lửng
            rb.isKinematic = true; // Không bị ảnh hưởng bởi physics
        }
    }

    // METHOD ĐỂ HOLE SYSTEM GỌI KHI ĂN VẬT
    public void OnEaten()
    {
        // Thêm hiệu ứng, âm thanh, particle...
        Debug.Log($"Object {size} eaten! +{points} points");
        Destroy(gameObject);
    }
}