using System;
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

using TMPro;

using ILogger = Assets.Scripts.Logger.Interfaces.ILogger;

public class ImageTrackingManager : MonoBehaviour
{
	#region Serialized Fields

	/// <summary>
	/// Degug info.
	/// </summary>
	[SerializeField] private TextMeshProUGUI _debugInfo;

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
	///  Ar Packets db manager.
	/// </summary>
	private IArPacketsDbManager _arPacketsDbManager;

	#endregion

	#region Main Pipeline

	private async void Start()
	{
		_debugInfo.text = "Start";

		_fileManager = CompositionRoot.FileManager;
		_logger = CompositionRoot.Logger;
		_arPacketsDbManager = CompositionRoot.ArPacketsDbManager;

		_debugInfo.text = "After logger";

		_arObjects = new Dictionary<string, GameObject>();
		_arManager = GetComponent<ARTrackedImageManager>();

		_debugInfo.text = "Before runtime library";

		_arManager.referenceLibrary = _arManager.CreateRuntimeLibrary();
		_arManager.enabled = true;

		_debugInfo.text = "After runtime library";

		_debugInfo.text = "Before markers loaded";

		await LoadMarkersAsync();

		_debugInfo.text = "After markers loaded";

		_debugInfo.text = "Try Load Models";

		try
		{
			if (_arManager != null)
			{
				_arManager.trackablesChanged.AddListener(OnImagesTrackedChanged);
				await LoadModelsAsync();
			}
		}
		catch (Exception e)
		{
			_debugInfo.text = $"{e.Message}\n\n\n{e.InnerException}";
		}

		_debugInfo.text = $"Images count: {_arManager.referenceLibrary.count}\n\n\nModel count:{_arObjects.Count}";
	}

	private void OnDestroy()
	{
		_arManager.trackablesChanged.RemoveListener(OnImagesTrackedChanged);
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
		_debugInfo.text = $"Hello From Image Tracking";

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

		_debugInfo.text = $"Reference image name: {image.referenceImage.name}\n" +
			$"Image size: {image.referenceImage.size}";

		//if (image.trackingState == TrackingState.Limited ||
		//	image.trackingState == TrackingState.None)
		//{
		//	_arObjects[image.referenceImage.name].SetActive(false);

		//	return;
		//}

		//_arObjects[image.referenceImage.name].SetActive(true);
		//_arObjects[image.referenceImage.name].transform.position = image.transform.position;
		//_arObjects[image.referenceImage.name].transform.rotation = image.transform.rotation;
	}

	/// <summary>
	/// Load markers for Ar tracking.
	/// </summary>
	private async Task LoadMarkersAsync()
	{
		_debugInfo.text = "Get data from db";

		var arPackets = _arPacketsDbManager.GetEnabledArPackets();
		var runtimeLibrary = _arManager.referenceLibrary as MutableRuntimeReferenceImageLibrary;

		foreach (var arPacket in arPackets)
		{
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
		_debugInfo.text = "Start to get markers";

		var filePathes = _fileManager.GetElementsPathes(path);
		var markers = await _fileManager.GetMarkers(filePathes);

		_debugInfo.text = "Start to upload markers";

		foreach (var marker in markers.Select((value, i) => new { i, value }))
		{
			var job = runtimeLibrary.ScheduleAddImageWithValidationJob(marker.value, Path.GetFileNameWithoutExtension(filePathes[marker.i]), 0.1f);

			job.jobHandle.Complete();
		}

		_debugInfo.text = "Markers uploaded";
	}

	#endregion
}
