using System;
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
		/// Base path to Ar Packets folders.
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
		/// Get logo by path.
		/// </summary>
		/// <param name="path">Logo path.</param>
		/// <returns>Logo 2D texture.</returns>
		Task<Texture2D> GetLogo(string path);

		/// <summary>
		/// Get markers names.
		/// </summary>
		/// <param name="path">Path to markers.</param>
		/// <returns>List of markers names.</returns>
		List<string> GetElementsPathes(string path);

		/// <summary>
		/// Get Ar Packet models.
		/// </summary>
		/// <param name="author">Author.</param>
		/// <param name="packetName">Ar Packet name.</param>
		/// <returns>List of Ar Packet models.</returns>
		List<Func<Task<GameObject>>> GetModels(string author, string packetName);

		/// <summary>
		/// Check if Ar Packet is downloaded.
		/// </summary>
		/// <param name="author">Author.</param>
		/// <param name="packetName">Ar Packet name.</param>
		/// <returns>Whether Ar Packet is downloaded or not.</returns>
		bool IsArPacketDownloaded(string author, string packetName);

		/// <summary>
		/// Delete Ar Packet from local storage.
		/// </summary>
		/// <param name="author">Author.</param>
		/// <param name="packetName">Ar Packet name.</param>
		void DeletArPacket(string author, string packetName);
	}
}
