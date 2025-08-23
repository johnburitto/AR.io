using System.Threading.Tasks;
using System.Collections.Generic;

using UnityEngine;

namespace Assets.Scripts.FileManagement.Interfaces
{
	/// <summary>
	/// Describes behaviour of file manager.
	/// </summary>
	public interface IFileManager
	{
		/// <summary>
		/// Base path to ar packets folders.
		/// </summary>
		string BasePath { get; set; }

		/// <summary>
		/// Get marker by path.
		/// </summary>
		/// <param name="path">Path to marker.</param>
		/// <returns>Marker's 2D texture.</returns>
		Task<Texture2D> GetMarker(string path);

		/// <summary>
		/// Get markers by pathes.
		/// </summary>
		/// <param name="pathes">Markers pathes.</param>
		/// <returns>List of marker's 2D textures.</returns>
		Task<List<Texture2D>> GetMarkers(List<string> pathes);

		/// <summary>
		/// Get markers names.
		/// </summary>
		/// <param name="path">Path to markers.</param>
		/// <returns>List of markers names.</returns>
		List<string> GetMarkerNames(string path);
	}
}
