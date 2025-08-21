using Assets.Scripts.Logger.Entities;

namespace Assets.Scripts.Logger.Interfaces
{
	/// <summary>
	/// Describes all logger provider behavior.
	/// </summary>
	public interface ILoggerProvider
	{
		/// <summary>
		/// Write log.
		/// </summary>
		/// <param name="log">Log.</param>
		void WriteLog(Log log);

		/// <summary>
		/// Write logs.
		/// </summary>
		/// <param name="logs">Logs.</param>
		void WriteLogs(params Log[] logs);
	}
}
