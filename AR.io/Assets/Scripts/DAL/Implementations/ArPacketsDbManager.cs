using System;
using System.IO;
using System.Collections.Generic;

using UnityEngine;

using Assets.Scripts.Entities;
using Assets.Scripts.DAL.Interfaces;

using SQLite;

namespace Assets.Scripts.DAL.Implementations
{
	/// <summary>
	/// Realisation of <see cref="IArPacketsDbManager"/>.
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

			_db = new SQLiteConnection(dbPath);
			_db.CreateTable<ArPacket>();
		}

		#endregion

		#region Implementation of IDbManager

		/// <inheritdoc/>
		public int Create(ArPacket entity)
			=> _db.Insert(entity);

		/// <inheritdoc/>
		public void Delete(Guid id)
			=> _db.Delete<ArPacket>(id);

		/// <inheritdoc/>
		public List<ArPacket> GetAll()
			=> _db.Table<ArPacket>().ToList();

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
