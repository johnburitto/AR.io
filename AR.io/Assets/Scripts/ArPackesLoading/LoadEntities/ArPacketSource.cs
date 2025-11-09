using System.Collections.Generic;

namespace Assets.Scripts.LoadEntities
{
	/// <summary>
	/// Holds information about all Ar Packet's elements.
	/// </summary>
	public class ArPacketSource
	{
		/// <summary>
		/// Ar Packet name.
		/// </summary>
		public string Name { get; set; }
		
		/// <summary>
		/// Author.
		/// </summary>
		public string Author { get; set; }

		/// <summary>
		/// Packet version.
		/// </summary>
		public string Version { get; set; }

		/// <summary>
		/// Elements of Ar Pack.
		/// </summary>
		public List<ArPacketData> Elements { get; set; }
	}
}
