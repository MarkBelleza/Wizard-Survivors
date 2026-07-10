using System.Collections.Generic;
using UnityEngine;

public class PickupManager : MonoBehaviour
{
    public static PickupManager Instance;
    
    private List<Rotator> pickups = new List<Rotator>();

    void Awake()
    {
        Instance = this;
    }

    public void RegisterPickup(Rotator pickup)
    {
        if (!pickups.Contains(pickup))
        {
            pickups.Add(pickup);
        }
    }

    public void UnregisterPickup(Rotator pickup)
    {
        pickups.Remove(pickup);
    }

    public int GetCurrentPickupCount()
    {
        pickups.RemoveAll(p => p == null); // Clean up any destroyed pickups from the list, bug prevention
        return pickups.Count;
    }

}
