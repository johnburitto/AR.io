using System;
using System.Threading.Tasks;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Load popup UI manager.
/// </summary>
public class LoadPopupUIManager : MonoBehaviour
{
	#region Serialized Fields

	/// <summary>
	/// Popup rectangle.
	/// </summary>
	[SerializeField] RectTransform _popupRect;

	/// <summary>
	/// Progress bar.
	/// </summary>
	[SerializeField] Image _progressBar;

	/// <summary>
	/// Popup header text element.
	/// </summary>
	[SerializeField] TextMeshProUGUI _popupHeader;

	/// <summary>
	/// Popup info count text element.
	/// </summary>
	[SerializeField] TextMeshProUGUI _popupInfo;

	#endregion

	#region Public Methods

	/// <summary>
	/// Run a process with progress reporting.
	/// </summary>
	/// <param name="processFunc">Progress function.</param>
	public async Task RunProcess(Func<IProgress<float>, Task> processFunc)
	{
		Show();

		var progress = new Progress<float>(value =>
		{
			_progressBar.fillAmount = value;
		});

		await processFunc(progress);

		Hide();
	}

	/// <summary>
	/// Set popup header text.
	/// </summary>
	/// <param name="header">Header text.</param>
	public void SetPopupHeader(string header)
	{
		_popupHeader.text = header;
	}

	/// <summary>
	/// Set popup info text.
	/// </summary>
	/// <param name="info"></param>
	public void SetPopupInfo(string info)
	{
		_popupInfo.text = info;
	}

	#endregion

	#region Private Methods

	/// <summary>
	/// Show the popup.
	/// </summary>
	private void Show()
	{
		_popupRect.gameObject.SetActive(true);
		_progressBar.fillAmount = 0f;
		_popupHeader.text = "";
		_popupInfo.text = "";
	}

	/// <summary>
	/// Hide the popup.
	/// </summary>
	private void Hide()
	{
		_popupRect.gameObject.SetActive(false);
	}

	#endregion
}
