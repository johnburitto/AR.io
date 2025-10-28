using System.Collections.Generic;

using Assets.Scripts.Entities;

namespace Assets.Scripts.DAL.Interfaces
{
	/// <summary>
	/// Describes behaviour of ar packets db manager.
	/// </summary>
	public interface IArPacketsDbManager : IDbManager<ArPacket>
	{
		/// <summary>
		/// Get all enabled ar packets.
		/// </summary>
		/// <returns>List of ar packets</returns>
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
