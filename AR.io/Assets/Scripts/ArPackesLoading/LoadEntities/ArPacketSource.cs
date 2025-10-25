using System.Collections.Generic;

namespace Assets.Scripts.LoadEntities
{
	/// <summary>
	/// Holds information about all ar packet's elements.
	/// </summary>
	public class ArPacketSource
	{
		/// <summary>
		/// Name.
		/// </summary>
		public string Name { get; set; }
	
		public List<ArPacketData> Elements { get; set; }
	}
}
