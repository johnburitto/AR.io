using System.IO;
using System.Threading.Tasks;

using Assets.Scripts.LoadEntities;
using Assets.Scripts.FileManagement.Interfaces;
using Assets.Scripts.FileManagement.Implementations;

using GLTFast;
using GLTFast.Export;
using GLTFast.Logging;

using UnityEngine;
using UnityEngine.Networking;


/// <summary>
/// Ar Packets loader.
/// </summary>
public class ArPacketLoader : MonoBehaviour
{
	#region Private Fields

	/// <summary>
	/// Placeholder for downloaded objects.
	/// </summary>
	private GameObject _targetObject;

	/// <summary>
	/// File manager.
	/// </summary>
	private IFileManager _fileManager;

	#endregion

	#region Main Pipeline

	private void Start()
	{
		_fileManager = new FileManager();
	}

	#endregion

	#region Public Methods

	/// <summary>
	/// Process Ar Packet source.
	/// </summary>
	/// <param name="arPacketSource">Ar Packet source.</param>
	public async Task ProcessArPacketSource(ArPacketSource arPacketSource)
	{
		CreateDirectories(arPacketSource.Author, arPacketSource.Name);

		foreach (var element in arPacketSource.Elements)
		{
			await DownloadAndSaveMarker(element.MarkerUrl, arPacketSource.Author, arPacketSource.Name, element.Name);
			await LoadModel(element.ModelUrl, element.Name);
			await ExportModel(arPacketSource.Author,arPacketSource.Name, element.Name);
		}
	}

	#endregion

	#region Private Methods

	/// <summary>
	/// Creates Ar Packet folders.
	/// </summary>
	/// <param name="packetName">Ar Packet name.</param>
	private void CreateDirectories(string author, string packetName)
	{
		if (!Directory.Exists($"{_fileManager.BasePath}/{author}/{packetName}"))
		{
			Directory.CreateDirectory($"{_fileManager.BasePath}/{author}/{packetName}");
		}

		if (!Directory.Exists($"{_fileManager.BasePath}/{author}/{packetName}/Models"))
		{
			Directory.CreateDirectory($"{_fileManager.BasePath}/{author}/{packetName}/Models");
		}

		if (!Directory.Exists($"{_fileManager.BasePath}/{author}/{packetName}/Markers"))
		{
			Directory.CreateDirectory($"{_fileManager.BasePath}/{author}/{packetName}/Markers");
		}
	}

	/// <summary>
	/// Downloads marker.
	/// </summary>
	/// <param name="url">Marker url.</param>
	/// <param name="author">Ar Packet author.</param>
	/// <param name="packetName">Ar Packet name.</param>
	/// <param name="elementName">Element name.</param>
	private async Task DownloadAndSaveMarker(string url, string author, string packetName, string elementName)
	{
		using (UnityWebRequest request = UnityWebRequest.Get(url))
		{
			request.downloadHandler = new DownloadHandlerFile($"{_fileManager.BasePath}/{author}/{packetName}/Markers/{elementName}.png");

			await request.SendWebRequest();

			if (request.result == UnityWebRequest.Result.Success)
			{
				Debug.Log($"Файл успішно завантажено: {_fileManager.BasePath}/{author}/{packetName}/Markers/{elementName}.png");
			}
			else
			{
				Debug.LogError($"Помилка при завантаженні: {request.error}");
			}
		}
	}

	/// <summary>
	/// Downloads model.
	/// </summary>
	/// <param name="url">Model url.</param>
	/// <param name="elementName">Element name.</param>
	private async Task LoadModel(string url, string elementName)
	{
		_targetObject = new GameObject(elementName);

		var gltf = new GltfImport();

		bool success = await gltf.Load(url);

		if (!success)
		{
			Debug.LogError("Не вдалося завантажити GLTF!");
			return;
		}

		await gltf.InstantiateMainSceneAsync(_targetObject.transform);

		Debug.Log("Модель успішно завантажено!");
	}

	/// <summary>
	/// Saves model on storage.
	/// </summary>
	/// <param name="author">Ar Packet author.</param>
	/// <param name="packetName">Ar Packet name.</param>
	/// <param name="elementName">Element name.</param>
	private async Task ExportModel(string author, string packetName, string elementName)
	{
		var logger = new CollectingLogger();
		var settings = new ExportSettings { Format = GltfFormat.Binary };
		var exporter = new GameObjectExport(settings, logger: logger);

		_targetObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

		exporter.AddScene(new GameObject[] { _targetObject }, "scene");
		
		using (var stream = new MemoryStream())
		{
			bool success = await exporter.SaveToStreamAndDispose(stream);

			if (success)
			{
				var path = Path.Combine($"{_fileManager.BasePath}/{author}/{packetName}/Models/{elementName}.glb");
				
				File.WriteAllBytes(path, stream.ToArray());
				Debug.Log("Saved: " + path);
				Destroy(_targetObject);
			}
			else
			{
				logger.LogAll();
			}
		}
	}

	#endregion
}
