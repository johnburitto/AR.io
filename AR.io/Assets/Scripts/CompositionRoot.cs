using Assets.Scripts.DAL.Interfaces;
using Assets.Scripts.Logger.Interfaces;
using Assets.Scripts.DAL.Implementations;
using Assets.Scripts.FileManagement.Interfaces;
using Assets.Scripts.FileManagement.Implementations;

namespace Assets.Scripts
{
	/// <summary>
	/// Holds all shared instances
	/// </summary>
	public static class CompositionRoot
	{
		/// <summary>
		/// Logger.
		/// </summary>
		public static ILogger Logger;

		/// <summary>
		/// File manager.
		/// </summary>
		public static IFileManager FileManager => new FileManager();

		public static IArPacketsDbManager ArPacketsDbManager = new ArPacketsDbManager();
	}
}
