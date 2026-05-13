using TMPro;
using UnityEngine;

public class LocationZoneTemplate : MonoBehaviour
{
    [Header("Location Data")]
    [SerializeField] private string locationName = "Локация";
    [SerializeField] [TextArea(1, 3)] private string locationSubtitle = "";

    [Header("References")]
    [SerializeField] private LocationTrigger locationTrigger;
    [SerializeField] private TMP_Text worldSignText;

    [Header("Sign Prefix")]
    [SerializeField] private string signPrefix = "Зона:";

    public void SetLocationData(string newLocationName, string newLocationSubtitle)
    {
        locationName = string.IsNullOrWhiteSpace(newLocationName) ? "Локация" : newLocationName;
        locationSubtitle = newLocationSubtitle ?? string.Empty;
        ApplyData();
    }

    public void SetReferences(LocationTrigger trigger, TMP_Text signText)
    {
        locationTrigger = trigger;
        worldSignText = signText;
        ApplyData();
    }

    private void Reset()
    {
        if (locationTrigger == null)
        {
            locationTrigger = GetComponentInChildren<LocationTrigger>();
        }

        ApplyData();
    }

    private void OnValidate()
    {
        ApplyData();
    }

    [ContextMenu("Apply Data")]
    public void ApplyData()
    {
        if (locationTrigger != null)
        {
            locationTrigger.SetLocationData(locationName, locationSubtitle);
        }

        if (worldSignText != null)
        {
            worldSignText.text = string.IsNullOrWhiteSpace(signPrefix)
                ? locationName
                : $"{signPrefix} {locationName}";
        }
    }
}


