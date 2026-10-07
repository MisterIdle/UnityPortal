using UnityEngine;
using UnityEngine.Rendering;

public class Portal : MonoBehaviour
{
    [SerializeField] private Camera portalCamera;
    [SerializeField] private MeshRenderer portalScreen;

    [Header("Paramètres Mathématiques")]
    [SerializeField] private float portalRadius = 1.0f;
    [SerializeField] private float teleportCooldown = 0.1f;

    private RenderTexture viewTexture;
    private Camera playerCam;
    private Transform playerTransform;

    private float lastTeleportTime;
    private bool wasInFront;
    private bool isDotInitialized = false;

    private void Awake()
    {
        playerCam = Camera.main;
        if (playerCam != null) playerTransform = playerCam.transform;

        viewTexture = new RenderTexture(Screen.width, Screen.height, 24);
        portalCamera.targetTexture = viewTexture;
        portalCamera.enabled = true;
    }

    private void OnEnable() => RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
    private void OnDisable() => RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;

    private void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        Shader.SetGlobalFloat("_IsPortalCamera", camera == portalCamera ? 1f : 0f);
    }

    public void UpdatePortals()
    {
        if (PortalManager.instance.TryGetLinkedPortal(this, out Portal linkedPortal))
        {
            SetTexture(linkedPortal.GetViewTexture());
        }
    }

    private void LateUpdate()
    {
        if (playerTransform == null) return;

        if (PortalManager.instance.TryGetLinkedPortal(this, out Portal linkedPortal))
        {
            // --- OPTIMISATION : Utilisation de l'utilitaire mathématique ---
            portalCamera.transform.position = PortalMathUtility.TransformPosition(playerTransform.position, transform, linkedPortal.transform);
            portalCamera.transform.rotation = PortalMathUtility.TransformRotation(playerTransform.rotation, transform, linkedPortal.transform);

            SetNearClipPlane();
        }

        CheckTeleportationMath();
    }

    private void SetNearClipPlane()
    {
        Vector3 clipPos = transform.position + transform.forward * 0.01f;
        Vector4 clipPlaneCameraSpace = PortalMathUtility.CalculateClipPlaneSpace(portalCamera, clipPos, transform.forward);
        portalCamera.projectionMatrix = playerCam.CalculateObliqueMatrix(clipPlaneCameraSpace);
    }

    private void CheckTeleportationMath()
    {
        if (Time.time - lastTeleportTime < teleportCooldown) return;

        if (PortalManager.instance.TryGetLinkedPortal(this, out Portal linkedPortal))
        {
            Vector3 offsetFromPortal = playerTransform.position - transform.position;
            float dotProduct = Vector3.Dot(transform.forward, offsetFromPortal);
            bool isInFront = dotProduct > 0f;

            if (!isDotInitialized)
            {
                wasInFront = isInFront;
                isDotInitialized = true;
                return;
            }

            float distanceToPlane = Mathf.Abs(dotProduct);

            // DEBUG : Affiche une ligne rouge si le joueur est proche du plan de téléportation
            if (distanceToPlane < 2.0f)
                Debug.DrawRay(transform.position, offsetFromPortal, Color.red);

            if (wasInFront && !isInFront && distanceToPlane < 2.0f)
            {
                Vector3 projectedOffset = offsetFromPortal - (transform.forward * dotProduct);

                if (projectedOffset.magnitude <= portalRadius)
                {
                    TeleportPlayer(linkedPortal);
                    linkedPortal.SetCooldown();
                    linkedPortal.ResetMathState();
                    this.ResetMathState();
                    return;
                }
            }
            wasInFront = isInFront;
        }
    }

    public void SetCooldown() => lastTeleportTime = Time.time;
    public void ResetMathState() => isDotInitialized = false;

    private void TeleportPlayer(Portal linkedPortal)
    {
        Vector3 newPosition = PortalMathUtility.TransformPosition(playerTransform.position, transform, linkedPortal.transform);

        playerTransform.position = newPosition + (linkedPortal.transform.forward * 0.1f);

        playerTransform.rotation = PortalMathUtility.TransformRotation(playerTransform.rotation, transform, linkedPortal.transform);

        if (playerTransform.TryGetComponent(out CameraController camController))
        {
            camController.SyncRotation();
        }
    }

    public RenderTexture GetViewTexture() => viewTexture;
    public void SetTexture(RenderTexture tex) => portalScreen.material.mainTexture = tex;
    public void SetPortalColor(Color color) { if (portalScreen != null) portalScreen.material.SetColor("_GlowColor", color); }
    private void OnDestroy() { if (viewTexture != null) viewTexture.Release(); }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 1.5f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, portalRadius);
    }
}