using System.Threading.Tasks;

using Assets.Scripts;
using Assets.Scripts.Enums;
using Assets.Scripts.Resources;
using Assets.Scripts.FileManagement.Interfaces;

using TMPro;

using UnityEngine;
using UnityEngine.UI;
using Unity.XR.CoreUtils;
using UnityEngine.XR.ARFoundation;

using QualityLevel = Assets.Scripts.Enums.QualityLevel;

/// <summary>
/// Image Evaluation Details UI manager.
/// </summary>
public class ImageEvaluationDetailsUIManager : MonoBehaviour
{
	#region Serialized Fields

	[Header("UI Elements")]
	/// <summary>
	/// Header.
	/// </summary>
	[SerializeField] private RectTransform _header;

	/// <summary>
	/// Header.
	/// </summary>
	[SerializeField] private RectTransform _details;

	/// <summary>
	/// Ar Packet name.
	/// </summary>
	[SerializeField] private TextMeshProUGUI _score;

	/// <summary>
	/// Ar Packet name.
	/// </summary>
	[SerializeField] private RawImage _image;

	/// <summary>
	/// Button for open marker evaluation button.
	/// </summary>
	[SerializeField] private Button _selectImageButton;

	/// <summary>
	/// Button for open marker evaluation button.
	/// </summary>
	[SerializeField] private Button _markerEvaluationButton;

	/// <summary>
	/// Button for hide the list.
	/// </summary>
	[SerializeField] private Button _cancelButton;

	/// <summary>
	/// Scroll view container.
	/// </summary>
	[SerializeField] private GameObject _scrollView;

	/// <summary>
	/// Content cointainer.
	/// </summary>
	[SerializeField] private Transform _contentContainer;

	/// <summary>
	/// List item.
	/// </summary>
	[SerializeField] private GameObject _listItem;

	[Header("Ar Components")]
	/// <summary>
	/// Ar tracked image manager.
	/// </summary>
	[SerializeField] private ARTrackedImageManager _arTrackedImageManager;

	/// <summary>
	/// Image tracking manager.
	/// </summary>
	[SerializeField] private ImageTrackingManager _imageTrackingManager;

	/// <summary>
	/// Ar QR scanner.
	/// </summary>
	[SerializeField] private ArQrCodeScanner _arQrScanner;

	/// <summary>
	/// Ar Packet loader.
	/// </summary>
	[SerializeField] private ArPacketLoader _arPacketLoader;

	[Header("UI Managers")]
	/// <summary>
	/// Ar tracked image manager.
	/// </summary>
	[SerializeField] private ArQrCodeScannerUIManager _arQrScannerUIManager;

	[Header("Images")]
	/// <summary>
	/// Base image.
	/// </summary>
	[SerializeField] private Texture2D _baseImage;

	[Header("Utils")]
	/// <summary>
	/// Image quality metrics manager.
	/// </summary>
	[SerializeField] private ImageQualityMetricsManager _imageQualityMetricsManager;

	#endregion

	#region Private Fields

	/// <summary>
	/// File Manager.
	/// </summary>
	private IFileManager _fileManager;

	/// <summary>
	/// Image evaluation info texts.
	/// </summary>
	private ImageEvaluationInfoText _imageEvaluationInfoText;

	/// <summary>
	/// File path.
	/// </summary>
	private string _filePath;

	#endregion

	#region Main Pipeline

	private void Start()
	{
		_fileManager = CompositionRoot.FileManager;
		_imageEvaluationInfoText = CompositionRoot.ImageEvaluationInfoText;
		_markerEvaluationButton.onClick.AddListener(ShowEvaluationDetails);
		_cancelButton.onClick.AddListener(HideEvaluationDetails);
		_selectImageButton.onClick.AddListener(async () => await LoadAndEvaluetImage());
	}

	private void OnDisable()
	{
		_markerEvaluationButton.onClick.RemoveAllListeners();
		_cancelButton.onClick.RemoveAllListeners();
		_selectImageButton.onClick.RemoveAllListeners();
	}

	#endregion

	#region Private Fields

	/// <summary>
	/// Loads an image and evaluates it using the configured evaluation criteria.
	/// </summary>
	private async Task LoadAndEvaluetImage()
	{
		ClearList();

		NativeFilePicker.PickFile((filePath) =>
		{
			_filePath = filePath;
		});

		if (string.IsNullOrEmpty(_filePath))
		{
			return;
		}

		var image = await _fileManager.GetMarker(_filePath);

		_image.texture = image;

		var evaluationResults = _imageQualityMetricsManager.ScoreMarker(image);
		var scoreColor = GetScoreColor(evaluationResults.OverallScore);

		_score.color = scoreColor;
		_score.text = $"Score: {evaluationResults.OverallScore:F2}";

		PopulateEvaluationDetails("Feature Count", evaluationResults.FeatureCount, evaluationResults.FeatureQuality, MetricType.FeatureCount);
		PopulateEvaluationDetails("Mean Corner Response", evaluationResults.MeanCornerResponse, evaluationResults.CornerQuality, MetricType.MeanCornerResponse);
		PopulateEvaluationDetails("Spatial Score", evaluationResults.SpatialScore, evaluationResults.SpatialQuality, MetricType.SpatialScore);
		PopulateEvaluationDetails("Entropy", evaluationResults.Entropy, evaluationResults.EntropyQuality, MetricType.Entropy);
		PopulateEvaluationDetails("Variance", evaluationResults.Variance, evaluationResults.VarianceQuality, MetricType.Variance);
		PopulateEvaluationDetails("Repetition Score", evaluationResults.RepetitionScore, evaluationResults.RepetitionQuality, MetricType.RepetitionScore);
		PopulateEvaluationDetails("Global Contrast", evaluationResults.GlobalContrast, evaluationResults.GlobalContrastQuality, MetricType.GlobalContrast);
		PopulateEvaluationDetails("Local Contrast", evaluationResults.LocalContrast, evaluationResults.LocalContrastQuality, MetricType.LocalContrast);
		PopulateEvaluationDetails("Compression Artifacts", evaluationResults.CompressionArtifacts, evaluationResults.CompressionQuality, MetricType.CompressionArtifacts);
	}

	/// <summary>
	/// Populate metric data to scroll view.
	/// </summary>
	/// <param name="metricName">Metric name.</param>
	/// <param name="metricValue">Metric value.</param>
	/// <param name="qualityLevel">Quality level.</param>
	/// <param name="metricType">Metric type.</param>
	private void PopulateEvaluationDetails(string metricName, float metricValue, QualityLevel qualityLevel, MetricType metricType)
	{
		var color = GetColorByQualtyLevel(qualityLevel);
		var infoText = _imageEvaluationInfoText[(metricType, qualityLevel)];

		var listItem = Instantiate(_listItem, _contentContainer);
		var data = listItem.GetNamedChild("Data");
		var button = listItem.GetNamedChild("Button");
		var icon = button.GetNamedChild("Icon").GetComponentInChildren<Image>();
		var name = button.GetComponentInChildren<TMP_Text>();
		var score = data.GetNamedChild("Score").GetComponentInChildren<TMP_Text>();
		var infoIcon = data.GetNamedChild("Info Icon").GetComponentInChildren<Image>();
		var info = data.GetNamedChild("Info").GetComponentInChildren<TMP_Text>();

		icon.color = color;

		name.text = $"{metricName}({qualityLevel})";
		name.color = color;

		score.text = $"Score: {metricValue:F2}";
		score.color = color;

		infoIcon.color = color;

		info.text = infoText;
		info.color = color;

		data.SetActive(false);

		button.GetComponent<Button>().onClick.AddListener(() =>
		{
			var isActive = data.gameObject.activeSelf;

			if (!isActive)
			{
				var rect = listItem.GetComponentInChildren<RectTransform>();
				var size = rect.sizeDelta;

				size.y = 1000;

				rect.sizeDelta = size;

				icon.transform.Rotate(new Vector3(0, 0, -90));
			}
			else
			{
				var rect = listItem.GetComponentInChildren<RectTransform>();
				var size = rect.sizeDelta;

				size.y = 200;

				rect.sizeDelta = size;

				icon.transform.Rotate(new Vector3(0, 0, 90));
			}

			data.SetActive(!isActive);
		});
	}

	/// <summary>
	/// Gets colotr by score.
	/// </summary>
	/// <param name="score">Score.</param>
	/// <returns>Color</returns>
	private Color GetScoreColor(float score)
	{
		if (score >= 85)
		{
			return Color.darkGreen;
		}
		else if (score >= 65)
		{
			return new Color(1, 0.87058823529f, 0.1294117647f);
		}
		else if (score >= 45)
		{
			return Color.orangeRed;
		}
		else if (score >= 25)
		{
			return Color.red;
		}
		else
		{
			return Color.darkRed;
		}
	}

	/// <summary>
	/// Get color by quality level.
	/// </summary>
	/// <param name="qualityLevel">Quality level.</param>
	/// <returns>Color.</returns>
	private Color GetColorByQualtyLevel(QualityLevel qualityLevel)
		=> qualityLevel switch
		{
			QualityLevel.Good => Color.darkGreen,
			QualityLevel.Medium => new Color(1, 0.87058823529f, 0.1294117647f),
			QualityLevel.Bad => Color.darkRed,
			_ => Color.black
		};

	/// <summary>
	/// Show evaluation details.
	/// </summary>
	private void ShowEvaluationDetails()
	{
		ClearList();
		ShowUI(true);
		EnableArComponents(false);
	}

	/// <summary>
	/// Hide evaluation details.
	/// </summary>
	private void HideEvaluationDetails()
	{
		ClearList();
		ShowUI(false);
		EnableArComponents(true);
	}

	/// <summary>
	/// Clear list.
	/// </summary>
	private void ClearList()
	{
		foreach (Transform child in _contentContainer.transform)
		{
			Destroy(child.gameObject);
		}
	}

	/// <summary>
	/// Show/hide UI elements.
	/// </summary>
	/// <param name="isShow">Indicates whether show UI or not.</param>
	private void ShowUI(bool isShow)
	{
		_score.text = string.Empty;
		_image.texture = _baseImage;

		_header.gameObject.SetActive(!isShow);
		_details.gameObject.SetActive(isShow);
		_score.gameObject.SetActive(isShow);
		_selectImageButton.gameObject.SetActive(isShow);
		_markerEvaluationButton.gameObject.SetActive(!isShow);
		_cancelButton.gameObject.SetActive(isShow);
		_scrollView.SetActive(isShow);
		_contentContainer.gameObject.SetActive(isShow);
	}

	/// <summary>
	/// Enable/disable Ar components.
	/// </summary>
	/// <param name="isEnabled">Is enabled.</param>
	private void EnableArComponents(bool isEnabled)
	{
		_arTrackedImageManager.enabled = isEnabled;
		_imageTrackingManager.enabled = isEnabled;
		_arQrScanner.enabled = isEnabled;
		_arPacketLoader.enabled = isEnabled;

		_arQrScannerUIManager.UpdateQrUi(false, null, null);
	}

	#endregion
}
