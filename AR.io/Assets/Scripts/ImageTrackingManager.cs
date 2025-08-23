using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

using Assets.Scripts;
using Assets.Scripts.Enums;
using Assets.Scripts.Entities;
using Assets.Scripts.DAL.Interfaces;
using Assets.Scripts.FileManagement.Interfaces;

using ILogger = Assets.Scripts.Logger.Interfaces.ILogger;

public class ImageTrackingManager : MonoBehaviour
{
	#region Serializes Fields

	[SerializeField] private List<GameObject> _prefabs = new();

	#endregion

	#region Private Fields

	private ARTrackedImageManager _arManager;
	private Dictionary<string, GameObject> _arObjects;
	private IFileManager _fileManager;
	private ILogger _logger;
	private MutableRuntimeReferenceImageLibrary _mutableLibrary;
	private IArPacketsDbManager _arPacketsDbManager;

	#endregion

	#region Main Pipeline

	private async void Start()
	{
		_fileManager = CompositionRoot.FileManager;
		_logger = CompositionRoot.Logger;
		_arPacketsDbManager = CompositionRoot.ArPacketsDbManager;

		_arObjects = new Dictionary<string, GameObject>();
		_arManager = GetComponent<ARTrackedImageManager>();

		await LoadMarkers();

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

	#endregion

	#region Private Methods

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

	private async Task LoadMarkers()
	{
		var arPackets = _arPacketsDbManager.GetEnabledArPackets();
		var runtimeLibrary = _arManager.referenceLibrary as MutableRuntimeReferenceImageLibrary;

		foreach (var packet in arPackets)
		{
			await ScheduleMarkers($"{packet.Name}/Markers", runtimeLibrary);
		}

		_logger.WriteLog($"{runtimeLibrary.count}");
		_arManager.referenceLibrary = runtimeLibrary;
	}

	private async Task ScheduleMarkers(string path, MutableRuntimeReferenceImageLibrary runtimeLibrary)
	{
		var filePathes = _fileManager.GetMarkerNames("Test/Markers");
		var markers = await _fileManager.GetMarkers(filePathes);

		foreach (var marker in markers.Select((value, i) => new { i, value }))
		{
			var job = runtimeLibrary.ScheduleAddImageWithValidationJob(marker.value, Path.GetFileNameWithoutExtension(filePathes[marker.i]), 0.1f);

			job.jobHandle.Complete();
		}
	}

	#endregion
}
