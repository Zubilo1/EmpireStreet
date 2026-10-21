using UnityEngine;

public class StreetEmpirePolice : MonoBehaviour
{
    public Transform target; public float chaseSpeed = 7f; public float activateDistance = 45f;
    void Update()
    {
        if (!target) return;
        var gm = StreetEmpireGameManager.Instance;
        if (gm == null || gm.WantedLevel <= 0) return;
        Vector3 delta = target.position - transform.position; delta.y = 0;
        if (delta.magnitude > activateDistance) return;
        if (delta.sqrMagnitude > 2f) { transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(delta), 2.5f * Time.deltaTime); transform.position += transform.forward * chaseSpeed * Time.deltaTime; }
        if (delta.magnitude < 2.2f) { gm.SetWanted(0); gm.ChangeMoney(-100); transform.position += Random.insideUnitSphere * 30f; transform.position = new Vector3(transform.position.x, 0, transform.position.z); }
    }
}
