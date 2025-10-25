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
using TMPro;

public class ImageTrackingManager : MonoBehaviour
{
	#region Serializes Fields

	[SerializeField] private List<GameObject> _prefabs = new();
	[SerializeField] private List<Texture2D> _markers = new();
	[SerializeField] private XRReferenceImageLibrary _library;
	[SerializeField] private TextMeshProUGUI _debugInfo;

	#endregion

	#region Private Fields

	private ARTrackedImageManager _arManager;
	private Dictionary<string, GameObject> _arObjects;
	//private IFileManager _fileManager;
	private ILogger _logger;
	//private IArPacketsDbManager _arPacketsDbManager;
	private bool _load = true;

	#endregion

	#region Main Pipeline

	private async void Start()
	{
		_debugInfo.text = "Start";

		//_fileManager = CompositionRoot.FileManager;
		//_logger = CompositionRoot.Logger;
		//_arPacketsDbManager = CompositionRoot.ArPacketsDbManager;

		_debugInfo.text = "After logger";

		_arObjects = new Dictionary<string, GameObject>();
		_arManager = GetComponent<ARTrackedImageManager>();

		_debugInfo.text = "Before runtime library";

		_arManager.referenceLibrary = _arManager.CreateRuntimeLibrary();
		_arManager.enabled = true;

		_debugInfo.text = "After runtime library";

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

	private void Update()
	{
		Debug.Log(ARSession.state);

		if (ARSession.state == ARSessionState.SessionTracking && _load)
		{
			_debugInfo.text = "Before markers loaded";

			LoadMarkers();

			_debugInfo.text = "After markers loaded";

			_load = false;
		}
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
		_debugInfo.text = $"Hello. Images is: {image.ToString()}";

		if (image == null)
		{
			return;
		}

		_debugInfo.text = $"Reference image name: {image.referenceImage.name}\n" +
			$"Image size: {image.referenceImage.size}";

		//if (image.trackingState == TrackingState.Limited ||
		//	image.trackingState == TrackingState.None)
		//{
		//	_arObjects[image.referenceImage.name].gameObject.SetActive(false);

		//	return;
		//}

		//_arObjects[image.referenceImage.name].gameObject.SetActive(true);
		//_arObjects[image.referenceImage.name].transform.position = image.transform.position;
		//_arObjects[image.referenceImage.name].transform.rotation = image.transform.rotation;
	}

	private void LoadMarkers()
	{
		_debugInfo.text = "Get data from db";

		//var arPackets = _arPacketsDbManager.GetEnabledArPackets();
		var runtimeLibrary = _arManager.referenceLibrary as MutableRuntimeReferenceImageLibrary;

		ScheduleMarkers("", runtimeLibrary);

		Debug.Log($"{runtimeLibrary.count}");
		_arManager.referenceLibrary = runtimeLibrary;
	}

	private void ScheduleMarkers(string path, MutableRuntimeReferenceImageLibrary runtimeLibrary)
	{
		_debugInfo.text = "Start to get markers";

		//var filePathes = _fileManager.GetMarkerNames("Test/Markers");
		//var markers = await _fileManager.GetMarkers(filePathes);

		_debugInfo.text = "Start to upload markers";

		foreach (var marker in _markers)
		{
			RenderTexture rt = RenderTexture.GetTemporary(
				marker.width,
				marker.height,
				0,
				RenderTextureFormat.Default,
				RenderTextureReadWrite.Linear);

			Graphics.Blit(marker, rt);
			RenderTexture previous = RenderTexture.active;
			RenderTexture.active = rt;

			Texture2D readableTexture = new Texture2D(marker.width, marker.height);
			readableTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
			readableTexture.Apply();

			var job = runtimeLibrary.ScheduleAddImageWithValidationJob(readableTexture, marker.name, 0.1f);

			job.jobHandle.Complete();
		}

		_debugInfo.text = "Markers uploaded";
	}

	#endregion
}
