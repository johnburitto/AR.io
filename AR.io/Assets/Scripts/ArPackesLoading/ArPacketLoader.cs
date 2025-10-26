using System.IO;
using System.Collections;
using System.Threading.Tasks;

using Assets.Scripts.LoadEntities;

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
	private GameObject targetObject;

	#endregion

	#region Public Methods

	/// <summary>
	/// Process ar packet source.
	/// </summary>
	/// <param name="arPacketSource">Ar packet source.</param>
	public async Task ProcessArPacketSource(ArPacketSource arPacketSource)
	{
		CreateDirectories(arPacketSource.Name);

		foreach (var element in arPacketSource.Elements)
		{
			StartCoroutine(DownloadAndSaveMarker(element.MarkerUrl, arPacketSource.Name, element.Name));
			await LoadModel(element.ModelUrl);
			await ExportModel(arPacketSource.Name, element.Name);
		}
	}

	#endregion

	#region Private Methods

	/// <summary>
	/// Creates ar packet folders.
	/// </summary>
	/// <param name="packetName">Ar packet name.</param>
	private void CreateDirectories(string packetName)
	{
		if (!Directory.Exists($"{Directory.GetCurrentDirectory()}/Assets/ARPackets/{packetName}"))
		{
			Directory.CreateDirectory($"{Directory.GetCurrentDirectory()}/Assets/ARPackets/{packetName}");
		}

		if (!Directory.Exists($"{Directory.GetCurrentDirectory()}/Assets/ARPackets/{packetName}/Models"))
		{
			Directory.CreateDirectory($"{Directory.GetCurrentDirectory()}/Assets/ARPackets/{packetName}/Models");
		}

		if (!Directory.Exists($"{Directory.GetCurrentDirectory()}/Assets/ARPackets/{packetName}/Markers"))
		{
			Directory.CreateDirectory($"{Directory.GetCurrentDirectory()}/Assets/ARPackets/{packetName}/Markers");
		}
	}

	/// <summary>
	/// Downloads marker.
	/// </summary>
	/// <param name="url">Marker url.</param>
	/// <param name="packetName">Ar packet name.</param>
	/// <param name="elementName">Element name.</param>
	private IEnumerator DownloadAndSaveMarker(string url, string packetName, string elementName)
	{
		using (UnityWebRequest request = UnityWebRequest.Get(url))
		{
			request.downloadHandler = new DownloadHandlerFile($"{Directory.GetCurrentDirectory()}/Assets/ARPackets/{packetName}/Markers/{elementName}.png");

			yield return request.SendWebRequest();

			if (request.result == UnityWebRequest.Result.Success)
			{
				Debug.Log($"Файл успішно завантажено: {Directory.GetCurrentDirectory()}/Assets/ARPackets/{packetName}/Markers/{elementName}.png");
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
	private async Task LoadModel(string url)
	{
		targetObject = new GameObject("ModelHolder");
		targetObject.transform.position = new Vector3(0, 0, 300);

		var gltf = new GltfImport();

		bool success = await gltf.Load(url);

		if (!success)
		{
			Debug.LogError("Не вдалося завантажити GLTF!");
			return;
		}

		await gltf.InstantiateMainSceneAsync(targetObject.transform);

		Debug.Log("Модель успішно завантажено!");
	}

	/// <summary>
	/// Saves model on storage.
	/// </summary>
	/// <param name="packetName">Ar packet name.</param>
	/// <param name="elementName">Element name.</param>
	private async Task ExportModel(string packetName, string elementName)
	{
		var logger = new CollectingLogger();
		var settings = new ExportSettings { Format = GltfFormat.Binary };
		var exporter = new GameObjectExport(settings, logger: logger);

		targetObject.transform.localScale = new Vector3(50, 50, 50);

		exporter.AddScene(new GameObject[] { targetObject }, "scene");
		
		using (var stream = new MemoryStream())
		{
			bool success = await exporter.SaveToStreamAndDispose(stream);

			if (success)
			{
				var path = Path.Combine($"{Directory.GetCurrentDirectory()}/Assets/ARPackets/{packetName}/Models/{elementName}.glb");
				
				File.WriteAllBytes(path, stream.ToArray());
				Debug.Log("Saved: " + path);
				Destroy(targetObject);
			}
			else
			{
				logger.LogAll();
			}
		}
	}

	#endregion
}
