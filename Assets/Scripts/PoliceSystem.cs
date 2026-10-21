using UnityEngine;

public class PoliceSystem : MonoBehaviour
{
    public static PoliceSystem Instance { get; private set; }

    [Range(0, 5)]
    public int wantedLevel = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddWantedStars(int stars)
    {
        wantedLevel = Mathf.Clamp(wantedLevel + stars, 0, 5);
        Debug.Log($"[Полиция]: Уровень розыска повышен! Текущий уровень: {wantedLevel}★");
    }
}