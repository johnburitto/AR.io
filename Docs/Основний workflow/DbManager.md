#main_workflow 
Як спосіб зберігання було обрано **SQLite**, адже це база даних, яка є дуже легкою, бо в ній відсутня більша частина такого функціоналу, як наприклад у Microsoft SQL Server, але нам цей функціонал і не потрібен) На базі C# є реалізації lightweight ORM-бібліотеки під назвою **[sqlite-net-pcl](https://www.nuget.org/packages/sqlite-net-pcl/)** для взаємодії із SQLite, яка ідеально підходить для мобільних пристроїв. Для опису необхідного функціоналу було створено інтерфейс:

```csharp
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
	void Delete(Guid id);
}
```

Нижче наведені приклади підключення до бази, створення бази та виконання запиту на створення запису в базі:

```csharp
...

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

...
```