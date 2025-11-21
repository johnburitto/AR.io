using TMPro;

using UnityEngine;

/// <summary>
/// Loaded Ar Packets UI manager.
/// </summary>
public class LoadedArPacketsUIManager : MonoBehaviour
{
	#region Serialized Fields

	/// <summary>
	/// Loaded images count text element.
	/// </summary>
	[SerializeField] TextMeshProUGUI _loadedMarkersCount;

	/// <summary>
	/// Loaded models count text element.
	/// </summary>
	[SerializeField] TextMeshProUGUI _loadedModelsCount;

	#endregion

	#region Public Methods

	/// <summary>
	/// Updates UI info about loaded markers and models.
	/// </summary>
	/// <param name="markersCount"></param>
	/// <param name="modelsCount"></param>
	public void UpdateUIinfo(int markersCount, int modelsCount)
	{
		_loadedMarkersCount.text = markersCount.ToString();
		_loadedModelsCount.text = modelsCount.ToString();
	}

	#endregion
}
