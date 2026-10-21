using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController3D : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float runMultiplier = 1.6f;
    public Transform cameraTransform;
    CharacterController controller;
    Renderer[] renderers;
    Vector3 velocity;
    bool driving;
    VehicleController currentVehicle;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        renderers = GetComponentsInChildren<Renderer>();
    }

    void Start()
    {
        if (cameraTransform == null && Camera.main) cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PrototypeUI.ShopOpen = false;
            PrototypeUI.InventoryOpen = false;
        }

        if (driving)
        {
            if (!currentVehicle) { driving = false; SetVisible(true); controller.enabled = true; return; }
            if (Input.GetKeyDown(KeyCode.E)) ExitVehicle();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Tab)) PrototypeUI.InventoryOpen = !PrototypeUI.InventoryOpen;
        if (PrototypeUI.ShopOpen) return; // в магазине не ходим

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 forward = cameraTransform ? Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized : Vector3.forward;
        Vector3 right = cameraTransform ? Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized : Vector3.right;
        Vector3 move = (forward * v + right * h).normalized;
        float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? runMultiplier : 1f);

        if (move.sqrMagnitude > 0.01f)
            transform.forward = Vector3.Slerp(transform.forward, move, 12f * Time.deltaTime);

        if (controller.isGrounded && velocity.y < 0) velocity.y = -2f;
        velocity.y += Physics.gravity.y * Time.deltaTime;

        // Один вызов Move вместо двух
        controller.Move((move * speed + Vector3.up * velocity.y) * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.E)) Interact();
    }

    void LateUpdate()
    {
        // Игрок "сидит" в машине без parent-а (у машины неравномерный scale, он бы искажал игрока)
        if (driving && currentVehicle)
            transform.position = currentVehicle.transform.position + Vector3.up * 0.5f;
    }

    void Interact()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 2.6f);
        Interactable best = null;
        float dist = float.MaxValue;
        foreach (var hit in hits)
        {
            var item = hit.GetComponentInParent<Interactable>();
            if (!item) continue;
            // расстояние до ближайшей точки коллайдера, а не до центра (большие здания)
            float d = Vector3.Distance(transform.position, hit.ClosestPoint(transform.position));
            if (d < dist) { best = item; dist = d; }
        }
        if (best) best.Interact(this);
        else if (GameManager.I) GameManager.I.Notice = "Рядом нет объектов для взаимодействия";
    }

    void SetVisible(bool value)
    {
        foreach (var r in renderers) r.enabled = value;
    }

    public void EnterVehicle(VehicleController vehicle)
    {
        driving = true; currentVehicle = vehicle;
        velocity = Vector3.zero;
        controller.enabled = false;
        SetVisible(false);
        vehicle.SetDriver(true);
        if (GameManager.I) GameManager.I.Notice = "Ты за рулём. WASD — ехать, E — выйти";
    }

    void ExitVehicle()
    {
        if (!currentVehicle) return;
        currentVehicle.SetDriver(false);
        transform.position = currentVehicle.transform.position + currentVehicle.transform.right * 2.5f + Vector3.up;
        currentVehicle = null; driving = false;
        SetVisible(true);
        controller.enabled = true;
    }
}
