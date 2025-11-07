using System.Collections.Generic;

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
		/// Gets enabled Ar Packet by its name and author.
		/// </summary>
		/// <param name="name">Packet name.</param>
		/// <param name="author">Packet author.</param>
		/// <returns>Ar Packet.</returns>
		ArPacket GetArPacketByNameAndAuthor(string name, string author);
	}
}
