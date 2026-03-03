using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationInfo : MonoBehaviour
{
    [SerializeField] private GameSceneSO _locationToLoad = default;

    [SerializeField] private LoadEventChannelSO _loadLocation = default;

    [SerializeField] private LocationID _currentLocation;

    private void OnEnable()
    {
        UIEvent.OnLocationToLoad += LocationLoading;
    }

    private void OnDisable()
    {
        UIEvent.OnLocationToLoad -= LocationLoading;
    }
    public void LocationLoading()
    {
        _loadLocation.RaiseEvent(_locationToLoad, false);
    }
}
