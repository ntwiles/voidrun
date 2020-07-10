
using UnityEngine;
using UnityEngine.UI;

public class DamageRadialController : MonoBehaviour
{
    [SerializeField]
    private Image radial;

    void Start()
    {
        radial = GetComponent<Image>();

        ShipController.ShipDamagedEvent += onShipDamaged;
    }

    private void onShipDamaged(float newHealth, float maxHealth)
    {
        radial.fillAmount = newHealth / maxHealth;
    }
}

