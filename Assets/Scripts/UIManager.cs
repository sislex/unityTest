using System.Collections;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD")]
    [SerializeField] private TMP_Text locationText;
    [SerializeField] private TMP_Text hintText;

    [Header("Popup")]
    [SerializeField] private CanvasGroup locationPopupGroup;
    [SerializeField] private TMP_Text locationPopupText;
    [SerializeField] private TMP_Text locationSubtitleText;
    [SerializeField] private float popupDuration = 2.5f;

    private Coroutine popupRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (locationPopupGroup != null)
        {
            locationPopupGroup.alpha = 0f;
        }
    }

    public void SetHintText(string text)
    {
        if (hintText != null)
        {
            hintText.text = text;
        }
    }

    public void ShowLocationEntered(string locationName, string subtitle)
    {
        if (locationText != null)
        {
            locationText.text = locationName;
        }

        if (popupRoutine != null)
        {
            StopCoroutine(popupRoutine);
        }

        popupRoutine = StartCoroutine(ShowLocationPopupRoutine(locationName, subtitle));
    }

    private IEnumerator ShowLocationPopupRoutine(string locationName, string subtitle)
    {
        if (locationPopupText != null)
        {
            locationPopupText.text = $"Вы вошли в: {locationName}";
        }

        if (locationSubtitleText != null)
        {
            locationSubtitleText.text = subtitle;
            locationSubtitleText.gameObject.SetActive(!string.IsNullOrWhiteSpace(subtitle));
        }

        if (locationPopupGroup != null)
        {
            locationPopupGroup.alpha = 1f;
        }

        yield return new WaitForSeconds(popupDuration);

        if (locationPopupGroup != null)
        {
            locationPopupGroup.alpha = 0f;
        }
    }
}

