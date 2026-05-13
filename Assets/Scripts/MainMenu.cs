using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject hudPanel;

    [Header("Gameplay Objects")]
    [SerializeField] private GameObject playerRoot;

    private void Start()
    {
        // Запускаем сцену с остановленным временем, пока игрок не нажмет Start.
        Time.timeScale = 0f;

        if (playerRoot != null)
        {
            playerRoot.SetActive(false);
        }

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }

        if (hudPanel != null)
        {
            hudPanel.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OnStartPressed()
    {
        Time.timeScale = 1f;

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }

        if (hudPanel != null)
        {
            hudPanel.SetActive(true);
        }

        if (playerRoot != null)
        {
            playerRoot.SetActive(true);
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetHintText("WASD - движение | Shift - бег | Space - прыжок | Mouse - обзор");
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OnQuitPressed()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

