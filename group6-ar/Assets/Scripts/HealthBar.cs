using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider healthSlider;

    public void UpdateHealth(int newHealth)
    {
        healthSlider.value = newHealth;
    }
}
