using UnityEngine;
using UnityEngine.InputSystem;

public class PortalShooter : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject portalPrefab;

    [Header("Paramètres")]
    [SerializeField] private float wallOffset = 0.01f;

    [ColorUsage(true, true)] public Color blueColor = new Color(0f, 1.5f, 3f);
    [ColorUsage(true, true)] public Color orangeColor = new Color(3f, 1f, 0f);

    private Portal activeBluePortal;
    private Portal activeOrangePortal;

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (Mouse.current == null) 
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
            activeBluePortal = ShootPortal(activeBluePortal, blueColor);

        else if (Mouse.current.rightButton.wasPressedThisFrame)
            activeOrangePortal = ShootPortal(activeOrangePortal, orangeColor);
    }

    private Portal ShootPortal(Portal targetPortal, Color portalColor)
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.DrawLine(ray.origin, hit.point, Color.white, 2f);

            Debug.DrawRay(hit.point, hit.normal * 1f, Color.green, 2f);

            Vector3 upDirection = Mathf.Abs(hit.normal.y) > 0.9f ? transform.forward : Vector3.up;
            Quaternion portalRotation = Quaternion.LookRotation(hit.normal, upDirection);

            Debug.DrawRay(hit.point, upDirection * 1f, Color.yellow, 2f);

            Vector3 offsetPosition = hit.point + (hit.normal * wallOffset);

            if (targetPortal == null)
                targetPortal = Instantiate(portalPrefab, offsetPosition, portalRotation).GetComponent<Portal>();
            else
                targetPortal.transform.SetPositionAndRotation(offsetPosition, portalRotation);

            targetPortal.SetPortalColor(portalColor);
            TryLinkPortals();
        }
        return targetPortal;
    }

    private void TryLinkPortals()
    {
        if (activeBluePortal != null && activeOrangePortal != null)
        {
            PortalManager.instance.LinkPortals(activeBluePortal, activeOrangePortal);
        }
    }
}