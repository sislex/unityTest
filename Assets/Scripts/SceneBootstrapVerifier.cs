using UnityEngine;

public class SceneBootstrapVerifier : MonoBehaviour
{
    [Header("Optional References")]
    [SerializeField] private MainMenu mainMenu;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private PlayerMovement playerMovement;

    private void Start()
    {
        // Небольшой рантайм-верификатор, чтобы быстро видеть критичные пропуски в Console.
        if (mainMenu == null)
        {
            Debug.LogWarning("[SceneBootstrapVerifier] MainMenu reference is not assigned.", this);
        }

        if (uiManager == null)
        {
            Debug.LogWarning("[SceneBootstrapVerifier] UIManager reference is not assigned.", this);
        }

        if (playerMovement == null)
        {
            Debug.LogWarning("[SceneBootstrapVerifier] PlayerMovement reference is not assigned.", this);
        }
    }
}

