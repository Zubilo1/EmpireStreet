
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class StreetEmpirePlayer : MonoBehaviour
{
    public float walkSpeed = 5.2f;
    public float sprintSpeed = 8.2f;
    public float jumpHeight = 1.15f;
    public float gravity = -22f;
    public Transform cameraPivot;
    public float interactionRange = 4f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool driving;
    private StreetEmpireCar currentCar;

    public bool IsDriving { get { return driving; } }

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (!isActiveAndEnabled || !gameObject.activeInHierarchy)
            return;

        if (driving)
        {
            if (Input.GetKeyDown(KeyCode.E))
                ExitCar();

            return;
        }

        if (controller == null || !controller.enabled)
            return;

        if (Input.GetKeyDown(KeyCode.E))
            TryEnterCar();

        // ≈сли сел в машину, дальше пешком не двигаемс€.
        if (driving || !controller.enabled)
            return;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.forward * z + transform.right * x;

        if (move.sqrMagnitude > 1f)
            move.Normalize();

        float speed = Input.GetKey(KeyCode.LeftShift)
            ? sprintSpeed
            : walkSpeed;

        if (controller.isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        if (controller.isGrounded && Input.GetButtonDown("Jump"))
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;

        Vector3 motion = move * speed + Vector3.up * velocity.y;
        controller.Move(motion * Time.deltaTime);
    }

    void TryEnterCar()
    {
        StreetEmpireCar[] cars = FindObjectsByType<StreetEmpireCar>();

        StreetEmpireCar nearest = null;
        float best = interactionRange;

        foreach (var car in cars)
        {
            if (car == null || car.IsOccupied)
                continue;

            float distance = Vector3.Distance(
                transform.position,
                car.transform.position
            );

            if (distance < best)
            {
                best = distance;
                nearest = car;
            }
        }

        if (nearest == null)
            return;

        currentCar = nearest;
        driving = true;

        controller.enabled = false;

        transform.SetParent(currentCar.transform);
        transform.localPosition = new Vector3(0f, 0.8f, 0f);

        currentCar.SetOccupied(true);
    }

    void ExitCar()
    {
        if (currentCar == null)
        {
            driving = false;

            if (controller != null)
                controller.enabled = true;

            return;
        }

        StreetEmpireCar car = currentCar;

        transform.SetParent(null);

        transform.position =
            car.transform.position
            + car.transform.right * 2.2f
            + Vector3.up * 0.2f;

        car.SetOccupied(false);

        currentCar = null;
        driving = false;
        velocity = Vector3.zero;

        controller.enabled = true;
    }

    void OnDisable()
    {
        if (controller != null && controller.enabled)
            velocity = Vector3.zero;
    }
}
