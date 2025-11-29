using System;
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
		public List<Func<Task<GameObject>>> GetModels(string author, string packetName)
		{
			var models = new List<Func<Task<GameObject>>>();
			var modelsPathes = GetElementsPathes($"{author}/{packetName}/Models");

			foreach (var path in modelsPathes)
			{
				models.Add(async () => await LoadModelAsync(path));
			}

			return models;
		}

		/// <inheritdoc/>
		public bool IsArPacketDownloaded(string author, string packetName)
			=> Directory.Exists($"{BasePath}/{author}/{packetName}");

		/// <inheritdoc/>
		public void DeletArPacket(string author, string packetName)
		{
			if (IsArPacketDownloaded(author, packetName))
			{
				Directory.Delete($"{BasePath}/{author}/{packetName}", true);
			}
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Get model by path.
		/// </summary>
		/// <param name="path">Path.</param>
		/// <returns>Model.</returns>
		private async Task<GameObject> LoadModelAsync(string path)
		{
			var modelName = Path.GetFileNameWithoutExtension(path);
			var gameObject = new GameObject(modelName);
			var gltf = new GltfImport();

			if (!await gltf.LoadFile(path))
			{
				Debug.LogError("Не вдалося завантажити модель!");

				return null;
			}

			await gltf.InstantiateSceneAsync(gameObject.transform);

			gameObject.transform.position = Vector3.zero;
			gameObject.SetActive(false);

			return gameObject;
		}

		#endregion
	}
}
