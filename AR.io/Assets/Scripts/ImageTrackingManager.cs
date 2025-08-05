using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ImageTrackingManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> _prefabs = new List<GameObject>();

    private ARTrackedImageManager _arManager;
    private Dictionary<string, GameObject> _arObjects;

    private void Start()
    {
        _arObjects = new Dictionary<string, GameObject>();
        _arManager = GetComponent<ARTrackedImageManager>();

        if (_arManager != null)
        {
            _arManager.trackablesChanged.AddListener(OnImagesTrackedChanged);
            UploadArObjects();
        }
    }

    private void OnDestroy()
    {
        _arManager.trackablesChanged.RemoveListener(OnImagesTrackedChanged);
    }

    private void UploadArObjects()
    {
        foreach (var prefab in _prefabs)    
        {
            var arObject = Instantiate(prefab, Vector3.zero, Quaternion.identity);

            arObject.name = prefab.name;
            arObject.gameObject.SetActive(false);
            _arObjects.TryAdd(arObject.name, arObject);
        }
    }

    private void OnImagesTrackedChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        foreach (var item in eventArgs.added)
        {
            UpdateTrackedImage(item);
        }

        foreach (var item in eventArgs.updated)
        {
            UpdateTrackedImage(item);
        }

        foreach (var item in eventArgs.removed)
        {
            UpdateTrackedImage(item.Value);
        }
    }

    private void UpdateTrackedImage(ARTrackedImage image)
    {
        if (image == null)
        {
            return;
        }

        if (image.trackingState == TrackingState.Limited ||
            image.trackingState == TrackingState.None)
        {
            _arObjects[image.referenceImage.name].gameObject.SetActive(false);

            return;
        }

        _arObjects[image.referenceImage.name].gameObject.SetActive(true);
        _arObjects[image.referenceImage.name].transform.position = image.transform.position;
        _arObjects[image.referenceImage.name].transform.rotation = image.transform.rotation;
    }
}
