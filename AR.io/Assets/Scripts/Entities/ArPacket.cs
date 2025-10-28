using System;

using SQLite;

namespace Assets.Scripts.Entities
{
	/// <summary>
	/// Hold information about ar data packet.
	/// </summary>
	public class ArPacket
	{
		/// <summary>
		/// Id.
		/// </summary>
		[PrimaryKey]
		public Guid Id { get; set; } = Guid.NewGuid();

		/// <summary>
		/// Packet name.
		/// </summary>
		public string Name { get; set; }
	
		/// <summary>
		/// Packet author.
		/// </summary>
		public string Author { get; set; }

		/// <summary>
		/// Show whether packet is enabled or not.
		/// </summary>
		public bool IsEnabled { get; set; }

		/// <summary>
		/// Date of adding.
		/// </summary>
		public DateTime AddedDate { get; set; } = DateTime.UtcNow;
	}
}
