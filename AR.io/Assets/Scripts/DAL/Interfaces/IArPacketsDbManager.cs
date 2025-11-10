using System.Collections.Generic;

using Assets.Scripts.Enums;
using Assets.Scripts.Entities;

namespace Assets.Scripts.DAL.Interfaces
{
	/// <summary>
	/// Describes behaviour of Ar Packets db manager.
	/// </summary>
	public interface IArPacketsDbManager : IDbManager<ArPacket>
	{
		/// <summary>
		/// Get all enabled Ar Packets.
		/// </summary>
		/// <returns>List of Ar Packets</returns>
		List<ArPacket> GetEnabledArPackets();

		/// <summary>
		/// Get enabled Ar Packet db state by its author, name and version.
		/// </summary>
		/// <param name="author">Packet author.</param>
		/// <param name="name">Packet name.</param>
		/// <param name="version">Packet version.</param>
		/// <returns>Ar Packet db state.</returns>
		ArPacketDbState GetArPacketDbState(string author, string name, string version);

		/// <summary>
		/// Get Ar Packet by its author and name.
		/// </summary>
		/// <param name="author">Packet author.</param>
		/// <param name="name">Packet name.</param>
		/// <returns>Ar Packet.</returns>
		ArPacket GetArPacketByAuthorAndName(string author, string name);

		/// <summary>
		/// Update Ar Packet version.
		/// </summary>
		/// <param name="arPacket">Ar Packet.</param>
		/// <returns>Number of created/updated rows.</returns>
		int CreateUpdateArPacket(ArPacket arPacket);
	}
}
