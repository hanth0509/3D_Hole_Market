using UnityEngine;
using UnityEngine.InputSystem;

public class Hole : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 10f;
    private Vector2 moveInput;
    private Move controls;

    [Header("Hole Growth Settings")]
    public float currentSize = 1f;
    public float baseSize = 1f;
    public float maxSize = 5f;

    [Header("Size Requirements")]
    public float sizeRequiredSmall = 1.0f;
    public float sizeRequiredMedium = 1.8f;
    public float sizeRequiredLarge = 3.0f;
    public float sizeRequiredSpecial = 4.0f;

    [Header("Growth Amounts")]
    public float growthSmall = 0.1f;
    public float growthMedium = 0.2f;
    public float growthLarge = 0.4f;
    public float growthSpecial = 0.6f;

    [Header("Current Stats")]
    public int smallEaten = 0;
    public int mediumEaten = 0;
    public int largeEaten = 0;
    public int specialEaten = 0;
    [Header("Hole Visual Settings")]
    public float fixedYScale = 0.02f;
    private void Start()
    {
        UpdateHoleVisual();
        Debug.Log("Hole initialized - Ready to eat objects!");
    }
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

        if (transform.localScale.y != 0.02f)
        {
            Debug.Log($"⚠️ Scale Y bị thay đổi tại frame: {Time.frameCount}");
            Debug.Log($"Scale hiện tại: {transform.localScale}");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 🔥 SYSTEM MỚI: KIỂM TRA OBJECT SIZE VÀ ĂN
        ObjectSize objectSize = other.GetComponent<ObjectSize>();
        if (objectSize != null)
        {
            TryEatObject(objectSize);
            return;
        }

        if (other.gameObject.CompareTag("Object"))
        {
            other.gameObject.GetComponent<Rigidbody>().useGravity = true;
        }

    }
    // 🔥 PHƯƠNG THỨC MỚI: THỬ ĂN OBJECT
    private void TryEatObject(ObjectSize objectSize)
    {
        string sizeType = objectSize.size;

        if (CanEatObject(sizeType))
        {
            EatObject(objectSize);
        }
        else
        {
            Debug.Log($"❌ Hole too small! Current: {currentSize:F1}, Need: {GetRequiredSize(sizeType):F1} for {sizeType}");
            // Có thể thêm hiệu ứng rung hoặc âm thanh ở đây
        }
    }
    private bool CanEatObject(string sizeType)
    {
        switch (sizeType)
        {
            case "SMALL": return currentSize >= sizeRequiredSmall;
            case "MEDIUM": return currentSize >= sizeRequiredMedium;
            case "LARGE": return currentSize >= sizeRequiredLarge;
            case "SPECIAL": return currentSize >= sizeRequiredSpecial;
            default: return true; // Mặc định cho object không xác định
        }
    }
    private float GetRequiredSize(string sizeType)
    {
        switch (sizeType)
        {
            case "SMALL": return sizeRequiredSmall;
            case "MEDIUM": return sizeRequiredMedium;
            case "LARGE": return sizeRequiredLarge;
            case "SPECIAL": return sizeRequiredSpecial;
            default: return 1f;
        }
    }
    private void EatObject(ObjectSize objectSize)
    {
        string sizeType = objectSize.size;
        int points = objectSize.points;

        // 🔥 TĂNG KÍCH THƯỚC HOLE
        float growth = GetGrowthAmount(sizeType);
        currentSize = Mathf.Min(currentSize + growth, maxSize);

        // 🔥 CẬP NHẬT STATS
        UpdateStats(sizeType);

        // 🔥 THÔNG BÁO
        Debug.Log($"🎯 Object Eaten: {sizeType} (+{points} points)");

        // 🔥 HIỆU ỨNG & XÓA VẬT
        objectSize.OnEaten();

        // 🔥 CẬP NHẬT KÍCH THƯỚC VISUAL
        UpdateHoleVisual();

        Debug.Log($"✅ Ate {sizeType}! Size: {currentSize:F2} (+{growth:F2})");
    }
    private float GetGrowthAmount(string sizeType)
    {
        switch (sizeType)
        {
            case "SMALL": return growthSmall;
            case "MEDIUM": return growthMedium;
            case "LARGE": return growthLarge;
            case "SPECIAL": return growthSpecial;
            default: return growthSmall;
        }
    }
    private void UpdateStats(string sizeType)
    {
        switch (sizeType)
        {
            case "SMALL": smallEaten++; break;
            case "MEDIUM": mediumEaten++; break;
            case "LARGE": largeEaten++; break;
            case "SPECIAL": specialEaten++; break;
        }
    }
    public void UpdateHoleVisual()
    {
        // 🔥 CẬP NHẬT KÍCH THƯỚC HOLE
        transform.localScale = new Vector3(currentSize, fixedYScale, currentSize);

        // 🔥 CÓ THỂ THÊM HIỆU ỨNG KHÁC:
        // - Particle effect khi lớn lên
        // - Animation scale
        // - Thay đổi material
    }
    // 🔥 METHOD ĐỂ KIỂM TRA TRẠNG THÁI (Debug)
    [ContextMenu(" Print Hole Status")]
    public void PrintStatus()
    {
        Debug.Log($"🕳️ Hole Status - Size: {currentSize:F2}/{(currentSize / baseSize) * 100:F0}%");
        Debug.Log($"🍴 Eaten: S{smallEaten}/M{mediumEaten}/L{largeEaten}/SP{specialEaten}");
        Debug.Log($"🎯 Can Eat: Small({currentSize >= sizeRequiredSmall}) Med({currentSize >= sizeRequiredMedium}) Large({currentSize >= sizeRequiredLarge})");
    }

    // 🔥 METHOD ĐỂ TEST NHANH
    [ContextMenu(" Test Grow Small")]
    public void TestGrowSmall()
    {
        currentSize = Mathf.Min(currentSize + growthSmall, maxSize);
        UpdateHoleVisual();
        PrintStatus();
    }

    [ContextMenu(" Test Grow Large")]
    public void TestGrowLarge()
    {
        currentSize = Mathf.Min(currentSize + growthLarge, maxSize);
        UpdateHoleVisual();
        PrintStatus();
    }
    private void OnDestroy()
    {
        if (controls != null)
        {
            controls.Dispose(); // Giải phóng tài nguyên Input System
        }
    }
}
