using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using Assets.Scripts;
using Assets.Scripts.Entities;

using UnityEngine;

using QualityLevel = Assets.Scripts.Enums.QualityLevel;

/// <summary>
/// Score image as per quality metrics for marker.
/// </summary>
public class ImageQualityMetricsManager : MonoBehaviour
{
	#region Main Pipeline

	private async void Start()
	{
		var filePathes = CompositionRoot.FileManager.GetElementsPathes($"John Buritto/Cars/Markers");
		var markers = await CompositionRoot.FileManager.GetMarkers(filePathes);
		
		foreach (var marker in markers.Select((value, i) => new { i, value }))
		{
			var result = ScoreMarker(marker.value);

			Debug.Log($"Marker: {Path.GetFileNameWithoutExtension(filePathes[marker.i])}");
			Debug.Log("Final score: " + result.FinalScore);

			Debug.Log($"Features: {result.FeatureCount} ({result.FeatureQuality})");
			Debug.Log($"Corner: {result.MeanCornerResponse} ({result.CornerQuality})");
			Debug.Log($"Spatial: {result.SpatialScore} ({result.SpatialQuality})");
			Debug.Log($"Entropy: {result.Entropy} ({result.EntropyQuality})");
			Debug.Log($"Variance: {result.Variance} ({result.VarianceQuality})");
			Debug.Log($"Repetition: {result.RepetitionScore} ({result.RepetitionQuality})");
			Debug.Log($"GlobalContrast: {result.GlobalContrast} ({result.GlobalContrastQuality})");
			Debug.Log($"LocalContrast: {result.LocalContrast} ({result.LocalContrastQuality})");
			Debug.Log($"Compression: {result.CompressionArtifacts} ({result.CompressionQuality})");
		}
	}

	#endregion

	#region Public Methods

	/// <summary>
	/// Score marker texture.
	/// </summary>
	/// <param name="texture">Texture.</param>
	/// <returns>Marker scores.</returns>
	public MarkerQualityResult ScoreMarker(Texture2D texture)
	{
		MarkerQualityResult result = new MarkerQualityResult();

		var (count, meanR, kps) = ComputeHarrisFeatures(texture);

		result.FeatureCount = count;
		result.MeanCornerResponse = meanR;

		result.FeatureQuality = Threshold(count, 150, 400);
		result.CornerQuality = Threshold(meanR, 0.001f, 0.003f);

		float spatial = ComputeSpatialDistributionScore(texture.width, texture.height, kps);

		result.SpatialScore = spatial;

		result.SpatialQuality = Threshold(spatial, 0.6f, 0.85f);

		var (entropy, variance) = ComputeEntropyAndVariance(texture);

		result.Entropy = entropy;
		result.Variance = variance;

		result.EntropyQuality = ThresholdDouble(entropy, 4.0, 5.5);
		result.VarianceQuality = ThresholdDouble(variance, 0.01, 0.03);

		double repeat = ComputeRepetitionScore(texture);
		result.RepetitionScore = repeat;

		if (repeat <= 0.15)
		{
			result.RepetitionQuality = QualityLevel.Good;
		}
		else if (repeat <= 0.30)
		{
			result.RepetitionQuality = QualityLevel.Medium;
		}
		else
		{
			result.RepetitionQuality = QualityLevel.Bad;
		}

		var contr = ComputeContrast(texture);

		result.GlobalContrast = contr.GlobalContrast;
		result.LocalContrast = contr.LocalContrast;

		result.GlobalContrastQuality = Threshold(contr.GlobalContrast, 0.1f, 0.2f);
		result.LocalContrastQuality = Threshold(contr.LocalContrast, 0.03f, 0.06f);

		float comp = ComputeCompressionArtifactsScore(texture);
		result.CompressionArtifacts = comp;

		if (comp <= 0.1f)
		{
			result.CompressionQuality = QualityLevel.Good;
		}
		else if (comp <= 0.25f)
		{
			result.CompressionQuality = QualityLevel.Medium;
		}
		else
		{
			result.CompressionQuality = QualityLevel.Bad;
		}

		float F_norm = Mathf.Clamp01(Mathf.Log(result.FeatureCount + 1) / Mathf.Log(500f));
		float C_norm = Mathf.Clamp01(result.MeanCornerResponse / 0.005f);
		float S_norm = result.SpatialScore;
		float E_norm = (float)(result.Entropy / 8.0);
		float V_norm = Mathf.Clamp01((float)result.Variance / 0.05f);
		float R_pen = 1f - (float)result.RepetitionScore;
		float G_norm = Mathf.Clamp01(result.GlobalContrast / 0.25f);
		float L_norm = Mathf.Clamp01(result.LocalContrast / 0.1f);
		float A_pen = 1f - result.CompressionArtifacts;

		float score =
			0.25f * F_norm +
			0.15f * C_norm +
			0.20f * S_norm +
			0.10f * E_norm +
			0.05f * V_norm +
			0.10f * R_pen +
			0.07f * G_norm +
			0.05f * L_norm +
			0.03f * A_pen;

		result.FinalScore = Mathf.Clamp(score * 100f, 0f, 100f);

		return result;
	}

	#endregion

	#region Constants

	/// <summary>
	/// Sobel matrix for X axis.
	/// </summary>
	private readonly int[,] SOBEL_X = new int[3, 3]
		{
			{ -1, 0, 1 },
			{ -2, 0, 2 },
			{ -1, 0, 1 }
		};

	/// <summary>
	/// Sobel matrix for Y axis.
	/// </summary>
	private readonly int[,] SOBEL_Y = new int[3, 3]
		{
			{ -1, -2, -1 },
			{ 0, 0, 0 },
			{ 1,  2, 1 }
		};

	private readonly int[,] NEIGHBORS = new int[,]
		{
			{ 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 }
		};

#endregion

	#region Utility Methods

	/// <summary>
	/// Converts texture to gray scale matrix.
	/// </summary>
	/// <param name="texture">Texture.</param>
	/// <returns>Gray scale matrix.</returns>
	private float[,] ToGrayScale(Texture2D texture)
	{
		var width = texture.width;
		var height = texture.height;
		var grayScaleMatrix = new float[width, height];
		var pixels = texture.GetPixels32();

		for (int y = 0; y < height; y++)
		{
			var offset = y * width;

			for (int x = 0; x < width; x++)
			{
				var pixelColor = pixels[offset + x];
				var grayColot = (0.299f * pixelColor.r + 0.587f * pixelColor.g + 0.114f * pixelColor.b) / 255f;

				grayScaleMatrix[x, y] = grayColot;
			}
		}

		return grayScaleMatrix;
	}

	/// <summary>
	/// Computes quality level based on thresholds.
	/// </summary>
	/// <param name="value">Value.</param>
	/// <param name="bad">Bad.</param>
	/// <param name="medium">Medium.</param>
	/// <returns>Threshold.</returns>
	private QualityLevel Threshold(float value, float bad, float medium)
	{
		if (value < bad)
		{
			return QualityLevel.Bad;
		}

		if (value < medium)
		{
			return QualityLevel.Medium;
		}

		return QualityLevel.Good;
	}

	/// <summary>
	/// Computes quality level based on thresholds.
	/// </summary>
	/// <param name="value">Value.</param>
	/// <param name="bad">Bad.</param>
	/// <param name="medium">Medium.</param>
	/// <returns>Threshold.</returns>
	private QualityLevel ThresholdDouble(double value, double bad, double medium)
	{
		if (value < bad)
		{
			return QualityLevel.Bad;
		}

		if (value < medium)
		{
			return QualityLevel.Medium;
		}

		return QualityLevel.Good;
	}

	#endregion

	#region Harris Features Evaluation

	/// <summary>
	/// Compure Sobel gradient of gray scale matrix.
	/// </summary>
	/// <param name="grayScaleMatrix">Gray scale matrix.</param>
	/// <param name="Ix">Values for X.</param>
	/// <param name="Iy">Values for Y.</param>
	private void ComputeSobelGradient(float[,] grayScaleMatrix, out float[,] Ix, out float[,] Iy)
	{
		var width = grayScaleMatrix.GetLength(0);
		var height = grayScaleMatrix.GetLength(1);
		
		Ix = new float[width, height];
		Iy = new float[width, height];

		for (int y = 1; y < height - 1; y++)
		{
			for (int x = 1; x < width - 1; x++)
			{
				float gradientX = 0f;
				float gradientY = 0f;

				for (int ky = -1; ky <= 1; ky++)
				{
					for (int kx = -1; kx <= 1; kx++)
					{
						gradientX += grayScaleMatrix[x + kx, y + ky] * SOBEL_X[kx + 1, ky + 1];
						gradientY += grayScaleMatrix[x + kx, y + ky] * SOBEL_Y[kx + 1, ky + 1];
					}
				}

				Ix[x, y] = gradientX;
				Iy[x, y] = gradientY;
			}
		}
	}

	/// <summary>
	/// Compute second moment matrix.
	/// </summary>
	/// <param name="Ix">First Ix value.</param>
	/// <param name="Iy">First Iy value.</param>
	/// <param name="windowSize">Window size.</param>
	/// <param name="Ix2">Second Ix value.</param>
	/// <param name="Iy2">Second Iy value.</param>
	/// <param name="Ixy">Value for XY.</param>
	private void ComputeSecondMomentMatrix(
		float[,] Ix,
		float[,] Iy,
		int windowSize,
		out float[,] Ix2,
		out float[,] Iy2,
		out float[,] Ixy)
	{
		var width = Ix.GetLength(0);
		var height = Ix.GetLength(1);

		Ix2 = new float[width, height];
		Iy2 = new float[width, height];
		Ixy = new float[width, height];

		int r = windowSize / 2;
		float norm = 1f / (windowSize * windowSize);

		for (int y = r; y < height - r; y++)
		{
			for (int x = r; x < width - r; x++)
			{
				float sumIx2 = 0f;
				float sumIy2 = 0f;
				float sumIxy = 0f;

				for (int ky = -r; ky <= r; ky++)
				{
					for (int kx = -r; kx <= r; kx++)
					{
						float ix = Ix[x + kx, y + ky];
						float iy = Iy[x + kx, y + ky];

						sumIx2 += ix * ix;
						sumIy2 += iy * iy;
						sumIxy += ix * iy;
					}
				}

				Ix2[x, y] = sumIx2 * norm;
				Iy2[x, y] = sumIy2 * norm;
				Ixy[x, y] = sumIxy * norm;
			}
		}
	}

	/// <summary>
	/// Compute Harris response matrix.
	/// </summary>
	/// <param name="Ix2">Second Ix value.</param>
	/// <param name="Iy2">Second Iy value.</param>
	/// <param name="Ixy">Value for XY.</param>
	/// <param name="k">Constant</param>
	/// <returns>Matrix of Harris responses.</returns>
	private float[,] ComputeHarrisResponse(float[,] Ix2, float[,] Iy2, float[,] Ixy, float k)
	{
		var width = Ix2.GetLength(0);
		var height = Ix2.GetLength(1);
		var R = new float[width, height];

		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				var a = Ix2[x, y];
				var b = Ixy[x, y];
				var c = Iy2[x, y];

				float delt = a * c - b * b;
				float trace = a + c;

				R[x, y] = delt - k * trace * trace;
			}
		}

		return R;
	}

	/// <summary>
	/// Find keypoints with value more than threshold.
	/// </summary>
	/// <param name="R">Response matrix.</param>
	/// <param name="threshold">Trashold.</param>
	/// <param name="nmsRadius">Radius.</param>
	/// <returns>List of keypoints.</returns>
	private List<Keypoint> NonMaxSuppression(float[,] R, float threshold, int nmsRadius)
	{
		var width = R.GetLength(0);
		var height = R.GetLength(1);
		var keypoints = new List<Keypoint>();

		for (int y = nmsRadius; y < height - nmsRadius; y++)
		{
			for (int x = nmsRadius; x < width - nmsRadius; x++)
			{
				var value = R[x, y];
				
				if (value < threshold)
				{
					continue;
				}

				var isMax = true;

				for (int ky = -nmsRadius; ky <= nmsRadius; ky++)
				{
					for (int kx = -nmsRadius; kx <= nmsRadius; kx++)
					{
						if (ky == 0 && kx == 0)
						{
							continue;
						}

						if (R[x + kx, y + ky] >= value)
						{
							isMax = false;
							
							break;
						}
					}
				}

				if (isMax)
				{
					keypoints.Add(new Keypoint(x, y, value));
				}
			}
		}

		return keypoints;
	}

	/// <summary>
	/// Computes Harris features from texture.
	/// </summary>
	/// <param name="texture">Texture.</param>
	/// <param name="windowSize">Windows size.</param>
	/// <param name="k">Constant.</param>
	/// <param name="threshold">Threshold.</param>
	/// <param name="nmsRadius">Radius.</param>
	/// <returns>Number of keypoints, mean Harris response, keypoints</returns>
	private (int count, float meanResponse, List<Keypoint> keypoints) ComputeHarrisFeatures(
		Texture2D texture,
		int windowSize = 5,
		float k = 0.04f,
		float threshold = 1e-4f,
		int nmsRadius = 2)
	{
		var grayScaleMatrix = ToGrayScale(texture);

		ComputeSobelGradient(grayScaleMatrix, out var Ix, out var Iy);
		ComputeSecondMomentMatrix(Ix, Iy, windowSize, out var Ix2, out var Iy2, out var Ixy);

		var R = ComputeHarrisResponse(Ix2, Iy2, Ixy, k);
		var keypoints = NonMaxSuppression(R, threshold, nmsRadius);

		var sum = 0f;

		foreach (var keypoint in keypoints)
		{
			sum += keypoint.Response;
		}

		var mean = keypoints.Count > 0 ? sum / keypoints.Count : 0f;

		return (keypoints.Count, mean, keypoints);
	}

	#endregion

	#region Entropy and Variance Evaluation

	/// <summary>
	/// Compute entropy and variance of texture.
	/// </summary>
	/// <param name="texture">Texture.</param>
	/// <returns>Entropy, variance.</returns>
	private (double entropy, double variance) ComputeEntropyAndVariance(Texture2D texture)
	{
		var grayScaleMatrix = ToGrayScale(texture);
		var width = grayScaleMatrix.GetLength(0);
		var height = grayScaleMatrix.GetLength(1);
		var n = width * height;

		var histogram = new int[256];
		var sum = 0.0;
		var sumSq = 0.0;

		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				var grayValue = Mathf.Clamp01(grayScaleMatrix[x, y]);
				var bin = (int)(grayValue * 255f);

				histogram[bin]++;
				sum += grayValue;
				sumSq += grayValue * grayValue;
			}
		}

		var mean = sum / n;
		var variance = (sumSq / n) - (mean * mean);

		var entropy = 0.0;

		for (int i = 0; i < 256; i++)
		{
			if (histogram[i] == 0)
			{
				continue;
			}

			var p = (double)histogram[i] / n;

			entropy -= p * Math.Log(p, 2);
		}
		
		return (entropy, variance);
	}

	#endregion

	#region Spatial Distribution Evaluation

	/// <summary>
	/// Compute spatial distribution score.
	/// </summary>
	/// <param name="width">Width.</param>
	/// <param name="heigth">Height.</param>
	/// <param name="keypoints">Keypoints.</param>
	/// <param name="gridX">Grid X.</param>
	/// <param name="gridY">Grid Y.</param>
	/// <param name="minFeaturesPerCell">Min value of feature points per cell.</param>
	/// <returns>Spatial distribution score.</returns>
	private float ComputeSpatialDistributionScore(
		int width,
		int heigth,
		List<Keypoint> keypoints,
		int gridX = 4,
		int gridY = 4,
		int minFeaturesPerCell = 3)
	{
		if (keypoints == null || keypoints.Count == 0)
		{
			return 0f;
		}

		var counts = new int[gridX, gridY];
		var cellWidth = (float)width / gridX;
		var cellHeight = (float)heigth / gridY;

		foreach (var keypoint in keypoints)
		{
			int cellX = Mathf.Clamp((int)(keypoint.X / cellWidth), 0, gridX - 1);
			int cellY = Mathf.Clamp((int)(keypoint.Y / cellHeight), 0, gridY - 1);
			
			counts[cellX, cellY]++;
		}

		var totalCells = gridX * gridY;
		var goodCells = 0f;

		for (int j = 0; j < gridY; j++)
		{
			for (int i = 0; i < gridX; i++)
			{
				if (counts[i, j] >= minFeaturesPerCell)
				{
					goodCells++;
				}
			}
		}

		return goodCells / totalCells;
	}

	#endregion

	#region Repetition Score Evaluation

	/// <summary>
	/// Downscale source image to new size.
	/// </summary>
	/// <param name="source">Source image.</param>
	/// <param name="newWidth">New width.</param>
	/// <param name="newHeight">New height.</param>
	/// <returns>Downscaled image.</returns>
	private float[,] Downscale(float[,] source, int newWidth, int newHeight)
	{
		var width = source.GetLength(0);
		var height = source.GetLength(1);
		var destination = new float[newWidth, newHeight];

		for (int y = 0; y < newHeight; y++)
		{
			int sy = (int)((y / (float)newHeight) * height);

			sy = Mathf.Clamp(sy, 0, height - 1);

			for (int x = 0; x < newWidth; x++)
			{
				int sx = (int)((x / (float)newWidth) * width);

				sx = Mathf.Clamp(sx, 0, width - 1);

				destination[x, y] = source[sx, sy];
			}
		}

		return destination;
	}

	/// <summary>
	/// Compure repetition score.
	/// </summary>
	/// <param name="texture">Texture.</param>
	/// <param name="downsampleWidth">Downscale width.</param>
	/// <param name="downsampleHeight">Downscale height.</param>
	/// <param name="maxShift">Max shift.</param>
	/// <returns>Repetition score.</returns>
	public float ComputeRepetitionScore(
		Texture2D texture,
		int downsampleWidth = 64,
		int downsampleHeight = 64,
		int maxShift = 8)
	{
		var grayScaleMatrix = ToGrayScale(texture);
		var downscaled = Downscale(grayScaleMatrix, downsampleWidth, downsampleHeight);

		var width = downscaled.GetLength(0);
		var height = downscaled.GetLength(1);

		var sum = 0f;
		var n = width * height;

		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				sum += downscaled[x, y];
			}
		}

		var mean = sum / n;
		var centered = new float[width, height];

		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				centered[x, y] = downscaled[x, y] - mean;
			}
		}

		var denomBase = 0f;

		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				denomBase += centered[x, y] * centered[x, y];
			}
		}

		denomBase = Mathf.Sqrt(denomBase);

		if (denomBase < 1e-8f)
		{
			return 0f;
		}

		var maxCorrelation = 0f;

		for (int dy = -maxShift; dy <= maxShift; dy++)
		{
			for (int dx = -maxShift; dx <= maxShift; dx++)
			{
				if (dx == 0 && dy == 0)
				{
					continue;
				}

				var num = 0f;
				var denomShift = 0f;

				for (int y = 0; y < height; y++)
				{
					int ys = y + dy;

					if (ys < 0 || ys >= height)
					{
						continue;
					}

					for (int x = 0; x < width; x++)
					{
						int xs = x + dx;

						if (xs < 0 || xs >= width)
						{
							continue;
						}
						
						num += centered[x, y] * centered[xs, ys];
						denomShift += centered[xs, ys] * centered[xs, ys];
					}
				}

				if (denomShift < 1e-8f)
				{
					continue;
				}

				var correlation = num / (denomBase * MathF.Sqrt(denomShift));

				if (correlation > maxCorrelation)
				{
					maxCorrelation = correlation;
				}
			}
		}

		if (maxCorrelation < 0)
		{
			maxCorrelation = 0;
		}

		return maxCorrelation;
	}

	#endregion

	#region Contrast Measure Evaluation
	
	/// <summary>
	/// Compute contrast metrics.
	/// </summary>
	/// <param name="texture">Texture.</param>
	/// <returns>Contrast metrics.</returns>
	private ContrastMetrics ComputeContrast(Texture2D texture)
	{
		var grayScaleMatrix = ToGrayScale(texture);
		var width = grayScaleMatrix.GetLength(0);
		var height = grayScaleMatrix.GetLength(1);
		int n = width * height;

		var hist = new int[256];

		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				int bin = (int)(Mathf.Clamp01(grayScaleMatrix[x, y]) * 255f);

				hist[bin]++;
			}
		}

		var sum = 0;
		var lowBin = 0;
		var highBin = 0;
		var p5 = (int)(0.05f * n);
		var p95 = (int)(0.95f * n);

		for (int i = 0; i < 256; i++)
		{
			sum += hist[i];

			if (sum >= p5)
			{
				lowBin = i;

				break;
			}
		}

		sum = 0;

		for (int i = 0; i < 256; i++)
		{
			sum += hist[i];

			if (sum >= p95)
			{
				highBin = i;

				break;
			}
		}

		var globalContrast = (highBin - lowBin) / 255f;
		var sumLocal = 0f;
		var countLocal = 0;

		for (int y = 1; y < height - 1; y++)
		{
			for (int x = 1; x < width - 1; x++)
			{
				var center = grayScaleMatrix[x, y];
				var neighSum = 0f;
				var neighCount = 0;

				for (int i = 0; i < 4; i++)
				{
					var nx = x + NEIGHBORS[i, 0];
					var ny = y + NEIGHBORS[i, 1];

					neighSum += grayScaleMatrix[nx, ny];
					neighCount++;
				}

				if (neighCount > 0)
				{
					var neighMean = neighSum / neighCount;

					sumLocal += MathF.Abs(center - neighMean);
					countLocal++;
				}
			}
		}

		var localContrast = countLocal > 0 ? sumLocal / countLocal : 0f;

		return new()
		{
			GlobalContrast = globalContrast,
			LocalContrast = localContrast
		};
	}

	#endregion

	#region Artifacts Score Evaluation

	/// <summary>
	/// Compute compressions artifacts score.
	/// </summary>
	/// <param name="texture">Texture.</param>
	/// <param name="blockSize">Block size.</param>
	/// <returns>Compressions artifacts score.</returns>
	public float ComputeCompressionArtifactsScore(Texture2D texture, int blockSize = 8)
	{
		var grayScaleMatrix = ToGrayScale(texture);
		var width = grayScaleMatrix.GetLength(0);
		var height = grayScaleMatrix.GetLength(1);

		var blocksX = width / blockSize;
		var blocksY = height / blockSize;
		
		if (blocksX < 2 || blocksY < 2)
		{
			return 0f;
		}

		var blockMean = new float[blocksX, blocksY];

		for (int by = 0; by < blocksY; by++)
		{
			for (int bx = 0; bx < blocksX; bx++)
			{
				var sum = 0f;
				var count = 0;

				for (int y = 0; y < blockSize; y++)
				{
					int py = by * blockSize + y;

					for (int x = 0; x < blockSize; x++)
					{
						int px = bx * blockSize + x;

						sum += grayScaleMatrix[px, py];
						count++;
					}
				}

				blockMean[bx, by] = sum / count;
			}
		}

		var sumDiff = 0f;
		var diffCount = 0;

		for (int by = 0; by < blocksY; by++)
		{
			for (int bx = 0; bx < blocksX - 1; bx++)
			{
				var m1 = blockMean[bx, by];
				var m2 = blockMean[bx + 1, by];

				sumDiff += Math.Abs(m1 - m2);
				diffCount++;
			}
		}

		for (int by = 0; by < blocksY - 1; by++)
		{
			for (int bx = 0; bx < blocksX; bx++)
			{
				var m1 = blockMean[bx, by];
				var m2 = blockMean[bx, by + 1];

				sumDiff += Math.Abs(m1 - m2);
				diffCount++;
			}
		}

		if (diffCount == 0)
		{
			return 0f;
		}

		var avgDiff = (float)(sumDiff / diffCount);
		var score = Mathf.Clamp01(avgDiff / 0.25f);

		return score;
	}

	#endregion
}
