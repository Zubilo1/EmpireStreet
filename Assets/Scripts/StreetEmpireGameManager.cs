using UnityEngine;

public class StreetEmpireGameManager : MonoBehaviour
{
    public static StreetEmpireGameManager Instance { get; private set; }
    public int Money { get; private set; }
    public int WantedLevel { get; private set; }
    public Transform player;
    float crimeCooldown;
    void Awake() { Instance = this; Money = PlayerPrefs.GetInt("SE_Money", 500); }
    void Update() { crimeCooldown -= Time.deltaTime; if (Input.GetKeyDown(KeyCode.G)) CommitCrime(); if (Input.GetKeyDown(KeyCode.M)) ChangeMoney(100); }
    public void CommitCrime() { if (crimeCooldown > 0) return; crimeCooldown = 2f; SetWanted(Mathf.Min(5, WantedLevel + 1)); }
    public void SetWanted(int value) { WantedLevel = Mathf.Clamp(value, 0, 5); }
    public void ChangeMoney(int amount) { Money = Mathf.Max(0, Money + amount); PlayerPrefs.SetInt("SE_Money", Money); PlayerPrefs.Save(); }
    void OnGUI()
    {
        GUI.skin.label.fontSize = 18;
        GUI.color = new Color(0, 0, 0, .65f); GUI.Box(new Rect(18, 18, 290, 112), GUIContent.none); GUI.color = Color.white;
        GUI.Label(new Rect(32, 28, 260, 28), "STREET EMPIRE"); GUI.Label(new Rect(32, 55, 260, 25), "Деньги: $" + Money); GUI.Label(new Rect(32, 80, 260, 25), "Розыск: " + new string('★', WantedLevel) + new string('☆', 5 - WantedLevel));
        GUI.color = new Color(1, 1, 1, .85f); GUI.Label(new Rect(20, Screen.height - 62, 800, 26), "WASD — движение/вождение   SHIFT — бег   SPACE — прыжок   E — сесть/выйти   G — преступление   M — +$100   ESC — курсор"); GUI.color = Color.white;
    }
}
