using System;
using System.IO;
using System.Collections.Generic;

using Assets.Scripts.Enums;
using Assets.Scripts.Entities;
using Assets.Scripts.DAL.Interfaces;

using UnityEngine;

using SQLite;

namespace Assets.Scripts.DAL.Implementations
{
	/// <summary>
	/// Implementation of <see cref="IArPacketsDbManager"/>.
	/// </summary>
	public class ArPacketsDbManager : IArPacketsDbManager
	{
		#region Private Fields

		/// <summary>
		/// Db connection.
		/// </summary>
		private readonly SQLiteConnection _db;

		#endregion

		#region Constructor

		/// <summary>
		/// Create instance of <see cref="ArPacketsDbManager"/> class.
		/// </summary>
		public ArPacketsDbManager()
		{
			var dbPath = Path.Combine(Application.persistentDataPath, "data.db");

			if (!File.Exists(dbPath))
			{
				File.Create(dbPath).Dispose();
			}

			SQLitePCL.Batteries.Init();
			SQLitePCL.Batteries_V2.Init();

			_db = new SQLiteConnection(dbPath);
			_db.CreateTable<ArPacket>();
		}

		#endregion

		#region Implementation of IDbManager

		/// <inheritdoc/>
		public int Create(ArPacket entity)
		{
			var arPacket = GetArPacketByAuthorAndName(entity.Author, entity.Name);

			if (arPacket != null)
			{
				return 0;
			}

			return _db.Insert(entity);
		}

		/// <inheritdoc/>
		public void Delete(Guid id)
			=> _db.Delete<ArPacket>(id);

		/// <inheritdoc/>
		public List<ArPacket> GetAll()
			=> _db.Table<ArPacket>().ToList();

		/// <inheritdoc/>
		public ArPacket GetArPacketByAuthorAndName(string author, string name)
			=> _db.Table<ArPacket>().FirstOrDefault(arPacket => arPacket.Author == author && arPacket.Name == name);

		/// <inheritdoc/>
		public ArPacketDbState GetArPacketDbState(string author, string name, string version)
		{
			var arPacket = GetArPacketByAuthorAndName(author, name);

			return arPacket == null ? ArPacketDbState.None
				: arPacket.Version == version ? ArPacketDbState.InDb
				: ArPacketDbState.DifferentVersion;
		}

		/// <inheritdoc/>
		public ArPacket GetById(Guid id)
			=> _db.Find<ArPacket>(id);

		/// <inheritdoc/>
		public List<ArPacket> GetEnabledArPackets()
			=> _db.Table<ArPacket>().Where(entity => entity.IsEnabled).ToList();

		/// <inheritdoc/>
		public int Update(ArPacket entity)
			=> _db.Update(entity);

		#endregion
	}
}
