#main_workflow
Даний клас використовується як DTO- та Entitty-об'єкт, що несе в собі та зберігає в базі всі інформацію, про заваyтажений  Ar-пакет. В ньому містяться наступні дані:

```csharp
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
	/// Show whether packet is enabled or not.
	/// </summary>
	public bool IsEnabled { get; set; }

	/// <summary>
	/// Date of adding.
	/// </summary>
	public DateTime AddedDate { get; set; } = DateTime.UtcNow;
}
```

Це необхідний мінімум даних, щоб потім підвантажувати маркери та об'єкти із пристрою. Для цього в об'єкті є флаг **IsEnabled**, тільки пакети, що мають його значення **true** будуть підвантажені. А поле **Name** буде використовуватися як назва паки на пристрої, з якої будуть завантажуватися дані.