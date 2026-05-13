using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RifleWeapon : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraRoot;
    [SerializeField] private Transform weaponRoot;
    [SerializeField] private Transform muzzlePoint;

    [Header("Fire")]
    [SerializeField] private float fireRate = 12f;
    [SerializeField] private float range = 120f;
    [SerializeField] private LayerMask hitMask = ~0;

    [Header("Impact")]
    [SerializeField] private bool impactOnlyWalls = true;
    [SerializeField] private float impactSize = 0.09f;
    [SerializeField] private float impactOffset = 0.01f;
    [SerializeField] private int maxImpactMarks = 200;

    [Header("Tracer")]
    [SerializeField] private float tracerWidth = 0.018f;
    [SerializeField] private float tracerDuration = 0.06f;
    [SerializeField] private Color tracerColor = new Color(1f, 0.75f, 0.25f, 1f);

    [Header("Chair Knockback")]
    [SerializeField] private float chairImpulse = 18f;
    [SerializeField] private float chairUpwardBias = 0.2f;
    [SerializeField] private float chairRandomTorque = 2.5f;
    [SerializeField] private float chairMass = 3.5f;

    private readonly Queue<GameObject> impactMarks = new Queue<GameObject>();
    private float nextShotTime;
    private Material impactMaterial;
    private Material tracerMaterial;

    private void Awake()
    {
        if (cameraRoot == null)
        {
            cameraRoot = transform;
        }

        EnsureWeaponModel();
    }

    private void Update()
    {
        if (Mouse.current == null || Time.time < nextShotTime)
        {
            return;
        }

        if (!Mouse.current.leftButton.isPressed)
        {
            return;
        }

        float normalizedRate = Mathf.Max(1f, fireRate);
        nextShotTime = Time.time + (1f / normalizedRate);
        Fire();
    }

    private void Fire()
    {
        Vector3 startPoint = muzzlePoint != null ? muzzlePoint.position : cameraRoot.position;
        Ray ray = new Ray(cameraRoot.position, cameraRoot.forward);

        Vector3 endPoint = ray.origin + ray.direction * range;
        if (Physics.Raycast(ray, out RaycastHit hit, range, hitMask, QueryTriggerInteraction.Ignore))
        {
            endPoint = hit.point;

            if (IsChairSurface(hit.transform))
            {
                ApplyChairKnockback(hit, ray.direction);
            }

            if (!impactOnlyWalls || IsWallSurface(hit.transform))
            {
                SpawnImpactMark(hit);
            }
        }

        SpawnTracer(startPoint, endPoint);
    }

    private bool IsWallSurface(Transform target)
    {
        if (target == null)
        {
            return false;
        }

        if (target.CompareTag("Wall"))
        {
            return true;
        }

        Transform current = target;
        while (current != null)
        {
            string n = current.name.ToLowerInvariant();
            if (n.Contains("wall") || n.Contains("walls") || n.Contains("perimeter"))
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private bool IsChairSurface(Transform target)
    {
        if (target == null)
        {
            return false;
        }

        Transform current = target;
        while (current != null)
        {
            string n = current.name.ToLowerInvariant();
            if (n.Contains("chair") || n.Contains("seat") || n.Contains("folding") || n.Contains("chfolding"))
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private void ApplyChairKnockback(RaycastHit hit, Vector3 shotDirection)
    {
        Transform chairRoot = FindChairRoot(hit.transform);
        if (chairRoot == null)
        {
            return;
        }

        Rigidbody rb = hit.rigidbody;
        if (rb == null)
        {
            rb = chairRoot.GetComponent<Rigidbody>();
        }

        if (rb == null)
        {
            rb = chairRoot.gameObject.AddComponent<Rigidbody>();
        }

        // Делаем стул физическим объектом в момент попадания.
        SetStaticRecursively(chairRoot, false);
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.mass = Mathf.Max(0.5f, chairMass);
        rb.linearDamping = 0.08f;
        rb.angularDamping = 0.05f;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        Vector3 forceDirection = (shotDirection + Vector3.up * chairUpwardBias).normalized;
        rb.AddForceAtPosition(forceDirection * chairImpulse, hit.point, ForceMode.Impulse);
        rb.AddTorque(Random.onUnitSphere * chairRandomTorque, ForceMode.Impulse);
    }

    private static Transform FindChairRoot(Transform target)
    {
        Transform current = target;
        Transform fallback = target;

        while (current != null)
        {
            string n = current.name.ToLowerInvariant();
            if (n.Contains("chairmesh") || n.Contains("chair") || n.Contains("chfolding"))
            {
                return current;
            }

            if (n.StartsWith("desk_"))
            {
                fallback = current;
            }

            current = current.parent;
        }

        return fallback;
    }

    private static void SetStaticRecursively(Transform root, bool value)
    {
        if (root == null)
        {
            return;
        }

        root.gameObject.isStatic = value;
        for (int i = 0; i < root.childCount; i++)
        {
            SetStaticRecursively(root.GetChild(i), value);
        }
    }

    private void SpawnImpactMark(RaycastHit hit)
    {
        GameObject mark = GameObject.CreatePrimitive(PrimitiveType.Quad);
        mark.name = "BulletImpact";

        Collider collider = mark.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        mark.transform.SetPositionAndRotation(
            hit.point + hit.normal * impactOffset,
            Quaternion.LookRotation(hit.normal)
        );

        mark.transform.Rotate(0f, 0f, Random.Range(0f, 360f), Space.Self);
        mark.transform.localScale = Vector3.one * impactSize;

        Renderer renderer = mark.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = GetImpactMaterial();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        mark.transform.SetParent(hit.collider.transform, true);

        impactMarks.Enqueue(mark);
        while (impactMarks.Count > Mathf.Max(1, maxImpactMarks))
        {
            GameObject old = impactMarks.Dequeue();
            if (old != null)
            {
                Destroy(old);
            }
        }
    }

    private void SpawnTracer(Vector3 start, Vector3 end)
    {
        GameObject tracerObject = new GameObject("TracerShot");
        TracerShot tracer = tracerObject.AddComponent<TracerShot>();
        tracer.Initialize(start, end, tracerWidth, tracerDuration, tracerColor, GetTracerMaterial());
    }

    private Material GetImpactMaterial()
    {
        if (impactMaterial != null)
        {
            return impactMaterial;
        }

        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
        {
            shader = Shader.Find("Unlit/Color");
        }

        impactMaterial = new Material(shader)
        {
            color = new Color(0.05f, 0.05f, 0.05f, 0.85f)
        };

        return impactMaterial;
    }

    private Material GetTracerMaterial()
    {
        if (tracerMaterial != null)
        {
            return tracerMaterial;
        }

        Shader shader = Shader.Find("Unlit/Color");
        tracerMaterial = new Material(shader)
        {
            color = tracerColor
        };

        return tracerMaterial;
    }

    private void EnsureWeaponModel()
    {
        if (weaponRoot == null)
        {
            GameObject root = new GameObject("RifleViewModel");
            weaponRoot = root.transform;
            weaponRoot.SetParent(cameraRoot, false);
            weaponRoot.localPosition = new Vector3(0.24f, -0.22f, 0.55f);
            weaponRoot.localRotation = Quaternion.Euler(5f, -8f, 0f);
        }

        if (weaponRoot.childCount == 0)
        {
            BuildSimpleRifleMesh();
        }

        if (muzzlePoint == null)
        {
            Transform existing = weaponRoot.Find("Muzzle");
            if (existing != null)
            {
                muzzlePoint = existing;
            }
            else
            {
                muzzlePoint = new GameObject("Muzzle").transform;
                muzzlePoint.SetParent(weaponRoot, false);
                muzzlePoint.localPosition = new Vector3(0f, -0.02f, 0.55f);
                muzzlePoint.localRotation = Quaternion.identity;
            }
        }
    }

    private void BuildSimpleRifleMesh()
    {
        Color baseColor = new Color(0.15f, 0.18f, 0.22f, 1f);
        Color detailColor = new Color(0.1f, 0.12f, 0.14f, 1f);

        CreatePart("Body", new Vector3(0f, -0.05f, 0.25f), new Vector3(0.12f, 0.12f, 0.5f), baseColor);
        CreatePart("Barrel", new Vector3(0f, -0.04f, 0.5f), new Vector3(0.04f, 0.04f, 0.35f), detailColor);
        CreatePart("Stock", new Vector3(0f, -0.06f, 0.0f), new Vector3(0.1f, 0.1f, 0.2f), detailColor);
        CreatePart("Grip", new Vector3(0f, -0.14f, 0.2f), new Vector3(0.06f, 0.16f, 0.08f), detailColor);
    }

    private void CreatePart(string partName, Vector3 localPos, Vector3 localScale, Color color)
    {
        GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
        part.name = partName;
        part.transform.SetParent(weaponRoot, false);
        part.transform.localPosition = localPos;
        part.transform.localRotation = Quaternion.identity;
        part.transform.localScale = localScale;

        Collider collider = part.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        Renderer renderer = part.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            renderer.sharedMaterial = mat;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }
}

