
using UnityEngine;

public enum InteractionType
{
    Citizen, Shop, Apartment, Business, Vehicle, Job
}

public class Interactable : MonoBehaviour
{
    public InteractionType type;
    public string displayName = "Объект";
    public int price = 500;

    bool used;

    public void Interact(PlayerController3D player)
    {
        var gm = GameManager.I;
        if (!gm || !player) return;

        switch (type)
        {
            case InteractionType.Citizen:

                if (used)
                {
                    gm.Notice = "Этот прохожий уже настороже";
                    return;
                }

                int chance = Mathf.Clamp(
                    55 + gm.Skill * 5, 10, 90);

                if (Random.Range(0, 100) < chance)
                {
                    int reward = Random.Range(20, 81)
                        + gm.Skill * 5;

                    gm.AddCash(reward, "Удачная кража");
                    gm.InventoryItems++;

                    used = true;

                    // Ищем свидетелей только после успеха.
                    if (CrimeSystem.I != null)
                    {
                        CrimeSystem.I.ReportCrime(
                            player.transform);
                    }
                }
                else
                {
                    gm.Notice = "Кража не удалась!";
                    used = true;

                    // Неудачная попытка тоже может быть замечена.
                    if (CrimeSystem.I != null)
                    {
                        CrimeSystem.I.ReportCrime(
                            player.transform);
                    }
                }

                break;

            case InteractionType.Shop:
                PrototypeUI.ShopOpen = true;
                break;

            case InteractionType.Apartment:
                if (gm.OwnsApartment)
                    gm.Notice = "У тебя уже есть квартира";
                else if (gm.SpendCash(price, "Покупка квартиры"))
                    gm.OwnsApartment = true;
                break;

            case InteractionType.Business:
                if (gm.OwnsBusiness)
                    gm.Notice = "Бизнес уже принадлежит тебе";
                else if (gm.SpendCash(price, "Покупка бизнеса"))
                    gm.OwnsBusiness = true;
                break;

            case InteractionType.Vehicle:
                var vehicle = GetComponentInParent<VehicleController>();
                if (vehicle) player.EnterVehicle(vehicle);
                break;

            case InteractionType.Job:
                gm.AddCash(100 + gm.Skill * 10, "Подработка");
                gm.Skill = Mathf.Min(10, gm.Skill + 1);
                gm.Notice = "Подработка выполнена. Навык повышен!";
                break;
        }
    }
}
