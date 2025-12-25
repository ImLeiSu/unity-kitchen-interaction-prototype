using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class OrderBoardUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private TMP_Text timerText;

    [Header("Time UI")]
    [Range(0f, 1f)]
    [SerializeField] private float dangerThreshold01 = 0.25f;

    public void Show(ItemDef item, int need)
    {
        if (icon != null)
            icon.sprite = item != null ? item.icon : null;

        if (countText != null)
            countText.text = $"×{Mathf.Max(0, need)}";

        gameObject.SetActive(true);
    }

    public void Clear()
    {
        if (icon != null) icon.sprite = null;
        if (countText != null) countText.text = "";
        if (timerText != null) timerText.text = "";

        gameObject.SetActive(false);
    }

    public void SetTimeSeconds(float timeLeft, float timeLimit)
    {
        if (timerText == null) return;

        timeLimit = Mathf.Max(1f, timeLimit);
        timeLeft = Mathf.Clamp(timeLeft, 0f, timeLimit);

        int seconds = Mathf.CeilToInt(timeLeft);
        timerText.text = $"Time: {seconds}s";

        float t01 = timeLeft / timeLimit;
        timerText.color = (t01 <= dangerThreshold01) ? Color.red : Color.white;
    }
}
