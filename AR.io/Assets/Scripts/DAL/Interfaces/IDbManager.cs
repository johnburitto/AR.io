using System;
using System.Collections.Generic;

namespace Assets.Scripts.DAL.Interfaces
{
	/// <summary>
	/// Describes behaviour of db manager.
	/// </summary>
	public interface IDbManager<T>
	{
		/// <summary>
		/// Gets all entities from db.
		/// </summary>
		/// <returns>List of entities.</returns>
		List<T> GetAll();

		/// <summary>
		/// Get entity by its id.
		/// </summary>
		/// <param name="id">Entity id.</param>
		/// <returns>Specific entity.</returns>
		T GetById(Guid id);

		/// <summary>
		/// Add entity to db.
		/// </summary>
		/// <param name="entity">Entity.</param>
		/// <returns>Number of added rows.</returns>
		int Create(T entity);

		/// <summary>
		/// Update entity into database.
		/// </summary>
		/// <param name="entity">Entity.</param>
		/// <returns>Number of updated rows.</returns>
		int Update(T entity);

		/// <summary>
		/// Delete entity from db.
		/// </summary>
		/// <param name="id">Entity id.</param>
		/// <returns>Number of deleted rows.</returns>
		int Delete(Guid id);
	}
}
