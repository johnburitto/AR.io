using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Assets.Scripts.FileManagement.Interfaces;

using GLTFast;

using UnityEngine;

namespace Assets.Scripts.FileManagement.Implementations
{
	/// <summary>
	/// Implementation of <see cref="IFileManager"/>.
	/// </summary>
	public class FileManager : IFileManager
	{
		#region Public Properties

#if UNITY_EDITOR
		/// <inheritdoc/>
		public string BasePath { get; set; } = $"{Directory.GetCurrentDirectory()}/Assets/ARPackets";
#else
		/// <inheritdoc/>
		public string BasePath { get; set; } = $"{Application.persistentDataPath}/Assets/ARPackets";
#endif

		#endregion

		#region Implementation of IFileManager

		/// <inheritdoc/>
		public async Task<Texture2D> GetMarker(string path)
		{
			var marker = new Texture2D(2, 2);
			
			marker.LoadImage(await File.ReadAllBytesAsync(path));

			return marker;
		}

		/// <inheritdoc/>
		public async Task<List<Texture2D>> GetMarkers(List<string> pathes)
		{
			var markers = new List<Texture2D>();

			foreach (var path in pathes)
			{
				markers.Add(await GetMarker(path));
			}

			return markers;
		}

		/// <inheritdoc/>
		public async Task<Texture2D> GetLogo(string path)
		{
			var logo = new Texture2D(2, 2);

			logo.LoadImage(await File.ReadAllBytesAsync(path));

			return logo;
		}

		/// <inheritdoc/>
		public List<string> GetElementsPathes(string path)
			=> Directory.GetFiles($"{BasePath}/{path}").Where(path => !path.EndsWith(".meta")).ToList();

		/// <inheritdoc/>
		public async Task<List<GameObject>> GetModels(string author, string packetName)
		{
			var models = new List<GameObject>();
			var modelsPathes = GetElementsPathes($"{author}/{packetName}/Models");

			foreach (var path in modelsPathes)
			{
				var modelName = Path.GetFileNameWithoutExtension(path);
				var gameObject = new GameObject(modelName);
				var gltf = new GltfImport();

				using (var stream = File.OpenRead(path))
				{
					if (!await gltf.LoadStream(stream))
					{
						Debug.LogError("Не вдалося завантажити модель!");
						
						continue;
					}

					await gltf.InstantiateMainSceneAsync(gameObject.transform);
					
					gameObject.transform.position = Vector3.zero;
					gameObject.SetActive(false);
					
					models.Add(gameObject);
				}
			}

			return models;
		}

		/// <inheritdoc/>
		public bool IsArPacketDownloaded(string author, string packetName)
			=> Directory.Exists($"{BasePath}/{author}/{packetName}");

		#endregion
	}
}
