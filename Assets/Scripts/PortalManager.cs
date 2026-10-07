using System.Collections.Generic;
using UnityEngine;

public class PortalManager : MonoBehaviour
{
    public static PortalManager instance;
    public Dictionary<Portal, Portal> portalPairs = new Dictionary<Portal, Portal>();

    private void Awake()
    {
        if (instance != null) 
        { 
            Destroy(gameObject); 
            return; 
        }

        instance = this;
    }

    public void LinkPortals(Portal portalA, Portal portalB)
    {
        portalPairs[portalA] = portalB;
        portalPairs[portalB] = portalA;

        portalA.UpdatePortals();
        portalB.UpdatePortals();
    }

    public bool TryGetLinkedPortal(Portal source, out Portal linked)
    {
        return portalPairs.TryGetValue(source, out linked);
    }
}