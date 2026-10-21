
using UnityEngine;

public class CrimeSystem : MonoBehaviour
{
    public static CrimeSystem I;

    [Header("Настройки свидетелей")]
    public float witnessRange = 15f;
    public float reportDelay = 3f;
    [Range(0f, 1f)] public float reportChance = 0.8f;
    public LayerMask obstacleMask = ~0;

    void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
    }

    public void ReportCrime(Transform suspect)
    {
        if (!suspect) return;

        CitizenAI[] citizens = FindObjectsByType<CitizenAI>();

        foreach (CitizenAI citizen in citizens)
        {
            if (!citizen || citizen.transform == suspect) continue;

            Vector3 offset = suspect.position - citizen.transform.position;
            if (offset.magnitude > witnessRange) continue;

            Vector3 eye = citizen.transform.position + Vector3.up * 1.5f;
            Vector3 target = suspect.position + Vector3.up;

            Vector3 direction = target - eye;
            float distance = direction.magnitude;

            if (distance <= 0f) continue;

            // Препятствия блокируют обзор.
            if (Physics.Raycast(eye, direction.normalized,
                distance, obstacleMask, QueryTriggerInteraction.Ignore))
                continue;

            if (Random.value > reportChance) continue;

            citizen.WitnessCrime(reportDelay);
        }
    }
}
