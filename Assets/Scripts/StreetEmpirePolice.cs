
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class StreetEmpirePolice : MonoBehaviour
{
    [Header("Полиция")]
    public Transform target;
    public float chaseSpeed = 7f;
    public float activateDistance = 45f;

    [Header("Сирена")]
    public AudioSource sirenAudio;

    bool sirenActive;

    void Start()
    {
        if (sirenAudio == null)
            sirenAudio = GetComponent<AudioSource>();

        if (sirenAudio != null)
        {
            sirenAudio.playOnAwake = false;
            sirenAudio.loop = true;
            sirenAudio.Stop();
        }
    }

    void Update()
    {
        var gm = StreetEmpireGameManager.Instance;

        if (gm == null || gm.WantedLevel <= 0)
        {
            StopSiren();
            return;
        }

        // Включаем сирену при появлении розыска.
        StartSiren();

        if (target == null)
        {
            if (gm.player != null)
                target = gm.player;

            if (target == null)
                return;
        }

        Vector3 delta = target.position - transform.position;
        delta.y = 0f;

        // Если игрок далеко, машина пока не преследует его.
        if (delta.magnitude > activateDistance)
            return;

        // Преследование игрока.
        if (delta.sqrMagnitude > 2f)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(delta),
                2.5f * Time.deltaTime
            );

            transform.position +=
                transform.forward * chaseSpeed * Time.deltaTime;
        }

        // Задержание.
        if (delta.magnitude < 2.2f)
        {
            gm.SetWanted(0);
            gm.ChangeMoney(-100);

            StopSiren();

            Vector3 escapePosition =
                transform.position + Random.insideUnitSphere * 30f;

            escapePosition.y = transform.position.y;
            transform.position = escapePosition;
        }
    }

    void StartSiren()
    {
        if (sirenActive)
            return;

        sirenActive = true;

        if (sirenAudio != null)
            sirenAudio.Play();
    }

    void StopSiren()
    {
        sirenActive = false;

        if (sirenAudio != null && sirenAudio.isPlaying)
            sirenAudio.Stop();
    }

    void OnDisable()
    {
        StopSiren();
    }
}
