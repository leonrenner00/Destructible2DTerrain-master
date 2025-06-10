using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Image fillImage;
    [SerializeField] private float lerpSpeed = 5f;

    void Update()
    {
        if (playerHealth != null && fillImage != null)
        {
            float targetFill = (float)playerHealth.CurrentHealth / playerHealth.maxHealth;
            fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, targetFill, Time.deltaTime * lerpSpeed);
        }
    }
}