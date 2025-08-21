using Assets.Scripts.Enums;

namespace Assets.Scripts.Logger.Entities
{
	/// <summary>
	/// Holds log information.
	/// </summary>
	public class Log
	{
		/// <summary>
		/// Gets or sets log level.
		/// </summary>
		public LogLevel Level { get; set; }

		/// <summary>
		/// Gets or sets log message.
		/// </summary>
		public string Message { get; set; }
	}
}
