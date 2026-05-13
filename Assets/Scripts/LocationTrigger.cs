using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LocationTrigger : MonoBehaviour
{
    [SerializeField] private string locationName = "Новая зона";
    [SerializeField] [TextArea(1, 3)] private string locationSubtitle = "";

    public void SetLocationData(string newName, string newSubtitle)
    {
        locationName = string.IsNullOrWhiteSpace(newName) ? "Новая зона" : newName;
        locationSubtitle = newSubtitle ?? string.Empty;
    }

    private void Reset()
    {
        // Триггер должен быть непроходимой зоной только для детекции входа.
        Collider trigger = GetComponent<Collider>();
        trigger.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLocationEntered(locationName, locationSubtitle);
        }
    }
}


