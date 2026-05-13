using UnityEditor;
using UnityEngine;

public static class CombatSetupTools
{
    [MenuItem("Tools/Research Complex/Setup Rifle On Main Camera")]
    public static void SetupRifleOnMainCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            cam = Object.FindFirstObjectByType<Camera>();
        }

        if (cam == null)
        {
            Debug.LogError("[CombatSetupTools] No camera found in scene.");
            return;
        }

        RifleWeapon rifle = cam.GetComponent<RifleWeapon>();
        if (rifle == null)
        {
            rifle = Undo.AddComponent<RifleWeapon>(cam.gameObject);
            EditorUtility.SetDirty(cam.gameObject);
            Debug.Log("[CombatSetupTools] RifleWeapon added to camera.");
        }
        else
        {
            Debug.Log("[CombatSetupTools] RifleWeapon is already configured on camera.");
        }
    }
}

