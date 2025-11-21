using System.Collections.Generic;

using Assets.Scripts.Enums;

namespace Assets.Scripts.Resources
{
	/// <summary>
	/// Holds image evaluation info text.
	/// </summary>
	public class ImageEvaluationInfoText
	{
		#region Private Fields

		private Dictionary<(MetricType, QualityLevel), string> _infos = new()
		{
			{ (MetricType.FeatureCount, QualityLevel.Good), "The image has a large number of unique features (keypoints), which ensures stable tracking even at difficult angles. The features are enough for the algorithm to work reliably, so no improvements are needed." },
			{ (MetricType.FeatureCount, QualityLevel.Medium), "The marker has a moderate number of features, which is enough for basic tracking, but the track can be unstable with significant rotation or partial occlusion of the image. You can improve the situation by increasing the detail: adding small contrast elements, textures, or reducing empty monochromatic areas." },
			{ (MetricType.FeatureCount, QualityLevel.Bad), "The image has very few features, so the algorithm finds almost no points to track, making tracking unreliable or impossible. To improve the quality, you need to significantly increase the amount of local detail, increase contrast, add textures, or break up large homogeneous areas." },
			{ (MetricType.MeanCornerResponse, QualityLevel.Good), "The corners in the image are clear and distinct, which ensures stable operation of the feature detector when zooming and changing the angle. No improvements are needed." },
			{ (MetricType.MeanCornerResponse, QualityLevel.Medium), "The corners are present, but their clarity is not sufficient, which can lead to the disappearance of some features when the camera moves. It is worth increasing the contrast of the edges, increasing the sharpness, or strengthening the contours of key marker elements." },
			{ (MetricType.MeanCornerResponse, QualityLevel.Bad), "The image has almost no sharp corners - it is either blurry or consists mostly of smooth shapes. To improve the quality, it is advisable to add geometric elements, clear edges, increase sharpness, and avoid heavy JPEG compression or blurring." },
			{ (MetricType.SpatialScore, QualityLevel.Good), "The features evenly cover the entire image, allowing the AR system to keep track even when the marker is partially covered or not fully visible. Optimal condition, no improvements needed." },
			{ (MetricType.SpatialScore, QualityLevel.Medium), "Most of the features are concentrated in certain parts of the marker, making tracking dependent on how the camera sees the image. Adding detail to empty areas or leveling the composition so that all areas have sufficient texture can help improve the situation." },
			{ (MetricType.SpatialScore, QualityLevel.Bad), "Features are almost completely absent in large areas of the marker, making tracking unstable or impossible if the camera only sees these areas. Improvements include redesigning the marker: adding textures to the center and edges, avoiding large blocks of solid color, and creating a more uniform visual structure." },
			{ (MetricType.Entropy, QualityLevel.Good), "The image contains a large number of unique shades and textures, which improves the ability to detect local differences. Optimally, no intervention is required." },
			{ (MetricType.Entropy, QualityLevel.Medium), "The information content is average, meaning the image has some distinct areas but is not rich enough. It could be improved by adding more variety in small elements, increasing contrast, or emphasizing textures." },
			{ (MetricType.Entropy, QualityLevel.Bad), "The entropy is low, which means that the image is almost completely homogeneous and there are no visible structures. To improve it, you should significantly increase the contrast, add textural elements, or revise the marker design towards more complex patterns." },
			{ (MetricType.Variance, QualityLevel.Good), "The image has a well-defined texture: sharp transitions, fine details, and brightness changes. This contributes to stable tracking, and no enhancements are needed." },
			{ (MetricType.Variance, QualityLevel.Medium), "Moderate texture - the image has some variation, but overall looks muted or too smooth. It can be improved by enhancing textures, adding fine details, or increasing local contrast." },
			{ (MetricType.Variance, QualityLevel.Bad), "The image contains almost no texture, which prevents algorithms from finding significant local changes. It is recommended to add textured elements, change the background to more detailed, or increase sharpness." },
			{ (MetricType.RepetitionScore, QualityLevel.Good), "The image consists of unique elements without regular repetitions, which allows the system to accurately match features. No enhancements are required." },
			{ (MetricType.RepetitionScore, QualityLevel.Medium), "Some parts of the marker are repeated, which can cause false mappings. It is recommended to change the repeated elements, change the markup, or add unique fragments between the repeated parts." },
			{ (MetricType.RepetitionScore, QualityLevel.Bad), "The image has many regular or highly repetitive structures, such as tiles, windows, grids, or patterns, that make recognition significantly more difficult. The design should be redesigned to make the fragments less regular, add chaotic details, or enhance unique shapes." },
			{ (MetricType.GlobalContrast, QualityLevel.Good), "The image has clear differences between light and dark areas, making features easily detectable. The image is very suitable for AR." },
			{ (MetricType.GlobalContrast, QualityLevel.Medium), "Contrast is mediocre, and some important details can get lost when scaled down or in real-world lighting. It's worth boosting contrast in key areas or increasing the difference between the background and objects." },
			{ (MetricType.GlobalContrast, QualityLevel.Bad), "The contrast is very low, making the image look flat and difficult for the algorithm to analyze. You need to increase the brightness difference, strengthen the edges, and streamline the lighting." },
			{ (MetricType.LocalContrast, QualityLevel.Medium), "Local areas are well differentiated from each other, which contributes to the emergence of weak but reliable features. No improvement is needed." },
			{ (MetricType.LocalContrast, QualityLevel.Bad), "Local differences are small, so fine details are poorly displayed. It is worth strengthening textures or adding more micro-details, such as grain or fine contours." },
			{ (MetricType.LocalContrast, QualityLevel.Good), "Local areas are too similar to each other, making the marker insensitive to small changes. Improvements include increasing sharpness, enhancing textures, and adding new elements." },
			{ (MetricType.CompressionArtifacts, QualityLevel.Good), "The image is clean, with no noticeable blocks or noise, so the features are correct and stable. No enhancement is needed." },
			{ (MetricType.CompressionArtifacts, QualityLevel.Medium), "There are moderate JPEG artifacts that can distort fine details. It is advisable to re-encode the image to PNG format or save as a higher quality JPEG." },
			{ (MetricType.CompressionArtifacts, QualityLevel.Bad), "Artifacts severely destroy the image structure, making detection accuracy low. It is recommended to obtain the original in higher quality or avoid re-compression with JPEG blocks." },
		};

		#endregion

		#region Public Properties

		/// <summary>
		/// Indexing of image evaluation info texts.
		/// </summary>
		/// <param name="key">Key.</param>
		/// <returns>Info.</returns>
		public string this[(MetricType, QualityLevel) key] => _infos[key];

		#endregion
	}
}
