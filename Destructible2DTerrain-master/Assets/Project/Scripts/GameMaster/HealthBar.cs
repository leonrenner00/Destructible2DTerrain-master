using UnityEngine;
using UnityEngine.UI;
using TMPro;               // needed only if you use TMP_Text

public class HealthBar : MonoBehaviour
{
    [Header("References")]
    public PlayerHealth player;          // drag the player prefab/instance
    public Image        fill;            // the green Fill Image
    public TMP_Text     valueLabel;      // optional “100 / 100” text

    void Update()
    {
        if (!player || !fill) return;

        float ratio = player.current / (float)player.maxHealth;
        fill.fillAmount = ratio;

        if (valueLabel)
            valueLabel.text = $"{player.current} / {player.maxHealth}";
    }
}

