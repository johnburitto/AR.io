using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Assets.Scripts;
using Assets.Scripts.DAL.Interfaces;
using Assets.Scripts.FileManagement.Interfaces;

using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

using ILogger = Assets.Scripts.Logger.Interfaces.ILogger;

public class ImageTrackingManager : MonoBehaviour
{
	#region Serialized Fields

	/// <summary>
	/// Loaded Ar Packets UI manager.
	/// </summary>
	[SerializeField] LoadedArPacketsUIManager _loadedArPacketsUIManager;

	#endregion

	#region Private Fields

	/// <summary>
	/// Ar iamge tarcking manager.
	/// </summary>
	private ARTrackedImageManager _arManager;

	/// <summary>
	/// Ar objects.
	/// </summary>
	private Dictionary<string, GameObject> _arObjects;

	/// <summary>
	/// File manager.
	/// </summary>
	private IFileManager _fileManager;
	
	/// <summary>
	/// Logger.
	/// </summary>
	private ILogger _logger;
	
	/// <summary>
	/// Ar Packets db manager.
	/// </summary>
	private IArPacketsDbManager _arPacketsDbManager;

	#endregion

	#region Public Properties

	/// <summary>
	/// Indexing of Ar objects by their names.
	/// </summary>
	/// <param name="name">Ar object name.</param>
	/// <returns>Ar object.</returns>
	public GameObject this[string name] => _arObjects[name];
	
	#endregion

	#region Main Pipeline

	private async void Start()
	{
		_logger = CompositionRoot.Logger;

		_logger.WriteLog("ImageTrackingManager start Start");

		_fileManager = CompositionRoot.FileManager;
		_arPacketsDbManager = CompositionRoot.ArPacketsDbManager;

		_arObjects = new Dictionary<string, GameObject>();
		_arManager = GetComponent<ARTrackedImageManager>();

		_arManager.referenceLibrary = _arManager.CreateRuntimeLibrary();
		_arManager.enabled = true;

		await LoadMarkersAsync();

		if (_arManager != null)
		{
			_arManager.trackablesChanged.AddListener(OnImagesTrackedChanged);
			await LoadModelsAsync();
		}

		_loadedArPacketsUIManager.UpdateUIinfo(_arManager.referenceLibrary.count, _arObjects.Count);
		_logger.WriteLog($"Images count: {_arManager.referenceLibrary.count}\n\n\nModel count:{_arObjects.Count}");
	}

	private void OnDestroy()
	{
		_arManager.trackablesChanged.RemoveListener(OnImagesTrackedChanged);
	}

	#endregion

	#region Public Methods
	
	/// <summary>
	/// Load to runtime library Ar Packet downloaded from QR code.
	/// </summary>
	public async Task ReloadArPackets()
	{
		_arManager.referenceLibrary = _arManager.CreateRuntimeLibrary();
		
		foreach (var arObject in _arObjects)
		{
			Destroy(arObject.Value);
		}

		_arObjects.Clear();

		await LoadMarkersAsync();
		await LoadModelsAsync();

		_loadedArPacketsUIManager.UpdateUIinfo(_arManager.referenceLibrary.count, _arObjects.Count);
		_logger.WriteLog($"Images count after reload: {_arManager.referenceLibrary.count}\n\n\nModel count after reload:{_arObjects.Count}");
	}

	#endregion

	#region Private Methods

	/// <summary>
	/// Upload models for markers.
	/// </summary>
	private async Task LoadModelsAsync()
	{
		var arPackets = _arPacketsDbManager.GetEnabledArPackets();

		foreach (var arPacket in arPackets)
		{
			if (!_fileManager.IsArPacketDownloaded(arPacket.Author, arPacket.Name))
			{
				continue;
			}

			var models = await _fileManager.GetModels(arPacket.Author, arPacket.Name);

			foreach (var model in models)
			{
				_arObjects.TryAdd(model.name, model);
			}
		}
	}

	/// <summary>
	/// Reacts on changing in tracked images.
	/// </summary>
	/// <param name="eventArgs"></param>
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

	/// <summary>
	/// Updates tracked image.
	/// </summary>
	/// <param name="image">Tracked image.</param>
	private void UpdateTrackedImage(ARTrackedImage image)
	{
		if (image == null)
		{
			return;
		}

		//_logger.WriteLog($"Reference image name: {image.referenceImage.name}\nImage size: {image.referenceImage.size}");

		if (image.trackingState == TrackingState.Limited ||
			image.trackingState == TrackingState.None)
		{
			_arObjects[image.referenceImage.name].SetActive(false);

			return;
		}

		_arObjects[image.referenceImage.name].SetActive(true);
		_arObjects[image.referenceImage.name].transform.position = image.transform.position;
		_arObjects[image.referenceImage.name].transform.rotation = image.transform.rotation;
	}

	/// <summary>
	/// Load markers for Ar tracking.
	/// </summary>
	private async Task LoadMarkersAsync()
	{
		var arPackets = _arPacketsDbManager.GetEnabledArPackets();
		var runtimeLibrary = _arManager.referenceLibrary as MutableRuntimeReferenceImageLibrary;

		foreach (var arPacket in arPackets)
		{
			if (!_fileManager.IsArPacketDownloaded(arPacket.Author, arPacket.Name))
			{
				continue;
			}

			await ScheduleMarkers($"{arPacket.Author}/{arPacket.Name}/Markers", runtimeLibrary);
		}

		_arManager.referenceLibrary = runtimeLibrary;
	}

	/// <summary>
	/// Schedules markers for uploading to runtime library.
	/// </summary>
	/// <param name="path">Marker path.</param>
	/// <param name="runtimeLibrary">Runtime library.</param>
	private async Task ScheduleMarkers(string path, MutableRuntimeReferenceImageLibrary runtimeLibrary)
	{
		_logger.WriteLog("Start to get markers");

		var filePathes = _fileManager.GetElementsPathes(path);
		var markers = await _fileManager.GetMarkers(filePathes);

		foreach (var marker in markers.Select((value, i) => new { i, value }))
		{
			var job = runtimeLibrary.ScheduleAddImageWithValidationJob(marker.value, Path.GetFileNameWithoutExtension(filePathes[marker.i]), 0.1f);

			job.jobHandle.Complete();
		}

		_logger.WriteLog("Markers uploaded");
	}

	#endregion
}
