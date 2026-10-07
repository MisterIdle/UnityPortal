using UnityEngine;

public class PlayerPortalClone : MonoBehaviour
{
    [Header("Visuel du Joueur")]
    [SerializeField] private GameObject playerVisualBox;

    [Header("Paramètres")]
    [SerializeField] private float activationDistance = 1.5f;

    private GameObject cloneObject;

    private void Start()
    {
        cloneObject = Instantiate(playerVisualBox);
        cloneObject.name = "Player_Clone_Visual";
        cloneObject.transform.SetParent(null);

        if (cloneObject.TryGetComponent(out Collider col)) Destroy(col);

        foreach (MonoBehaviour script in cloneObject.GetComponents<MonoBehaviour>())
            Destroy(script);

        cloneObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (PortalManager.instance == null || PortalManager.instance.portalPairs.Count == 0) return;

        Portal closestPortal = null;
        float minDistance = activationDistance;

        foreach (Portal portal in PortalManager.instance.portalPairs.Keys)
        {
            float dist = Vector3.Distance(playerVisualBox.transform.position, portal.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closestPortal = portal;
            }
        }

        if (closestPortal != null && PortalManager.instance.TryGetLinkedPortal(closestPortal, out Portal linkedPortal))
        {
            cloneObject.SetActive(true);

            cloneObject.transform.position = PortalMathUtility.TransformPosition(playerVisualBox.transform.position, closestPortal.transform, linkedPortal.transform);
            cloneObject.transform.rotation = PortalMathUtility.TransformRotation(playerVisualBox.transform.rotation, closestPortal.transform, linkedPortal.transform);
            
            Debug.DrawLine(playerVisualBox.transform.position, closestPortal.transform.position, Color.yellow);
            Debug.DrawLine(linkedPortal.transform.position, cloneObject.transform.position, Color.magenta);
        }
        else
        {
            cloneObject.SetActive(false);
        }
    }
}