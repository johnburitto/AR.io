using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using UnityEngine;

using Assets.Scripts.FileManagement.Interfaces;

namespace Assets.Scripts.FileManagement.Implementations
{
	/// <summary>
	/// Realisation of <see cref="IFileManager"/>.
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
			Texture2D marker = new Texture2D(2, 2);
			
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
		public List<string> GetMarkerNames(string path)
			=> Directory.GetFiles($"{BasePath}/{path}").Where(path => !path.EndsWith(".meta")).ToList();

		#endregion
	}
}
