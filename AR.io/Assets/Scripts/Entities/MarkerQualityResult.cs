using Assets.Scripts.Enums;

namespace Assets.Scripts.Entities
{
	/// <summary>
	/// Holds information about marker quality results.
	/// </summary>
	public class MarkerQualityResult
	{
		#region Final Score

		/// <summary>
		/// Final score.
		/// </summary>
		public float FinalScore;

		#endregion

		#region Feature Count

		/// <summary>
		/// Features count.
		/// </summary>
		public int FeatureCount;

		/// <summary>
		/// Feature quality.
		/// </summary>
		public QualityLevel FeatureQuality;

		#endregion

		#region Mean Corner Response

		/// <summary>
		/// Mean corner response.
		/// </summary>
		public float MeanCornerResponse;

		/// <summary>
		/// Corner quality.
		/// </summary>
		public QualityLevel CornerQuality;

		#endregion

		#region Spatial Score

		/// <summary>
		/// Spatial score.
		/// </summary>
		public float SpatialScore;

		/// <summary>
		/// Spatial quality.
		/// </summary>
		public QualityLevel SpatialQuality;

		#endregion

		#region Entropy

		/// <summary>
		/// Entropy.
		/// </summary>
		public double Entropy;

		/// <summary>
		/// Entropy quality.
		/// </summary>
		public QualityLevel EntropyQuality;

		#endregion

		#region Variance

		/// <summary>
		/// Variance.
		/// </summary>
		public double Variance;

		/// <summary>
		/// Variance quality.
		/// </summary>
		public QualityLevel VarianceQuality;

		#endregion

		#region Repetition Score

		/// <summary>
		/// Repetition score.
		/// </summary>
		public double RepetitionScore;

		/// <summary>
		/// Repetition quality.
		/// </summary>
		public QualityLevel RepetitionQuality;

		#endregion

		#region Global Contrast

		/// <summary>
		/// Global contrast.
		/// </summary>
		public float GlobalContrast;

		/// <summary>
		/// Global contrast quality.
		/// </summary>
		public QualityLevel GlobalContrastQuality;

		#endregion

		#region Local Contrast

		/// <summary>
		/// Local contrast.
		/// </summary>
		public float LocalContrast;

		/// <summary>
		/// Local contrast quality.
		/// </summary>
		public QualityLevel LocalContrastQuality;

		#endregion

		#region Compression artifacts

		/// <summary>
		/// Compression artifacts.
		/// </summary>
		public float CompressionArtifacts;

		/// <summary>
		/// Compression quality.
		/// </summary>
		public QualityLevel CompressionQuality;

		#endregion
	}
}
