using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class StreetEmpireCar : MonoBehaviour
{
    public float acceleration = 22f;
    public float reverseAcceleration = 12f;
    public float turnSpeed = 75f;
    public float maxSpeed = 24f;
    Rigidbody rb;
    bool occupied;
    public bool IsOccupied { get { return occupied; } }
    void Awake() { rb = GetComponent<Rigidbody>(); rb.mass = 1100; rb.linearDamping = .25f; rb.angularDamping = 2.5f; rb.interpolation = RigidbodyInterpolation.Interpolate; rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ; }
    void FixedUpdate()
    {
        if (!occupied) { rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, new Vector3(0, rb.linearVelocity.y, 0), Time.fixedDeltaTime * .2f); return; }
        float throttle = Input.GetAxis("Vertical"); float steer = Input.GetAxis("Horizontal");
        if (rb.linearVelocity.magnitude < maxSpeed || Vector3.Dot(rb.linearVelocity, transform.forward) * throttle < 0) rb.AddForce(transform.forward * throttle * (throttle >= 0 ? acceleration : reverseAcceleration), ForceMode.Acceleration);
        if (rb.linearVelocity.sqrMagnitude > .5f) transform.Rotate(0, steer * turnSpeed * Mathf.Clamp01(rb.linearVelocity.magnitude / 5f) * Time.fixedDeltaTime, 0);
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, rb.linearVelocity.z);
    }
    public void SetOccupied(bool value) { occupied = value; if (rb) rb.isKinematic = false; }
}
