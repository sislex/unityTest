using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TracerShot : MonoBehaviour
{
    private LineRenderer line;
    private float duration;
    private float timer;
    private Color baseColor;

    public void Initialize(Vector3 start, Vector3 end, float width, float shotDuration, Color color, Material material)
    {
        line = GetComponent<LineRenderer>();
        duration = Mathf.Max(0.01f, shotDuration);
        baseColor = color;

        line.useWorldSpace = true;
        line.positionCount = 2;
        line.SetPosition(0, start);
        line.SetPosition(1, end);

        line.startWidth = width;
        line.endWidth = width * 0.7f;
        line.alignment = LineAlignment.View;
        line.textureMode = LineTextureMode.Stretch;
        line.material = material;
        line.numCapVertices = 2;

        line.startColor = color;
        line.endColor = color;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / duration);

        Color c = baseColor;
        c.a = 1f - t;

        if (line != null)
        {
            line.startColor = c;
            line.endColor = c;
        }

        if (timer >= duration)
        {
            Destroy(gameObject);
        }
    }
}

