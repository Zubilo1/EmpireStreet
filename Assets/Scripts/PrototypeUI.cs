using UnityEngine;

public class PrototypeUI : MonoBehaviour
{
    public static bool ShopOpen;
    public static bool InventoryOpen;

    void OnGUI()
    {
        var gm = GameManager.I;
        if (gm == null) return;

        if (ShopOpen)
        {
            GUI.Box(new Rect(Screen.width/2-150, Screen.height/2-115, 300, 230), "Магазин");
            GUI.Label(new Rect(Screen.width/2-130, Screen.height/2-75, 260, 25), "Esc — закрыть");
            if (GUI.Button(new Rect(Screen.width/2-125, Screen.height/2-42, 250, 30), "Купить предмет — 50$"))
            {
                if (gm.SpendCash(50, "Покупка предмета")) gm.InventoryItems++;
            }
            if (GUI.Button(new Rect(Screen.width/2-125, Screen.height/2+4, 250, 30), "Тренировка навыка — 150$"))
            {
                if (gm.Skill >= 10) gm.Notice = "Навык уже максимальный";
                else if (gm.SpendCash(150, "Тренировка")) gm.Skill = Mathf.Min(10, gm.Skill + 1);
            }
            if (GUI.Button(new Rect(Screen.width/2-125, Screen.height/2+50, 250, 30), "Закрыть"))
                ShopOpen = false;
        }

        if (InventoryOpen)
        {
            GUI.Box(new Rect(Screen.width-240, 12, 225, 95), "Инвентарь");
            GUI.Label(new Rect(Screen.width-225, 42, 200, 22), "Предметы: " + gm.InventoryItems);
            GUI.Label(new Rect(Screen.width-225, 64, 200, 22), "Навык: " + gm.Skill + "/10");
        }

        if (!ShopOpen)
            GUI.Label(new Rect(Screen.width/2-170, Screen.height-75, 340, 25), "E — взаимодействовать | Tab — инвентарь");
    }
}
