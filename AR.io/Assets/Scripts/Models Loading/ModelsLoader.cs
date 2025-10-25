using GLTFast;
using GLTFast.Export;
using GLTFast.Logging;
using System.IO;
using System.Threading.Tasks;

using UnityEngine;

public class ModelsLoader : MonoBehaviour
{
	#region Private Fields

	private string modelUrl = "https://raw.githubusercontent.com/johnburitto/models/refs/heads/main/ford-gt-17.glb";
	private GameObject targetObject;

	#endregion

	#region Main Pipeline

	async void Start()
	{
		await LoadModel(modelUrl);
		await ExportModel();
	}

	#endregion

	#region Private Methods

	private async Task LoadModel(string url)
	{
		targetObject = new GameObject("ModelHolder");

		var gltf = new GltfImport();

		bool success = await gltf.Load(modelUrl);

		if (!success)
		{
			Debug.LogError("Не вдалося завантажити GLTF!");
			return;
		}

		await gltf.InstantiateMainSceneAsync(targetObject.transform);

		//SetVisible(targetObject, false);

		Debug.Log("Модель успішно завантажено!");
	}

	private async Task ExportModel()
	{
		var logger = new CollectingLogger();
		var settings = new ExportSettings { Format = GltfFormat.Binary };
		var exporter = new GameObjectExport(settings, logger: logger);

		exporter.AddScene(new GameObject[] { targetObject }, "scene");
		
		using (var stream = new MemoryStream())
		{
			bool success = await exporter.SaveToStreamAndDispose(stream);

			if (success)
			{
				var path = Path.Combine($"{Directory.GetCurrentDirectory()}/Assets/Models", targetObject.name + ".glb");
				
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

	void SetVisible(GameObject go, bool visible)
	{
		foreach (var renderer in go.GetComponentsInChildren<MeshRenderer>(true))
		{
			renderer.enabled = visible;
		}
	}

	#endregion
}
