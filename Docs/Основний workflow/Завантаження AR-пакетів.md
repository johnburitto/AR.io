#main_workflow 
### Завантаження маркерів
Користувачі при вході будуть підвантажуватися ті AR-пакети, які він включив. Для цього в дб менеджер було додано новий метод, який витягує з бази даних тільки активні пакети:

```csharp
/// <inheritdoc/>
public int Update(ArPacket entity)
	=> _db.Update(entity);
```

Далі на основі даних AR-пакету завантажуються його маркери із сховища пристрою. Далі ці маркери перетворюються у **Texture2D** так додаються як маркери до **MutableRuntimeReferenceImageLibrary**, що є бібліотекою маркерів, які застосунок в майбутньому буде розпізнавати:

```csharp
private async Task LoadMarkers()
{
	var arPackets = _arPacketsDbManager.GetEnabledArPackets();
	var runtimeLibrary = _arManager.referenceLibrary as MutableRuntimeReferenceImageLibrary;

	foreach (var packet in arPackets)
	{
		await ScheduleMarkers($"{packet.Name}/Markers", runtimeLibrary);
	}

	_logger.WriteLog($"{runtimeLibrary.count}");
	_arManager.referenceLibrary = runtimeLibrary;
}

private async Task ScheduleMarkers(string path, MutableRuntimeReferenceImageLibrary runtimeLibrary)
{
	var filePathes = _fileManager.GetMarkerNames("Test/Markers");
	var markers = await _fileManager.GetMarkers(filePathes);

	foreach (var marker in markers.Select((value, i) => new { i, value }))
	{
		var job = runtimeLibrary.ScheduleAddImageWithValidationJob(marker.value, Path.GetFileNameWithoutExtension(filePathes[marker.i]), 0.1f);

		job.jobHandle.Complete();
	}
}
```
