namespace Assets.Scripts.Entities
{
	/// <summary>
	/// Keypoint structure.
	/// </summary>
	public struct Keypoint
	{
		#region Public Fields

		/// <summary>
		/// X axis coordinate.
		/// </summary>
		public int X;

		/// <summary>
		/// Y axis coordinate.
		/// </summary>
		public int Y;
		
		/// <summary>
		/// Response value.
		/// </summary>
		public float Response;

		#endregion

		#region Constructor

		/// <summary>
		/// Create instance of <see cref="Keypoint"/> struct.
		/// </summary>
		/// <param name="x">X.</param>
		/// <param name="y">Y.</param>
		/// <param name="response">Response.</param>
		public Keypoint(int x, int y, float response)
		{
			X = x;
			Y = y;
			Response = response;
		}

		#endregion
	}
}
