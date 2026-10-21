
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager I;

    public int Cash = 250;
    public int WantedLevel = 0;
    public int Skill = 1;
    public int InventoryItems = 0;
    public bool OwnsApartment = false;
    public bool OwnsBusiness = false;
    public string Notice = "Добро пожаловать в Street Empire";

    float incomeTimer;

    void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
    }

    public void AddCash(int amount, string reason)
    {
        Cash += amount;
        Notice = reason + "  +" + amount + "$";
    }

    public bool SpendCash(int amount, string reason)
    {
        if (Cash < amount)
        {
            Notice = "Не хватает денег";
            return false;
        }

        Cash -= amount;
        Notice = reason + "  -" + amount + "$";
        return true;
    }

    public void AddWanted(int amount)
    {
        WantedLevel = Mathf.Clamp(WantedLevel + amount, 0, 5);

        if (WantedLevel > 0)
            Notice = "РОЗЫСК: " + WantedLevel + "/5";
        else
            Notice = "Розыск снят";
    }

    public void ClearWanted()
    {
        if (WantedLevel <= 0) return;

        WantedLevel = 0;
        Notice = "Полиция потеряла твой след";
    }

    void Update()
    {
        if (OwnsBusiness)
        {
            incomeTimer += Time.deltaTime;

            if (incomeTimer >= 10f)
            {
                incomeTimer = 0f;
                AddCash(40, "Доход бизнеса");
            }
        }
    }

    void OnGUI()
    {
        GUI.Box(new Rect(12, 12, 245, 135), "");
        GUI.Label(new Rect(24, 20, 220, 24), "STREET EMPIRE");
        GUI.Label(new Rect(24, 44, 220, 20), "Наличные: " + Cash + "$");
        GUI.Label(new Rect(24, 64, 220, 20),
            "Розыск: " + new string('*', WantedLevel)
            + new string('-', 5 - WantedLevel));
        GUI.Label(new Rect(24, 84, 220, 20),
            "Навык: " + Skill + "   Инвентарь: " + InventoryItems);

        if (OwnsApartment)
            GUI.Label(new Rect(24, 104, 220, 20), "Квартира куплена");

        if (OwnsBusiness)
            GUI.Label(new Rect(24, 124, 220, 20), "Бизнес куплен (+40$ / 10 сек)");

        GUI.Label(new Rect(12, Screen.height - 42,
            Screen.width - 24, 30), Notice);
    }
}
