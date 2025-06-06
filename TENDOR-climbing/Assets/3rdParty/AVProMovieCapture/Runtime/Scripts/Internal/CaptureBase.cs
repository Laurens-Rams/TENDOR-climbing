#if UNITY_2017_3_OR_NEWER
	#define AVPRO_MOVIECAPTURE_OFFLINE_AUDIOCAPTURE
#endif
#if UNITY_5_6_OR_NEWER && UNITY_2018_3_OR_NEWER
	#define AVPRO_MOVIECAPTURE_VIDEOPLAYER_SUPPORT
#endif
#if UNITY_2017_1_OR_NEWER
	#define AVPRO_MOVIECAPTURE_PLAYABLES_SUPPORT
#endif
#if UNITY_2019_2_OR_NEWER
	#define AVPRO_MOVIECAPTURE_CAPTUREDELTA_SUPPORT
#endif
#if UNITY_2018_1_OR_NEWER
	#define UNITY_NATIVEARRAY_UNSAFE_SUPPORT
#endif

// Object.FindFirstObjectByType was added in Unity 2020.3.45, 2021.3.18, 2022.2.5 and 2023.1.0
#if UNITY_2022_2_OR_NEWER
	#if !(UNITY_2022_2_0 || UNITY_2022_2_1 || UNITY_2022_2_2 || UNITY_2022_2_3 || UNITY_2022_2_4)
		#define UNITY_HAS_FINDFIRSTOBJECTBYTYPE
	#endif
#elif !UNITY_2022_1_OR_NEWER && UNITY_2021_3_OR_NEWER
	#if !(UNITY_2021_3_0 || !UNITY_2021_3_1 || !UNITY_2021_3_2 || !UNITY_2021_3_3 || !UNITY_2021_3_4 || !UNITY_2021_3_5 || !UNITY_2021_3_6 || !UNITY_2021_3_7 || !UNITY_2021_3_8 || !UNITY_2021_3_9 || !UNITY_2021_3_10 || !UNITY_2021_3_11 || !UNITY_2021_3_12 || !UNITY_2021_3_13 || !UNITY_2021_3_14 || !UNITY_2021_3_15 || !UNITY_2021_3_16 || !UNITY_2021_3_17)
		#define UNITY_HAS_FINDFIRSTOBJECTBYTYPE
	#endif
#elif !UNITY_2021_1_OR_NEWER
	#if UNITY_2020_3_45 || UNITY_2020_3_46 || UNITY_2020_3_47 || UNITY_2020_3_48
		#define UNITY_HAS_FINDFIRSTOBJECTBYTYPE
	#endif
#endif

#if ENABLE_IL2CPP
using AOT;
#endif
#if UNITY_2018_1_OR_NEWER
using Unity.Collections;
#else
using UnityEngine.Collections;
#endif
using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;
using UnityEngine.Events;
using System.Text;
using System.ComponentModel;
using UnityEngine.Assertions;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

//-----------------------------------------------------------------------------
// Copyright 2012-2022 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProMovieCapture
{
	/// <summary>
	/// Live stats about an active capture session
	/// </summary>
	public class CaptureStats
	{
		public float FPS { get { return _fps; } }
		public float FramesTotal { get { return _frameTotal; } }

		public uint NumDroppedFrames { get { return _numDroppedFrames; } internal set { _numDroppedFrames = value; } }
		public uint NumDroppedEncoderFrames { get { return _numDroppedEncoderFrames; } internal set { _numDroppedEncoderFrames = value; } }
		public uint NumEncodedFrames { get { return _numEncodedFrames; } internal set { _numEncodedFrames = value; } }
		public float TotalEncodedSeconds { get { return _totalEncodedSeconds; } internal set { _totalEncodedSeconds = value; } }
		public uint TotalBuffersAvailable { get { return _totalBuffersAvailable; } internal set { _totalBuffersAvailable = value; } }
		public uint NumBufferedFrames { get { return _numBufferedFrames; } internal set { _numBufferedFrames = value; } }
		public UInt64 ActiveWorkers { get { return _activeWorkers; } internal set { _activeWorkers = value; } }
		public int MaxActiveWorkers { get { return _maxActiveWorkers; } internal set { _maxActiveWorkers = value; } }

        public AudioCaptureSource AudioCaptureSource { get { return _audioCaptureSource; } internal set { _audioCaptureSource = value; } }
		public int UnityAudioSampleRate { get { return _unityAudioSampleRate; } internal set { _unityAudioSampleRate = value; } }
		public int UnityAudioChannelCount { get { return _unityAudioChannelCount; } internal set { _unityAudioChannelCount = value; } }

		// Frame stats
		private uint _numDroppedFrames = 0;
		private uint _numDroppedEncoderFrames = 0;
		private uint _numEncodedFrames = 0;
		private float _totalEncodedSeconds = 0;
		private uint _totalBuffersAvailable = 0;
		private uint _numBufferedFrames = 0;
		private UInt64 _activeWorkers = 0;
		private int _maxActiveWorkers = 0;

        // Audio
        private AudioCaptureSource _audioCaptureSource = AudioCaptureSource.None;
		private int _unityAudioSampleRate = -1;
		private int _unityAudioChannelCount = -1;

		// Capture rate
		private float _fps = 0f;
		private int _frameTotal = 0;
		private int _frameCount = 0;
		private float _startFrameTime = 0f;

		internal void ResetFPS()
		{
			_frameCount = 0;
			_frameTotal = 0;
			_fps = 0.0f;
			_startFrameTime = 0.0f;
		}

		internal void UpdateFPS()
		{
			_frameCount++;
			_frameTotal++;

			float timeNow = Time.realtimeSinceStartup;
			float timeDelta = timeNow - _startFrameTime;
			if (timeDelta >= 1.0f)
			{
				_fps = (float)_frameCount / timeDelta;
				_frameCount = 0;
				_startFrameTime = timeNow;
			}
		}
	}

	[System.Serializable]
	[StructLayout(LayoutKind.Sequential, Pack=1)]
	public class VideoEncoderHints
	{
		public VideoEncoderHints()
		{
			SetDefaults();
		}

		public void SetDefaults()
		{
			averageBitrate = 0;
			maximumBitrate = 0;
			quality = 1.0f;
			keyframeInterval = 0;
			allowFastStartStreamingPostProcess = true;
			supportTransparency = false;
			useHardwareEncoding = true;
			injectStereoPacking = NoneAutoCustom.Auto;
			stereoPacking = StereoPacking.None;
			injectSphericalVideoLayout = NoneAutoCustom.Auto;
			sphericalVideoLayout = SphericalVideoLayout.None;
			enableConstantQuality = false;
			enableFragmentedWriting = false;
			movieFragmentInterval = 120;	// Default to two minutes
			colourSpace = ColourSpace.Unknown;
			sourceWidth = 0;
			sourceHeight = 0;
			androidNoCaptureRotation = false;
			iOSSaveCaptureWhenAppLosesFocus = true;
			transparency = Transparency.None;
			androidVulkanPreTransform = AndroidVulkanPreTransform.None;
			colourRange = ColourRange.Limited;
			realtimeFramePresentationTimestampOptions = RealtimeFramePresentationTimestampOptions.Realtime;
			orientationMetadata = OrientationMetadata.None;
		}

		internal void Validate()
		{
			quality = Mathf.Clamp(quality, 0f, 1f);
		}

		[Tooltip("Average number of bits per second for the resulting video. Zero uses the codec defaults.")]
		public uint averageBitrate;

		[Tooltip("Maximum number of bits per second for the resulting video. Zero uses the codec defaults.")]
		public uint maximumBitrate;

		[Range(0f, 1f)] public float quality;
		[Tooltip("How often a keyframe is inserted.  Zero uses the codec defaults.")]
		public uint keyframeInterval;

		// Only for MOV / MP4 files
		[Tooltip("Move the 'moov' atom in the video file from the end to the start of the file to make streaming start fast.  Also known as 'Fast Start' in some encoders")]
		[MarshalAs(UnmanagedType.U1)]
		public bool allowFastStartStreamingPostProcess;

		// Currently only for HEVC and ProRes 4444 on macOS/iOS, and supported DirectShow codecs (eg Lagarith/Uncompressed) on Windows
		[Tooltip("Hints to the encoder to use the alpha channel for transparency if possible")]
		[MarshalAs(UnmanagedType.U1)]
		public bool supportTransparency;

		// Windows only (on macOS/iOS hardware is always used if available)
		[MarshalAs(UnmanagedType.U1)]
		public bool useHardwareEncoding;

		// Enable constant quality for platforms/codecs that support it
		[Tooltip("Enable Constant Quality")]
		[MarshalAs(UnmanagedType.U1)]
		public bool enableConstantQuality;

		// Support fragmented writing of mov/mp4 on macOS/iOS
		[Tooltip("Enable fragmented writing support for QuickTime (mov, mp4) files")]
		[MarshalAs(UnmanagedType.U1)]
		public bool enableFragmentedWriting;

		[MarshalAs(UnmanagedType.U1)]
		public bool androidNoCaptureRotation;

		[MarshalAs(UnmanagedType.U1)]
		public bool iOSSaveCaptureWhenAppLosesFocus;

		[MarshalAs(UnmanagedType.U1)]
		public bool padding;

		// Only for MP4 files on Windows
		[Tooltip("Inject atoms to define stereo video mode")]
		public NoneAutoCustom injectStereoPacking;

		[Tooltip("Inject atoms to define stereo video mode")]
		public StereoPacking stereoPacking;

		// Only for MP4 files on Windows
		[Tooltip("Inject atoms to define spherical video layout")]
		public NoneAutoCustom injectSphericalVideoLayout;

		[Tooltip("Inject atoms to define spherical video layout")]
		public SphericalVideoLayout sphericalVideoLayout;

		// Not sure of sensible uppoer/lower bounds for this
		[Tooltip("The interval at which to write movie fragments in seconds")]
		[Range(0f, 300f)]
		public double movieFragmentInterval;

		public enum ColourSpace : int { Unknown = -1, Gamma = 0, Linear = 1 };
		public ColourSpace colourSpace;

		// The width and height of the source
		public int sourceWidth;
		public int sourceHeight;

		// Transparency
		[Tooltip("Transparency mode")]
		public Transparency transparency;

		// Android Vulkan only - not user configurable
		public AndroidVulkanPreTransform androidVulkanPreTransform;

		[Tooltip("Use Limited range for maximum compatibility")]
		public ColourRange colourRange;

		/// <summary>
		/// Controls how each frames timestamp is generated when capturing in realtime.
		/// </summary>
		[Tooltip("Options for controlling the presentation timestamp for each frame that is captured")]
		public RealtimeFramePresentationTimestampOptions realtimeFramePresentationTimestampOptions;

		public OrientationMetadata orientationMetadata;
	}

	[System.Serializable]
	[StructLayout(LayoutKind.Sequential, Pack=1)]
	public class ImageEncoderHints
	{
		public ImageEncoderHints()
		{
			SetDefaults();
		}

		public void SetDefaults()
		{
			quality = 0.85f;
			supportTransparency = false;
			colourSpace = ColourSpace.Unknown;
			sourceWidth = 0;
			sourceHeight = 0;
			transparency = Transparency.None;
			androidVulkanPreTransform = AndroidVulkanPreTransform.None;
		}

		internal void Validate()
		{
			quality = Mathf.Clamp(quality, 0f, 1f);
		}

		// Currently only affects JPG and HEIF formats (macOS only)
		[Range(0f, 1f)] public float quality;

		// Currently only for PNG
		[Tooltip("Hints to the encoder to use the alpha channel for transparency if possible")]
		[MarshalAs(UnmanagedType.U1)]
		public bool supportTransparency;

		public enum ColourSpace : int { Unknown = -1, Gamma = 0, Linear = 1 };
		public ColourSpace colourSpace;

		// The width and height of the source
		public int sourceWidth;
		public int sourceHeight;

		// Transparency
		[Tooltip("Transparency mode")]
		public Transparency transparency;

		// Android Vulkan only - not user configurable
		public AndroidVulkanPreTransform androidVulkanPreTransform;
	}

	[System.Serializable]
	public class EncoderHints
	{
		public EncoderHints()
		{
			SetDefaults();
		}

		public void SetDefaults()
		{
			videoHints = new VideoEncoderHints();
			imageHints = new ImageEncoderHints();
		}

		public VideoEncoderHints videoHints;
		public ImageEncoderHints imageHints;
	}

	/// <summary>
	/// Options for configuring the microphone recording session
	/// </summary>
	[Flags]
	public enum MicrophoneRecordingOptions: int
	{
		/// <summary>
		/// Uses the default options for configurung the capture session
		/// </summary>
		Defaults                 = 0,
		/// <summary>
		/// iOS - Allows playing audio from this application to be mixed with audio from other applications
		/// </summary>
		MixWithOthers            = 1 << 0,
		/// <summary>
		/// iOS - Audio will be routed to the speaker even if headphones or other accessories are in use
		/// </summary>
		DefaultToSpeaker         = 1 << 1,
		/// <summary>
		/// iOS - Enables support for bluetooth microphones
		/// </summary>
		AllowBluetoothMicrophone = 1 << 2,
	}

	/// <summary>
	/// Base class wrapping common capture functionality
	/// </summary>
	public partial class CaptureBase : MonoBehaviour
	{
		private const string DocEditionsURL = "https://www.renderheads.com/content/docs/AVProMovieCapture/articles/download.html#editions";

		public enum Resolution
		{
			POW2_8192x8192,
			POW2_8192x4096,
			POW2_4096x4096,
			POW2_4096x2048,
			POW2_2048x4096,
			UHD_3840x2160,
			UHD_3840x2048,
			UHD_3840x1920,
			UHD_2560x1440,
			POW2_2048x2048,
			POW2_2048x1024,
			HD_1920x1080,
			HD_1280x720,
			SD_1024x768,
			SD_800x600,
			SD_800x450,
			SD_640x480,
			SD_640x360,
			SD_320x240,
			Original,
			Custom,
		}

		public enum CubemapDepth
		{
			Depth_24 = 24,
			Depth_16 = 16,
			Depth_Zero = 0,
		}

		public enum CubemapResolution
		{
			POW2_8192 = 8192,
			POW2_4096 = 4096,
			POW2_2048 = 2048,
			POW2_1024 = 1024,
			POW2_512 = 512,
			POW2_256 = 256,
		}

		public enum AntiAliasingLevel
		{
			UseCurrent,
			ForceNone,
			ForceSample2,
			ForceSample4,
			ForceSample8,
		}

		public enum DownScale
		{
			Original = 1,
			Half = 2,
			Quarter = 4,
			Eighth = 8,
			Sixteenth = 16,
			Custom = 100,
		}

		public enum OutputPath
		{
			RelativeToProject,
			RelativeToPersistentData,
			Absolute,
			RelativeToDesktop,
			RelativeToPictures,
			RelativeToVideos,
			PhotoLibrary,
			RelativeToTemporaryCachePath,
			[Obsolete("Use RelativeToPersistentData")]
			RelativeToPeristentData = RelativeToPersistentData,
		}

		public enum FrameUpdateMode
		{
			Automatic,
			Manual,
		}

		/*public enum OutputExtension
		{
			AVI,
			MP4,
			PNG,
			Custom = 100,
		}*/

#if false
		[System.Serializable]
		public class WindowsPostCaptureSettings
		{
			[SerializeField]
			[Tooltip("Move the 'moov' atom in the MP4 file from the end to the start of the file to make streaming start fast.  Also called 'Fast Start' in some encoders")]
			public bool writeFastStartStreamingForMp4 = true;
		}

		[System.Serializable]
		public class PostCaptureSettings
		{
			[SerializeField]
			[Tooltip("Move the 'moov' atom in the MP4 file from the end to the start of the file to make streaming start fast.  Also called 'Fast Start' in some encoders")]
			public WindowsPostCaptureSettings windows = new WindowsPostCaptureSettings();
		}

		[SerializeField] PostCaptureSettings _postCaptureSettings = new PostCaptureSettings();
#endif

		[SerializeField] EncoderHints _encoderHintsWindows = new EncoderHints();
		[SerializeField] EncoderHints _encoderHintsMacOS = new EncoderHints();
		[SerializeField] EncoderHints _encoderHintsIOS = new EncoderHints();
		[SerializeField] EncoderHints _encoderHintsAndroid = new EncoderHints();

		// General options

		[SerializeField] KeyCode _captureKey = KeyCode.None;
		[SerializeField] bool _isRealTime = true;
		[SerializeField] bool _persistAcrossSceneLoads = false;

		// Start options

		[SerializeField] StartTriggerMode _startTrigger = StartTriggerMode.Manual;
		[SerializeField] StartDelayMode _startDelay = StartDelayMode.None;
		[SerializeField] float _startDelaySeconds = 0f;

		// Stop options

		[SerializeField] StopMode _stopMode = StopMode.None;
		// TODO: add option to pause instead of stop?
		[SerializeField] int _stopFrames = 0;
		[SerializeField] float _stopSeconds = 0f;
		#pragma warning disable 0414        // "is assigned but its value is never used"
		[SerializeField] bool _pauseCaptureOnAppPause = true;

		// Video options

		public static readonly string[] DefaultVideoCodecPriorityWindows = { 	"H264",
																				"HEVC",
																				"Lagarith Lossless Codec",
																				"Uncompressed",
																				"x264vfw - H.264/MPEG-4 AVC codec",
																				"Xvid MPEG-4 Codec" };

		public static readonly string[] DefaultVideoCodecPriorityMacOS =  {		"H264",
																				"HEVC",
																				"Apple ProRes 422",
																				"Apple ProRes 4444" };

		public static readonly string[] DefaultVideoCodecPriorityAndroid =  {	"H264",
																				"HEVC"/*,
																				"VP8",
																				"VP9"*/ };

		public static readonly string[] DefaultAudioCodecPriorityWindows = {	"AAC",
																				"FLAC",
																				"Uncompressed" };

		public static readonly string[] DefaultAudioCodecPriorityMacOS =  {	"AAC",
																			"FLAC",
																			"Apple Lossless",
																			"Linear PCM",
																			"Uncompresssed" };

		public static readonly string[] DefaultAudioCodecPriorityIOS =  {	"AAC",
																			"FLAC",
																			"Apple Lossless",
																			"Linear PCM",
																			"Uncompresssed" };

		public static readonly string[] DefaultAudioCodecPriorityAndroid =  {"AAC"/*,
																			"FLAC",
																			"OPUS"*/};

		public static readonly string[] DefaultAudioCaptureDevicePriorityWindow = { "Microphone (Realtek Audio)", "Stereo Mix", "What U Hear", "What You Hear", "Waveout Mix", "Mixed Output" };
		public static readonly string[] DefaultAudioCaptureDevicePriorityMacOS = { };
		public static readonly string[] DefaultAudioCaptureDevicePriorityIOS = { };
		public static readonly string[] DefaultAudioCaptureDevicePriorityAndroid = { };

		[SerializeField] string[] _videoCodecPriorityWindows = DefaultVideoCodecPriorityWindows;
		[SerializeField] string[] _videoCodecPriorityMacOS = DefaultVideoCodecPriorityMacOS;
		#pragma warning disable 0414		// "is assigned but its value is never used"
		[SerializeField] string[] _videoCodecPriorityAndroid = DefaultVideoCodecPriorityAndroid;
		#pragma warning restore 0414

		[SerializeField] string[] _audioCodecPriorityWindows = DefaultAudioCodecPriorityWindows;
		[SerializeField] string[] _audioCodecPriorityMacOS = DefaultAudioCodecPriorityMacOS;
		#pragma warning disable 0414		// "is assigned but its value is never used"
		[SerializeField] string[] _audioCodecPriorityAndroid = DefaultAudioCodecPriorityAndroid;
		#pragma warning restore 0414

		[SerializeField] float _frameRate = 30f;

		[Tooltip("Timelapse scale makes the frame capture run at a fraction of the target frame rate.  Default value is 1")]
		[SerializeField] int _timelapseScale = 1;
		[Tooltip("Manual update mode requires user to call FrameUpdate() each time a frame is ready")]
		[SerializeField] FrameUpdateMode _frameUpdateMode = FrameUpdateMode.Automatic;

		[SerializeField] DownScale _downScale = DownScale.Original;
		[SerializeField] Vector2 _maxVideoSize = Vector2.zero;

		#pragma warning disable 414
		[SerializeField, Range(-1, 128)] int _forceVideoCodecIndexWindows = -1;
		[SerializeField, Range(-1, 128)] int _forceVideoCodecIndexMacOS = 0;
		[SerializeField, Range(0, 128)] int _forceVideoCodecIndexIOS = 0;
		[SerializeField, Range(0, 128)] int _forceVideoCodecIndexAndroid = 0;
		[SerializeField, Range(-1, 128)] int _forceAudioCodecIndexWindows = -1;
		[SerializeField, Range(-1, 128)] int _forceAudioCodecIndexMacOS = 0;
		[SerializeField, Range(0, 128)] int _forceAudioCodecIndexIOS = 0;
		[SerializeField, Range(0, 128)] int _forceAudioCodecIndexAndroid = -1;
		#pragma warning restore 414
		[SerializeField] bool _flipVertically = false;

		[Tooltip("Flushing the GPU during each capture results in less latency, but can slow down rendering performance for complex scenes.")]
		[SerializeField] bool _forceGpuFlush = false;

		[Tooltip("This option can help issues where skinning is used, or other animation/rendering effects that only complete later in the frame.")]
		[SerializeField] protected bool _useWaitForEndOfFrame = true;

		[Tooltip("Update the media gallery")]
		[SerializeField] protected bool _androidUpdateMediaGallery = true;

		[Tooltip("Portrait captures may be rotated 90° to better utilise the encoder, check this to disable the rotation at the risk of not being able to capture the full vertical resolution.")]
		[SerializeField] bool _androidNoCaptureRotation = false;

		//
		[SerializeField] bool _iOSSaveCaptureWhenAppLosesFocus = true;

		[Tooltip("Log the start and stop of the capture.  Disable this for less garbage generation.")]
		[SerializeField] bool _logCaptureStartStop = true;

		// Audio options

		[SerializeField] AudioCaptureSource _audioCaptureSource = AudioCaptureSource.None;
		[SerializeField] UnityAudioCapture _unityAudioCapture = null;
		[SerializeField, Range(0, 32)] int _forceAudioInputDeviceIndex = 0;
		[SerializeField, Range(8000, 96000)] int _manualAudioSampleRate = 48000;
		[SerializeField, Range(1, 8)] int _manualAudioChannelCount = 2;

		// Output options

		[SerializeField] protected OutputTarget _outputTarget = OutputTarget.VideoFile;

		public OutputTarget OutputTarget
		{
			get { return _outputTarget; }
			set { _outputTarget = value; }
		}

		public const OutputPath DefaultOutputFolderType = OutputPath.RelativeToProject;
		private const string DefaultOutputFolderPath = "Captures";

		[SerializeField] OutputPath _outputFolderType = DefaultOutputFolderType;
		[SerializeField] string _outputFolderPath = DefaultOutputFolderPath;
		[SerializeField] string _filenamePrefix = "MovieCapture";
		[SerializeField] bool _appendFilenameTimestamp = true;
		[SerializeField] bool _allowManualFileExtension = false;
		[SerializeField] string _filenameExtension = "mp4";
		[SerializeField] string _namedPipePath = @"\\.\pipe\test_pipe";
		[SerializeField] bool _writeOrientationMetadata = false;

		public OutputPath OutputFolder
		{
			get { return _outputFolderType; }
			set { _outputFolderType = value; }
		}
		public string OutputFolderPath
		{
			get { return _outputFolderPath; }
			set { _outputFolderPath = value; }
		}
		public string FilenamePrefix
		{
			get { return _filenamePrefix; }
			set { _filenamePrefix = value; }
		}
		public bool AppendFilenameTimestamp
		{
			get { return _appendFilenameTimestamp; }
			set { _appendFilenameTimestamp = value; }
		}
		public bool AllowManualFileExtension
		{
			get { return _allowManualFileExtension; }
			set { _allowManualFileExtension = value; }
		}
		public string FilenameExtension
		{
			get { return _filenameExtension; }
			set { _filenameExtension = value; }
		}
		public string NamedPipePath
		{
			get { return _namedPipePath; }
			set { _namedPipePath = value; }
		}

		public bool WriteOrientationMetadata
		{
			get { return _writeOrientationMetadata; }
			set { _writeOrientationMetadata = value; }
		}

		[SerializeField] int _imageSequenceStartFrame = 0;
		[SerializeField, Range(2, 12)] int _imageSequenceZeroDigits = 6;
		#pragma warning disable 414
		[SerializeField] ImageSequenceFormat _imageSequenceFormatWindows = ImageSequenceFormat.PNG;
		[SerializeField] ImageSequenceFormat _imageSequenceFormatMacOS = ImageSequenceFormat.PNG;
		[SerializeField] ImageSequenceFormat _imageSequenceFormatIOS = ImageSequenceFormat.PNG;
		[SerializeField] ImageSequenceFormat _imageSequenceFormatAndroid = ImageSequenceFormat.PNG;
		#pragma warning restore 414

		public int ImageSequenceStartFrame
		{
			get { return _imageSequenceStartFrame; }
			set { _imageSequenceStartFrame = value; }
		}
		public int ImageSequenceZeroDigits
		{
			get { return _imageSequenceZeroDigits; }
			set { _imageSequenceZeroDigits = Mathf.Clamp(value, 2, 12); }
		}

		// Camera specific options

		[SerializeField] protected Resolution _renderResolution = Resolution.Original;
		[SerializeField] protected Vector2 _renderSize = Vector2.one;
		[SerializeField] protected int _renderAntiAliasing = -1;

		// Motion blur options

		[SerializeField] protected bool _useMotionBlur = false;
		[SerializeField, Range(0, 64)] protected int _motionBlurSamples = 16;
		[SerializeField] protected Camera[] _motionBlurCameras = null;
		[SerializeField] protected MotionBlur _motionBlur;

		public bool UseMotionBlur
		{
			get { return _useMotionBlur; }
			set { _useMotionBlur = value; }
		}
		public int MotionBlurSamples
		{
			get { return _motionBlurSamples; }
			set { _motionBlurSamples = (int)Mathf.Clamp((float)value, 0f, 64f); }
		}
		public Camera[] MotionBlurCameras
		{
			get { return _motionBlurCameras; }
			set { _motionBlurCameras = value; }
		}
		public MotionBlur MotionBlur
		{
			get { return _motionBlur; }
			set { _motionBlur = value; }
		}

		// Performance options

		[SerializeField] bool _allowVSyncDisable = true;
		[SerializeField] protected bool _supportTextureRecreate = false;

		// Other options

		[SerializeField] int _minimumDiskSpaceMB = -1;

#if AVPRO_MOVIECAPTURE_PLAYABLES_SUPPORT
		[SerializeField] TimelineController _timelineController = null;
#endif
#if AVPRO_MOVIECAPTURE_VIDEOPLAYER_SUPPORT
		[SerializeField] VideoPlayerController _videoPlayerController = null;
#endif

		//public bool _allowFrameRateChange = true;

		protected Texture2D _texture;
		protected int _handle = -1;
		protected int _sourceWidth, _sourceHeight;
		protected int _targetWidth, _targetHeight;
		protected bool _capturing = false;
		protected bool _paused = false;
		protected string _filePath;
		protected string _finalFilePath;
		protected FileInfo _fileInfo;
		protected NativePlugin.PixelFormat _pixelFormat = NativePlugin.PixelFormat.YCbCr422_YUY2;
		private Codec _selectedVideoCodec = null;
		private Codec _selectedAudioCodec = null;
		private Device _selectedAudioInputDevice = null;
		private int _oldVSyncCount = 0;
		//private int _oldTargetFrameRate = -1;
		private float _oldFixedDeltaTime = 0f;
		protected bool _isTopDown = true;
		protected bool _isDirectX11 = false;
		private bool _queuedStartCapture = false;
		private bool _queuedStopCapture = false;
		private float _captureStartTime = 0f;
		private float _capturePrePauseTotalTime = 0f;
		private float _timeSinceLastFrame = 0f;
		protected YieldInstruction _waitForEndOfFrame;
		private long _freeDiskSpaceMB;

		protected Transparency _Transparency = Transparency.None;
		public Transparency Transparency	{ get { return _Transparency; } }
		//
		protected RenderTexture _sideBySideTexture;
		protected Material _sideBySideMaterial;

#if !(UNITY_EDITOR || UNITY_STANDALONE_WIN)
		private bool _wasCapturingOnPause = false;
#endif

		private float _startDelayTimer;
		private bool _startPaused;
		private System.Action<FileWritingHandler> _beginFinalFileWritingAction;
		private System.Action<FileWritingHandler> _completedFileWritingAction;
		private List<FileWritingHandler> _pendingFileWrites = new List<FileWritingHandler>(4);

		private static HashSet<string> _activeFilePaths = new HashSet<string>();
		public static HashSet<string> ActiveFilePaths
		{
			get { return _activeFilePaths; }
		}

		public string LastFilePath
		{
			get { return _filePath; }
		}

		private UnityEvent _onCaptureStart = new UnityEvent();
		public UnityEvent OnCaptureStart
		{
			get { return _onCaptureStart; }
		}

		// Register for notification of when the final file writing begins
		public System.Action<FileWritingHandler> BeginFinalFileWritingAction
		{
			get { return _beginFinalFileWritingAction; }
			set { _beginFinalFileWritingAction = value; }
		}

		// Register for notification of when the final file writing completes
		public System.Action<FileWritingHandler> CompletedFileWritingAction
		{
			get { return _completedFileWritingAction; }
			set { _completedFileWritingAction = value; }
		}

		// Stats
		private CaptureStats _stats = new CaptureStats();

		private static bool _isInitialised = false;
		private static bool _isApplicationQuiting = false;

		public Resolution CameraRenderResolution
		{
			get { return _renderResolution; }
			set { _renderResolution = value; }
		}
		public Vector2 CameraRenderCustomResolution
		{
			get { return _renderSize; }
			set { _renderSize = value; }
		}

		public int CameraRenderAntiAliasing
		{
			get { return _renderAntiAliasing; }
			set { _renderAntiAliasing = value; }
		}

		public bool IsRealTime
		{
			get { return _isRealTime; }
			set { _isRealTime = value; }
		}

		public bool PersistAcrossSceneLoads
		{
			get { return _persistAcrossSceneLoads; }
			set { _persistAcrossSceneLoads = value; }
		}

		public AudioCaptureSource AudioCaptureSource
		{
			get { return _audioCaptureSource; }
			set { _audioCaptureSource = value; }
		}

		public int ManualAudioSampleRate
		{
			get { return _manualAudioSampleRate; }
			set { _manualAudioSampleRate = value; }
		}

		public int ManualAudioChannelCount
		{
			get { return _manualAudioChannelCount; }
			set { _manualAudioChannelCount = value; }
		}

		public UnityAudioCapture UnityAudioCapture
		{
			get { return _unityAudioCapture; }
			set { _unityAudioCapture = value; }
		}

		public int ForceAudioInputDeviceIndex
		{
			get { return _forceAudioInputDeviceIndex; }
			set { _forceAudioInputDeviceIndex = value; SelectAudioInputDevice(); }
		}

		public float FrameRate
		{
			get { return _frameRate; }
			set { _frameRate = Mathf.Clamp(value, 0.01f, 240f); }
		}

		public StartTriggerMode StartTrigger
		{
			get { return _startTrigger; }
			set { _startTrigger = value; }
		}

		public StartDelayMode StartDelay
		{
			get { return _startDelay; }
			set { _startDelay = value; }
		}

		public float StartDelaySeconds
		{
			get { return _startDelaySeconds; }
			set { _startDelaySeconds =  Mathf.Max(0f, value); }
		}

		public StopMode StopMode
		{
			get { return _stopMode; }
			set { _stopMode = value; }
		}

		public int StopAfterFramesElapsed
		{
			get { return _stopFrames; }
			set { _stopFrames = Mathf.Max(0, value); }
		}

		public float StopAfterSecondsElapsed
		{
			get { return _stopSeconds; }
			set { _stopSeconds = Mathf.Max(0f, value); }
		}

		public bool PauseCaptureOnAppPause
		{
			get { return _pauseCaptureOnAppPause; }
			set { _pauseCaptureOnAppPause = value; }
		}

		public CaptureStats CaptureStats
		{
			get { return _stats; }
		}

		public string[] VideoCodecPriorityWindows
		{
			get { return _videoCodecPriorityWindows; }
			set { _videoCodecPriorityWindows = value; SelectVideoCodec(false); }
		}

		public string[] VideoCodecPriorityMacOS
		{
			get { return _videoCodecPriorityMacOS; }
			set { _videoCodecPriorityMacOS = value; SelectVideoCodec(false); }
		}

		public string[] AudioCodecPriorityWindows
		{
			get { return _audioCodecPriorityWindows; }
			set { _audioCodecPriorityWindows = value; SelectAudioCodec(); }
		}

		public string[] AudioCodecPriorityMacOS
		{
			get { return _audioCodecPriorityMacOS; }
			set { _audioCodecPriorityMacOS = value; SelectAudioCodec(); }
		}

		public int TimelapseScale
		{
			get { return _timelapseScale; }
			set { _timelapseScale = value; }
		}

		public FrameUpdateMode FrameUpdate
		{
			get { return _frameUpdateMode; }
			set { _frameUpdateMode = value; }
		}

		public DownScale ResolutionDownScale
		{
			get { return _downScale; }
			set { _downScale = value; }
		}

		public Vector2 ResolutionDownscaleCustom
		{
			get { return _maxVideoSize; }
			set { _maxVideoSize = value; }
		}

		public bool FlipVertically
		{
			get { return _flipVertically; }
			set { _flipVertically = value; }
		}

		public bool UseWaitForEndOfFrame
		{
			get { return _useWaitForEndOfFrame; }
			set { _useWaitForEndOfFrame = value; }
		}

		public bool LogCaptureStartStop
		{
			get { return _logCaptureStartStop; }
			set { _logCaptureStartStop = value; }
		}

#if false
		public PostCaptureSettings PostCapture
		{
			get { return _postCaptureSettings; }
		}
#endif

		public bool AllowOfflineVSyncDisable
		{
			get { return _allowVSyncDisable; }
			set { _allowVSyncDisable = value; }
		}

		public bool SupportTextureRecreate
		{
			get { return _supportTextureRecreate; }
			set { _supportTextureRecreate = value; }
		}

		#if AVPRO_MOVIECAPTURE_PLAYABLES_SUPPORT
		public TimelineController TimelineController
		{
			get { return _timelineController; }
			set { _timelineController = value; }
		}
		#endif

		#if AVPRO_MOVIECAPTURE_VIDEOPLAYER_SUPPORT
		public VideoPlayerController VideoPlayerController
		{
			get { return _videoPlayerController; }
			set { _videoPlayerController = value; }
		}
		#endif

		public Codec SelectedVideoCodec
		{
			get { return _selectedVideoCodec; }
		}

		public Codec SelectedAudioCodec
		{
			get { return _selectedAudioCodec; }
		}

		public Device SelectedAudioInputDevice
		{
			get { return _selectedAudioInputDevice; }
		}

		public int NativeForceVideoCodecIndex
		{
			#if UNITY_EDITOR
				#if UNITY_EDITOR_WIN
					get { return _forceVideoCodecIndexWindows; }
					set { _forceVideoCodecIndexWindows = value; }
				#elif UNITY_EDITOR_OSX
					get { return _forceVideoCodecIndexMacOS; }
					set { _forceVideoCodecIndexMacOS = value; }
				#else
					get { return -1; }
					set { }
				#endif
			#else
				#if UNITY_STANDALONE_WIN
					get { return _forceVideoCodecIndexWindows; }
					set { _forceVideoCodecIndexWindows = value; }
				#elif UNITY_STANDALONE_OSX
					get { return _forceVideoCodecIndexMacOS; }
					set { _forceVideoCodecIndexMacOS = value; }
				#elif UNITY_IOS
					get { return _forceVideoCodecIndexIOS; }
					set { _forceVideoCodecIndexIOS = value; }
				#elif UNITY_ANDROID
					get { return _forceVideoCodecIndexAndroid; }
					set { _forceVideoCodecIndexAndroid = value; }
				#else
					get { return -1; }
					set { }
				#endif
			#endif
		}

		public int NativeForceAudioCodecIndex
		{
			#if UNITY_EDITOR
				#if UNITY_EDITOR_WIN
					get { return _forceAudioCodecIndexWindows; }
					set { _forceAudioCodecIndexWindows = value; }
				#elif UNITY_EDITOR_OSX
					get { return _forceAudioCodecIndexMacOS; }
					set { _forceAudioCodecIndexMacOS = value; }
				#else
					get { return -1; }
					set { }
				#endif
			#else
				#if UNITY_STANDALONE_WIN
					get { return _forceAudioCodecIndexWindows; }
					set { _forceAudioCodecIndexWindows = value; }
				#elif UNITY_STANDALONE_OSX
					get { return _forceAudioCodecIndexMacOS; }
					set { _forceAudioCodecIndexMacOS = value; }
				#elif UNITY_IOS
					get { return _forceAudioCodecIndexIOS; }
					set { _forceAudioCodecIndexIOS = value; }
				#elif UNITY_ANDROID
					get { return _forceAudioCodecIndexAndroid; }
					set { _forceAudioCodecIndexAndroid = value; }
				#else
					get { return -1; }
					set { }
				#endif
			#endif
		}

		public ImageSequenceFormat NativeImageSequenceFormat
		{
			#if UNITY_EDITOR
				#if UNITY_EDITOR_WIN
					get { return _imageSequenceFormatWindows; }
					set { _imageSequenceFormatWindows = value; }
				#elif UNITY_EDITOR_OSX
					get { return _imageSequenceFormatMacOS; }
					set { _imageSequenceFormatMacOS = value; }
				#else
					get { return ImageSequenceFormat.PNG; }
					set { }
				#endif
			#else
				#if UNITY_STANDALONE_WIN
					get { return _imageSequenceFormatWindows; }
					set { _imageSequenceFormatWindows = value; }
				#elif UNITY_STANDALONE_OSX
					get { return _imageSequenceFormatMacOS; }
					set { _imageSequenceFormatMacOS = value; }
				#elif UNITY_IOS
					get { return _imageSequenceFormatIOS; }
					set { _imageSequenceFormatIOS = value; }
				#elif UNITY_ANDROID
					get { return _imageSequenceFormatAndroid; }
					set { _imageSequenceFormatAndroid = value; }
				#else
					get { return ImageSequenceFormat.PNG; }
					set { }
				#endif
			#endif
		}

		protected static NativePlugin.Platform GetCurrentPlatform()
		{
			NativePlugin.Platform result = NativePlugin.Platform.Unknown;
			#if UNITY_EDITOR
				#if UNITY_EDITOR_WIN
					result = NativePlugin.Platform.Windows;
				#elif UNITY_EDITOR_OSX
					result = NativePlugin.Platform.macOS;
				#endif
			#else
				#if UNITY_STANDALONE_WIN
					result = NativePlugin.Platform.Windows;
				#elif UNITY_STANDALONE_OSX
					result = NativePlugin.Platform.macOS;
				#elif UNITY_IOS
					result = NativePlugin.Platform.iOS;
				#elif UNITY_ANDROID
					result = NativePlugin.Platform.Android;
				#endif
			#endif
			return result;
		}

		public EncoderHints GetEncoderHints(NativePlugin.Platform platform = NativePlugin.Platform.Current)
		{
			EncoderHints result = null;

			if (platform == NativePlugin.Platform.Current)
			{
				platform = GetCurrentPlatform();
			}
			switch (platform)
			{
				case NativePlugin.Platform.Windows:
					result = _encoderHintsWindows;
					break;
				case NativePlugin.Platform.macOS:
					result = _encoderHintsMacOS;
					break;
				case NativePlugin.Platform.iOS:
					result = _encoderHintsIOS;
					break;
				case NativePlugin.Platform.Android:
					result = _encoderHintsAndroid;
					break;
			}
			return result;
		}

		public void SetEncoderHints(EncoderHints hints, NativePlugin.Platform platform = NativePlugin.Platform.Current)
		{
			if (platform == NativePlugin.Platform.Current)
			{
				platform = GetCurrentPlatform();
			}
			switch (platform)
			{
				case NativePlugin.Platform.Windows:
					_encoderHintsWindows = hints;
					break;
				case NativePlugin.Platform.macOS:
					_encoderHintsMacOS = hints;
					break;
				case NativePlugin.Platform.iOS:
					_encoderHintsIOS = hints;
					break;
				case NativePlugin.Platform.Android:
					_encoderHintsAndroid = hints;
					break;
			}
		}


#if UNITY_ANDROID && !UNITY_EDITOR
		protected static AndroidJavaObject s_ActivityContext = null;
		protected static AndroidJavaClass s_Interface = null;
#endif

		public static void UpdateMediaGallery( string videoFilePath )
		{
			if( videoFilePath != null )
			{
#if UNITY_ANDROID && !UNITY_EDITOR
				// Update video gallery on Android
				if( s_Interface != null )
				{
					s_Interface.CallStatic("UpdateMediaGallery", videoFilePath);
				}
#endif
			}
		}

		protected virtual void Awake()
		{
			if (!_isInitialised)
			{
#if UNITY_ANDROID && !UNITY_EDITOR
				// Get the activity context
				if (s_ActivityContext == null)
				{
					AndroidJavaClass activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
					if (activityClass != null)
					{
						s_ActivityContext = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
					}
				}

				s_Interface = new AndroidJavaClass("com.renderheads.AVPro.MovieCapture.Manager");
				s_Interface.CallStatic("setContext", s_ActivityContext);

				// Get the external storage path early to reduce impact on initial call to StartCapture
				GetAndroidExternalDCIMStoragePath();
#endif

				try
				{
					string pluginVersionString = NativePlugin.GetPluginVersionString();

					// Check that the plugin version number is not too old
					if (!pluginVersionString.StartsWith(NativePlugin.ExpectedPluginVersion))
					{
						Debug.LogWarning("[AVProMovieCapture] Plugin version number " + pluginVersionString + " doesn't match the expected version number " + NativePlugin.ExpectedPluginVersion + ".  It looks like the plugin didn't upgrade correctly.  To resolve this please restart Unity and try to upgrade the package again.");
					}

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
#if !UNITY_2017_1_OR_NEWER
					if (SystemInfo.graphicsDeviceVersion.StartsWith("Metal"))
					{
						Debug.LogError("[AVProMovieCapture] Metal is not supported below Unity 2017, please switch to OpenGLCore in Player Settings.");
						return;
					}
#endif
#elif !UNITY_EDITOR && UNITY_IOS && !UNITY_2017_1_OR_NEWER
					if (Application.isPlaying)
					{
						Debug.LogError("[AVProMovieCapture] iOS is not supported below Unity 2017.");
						return;
					}
#endif
					if (NativePlugin.Init())
					{
						Debug.Log("[AVProMovieCapture] Init version: " + NativePlugin.ScriptVersion + " (plugin v" + pluginVersionString +") with GPU " + SystemInfo.graphicsDeviceName + " " + SystemInfo.graphicsDeviceVersion + " OS: " + SystemInfo.operatingSystem);
						_isInitialised = true;
					}
					else
					{
						Debug.LogError("[AVProMovieCapture] Failed to initialise plugin on version: " + NativePlugin.ScriptVersion + " (plugin v" + pluginVersionString + ") with GPU " + SystemInfo.graphicsDeviceName + " " + SystemInfo.graphicsDeviceVersion + " OS: " + SystemInfo.operatingSystem);
					}
				}
				catch (DllNotFoundException e)
				{
					string missingDllMessage = string.Empty;
					missingDllMessage = "Unity couldn't find the plugin DLL. Please select the native plugin files in 'Plugins/RenderHeads/AVProMovieCapture/Runtime/Plugins' folder and select the correct platform in the Inspector.";
					Debug.LogError("[AVProMovieCapture] " + missingDllMessage);
	#if UNITY_EDITOR
					UnityEditor.EditorUtility.DisplayDialog("Plugin files not found", missingDllMessage, "Ok");
	#endif
					throw e;
				}
			}

			_isDirectX11 = SystemInfo.graphicsDeviceVersion.StartsWith("Direct3D 11");

// Moved to Start() now due to no audio on Andorid when creating a capture component in script
//			SelectVideoCodec();
//			SelectAudioCodec();
//			SelectAudioInputDevice();

			if (_persistAcrossSceneLoads)
			{
				GameObject.DontDestroyOnLoad(this.gameObject);
			}
		}

		static CaptureBase()
		{
			// Force date time to configure locale early to reduce impact on initial call to StartCapture
			DateTime now = DateTime.Now;
#if UNITY_EDITOR
			SetupEditorPlayPauseSupport();
#endif
		}

		public virtual void Start()
		{
			// Moved from Awake() now due to no audio on Andorid when creating a capture component in script
			SelectVideoCodec();
			SelectAudioCodec();
			SelectAudioInputDevice();

			Application.runInBackground = true;
			_waitForEndOfFrame = new WaitForEndOfFrame();

			if (_startTrigger == StartTriggerMode.OnStart)
			{
				StartCapture();
			} 
		}

		// Select the best codec based on criteria
		private static bool SelectCodec(ref Codec codec, CodecList codecList, int forceCodecIndex, string[] codecPriorityList, MediaApi matchMediaApi, bool allowFallbackToFirstCodec, bool logFallbackWarning)
		{
			codec = null;

			// The user has specified their own codec index
			if (forceCodecIndex >= 0)
			{
				if (forceCodecIndex < codecList.Count)
				{
					codec = codecList.Codecs[forceCodecIndex];
				}
			}
			else
			{
				// The user has specified an ordered list of codec name to search for
				if (codecPriorityList != null && codecPriorityList.Length > 0)
				{
					foreach (string codecName in codecPriorityList)
					{
						codec = codecList.FindCodec(codecName.Trim(), matchMediaApi);
						if (codec != null)
						{
							break;
						}
					}
				}
			}

			// If the found codec doesn't match the required MediaApi, set it to null
			if (codec != null && matchMediaApi != MediaApi.Unknown)
			{
				if (codec.MediaApi != matchMediaApi)
				{
					codec = null;
				}
			}

			// Fallback to the first codec
			if (codec == null && allowFallbackToFirstCodec)
			{
				if (codecList.Count > 0)
				{
					if (matchMediaApi != MediaApi.Unknown)
					{
						codec = codecList.GetFirstWithMediaApi(matchMediaApi);
					}
					else
					{
						codec = codecList.Codecs[0];
					}
					if (logFallbackWarning)
					{
						Debug.LogWarning("[AVProMovieCapture] Codec not found. Using the first codec available.");
					}
				}
			}

			return (codec != null);
		}

		public Codec SelectVideoCodec(bool isStartingCapture = false)
		{
			_selectedVideoCodec = null;
#if UNITY_EDITOR_WIN || (!UNITY_EDITOR && UNITY_STANDALONE_WIN)
			SelectCodec(ref _selectedVideoCodec, CodecManager.VideoCodecs, NativeForceVideoCodecIndex, _videoCodecPriorityWindows, MediaApi.Unknown, true, isStartingCapture);
#elif UNITY_EDITOR_OSX || (!UNITY_EDITOR && UNITY_STANDALONE_OSX)
			SelectCodec(ref _selectedVideoCodec, CodecManager.VideoCodecs, NativeForceVideoCodecIndex, _videoCodecPriorityMacOS, MediaApi.Unknown, true, isStartingCapture);
#elif !UNITY_EDITOR && UNITY_IOS
			SelectCodec(ref _selectedVideoCodec, CodecManager.VideoCodecs, NativeForceVideoCodecIndex, null, MediaApi.Unknown, true, isStartingCapture);
#elif !UNITY_EDITOR && UNITY_ANDROID
			SelectCodec(ref _selectedVideoCodec, CodecManager.VideoCodecs, NativeForceVideoCodecIndex, _videoCodecPriorityAndroid, MediaApi.Unknown, true, isStartingCapture);
#endif

			if (isStartingCapture && _selectedVideoCodec == null)
			{
				Debug.LogError("[AVProMovieCapture] Failed to select a suitable video codec");
			}
			return _selectedVideoCodec;
		}

		public Codec SelectAudioCodec()
		{
			_selectedAudioCodec = null;
			if (_audioCaptureSource != AudioCaptureSource.None)
			{
#if UNITY_EDITOR_WIN || (!UNITY_EDITOR && UNITY_STANDALONE_WIN)
				// Audio codec selection requires a video codec to be selected first on Windows
				if (_selectedVideoCodec != null)
				{
					SelectCodec(ref _selectedAudioCodec, CodecManager.AudioCodecs, NativeForceAudioCodecIndex, _audioCodecPriorityWindows, _selectedVideoCodec.MediaApi, true, false);
				}
#elif UNITY_EDITOR_OSX || (!UNITY_EDITOR && UNITY_STANDALONE_OSX)
				SelectCodec(ref _selectedAudioCodec, CodecManager.AudioCodecs, NativeForceAudioCodecIndex, _audioCodecPriorityMacOS, MediaApi.Unknown, true, false);
#elif !UNITY_EDITOR && UNITY_IOS
				SelectCodec(ref _selectedAudioCodec, CodecManager.AudioCodecs, NativeForceAudioCodecIndex, null, MediaApi.Unknown, true, false);
#elif !UNITY_EDITOR && UNITY_ANDROID
				SelectCodec(ref _selectedAudioCodec, CodecManager.AudioCodecs, NativeForceAudioCodecIndex, null, MediaApi.Unknown, true, false);
#endif

				if (_selectedAudioCodec == null)
				{
					//Debug.LogError("[AVProMovieCapture] Failed to select a suitable audio codec");
				}
			}
			return _selectedAudioCodec;
		}

		public Device SelectAudioInputDevice()
		{
			_selectedAudioInputDevice = null;
			if (_audioCaptureSource == AudioCaptureSource.Microphone)
			{
				// Audio input device selection requires a video codec to be selected first
				if (_selectedVideoCodec != null)
				{
					if (_forceAudioInputDeviceIndex >= 0 && _forceAudioInputDeviceIndex < DeviceManager.AudioInputDevices.Count)
					{
						_selectedAudioInputDevice = DeviceManager.AudioInputDevices.Devices[_forceAudioInputDeviceIndex];
					}

					// If the found codec doesn't match the required MediaApi, set it to null
					if (_selectedAudioInputDevice != null && _selectedAudioInputDevice.MediaApi != _selectedVideoCodec.MediaApi)
					{
						_selectedAudioInputDevice = null;
					}

					// Fallback to the first device
					if (_selectedAudioInputDevice == null)
					{
						if (DeviceManager.AudioInputDevices.Count > 0)
						{
							_selectedAudioInputDevice = DeviceManager.AudioInputDevices.GetFirstWithMediaApi(_selectedVideoCodec.MediaApi);
						}
					}
				}
			}
			return _selectedAudioInputDevice;
		}

		public static Vector2 GetRecordingResolution(int width, int height, DownScale downscale, Vector2 maxVideoSize)
		{
			int targetWidth = width;
			int targetHeight = height;
			if (downscale != DownScale.Custom)
			{
				targetWidth /= (int)downscale;
				targetHeight /= (int)downscale;
			}
			else
			{
				if (maxVideoSize.x >= 1.0f && maxVideoSize.y >= 1.0f)
				{
					targetWidth = Mathf.FloorToInt(maxVideoSize.x);
					targetHeight = Mathf.FloorToInt(maxVideoSize.y);
				}
			}

			// Some codecs like Lagarith in YUY2 mode need size to be multiple of 4
			targetWidth = NextMultipleOf4(targetWidth);
			targetHeight = NextMultipleOf4(targetHeight);

			return new Vector2(targetWidth, targetHeight);
		}

		public void SelectRecordingResolution(int width, int height)
		{
			_sourceWidth = width;
			_sourceHeight = height;
			_targetWidth = width;
			_targetHeight = height;
			if (_downScale != DownScale.Custom)
			{
				_targetWidth /= (int)_downScale;
				_targetHeight /= (int)_downScale;
			}
			else
			{
				if (_maxVideoSize.x >= 1.0f && _maxVideoSize.y >= 1.0f)
				{
					_targetWidth = Mathf.FloorToInt(_maxVideoSize.x);
					_targetHeight = Mathf.FloorToInt(_maxVideoSize.y);
				}
			}

			// Some codecs like Lagarith in YUY2 mode need size to be multiple of 4
			_targetWidth = NextMultipleOf4(_targetWidth);
			_targetHeight = NextMultipleOf4(_targetHeight);
		}

		public virtual void OnDestroy()
		{
			_waitForEndOfFrame = null;
			StopCapture(true, true);
			FreePendingFileWrites();

			// Make sure there are no other capture instances running and then deinitialise the plugin
			if (_isApplicationQuiting && _isInitialised)
			{
				// TODO: would it be faster to just look for _pendingFileWrites?
				bool anyCapturesRunning = false;
#if UNITY_EDITOR
				// In editor we have to search hidden objects as well, as the editor window components are created hidden
				CaptureBase[] captures = (CaptureBase[])Resources.FindObjectsOfTypeAll(typeof(CaptureBase));
#else
	#if UNITY_2022_2_OR_NEWER || (!UNITY_2022_1_OR_NEWER && UNITY_2021_3_OR_NEWER) || (!UNITY_2021_1_OR_NEWER && UNITY_2020_3_OR_NEWER)
				CaptureBase[] captures = FindObjectsByType<CaptureBase>(FindObjectsSortMode.None);
	#else
				CaptureBase[] captures = (CaptureBase[])Component.FindObjectsOfType(typeof(CaptureBase));
	#endif
#endif
				foreach (CaptureBase capture in captures)
				{
					if (capture != null && capture.IsCapturing())
					{
						anyCapturesRunning = true;
						break;
					}
				}
				if (!anyCapturesRunning)
				{
					NativePlugin.Deinit();
					_isInitialised = false;
				}
			}
		}

		private void OnDisable()
		{
			// If there are any pending file writes, warn that disabling the component will lead to resource leaks
			if (_pendingFileWrites.Count > 0)
			{
				Debug.LogWarning("Disabling the capture component before file writing has completed is NOT advised");
			}
		}

		private void FreePendingFileWrites()
		{
			foreach (FileWritingHandler handler in _pendingFileWrites)
			{
				handler.Dispose();
			}
			_pendingFileWrites.Clear();

		}

		private void OnApplicationQuit()
		{
			_isApplicationQuiting = true;
		}

// [MOZ] Android only now?
// [MOZ] Need to check macOS behaviour when background execution is disabled
#if !(UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_IPHONE)
		void OnApplicationFocus(bool focusStatus)
		{
			if( focusStatus )
			{
				if( _wasCapturingOnPause )
				{
					_wasCapturingOnPause = false;
					ResumeCapture();

					Debug.Log("OnApplicationFocus: resuming video capture");
				}
			}
		}

		void OnApplicationPause(bool isPaused)
		{
			if (isPaused)
			{
				if (_pauseCaptureOnAppPause)
				{
					if (IsCapturing())
					{
						_wasCapturingOnPause = true;
						Debug.Log("OnApplicationPause: pausing video capture");
						PauseCapture();
					}
				}
			}
			else
			{
				if (_pauseCaptureOnAppPause)
				{
					// Catch coming back from power off state when no lock screen
					OnApplicationFocus(true);
				}
			}
		}
#endif

		protected void EncodeTexture(Texture2D texture)
		{
			Color32[] bytes = texture.GetPixels32();
			GCHandle _frameHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned);

			EncodePointer(_frameHandle.AddrOfPinnedObject());

			if (_frameHandle.IsAllocated)
			{
				_frameHandle.Free();
			}
		}

		protected bool IsUsingUnityAudioComponent()
		{
			if (_outputTarget == OutputTarget.VideoFile && _unityAudioCapture != null)
			{
				if (_audioCaptureSource == AudioCaptureSource.Unity
			#if !AVPRO_MOVIECAPTURE_OFFLINE_AUDIOCAPTURE
					&& _isRealTime
			#endif
					)
				{
					return true;
				}
				#if AVPRO_MOVIECAPTURE_WWISE_SUPPORT
				else if (_audioCaptureSource == AudioCaptureSource.Wwise && !_isRealTime)
				{
					return true;
				}
				#endif
			}
			return false;
		}

		protected bool IsUsingMotionBlur()
		{
			return (_useMotionBlur && !_isRealTime && _motionBlur != null);
		}

		public virtual void EncodePointer(System.IntPtr ptr)
		{
			Debug.Log("Encoding Frame");	
			if (!IsUsingUnityAudioComponent())
			{
				Debug.Log("Encoding with no audio");
				NativePlugin.EncodeFrame(_handle, ptr);
			}
			else
			{
				int audioDataLength = 0;
				System.IntPtr audioDataPtr = _unityAudioCapture.ReadData(out audioDataLength);
				if (audioDataLength > 0)
				{
                    Debug.Log("Encoding with audio data");
                    NativePlugin.EncodeFrameWithAudio(_handle, ptr, audioDataPtr, (uint)audioDataLength);
				}
				else
				{
                    Debug.Log("Encoding without audio as their is none");
                    NativePlugin.EncodeFrame(_handle, ptr);
				}
			}
		}

		public bool IsPrepared()
		{
			return (_handle >= 0);
		}

		public bool IsCapturing()
		{
			return _capturing;
		}

		public bool IsPaused()
		{
			return _paused;
		}

		public int GetRecordingWidth()
		{
			return _targetWidth;
		}

		public int GetRecordingHeight()
		{
			return _targetHeight;
		}

		protected virtual string GenerateTimestampedFilename(string filenamePrefix, string filenameExtension)
		{
			DateTime now = DateTime.Now;

			// StringBuilder significantly faster than using a custom format string for building up the date time string
			StringBuilder sb = new StringBuilder(200);
			sb.Append(now.Year)
			  .Append("-")
			  .Append(now.Month.ToString("D2"))
			  .Append("-")
			  .Append(now.Day.ToString("D2"))
			  .Append("_")
			  .Append(now.Hour.ToString("D2"))
			  .Append("-")
			  .Append(now.Minute.ToString("D2"))
			  .Append("-")
			  .Append(now.Second.ToString("D2"));

			string dateTime = sb.ToString();

			// And significantly faster than using string.format
			sb.Clear()
			  .Append(filenamePrefix)
			  .Append("_")
			  .Append(dateTime)
			  .Append("_")
			  .Append(_targetWidth)
			  .Append("x")
			  .Append(_targetHeight);

			// File extension is optional
			if (!string.IsNullOrEmpty(filenameExtension))
			{
				sb.Append(".");
				sb.Append(filenameExtension);
			}

			return sb.ToString();
		}

#if UNITY_ANDROID && !UNITY_EDITOR
		private static string _externalDCIMStoragePath = null;
		private static string GetAndroidExternalDCIMStoragePath()
		{
			if (_externalDCIMStoragePath != null)
			{
				return _externalDCIMStoragePath;
			}

			if (Application.platform != RuntimePlatform.Android)
			{
				_externalDCIMStoragePath = Application.persistentDataPath;
				return _externalDCIMStoragePath;
			}

			var jClass = new AndroidJavaClass("android.os.Environment");
			_externalDCIMStoragePath = jClass.CallStatic<AndroidJavaObject>("getExternalStoragePublicDirectory", jClass.GetStatic<string>("DIRECTORY_DCIM")).Call<string>("getAbsolutePath");

			return _externalDCIMStoragePath;
		}
#endif

		private static string GetFolder(OutputPath outputPathType, string path)
		{
			string folder = string.Empty;

#if UNITY_IOS && !UNITY_EDITOR
			// iOS only supports a very limited subset of OutputPath so fix up and warn the user
			switch (outputPathType)
			{
				case OutputPath.RelativeToPersistentData:
				case OutputPath.PhotoLibrary:
				case OutputPath.RelativeToTemporaryCachePath:
					// These are fine
					break;
				case OutputPath.RelativeToProject:
				case OutputPath.Absolute:
				case OutputPath.RelativeToDesktop:
				case OutputPath.RelativeToVideos:
				case OutputPath.RelativeToPictures:
					// These are unsupported
				default:
					Debug.LogWarning(string.Format("[AVProMovieCapture] 'OutputPath.{0}' is not supported on iOS, defaulting to 'OutputPath.RelativeToPersistentData'", outputPathType));
					outputPathType = OutputPath.RelativeToPersistentData;
					break;
			}
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
			// Android only supports a very limited subset of OutputPath so fix up and warn the user
			switch (outputPathType)
			{
				case OutputPath.RelativeToPersistentData:
				case OutputPath.RelativeToVideos:
				case OutputPath.RelativeToPictures:
				case OutputPath.PhotoLibrary:
				case OutputPath.Absolute:
				case OutputPath.RelativeToTemporaryCachePath:
					// These are fine
					break;
				case OutputPath.RelativeToProject:
				case OutputPath.RelativeToDesktop:
					// These are unsupported
				default:
					Debug.LogWarning(string.Format("[AVProMovieCapture] 'OutputPath.{0}' is not supported on Android, defaulting to 'OutputPath.RelativeToPersistentData'", outputPathType));
					outputPathType = OutputPath.RelativeToPersistentData;
					break;
			}
#endif
#if UNITY_EDITOR
			// Photo Library is unavailable in the editor
			if (outputPathType == OutputPath.PhotoLibrary)
			{
				Debug.LogWarning("[AVProMovieCapture] 'OutputPath.PhotoLibrary' is not available in the Unity Editor, defaulting to 'OutputPath.RelativeToProject'");
				outputPathType = OutputPath.RelativeToProject;
			}
#endif

			switch (outputPathType)
			{
				case OutputPath.RelativeToProject:
					#if UNITY_STANDALONE_OSX
					// For standalone macOS builds this puts the path at the same level as the application bundle
					folder = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "../.."));
					#else
					folder = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, ".."));
					#endif
					break;
				case OutputPath.RelativeToPersistentData:
					folder = System.IO.Path.GetFullPath(Application.persistentDataPath);
					break;
				case OutputPath.Absolute:
					break;
				case OutputPath.RelativeToDesktop:
					folder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory);
					break;
				case OutputPath.PhotoLibrary:
					#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || (!UNITY_EDITOR && UNITY_IOS)
						// use avpmc-photolibrary as the scheme
						folder = "avpmc-photolibrary:///";	// Three slashes are good as we don't need the host component
						break;
					#else
						// fall through into RelativeToPictures for the other platforms
					#endif
				case OutputPath.RelativeToPictures:
					#if UNITY_ANDROID && !UNITY_EDITOR
						folder = GetAndroidExternalDCIMStoragePath();
					#else
						folder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures);
					#endif
					break;
				case OutputPath.RelativeToVideos:
					#if UNITY_ANDROID && !UNITY_EDITOR
						folder = GetAndroidExternalDCIMStoragePath();
					#else
						#if NET_4_6
							folder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyVideos);
						#else
							folder = System.Environment.GetFolderPath((System.Environment.SpecialFolder)14);    // Older Mono doesn't have MyVideos defined - but still works!
						#endif
					#endif
					break;
				case OutputPath.RelativeToTemporaryCachePath:
					folder = System.IO.Path.GetFullPath(Application.temporaryCachePath);
					break;
			}

			return System.IO.Path.Combine(folder, path);
		}

		private static string GenerateFilePath(OutputPath outputPathType, string path, string filename)
		{
			// Resolve folder
			string fileFolder = GetFolder(outputPathType, path);

			// Combine path and filename
			return System.IO.Path.Combine(fileFolder, filename);
		}

		protected static bool HasExtension(string path, string extension)
		{
			return path.ToLower().EndsWith(extension, StringComparison.OrdinalIgnoreCase);
		}

		protected void GenerateFilename()
		{
			string filename = string.Empty;
			if (_outputTarget == OutputTarget.VideoFile)
			{
				if (!_allowManualFileExtension)
				{
					if (_selectedVideoCodec == null)
					{
						SelectVideoCodec();
						SelectAudioCodec();
					}
					int videoCodec = (_selectedVideoCodec != null) ? _selectedVideoCodec.Index : -1;
					int audioCodec = (_selectedAudioCodec != null) ? _selectedAudioCodec.Index : -1;
					string[] extensions = NativePlugin.GetContainerFileExtensions(videoCodec, audioCodec);
					if (extensions != null && extensions.Length > 0)
					{
						_filenameExtension = extensions[0];
					}
				}

				if (_appendFilenameTimestamp)
				{
					filename = GenerateTimestampedFilename(_filenamePrefix, _filenameExtension);
				}
				else
				{
					StringBuilder sb = new StringBuilder(200);
					sb.Append(_filenamePrefix);
					sb.Append(".");
					sb.Append(_filenameExtension);
					filename = sb.ToString();
				}
			}
			else if (_outputTarget == OutputTarget.ImageSequence)
			{
				// [MOZ] Made the enclosing folder uniquely named, easier for extraction on iOS and simplifies scripts for processing the frames
				string fileExtension = Utils.GetImageFileExtension(NativeImageSequenceFormat);
				filename = GenerateTimestampedFilename(_filenamePrefix, null) + "/frame" + string.Format("-%0{0}d.{1}", _imageSequenceZeroDigits, fileExtension);
			}
			else if (_outputTarget == OutputTarget.NamedPipe)
			{
				_filePath = _namedPipePath;
			}

			if (_outputTarget == OutputTarget.VideoFile ||
				_outputTarget == OutputTarget.ImageSequence)
			{
				OutputPath outputFolderType = _outputFolderType;
				string outputFolderPath = _outputFolderPath;
				_finalFilePath = null;

#if UNITY_ANDROID && !UNITY_EDITOR
				if( _outputFolderType != OutputPath.RelativeToPersistentData && _outputTarget == OutputTarget.VideoFile )
				{
					// Where do we want to write the final file to?
					_finalFilePath = GenerateFilePath(_outputFolderType, _outputFolderPath, filename);

					// Capture to path relative to the project
					outputFolderType = OutputPath.RelativeToPersistentData;
					outputFolderPath = "Captures";

					// Create target final directory if it doesn't exist
					String finalDirectory = Path.GetDirectoryName(_finalFilePath);
					if (!string.IsNullOrEmpty(finalDirectory))
					{
						Directory.CreateDirectory(finalDirectory);
					}
				}
#endif
				_filePath = GenerateFilePath(outputFolderType, outputFolderPath, filename);

				// Check to see if this filename is already in use
				if (ActiveFilePaths.Contains(_filePath))
				{
					// It is, strip the extension
					string extension = Path.GetExtension(_filePath);
					string name = Path.GetFileNameWithoutExtension(_filePath);
					string path = Path.GetDirectoryName(_filePath);
					string newPath = null;
					int i = 2;
					do
					{
						const string fmt = "{0} {1}";
						string newName = String.Format(fmt, name, i++);
						newPath = Path.Combine(path, newName);
						newPath = Path.ChangeExtension(newPath, extension);
					}
					while (ActiveFilePaths.Contains(newPath));
					_filePath = newPath;
				}

				ActiveFilePaths.Add(_filePath);

#if !UNITY_EDITOR && (UNITY_STANDALONE_OSX || UNITY_IOS)
				if (_outputFolderType != OutputPath.PhotoLibrary)
#endif
				{
					// Create target directory if it doesn't exist
					String directory = Path.GetDirectoryName(_filePath);
					if (!string.IsNullOrEmpty(directory))
					{
						Directory.CreateDirectory(directory);
					}
				}
			}
		}

		public UnityAudioCapture FindOrCreateUnityAudioCapture(bool logWarnings)
		{
			UnityAudioCapture result = null;

			if (_audioCaptureSource == AudioCaptureSource.Unity)
			{
				Type audioCaptureType = null;
				if (_isRealTime)
				{
					audioCaptureType = typeof(CaptureAudioFromAudioListener);
				}
				else
				{
					#if AVPRO_MOVIECAPTURE_OFFLINE_AUDIOCAPTURE
					audioCaptureType = typeof(CaptureAudioFromAudioRenderer);
					#endif
				}

				if (audioCaptureType != null)
				{
					// Try to find an existing matching component locally
					result = (UnityAudioCapture)this.GetComponent(audioCaptureType);
					if (result == null)
					{
						// Try to find an existing matching component globally
#if UNITY_HAS_FINDFIRSTOBJECTBYTYPE
						result = FindFirstObjectByType<UnityAudioCapture>();
#else
						result = (UnityAudioCapture)GameObject.FindObjectOfType(audioCaptureType);
#endif
					}

					// No existing component was found, so create one
					if (result == null)
					{
						// Find a suitable gameobject to add the component to
						GameObject parentGameObject = null;
						if (_isRealTime)
						{
							// Find an AudioListener to attach the UnityAudioCapture component to
							AudioListener audioListener = this.GetComponent<AudioListener>();
							if (audioListener == null)
							{
#if UNITY_HAS_FINDFIRSTOBJECTBYTYPE
								audioListener = FindFirstObjectByType<AudioListener>();
#else
								audioListener = GameObject.FindObjectOfType<AudioListener>();
#endif
							}
							parentGameObject = audioListener.gameObject;
						}
						else
						{
							parentGameObject = this.gameObject;
						}

						// Create the component
						if (_isRealTime)
						{
							if (parentGameObject != null)
							{
								result = (UnityAudioCapture)parentGameObject.AddComponent(audioCaptureType);
								if (logWarnings)
								{
									Debug.LogWarning("[AVProMovieCapture] Capturing audio from Unity without an UnityAudioCapture assigned so we had to create one manually (very slow).  Consider adding a UnityAudioCapture component to your scene and assigned it to this MovieCapture component.");
								}
							}
							else
							{
								if (logWarnings)
								{
									Debug.LogWarning("[AVProMovieCapture] No AudioListener found in scene.  Unable to capture audio from Unity.");
								}
							}
						}
						else
						{
							#if AVPRO_MOVIECAPTURE_OFFLINE_AUDIOCAPTURE
							result = (UnityAudioCapture)parentGameObject.AddComponent(audioCaptureType);
							((CaptureAudioFromAudioRenderer)result).Capture = this;
							if (logWarnings)
							{
								Debug.LogWarning("[AVProMovieCapture] Capturing audio from Unity without an UnityAudioCapture assigned so we had to create one manually (very slow).  Consider adding a UnityAudioCapture component to your scene and assigned it to this MovieCapture component.");
							}
							#endif
						}
					}
					else
					{
						if (logWarnings)
						{
							Debug.LogWarning("[AVProMovieCapture] Capturing audio from Unity without an UnityAudioCapture assigned so we had to search for one manually (very slow)");
						}
					}
				}
			}
#if AVPRO_MOVIECAPTURE_WWISE_SUPPORT
			else if (_audioCaptureSource == AudioCaptureSource.Wwise)
			{
				Type audioCaptureType = null;
				if (!_isRealTime)
				{
					audioCaptureType = typeof(CaptureAudioFromWwise);
				}
				if (audioCaptureType != null)
				{
					// Try to find an existing matching component locally
					result = (UnityAudioCapture)this.GetComponent(audioCaptureType);
					if (result == null)
					{
						// Try to find an existing matching component globally
						result = (UnityAudioCapture)GameObject.FindObjectOfType(audioCaptureType);
					}

					// No existing component was found, so create one
					if (result == null)
					{
						result = (UnityAudioCapture)this.gameObject.AddComponent(audioCaptureType);
					}

					if (result)
					{
						((CaptureAudioFromWwise)result).Capture = this;
					}
				}
			}
#endif // AVPRO_MOVIECAPTURE_WWISE_SUPPORT

			return result;
		}

		private bool ValidateEditionFeatures()
		{
			bool canContinueCapture = true;
			if (NativePlugin.IsBasicEdition())
			{
				string issues = string.Empty;

				// Abortable issues
				// RJT NOTE: D3D12 should be available in all builds as per: https://github.com/RenderHeads/UnityPlugin-AVProMovieCapture/issues/289
/*				if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Direct3D12)
				{
					issues += "• D3D12 is not supported, please switch to D3D11.  Aborting capture.\n";
					canContinueCapture = false;
				}*/
				if (this is CaptureFromCamera || this is CaptureFromTexture || this is CaptureFromCamera360 || this is CaptureFromCamera360ODS)
				{
					issues += "• Only CaptureFromScreen component is supported.  Aborting capture.\n";
					canContinueCapture = false;
				}

				// Continuable issues
				if (canContinueCapture)
				{
					if (SelectedVideoCodec != null && SelectedVideoCodec.Name.IndexOf("H264") < 0)
					{
						issues += "• Only H264 video codec supported.  Switching to H264\n";
						NativeForceVideoCodecIndex = 0;
						SelectVideoCodec(false);
					}
					if (SelectedAudioCodec != null && SelectedAudioCodec.Name.IndexOf("AAC") < 0)
					{
						issues += "• Only AAC audio codec supported.  Switching to AAC\n";
						NativeForceAudioCodecIndex = 0;
						SelectAudioCodec();
					}
					if (!IsRealTime)
					{
						issues += "• Non-realtime captures are not supported.  Switching to realtime capture mode.\n";
						IsRealTime = true;
					}
					if (OutputTarget != OutputTarget.VideoFile)
					{
						issues += "• Only output to video file is supported.  Switching to video file output.\n";
						OutputTarget = OutputTarget.VideoFile;
						FilenameExtension = "mp4";
						GenerateFilename();
					}
					if (AudioCaptureSource != AudioCaptureSource.None && AudioCaptureSource != AudioCaptureSource.Unity)
					{
						issues += "• Audio source '" + AudioCaptureSource + "' not supported.  Disabling audio capture.\n";
						AudioCaptureSource = AudioCaptureSource.None;
					}
					if (FrameRate != 30f)
					{
						issues += "• Frame rate '" + FrameRate + "' not supported.  Switching to 30 FPS.\n";
						FrameRate = 30f;
					}
					if (GetEncoderHints().videoHints.supportTransparency || GetEncoderHints().videoHints.transparency != Transparency.None)
					{
						issues += "• Transparent capture not supported.  Disabling transparent capture\n";
						GetEncoderHints().videoHints.supportTransparency = false;
						GetEncoderHints().videoHints.transparency = Transparency.None;
						_Transparency = Transparency.None;
					}
				}

				// Log/Display issues
				if (!string.IsNullOrEmpty(issues))
				{
					string message = "Limitations of Basic Edition reached:\n" + issues + "Please upgrade to use these feature, or visit '" + DocEditionsURL + "' for more information.";
					if (canContinueCapture)
					{
						Debug.LogWarning("[AVProMovieCapture] " + message);
					}
					else
					{
						Debug.LogError("[AVProMovieCapture] " + message);
					}

					#if UNITY_EDITOR
					message = "Limitations of Basic Edition reached:\n\n" + issues + "\nPlease upgrade to use these feature.  View documention for more information.";
					if (!UnityEditor.EditorUtility.DisplayDialog("AVPro Movie Capture", message, "Ok", "More Info"))
					{
						Application.OpenURL(DocEditionsURL);
					}
					#endif
				}
			}
			return canContinueCapture;
		}

		public virtual bool PrepareCapture()
		{
			if (!ValidateEditionFeatures())
			{
				return false;
			}

			// Delete file if it already exists
			if (_outputTarget == OutputTarget.VideoFile && File.Exists(_filePath))
			{
				File.Delete(_filePath);
			}

			_stats = new CaptureStats();

			if (_minimumDiskSpaceMB > 0 && _outputTarget == OutputTarget.VideoFile)
			{
				ulong freeSpace = 0;
				if (Utils.DriveFreeBytes(_filePath, out freeSpace))
				{
					_freeDiskSpaceMB = (long)(freeSpace / (1024 * 1024));
				}

				if (!IsEnoughDiskSpace())
				{
					Debug.LogError("[AVProMovieCapture] Not enough free space to start capture.  Stopping capture.");
					return false;
				}
			}

			if (_isRealTime)
			{
				/*if (_allowFrameRateChange)
				{
					_oldTargetFrameRate = Application.targetFrameRate;
					Application.targetFrameRate = (int)_frameRate;
				}*/
			}
			else
			{
				// Disable vsync
				#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
					if (_allowVSyncDisable)
					{
						// iOS and Android do not support disabling vsync so use _oldVsyncCount to store the current target framerate.
						_oldVSyncCount = Application.targetFrameRate;
						// We want to runs as fast as possible.
						Application.targetFrameRate = -1;
					}
				#else
					if (_allowVSyncDisable && !Screen.fullScreen && QualitySettings.vSyncCount > 0)
					{
						_oldVSyncCount = QualitySettings.vSyncCount;
						QualitySettings.vSyncCount = 0;
					}
				#endif

				if (_useMotionBlur && _motionBlurSamples > 1)
				{
					#if AVPRO_MOVIECAPTURE_CAPTUREDELTA_SUPPORT
					Time.captureDeltaTime = 1f / (_motionBlurSamples * _frameRate);
					#else
					Time.captureFramerate = (int)(_motionBlurSamples * _frameRate);
					#endif

					// FromTexture and FromCamera360 captures don't require a camera for rendering, so set up the motion blur component differently
					if (this is CaptureFromTexture || this is CaptureFromCamera360 || this is CaptureFromCamera360ODS)
					{
						if (_motionBlur == null)
						{
							_motionBlur = this.GetComponent<MotionBlur>();
						}
						if (_motionBlur == null)
						{
							_motionBlur = this.gameObject.AddComponent<MotionBlur>();
						}
						if (_motionBlur != null)
						{
							_motionBlur.NumSamples = _motionBlurSamples;
							_motionBlur.SetTargetSize(_targetWidth, _targetHeight);
							_motionBlur.enabled = false;
						}
					}
					// FromCamera and FromScreen use this path
					else if (_motionBlurCameras.Length > 0)
					{
						// Setup the motion blur filters where cameras are used
						foreach (Camera camera in _motionBlurCameras)
						{
							MotionBlur mb = camera.GetComponent<MotionBlur>();
							if (mb == null)
							{
								mb = camera.gameObject.AddComponent<MotionBlur>();
							}
							if (mb != null)
							{
								mb.NumSamples = _motionBlurSamples;
								mb.enabled = true;
								_motionBlur = mb;
							}
						}
					}
				}
				else
				{
					#if AVPRO_MOVIECAPTURE_CAPTUREDELTA_SUPPORT
					Time.captureDeltaTime = 1f / _frameRate;
					#else
					Time.captureFramerate = (int)_frameRate;
					#endif
				}

				// Change physics update speed
				_oldFixedDeltaTime = Time.fixedDeltaTime;
				#if AVPRO_MOVIECAPTURE_CAPTUREDELTA_SUPPORT
				Time.fixedDeltaTime = Time.captureDeltaTime;
				#else
				Time.fixedDeltaTime = 1.0f / Time.captureFramerate;
				#endif
			}

			// Resolve desired audio source
			_stats.AudioCaptureSource = AudioCaptureSource.None;
			if (_audioCaptureSource != AudioCaptureSource.None && _outputTarget == OutputTarget.VideoFile)
			{
				if (_audioCaptureSource == AudioCaptureSource.Microphone && _isRealTime)
				{
					if (_selectedAudioInputDevice != null)
					{
						_stats.AudioCaptureSource = AudioCaptureSource.Microphone;
					}
					else
					{
						Debug.LogWarning("[AVProMovieCapture] No microphone found");
					}
				}
				else if (_audioCaptureSource == AudioCaptureSource.Unity || _audioCaptureSource == AudioCaptureSource.Wwise)
				{
					// If there is already a capture component, make sure it's the right one otherwise remove it
					if (_unityAudioCapture != null)
					{
						bool removeComponent = false;
						if (_audioCaptureSource == AudioCaptureSource.Unity)
						{
							if (_isRealTime)
							{
								removeComponent = !(_unityAudioCapture is CaptureAudioFromAudioListener);
							}
							else
							{
								removeComponent = (_unityAudioCapture is CaptureAudioFromAudioListener);
							}
						}
						else if (_audioCaptureSource == AudioCaptureSource.Wwise)
						{
							#if !AVPRO_MOVIECAPTURE_WWISE_SUPPORT
							removeComponent = true;
							#else
							if (_isRealTime)
							{
								removeComponent = true;
							}
							else
							{
								removeComponent = !(_unityAudioCapture is CaptureAudioFromWwise);
							}
							#endif
						}
						if (removeComponent)
						{
							Destroy(_unityAudioCapture);
							_unityAudioCapture = null;
						}
					}

					// We if try to capture audio from Unity but there isn't an UnityAudioCapture component set
					if (_unityAudioCapture == null)
					{
						_unityAudioCapture = FindOrCreateUnityAudioCapture(true);
					}
					if (_unityAudioCapture != null)
					{
						_unityAudioCapture.PrepareCapture();
						_stats.UnityAudioSampleRate = _unityAudioCapture.SampleRate;
						_stats.UnityAudioChannelCount = _unityAudioCapture.ChannelCount;
						_stats.AudioCaptureSource = _audioCaptureSource;
					}
					else
					{
						Debug.LogWarning("[AVProMovieCapture] Unable to create AudioCapture component in mode " + _audioCaptureSource.ToString());
					}
				}
				else if (_audioCaptureSource == AudioCaptureSource.UnityAudioMixer)
				{
					_stats.UnityAudioSampleRate = AudioSettings.outputSampleRate;
					_stats.UnityAudioChannelCount = UnityAudioCapture.GetUnityAudioChannelCount();
					_stats.AudioCaptureSource = _audioCaptureSource;
				}
				else if (_audioCaptureSource == AudioCaptureSource.Manual)
				{
					_stats.UnityAudioSampleRate = _manualAudioSampleRate;
					_stats.UnityAudioChannelCount = _manualAudioChannelCount;
					_stats.AudioCaptureSource = AudioCaptureSource.Manual;
				}
			}

			if (_selectedVideoCodec == null)
				return false;

			string info = string.Empty;
			if (_logCaptureStartStop)
			{
				StringBuilder sb = new StringBuilder(200);
				sb.Append(_targetWidth)
				  .Append("x")
				  .Append(_targetHeight)
				  .Append(" @ ")
				  .AppendFormat("{0:F2}", _frameRate)
				  .Append("fps [")
				  .Append(NativePlugin.GetPixelFormatName(_pixelFormat))
				  .Append("]");
				info = sb.ToString();

				if (_outputTarget == OutputTarget.VideoFile)
				{
					info += string.Format(" vcodec:'{0}'", _selectedVideoCodec.Name);
					if (_stats.AudioCaptureSource != AudioCaptureSource.None)
					{
						if (_audioCaptureSource == AudioCaptureSource.Microphone && _selectedAudioInputDevice != null)
						{
							info += string.Format(" audio source:'{0}'", _selectedAudioInputDevice.Name);
						}
						else if (_audioCaptureSource == AudioCaptureSource.Unity && _unityAudioCapture != null)
						{
							info += string.Format(" audio source:'Unity' {0}hz {1} channels", _stats.UnityAudioSampleRate, _stats.UnityAudioChannelCount);
						}
						else if (_audioCaptureSource == AudioCaptureSource.UnityAudioMixer)
						{
							info += string.Format(" audio source:'AudioMixer' {0}hz {1} channels", _stats.UnityAudioSampleRate, _stats.UnityAudioChannelCount);
						}
						else if (_audioCaptureSource == AudioCaptureSource.Manual)
						{
							info += string.Format(" audio source:'Manual' {0}hz {1} channels", _stats.UnityAudioSampleRate, _stats.UnityAudioChannelCount);
						}
						else if (_audioCaptureSource == AudioCaptureSource.Wwise && _unityAudioCapture != null)
						{
							info += string.Format(" audio source:'Wwise' {0}hz {1} channels", _stats.UnityAudioSampleRate, _stats.UnityAudioChannelCount);
						}
						if (_selectedAudioCodec != null)
						{
							info += string.Format(" acodec:'{0}'", _selectedAudioCodec.Name);
						}
					}

					info += string.Format(" to file: '{0}'", _filePath);
				}
				else if (_outputTarget == OutputTarget.ImageSequence)
				{
					info += string.Format(" to file: '{0}'", _filePath);
				}
				else if (_outputTarget == OutputTarget.NamedPipe)
				{
					info += string.Format(" to pipe: '{0}'", _filePath);
				}
			}

			if (_outputTarget == OutputTarget.VideoFile)
			{
				if (_logCaptureStartStop)
				{
					Debug.Log("[AVProMovieCapture] Start File Capture: " + info);
				}
				bool useRealtimeClock = (_isRealTime && _timelapseScale <= 1);
				AudioCaptureSource audioCaptureSource = _stats.AudioCaptureSource;
				if (audioCaptureSource == AudioCaptureSource.Wwise) { audioCaptureSource = AudioCaptureSource.Unity; }	// This is a mild hack until we rebuild the plugins

				VideoEncoderHints hints = GetEncoderHints().videoHints;
				hints.colourSpace = (VideoEncoderHints.ColourSpace)QualitySettings.activeColorSpace;
				hints.sourceWidth = _sourceWidth;
				hints.sourceHeight = _sourceHeight;

				// Android only
				hints.androidNoCaptureRotation = _androidNoCaptureRotation;

				// iOS only
				hints.iOSSaveCaptureWhenAppLosesFocus = _iOSSaveCaptureWhenAppLosesFocus;

				hints.supportTransparency = (hints.transparency == Transparency.Codec);

				_handle = NativePlugin.CreateRecorderVideo(
					_filePath,
					(uint)_targetWidth,
					(uint)_targetHeight,
					_frameRate,
					(int)_pixelFormat,
					useRealtimeClock,
					_flipVertically ? !_isTopDown : _isTopDown,
					_selectedVideoCodec.Index,
					audioCaptureSource,
					_stats.UnityAudioSampleRate,
					_stats.UnityAudioChannelCount,
					(_selectedAudioInputDevice != null) ? _selectedAudioInputDevice.Index : -1,
					(_selectedAudioCodec != null) ? _selectedAudioCodec.Index : -1,
					_forceGpuFlush,
					hints);
			}
			else if (_outputTarget == OutputTarget.ImageSequence)
			{
				if (_logCaptureStartStop)
				{
					Debug.Log("[AVProMovieCapture] Start Images Capture: " + info);
				}
				bool useRealtimeClock = (_isRealTime && _timelapseScale <= 1);

				ImageEncoderHints hints = GetEncoderHints().imageHints;
				hints.colourSpace = (ImageEncoderHints.ColourSpace)QualitySettings.activeColorSpace;
				hints.sourceWidth = _sourceWidth;
				hints.sourceHeight = _sourceHeight;
				hints.supportTransparency = ( hints.transparency == Transparency.Codec );

				Debug.Log("[AVProMovieCapture] Setting Image Sequence Format to: " + (int)NativeImageSequenceFormat + " With File Extension: " + Utils.GetImageFileExtension(NativeImageSequenceFormat));

				_handle = NativePlugin.CreateRecorderImages(
					_filePath,
					(uint)_targetWidth,
					(uint)_targetHeight,
					_frameRate,
					(int)_pixelFormat,
					useRealtimeClock,
					_isTopDown,
					(int)NativeImageSequenceFormat,
					_forceGpuFlush,
					_imageSequenceStartFrame,
					hints);
			}
			else if (_outputTarget == OutputTarget.NamedPipe)
			{
				if (_logCaptureStartStop)
				{
					Debug.Log("[AVProMovieCapture] Start Pipe Capture: " + info);
				}
				_handle = NativePlugin.CreateRecorderPipe(_filePath, (uint)_targetWidth, (uint)_targetHeight, _frameRate,
																	 (int)_pixelFormat, _isTopDown,
																	 /*GetEncoderHints().videoHints.supportTransparency*//*(GetEncoderHints().videoHints.transparency == Transparency.Codec)*/(int)(GetEncoderHints().videoHints.transparency),
																	 _forceGpuFlush);
			}

			if (_handle >= 0)
			{
				RenderThreadEvent(NativePlugin.PluginEvent.Setup);
			}
			else
			{
				Debug.LogError("[AVProMovieCapture] Failed to create recorder");

				// Try to give a reason why it failed
#if UNITY_EDITOR_WIN || (!UNITY_EDITOR && UNITY_STANDALONE_WIN)
				if (_selectedVideoCodec.MediaApi == MediaApi.MediaFoundation)
				{
					if (!HasExtension(_filePath, ".mp4"))
					{
						Debug.LogError("[AVProMovieCapture] When using a MediaFoundation codec the MP4 extension must be used");
					}

					// MF H.264 encoder has a limit of Level 5.2 which is 9,437,184 luma pixels
					// but we've seen it fail slightly below this limit, so we test against 9360000
					// to offer a useful potential error message
					if (((_targetWidth * _targetHeight) >= 9360000) && _selectedVideoCodec.Name.Contains("H264"))
					{
						Debug.LogError("[AVProMovieCapture] Resolution is possibly too high for the MF H.264 codec");
					}
				}
				else if (_selectedVideoCodec.MediaApi == MediaApi.DirectShow)
				{
					if (HasExtension(_filePath, ".mp4") && _selectedVideoCodec.Name.Contains("Uncompressed"))
					{
						Debug.LogError("[AVProMovieCapture] Uncompressed video codec not supported with MP4 extension, use AVI instead for uncompressed");
					}
				}
#endif

				StopCapture();
			}

// (mac|i)OS only for now
#if UNITY_EDITOR_OSX || (!UNITY_EDITOR && (UNITY_STANDALONE_OSX || UNITY_IOS))
			SetupErrorHandler();
#endif
			return (_handle >= 0);
		}

#if UNITY_EDITOR_OSX || (!UNITY_EDITOR && (UNITY_STANDALONE_OSX || UNITY_IOS))

		static Dictionary<int, CaptureBase> _HandleToCaptureMap = new Dictionary<int, CaptureBase>();

		private void SetupErrorHandler()
		{
			NativePlugin.ErrorHandlerDelegate errorHandlerDelegate = new NativePlugin.ErrorHandlerDelegate(ErrorHandler);
			System.IntPtr func = Marshal.GetFunctionPointerForDelegate(errorHandlerDelegate);
			NativePlugin.SetErrorHandler(_handle, func);
			_HandleToCaptureMap.Add(_handle, this);
		}

		private void CleanupErrorHandler()
		{
			_HandleToCaptureMap.Remove(_handle);
		}

#if ENABLE_IL2CPP
		[MonoPInvokeCallback(typeof(NativePlugin.ErrorHandlerDelegate))]
#endif
		private static void ErrorHandler(int handle, int domain, int code, string message)
		{
			CaptureBase capture;
			if (_HandleToCaptureMap.TryGetValue(handle, out capture))
			{
				capture.ActualErrorHandler((MCErrorDomain)domain, (MCPluginError)code, message);
			}
		}

		private enum MCErrorDomain: int
		{
			Unknown = -1,
			Plugin,
			AVFoundation
		}

		private enum MCPluginError: int
		{
			None =  0,
			Unknown = -1,

			CaptureStoppedDueToAppSuspension = -100,

			// Unity specific error codes
			UnityRendererUnsupported = -500,
			UnityMetalUnsupported    = -501,
		}

		private void ActualErrorHandler(MCErrorDomain domain, MCPluginError code, string message) {
			Debug.LogErrorFormat("Error: domain: {0}, code: {1}, message: {2}", domain, code, message);

			bool cancelCapture = false;
			switch (domain)
			{
				case MCErrorDomain.Plugin:
					switch (code)
					{
						case MCPluginError.CaptureStoppedDueToAppSuspension:
							// The capture will already have been stopped but go through the motions so that Unity's state
							// matched the plugins.
							StopCapture();
							break;

						default:
							cancelCapture = true;
							break;
					}
					break;

				default:
					cancelCapture = true;
					break;
			}

			if (_capturing && cancelCapture)
			{
				CancelCapture();
				Debug.LogError("Capture cancelled");
			}
		}
#endif

		public void QueueStartCapture()
		{
			_queuedStartCapture = true;
			_stats = new CaptureStats();
		}

		public bool IsStartCaptureQueued()
		{
			return _queuedStartCapture;
		}

		protected void UpdateInjectionOptions(StereoPacking stereoPacking, SphericalVideoLayout sphericalVideoLayout)
		{
			VideoEncoderHints videoHints = GetEncoderHints().videoHints;
			if (videoHints.injectStereoPacking == NoneAutoCustom.Auto) { videoHints.stereoPacking = stereoPacking; }
			if (videoHints.injectSphericalVideoLayout == NoneAutoCustom.Auto) {	videoHints.sphericalVideoLayout = sphericalVideoLayout;	}
		}

		public bool StartCapture()
		{
			// make sure if their are any GUI compoents in the scene that they are using this (the currently running item as their Movie Capture)
#if UNITY_HAS_FINDFIRSTOBJECTBYTYPE
            var testingGUI = FindFirstObjectByType<CaptureGUI>();
#else
			var testingGUI = (CaptureGUI)GameObject.FindObjectOfType(typeof(CaptureGUI));
#endif
			if ( testingGUI != null)
                testingGUI.MovieCapture = this;


            if (_capturing)
			{
				return false;
			}

			if (_waitForEndOfFrame == null)
			{
				// Start() hasn't happened yet, so queue the StartCapture
				QueueStartCapture();
				return false;
			}

			if (_handle < 0)
			{
				if (!PrepareCapture())
				{
					return false;
				}
			}

			if (_handle >= 0)
			{
				if (IsUsingUnityAudioComponent())
				{
					_unityAudioCapture.StartCapture();
				}

				// Set limit to number of frames encoded (or 0 for unlimited)
				{
					uint frameLimit = 0;
					if (_stopMode == StopMode.FramesEncoded)
					{
						frameLimit = (uint)_stopFrames;
					}
					else if (_stopMode == StopMode.SecondsEncoded && !_isRealTime)
					{
						frameLimit = (uint)Mathf.FloorToInt(_stopSeconds * _frameRate);
					}
					NativePlugin.SetEncodedFrameLimit(_handle, frameLimit);
				}

				if (!NativePlugin.Start(_handle))
				{
					StopCapture(true);
					Debug.LogError("[AVProMovieCapture] Failed to start recorder");
					return false;
				}
				OnCaptureStart.Invoke();

				ResetFPS();
				_captureStartTime = Time.realtimeSinceStartup;
				_capturePrePauseTotalTime = 0f;

				// NOTE: We set this to the elapsed time so that the first frame is captured immediately
				_timeSinceLastFrame = GetSecondsPerCaptureFrame();

				#if AVPRO_MOVIECAPTURE_PLAYABLES_SUPPORT
				if (!_isRealTime && _timelineController != null)
				{
					_timelineController.StartCapture();
				}
				#endif
				#if AVPRO_MOVIECAPTURE_VIDEOPLAYER_SUPPORT
				if (!_isRealTime && _videoPlayerController != null)
				{
					_videoPlayerController.StartCapture();
				}
				#endif

				_capturing = true;
				_paused = false;

				if (_startDelay != StartDelayMode.None)
				{
					_startDelayTimer = 0f;
					_startPaused = true;
					PauseCapture();
				}

				#if UNITY_EDITOR
				if (UnityEditor.EditorApplication.isPaused)
				{
					PauseCapture();
				}
				#endif
			}

			return _capturing;
		}

		public void PauseCapture()
		{
			if (_capturing && !_paused)
			{
				if (IsUsingUnityAudioComponent())
				{
					_unityAudioCapture.PauseCapture();
				}
				NativePlugin.Pause(_handle);

				if (!_isRealTime)
				{
					// TODO: should be store the timeScale value and restore it instead of assuming timeScale == 1.0?
					Time.timeScale = 0f;
				}

				_paused = true;
				ResetFPS();
			}
		}

		public void ResumeCapture()
		{
			if (_capturing && _paused)
			{
				if (IsUsingUnityAudioComponent())
				{
					_unityAudioCapture.ResumeCapture();
				}

				NativePlugin.Start(_handle);

				if (!_isRealTime)
				{
					Time.timeScale = 1f;
				}

				_paused = false;
				if (_startPaused)
				{
					_captureStartTime = Time.realtimeSinceStartup;
					_capturePrePauseTotalTime = 0f;
					_startPaused = false;
				}
			}
		}

		public void CancelCapture()
		{
			StopCapture(true, false, true);
		}

		public static void DeleteCapture(OutputTarget outputTarget, string path)
		{
			try
			{
				if (outputTarget == OutputTarget.VideoFile && File.Exists(path))
				{
					File.Delete(path);
				}
				else if (outputTarget == OutputTarget.ImageSequence)
				{
					string directory = Path.GetDirectoryName(path);
					if (Directory.Exists(directory))
					{
						Directory.Delete(directory, true);
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning("[AVProMovieCapture] Failed to delete capture - " + ex.Message);
			}
		}

		public virtual void UnprepareCapture()
		{
#if UNITY_EDITOR_OSX || (!UNITY_EDITOR && (UNITY_STANDALONE_OSX || UNITY_IOS))
			CleanupErrorHandler();
#endif
		}

		public static string LastFileSaved
		{
			get
			{
#if UNITY_EDITOR
				return UnityEditor.EditorPrefs.GetString("AVProMovieCapture-LastSavedFile", string.Empty);
#else
				return PlayerPrefs.GetString("AVProMovieCapture-LastSavedFile", string.Empty);
#endif
			}
			set
			{
				PlayerPrefs.SetString("AVProMovieCapture-LastSavedFile", value);
#if UNITY_EDITOR
				UnityEditor.EditorPrefs.SetString("AVProMovieCapture-LastSavedFile", value);
#endif
			}
		}

		protected void RenderThreadEvent(NativePlugin.PluginEvent renderEvent)
		{
			NativePlugin.RenderThreadEvent(renderEvent, _handle);
		}

		public virtual void StopCapture(bool skipPendingFrames = false, bool ignorePendingFileWrites = false, bool deleteCapture = false)
		{
			if (_capturing)
			{
				if (_logCaptureStartStop)
				{
					if (!deleteCapture)
					{
						Debug.Log("[AVProMovieCapture] Stopping capture " + _handle);
					}
					else
					{
						Debug.Log("[AVProMovieCapture] Canceling capture " + _handle);
					}
				}
				_capturing = false;
			}

			bool applyPostOperations = false;
			FileWritingHandler fileWritingHandler = null;
			if (_handle >= 0)
			{
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
				// RJT NOTE: If we try to flush frames before finalising under Windows it can lead to a hang (and doesn't actually have
				// any impact otherwise) so only allow flushing of pending data if we're cancelling (and thus not finalising) our capture
				// - https://github.com/RenderHeads/UnityPlugin-AVProMovieCapture/issues/343
				if (!deleteCapture) { skipPendingFrames = false; }
#endif
				NativePlugin.Stop(_handle, skipPendingFrames);

				UnprepareCapture();

				if (_outputTarget == OutputTarget.VideoFile)
				{
					applyPostOperations = true;
				}

#if UNITY_ANDROID && !UNITY_EDITOR
				bool updateMediaGallery = ( _androidUpdateMediaGallery && ( _outputFolderType == OutputPath.RelativeToPictures || _outputFolderType == OutputPath.RelativeToVideos || _outputFolderType == OutputPath.PhotoLibrary ) );
#else
				bool updateMediaGallery = ( _outputFolderType == OutputPath.RelativeToPictures || _outputFolderType == OutputPath.RelativeToVideos || _outputFolderType == OutputPath.PhotoLibrary );
#endif

				fileWritingHandler = new FileWritingHandler(_outputTarget, _filePath, _handle, deleteCapture, _finalFilePath, updateMediaGallery);
				if (_completedFileWritingAction != null)
				{
					fileWritingHandler.CompletedFileWritingAction = _completedFileWritingAction;
				}

				// Free the recorder, or if the file is still being written, store the action to be invoked where it is complete
				bool canFreeRecorder = (ignorePendingFileWrites || NativePlugin.IsFileWritingComplete(_handle));

				if (canFreeRecorder)
				{
					// If there is an external action set up, then notify it that writing has begun
					if (_beginFinalFileWritingAction != null)
					{
						_beginFinalFileWritingAction.Invoke(fileWritingHandler);
					}

					// Complete writing immediately
					fileWritingHandler.Dispose();
					fileWritingHandler = null;
				}
				else
				{
					// If no external action has been set up for the checking when the file writing begins and end,
					// add it to an internal list so we can make sure it completes
					if (_beginFinalFileWritingAction == null)
					{
						_pendingFileWrites.Add(fileWritingHandler);
					}

					if (!deleteCapture)
					{
						VideoEncoderHints hints = GetEncoderHints().videoHints;
						if (applyPostOperations && CanApplyPostOperations(_filePath, hints, _finalFilePath))
						{
							MP4FileProcessing.Options options = CreatePostOperationsOptions(hints, _finalFilePath);
							fileWritingHandler.SetFilePostProcess(options);
						}
					}
					applyPostOperations = false;
				}

				// If there is an external action set up, then notify it that writing has begun
				if (_beginFinalFileWritingAction != null && fileWritingHandler != null)
				{
					_beginFinalFileWritingAction.Invoke(fileWritingHandler);
				}

				_handle = -1;

				// Save the last captured path
				if (!deleteCapture)
				{
					if (!string.IsNullOrEmpty(_filePath))
					{
						if (_outputTarget == OutputTarget.VideoFile)
						{
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || (!UNITY_EDITOR && UNITY_IOS)
							if (_outputFolderType == OutputPath.PhotoLibrary)
							{
								// Writing to the photo library is async so we won't have a final path until file writing has completed
								fileWritingHandler.CompletedFileWritingAction += delegate(FileWritingHandler fwh)
								{
									Debug.Log($"CompletedFileWritingAction - updating LastFileSaved to {fwh.FinalPath}");
									LastFileSaved = fwh.FinalPath;
								};
							}
							else
#endif
							// If there is a final file path we're moving the file to then use that
							LastFileSaved = _finalFilePath != null ? _finalFilePath : _filePath;
						}
						else if (_outputTarget == OutputTarget.ImageSequence)
						{
							LastFileSaved = System.IO.Path.GetDirectoryName(_filePath);
						}
					}
				}
				#if AVPRO_MOVIECAPTURE_VIDEOPLAYER_SUPPORT
				if (_videoPlayerController != null)
				{
					_videoPlayerController.StopCapture();
				}
				#endif
				#if AVPRO_MOVIECAPTURE_PLAYABLES_SUPPORT
				if (_timelineController != null)
				{
					_timelineController.StopCapture();
				}
				#endif
			}

			_fileInfo = null;

			if (_unityAudioCapture)
			{
				_unityAudioCapture.StopCapture();
			}
			if (_motionBlur)
			{
				_motionBlur.enabled = false;
			}

			// Restore Unity timing
			Time.captureFramerate = 0;
			//Application.targetFrameRate = _oldTargetFrameRate;
			//_oldTargetFrameRate = -1;

			if (_oldFixedDeltaTime > 0f)
			{
				Time.fixedDeltaTime = _oldFixedDeltaTime;
			}
			_oldFixedDeltaTime = 0f;

			#if !UNITY_EDITOR_OSX && (UNITY_IOS || UNITY_ANDROID)
				// Android and iOS do not support disabling vsync so _oldVsyncCount is actually the target framerate before we started capturing.
				if (_oldVSyncCount != 0)
				{
					Application.targetFrameRate = _oldVSyncCount;
					_oldVSyncCount = 0;
				}
			#else
				if (_oldVSyncCount > 0)
				{
					QualitySettings.vSyncCount = _oldVSyncCount;
					_oldVSyncCount = 0;
				}
			#endif

			_motionBlur = null;

			if (_texture != null)
			{
				Destroy(_texture);
				_texture = null;
			}

			if( _sideBySideMaterial != null )
			{
				Destroy( _sideBySideMaterial );
				_sideBySideMaterial = null;
			}

			if( _sideBySideTexture != null )
			{
				RenderTexture.ReleaseTemporary( _sideBySideTexture );
				_sideBySideTexture = null;
			}

			if (applyPostOperations &&!deleteCapture)
			{
				ApplyPostOperations(_filePath, GetEncoderHints().videoHints, _finalFilePath);
			}
		}

		private static MP4FileProcessing.Options CreatePostOperationsOptions(VideoEncoderHints hints, string finalFilePath)
		{
			MP4FileProcessing.Options options = new MP4FileProcessing.Options();
			#if UNITY_EDITOR_WIN || (!UNITY_EDITOR && (UNITY_STANDALONE_WIN || UNITY_ANDROID))
			// macOS and iOS don't require fast start postprocess as it is handled internally
			options.applyFastStart = hints.allowFastStartStreamingPostProcess;
			#endif

			options.applyStereoMode = (hints.injectStereoPacking != NoneAutoCustom.None) && (hints.stereoPacking != StereoPacking.None);
			if (options.applyStereoMode)
			{
				options.stereoMode = hints.stereoPacking;
			}

			options.applySphericalVideoLayout = (hints.injectSphericalVideoLayout != NoneAutoCustom.None) && (hints.sphericalVideoLayout != SphericalVideoLayout.None);
			if (options.applySphericalVideoLayout)
			{
				options.sphericalVideoLayout = hints.sphericalVideoLayout;
			}

			options.applyMoveCaptureFile = (finalFilePath != null);
			if(options.applyMoveCaptureFile)
			{
				options.finalCaptureFilePath = finalFilePath;
			}

			return options;
		}

		private static bool CanApplyPostOperations(string filePath, VideoEncoderHints hints, string finalFilePath)
		{
			bool result = false;
			if (HasExtension(filePath, ".mp4") || HasExtension(filePath, ".mov") && File.Exists(filePath))
			{
				result = CreatePostOperationsOptions(hints, finalFilePath).HasOptions();
			}
			return result;
		}

		protected void ApplyPostOperations(string filePath, VideoEncoderHints hints, string finalFilePath)
		{
			if (CanApplyPostOperations(filePath, hints, finalFilePath))
			{
				try
				{
					MP4FileProcessing.Options options = CreatePostOperationsOptions(hints, finalFilePath);
					if (!MP4FileProcessing.ProcessFile(filePath, false, options))
					{
						Debug.LogWarning("[AVProMovieCapture] failed to postprocess file: " + filePath);
					}
				}
				catch (System.Exception e)
				{
					Debug.LogException(e);
				}
			}
		}

		private void ToggleCapture()
		{
			if (_capturing)
			{
				//_queuedStopCapture = true;
				//_queuedStartCapture = false;
				StopCapture();
			}
			else
			{
				//_queuedStartCapture = true;
				//_queuedStopCapture = false;
				StartCapture();
			}
		}

		private bool IsEnoughDiskSpace()
		{
			bool result = true;
			long fileSizeMB = GetCaptureFileSize() / (1024 * 1024);
			if ((_freeDiskSpaceMB - fileSizeMB) < _minimumDiskSpaceMB)
			{
				result = false;
			}
			return result;
		}

		protected bool CanContinue()
		{
			bool result = true;
			#if AVPRO_MOVIECAPTURE_VIDEOPLAYER_SUPPORT
			if (IsCapturing() && !IsPaused() && !_isRealTime && _videoPlayerController != null)
			{
				result = _videoPlayerController.CanContinue();
			}
			#endif
			return result;
		}

		private void Update()
		{
			if (_queuedStopCapture)
			{
				_queuedStopCapture = _queuedStartCapture = false;
				StopCapture(false, false);
			}
			if (_queuedStartCapture)
			{
				_queuedStopCapture = _queuedStartCapture = false;
				StartCapture();
			}
		}

		private void LateUpdate()
		{
			if (_handle >= 0 && !_paused)
			{
				CheckFreeDiskSpace();
			}

			if (_captureKey != KeyCode.None)
			{
				#if (!ENABLE_INPUT_SYSTEM || ENABLE_LEGACY_INPUT_MANAGER)
				if (Input.GetKeyDown(_captureKey))
				{
					ToggleCapture();
				}
				#endif
			}

			RemoveCompletedFileWrites();

			if (_frameUpdateMode == FrameUpdateMode.Automatic)
			{
				// Resume capture if a start delay has been specified
				if (IsCapturing() && IsPaused() && _stats.NumEncodedFrames == 0)
				{
					float delta = 0f;
					if (_startDelay == StartDelayMode.GameSeconds)
					{
						if (!_isRealTime)
						{
							// In offline render mode Time.deltaTime is always zero due Time.timeScale being set to zero,
							// so just use the real world time
							delta = Time.unscaledDeltaTime;
						}
						else
						{
							delta = Time.deltaTime;
						}
					}
					else if (_startDelay == StartDelayMode.RealSeconds)
					{
						delta = Time.unscaledDeltaTime;
					}
					if (delta > 0f)
					{
						_startDelayTimer += delta;
						if (IsStartDelayComplete())
						{
							ResumeCapture();
						}
					}
				}

				PreUpdateFrame();
				UpdateFrame();
			}
		}

		private void RemoveCompletedFileWrites()
		{
			for (int i = _pendingFileWrites.Count - 1; i >= 0; i--)
			{
				FileWritingHandler handler = _pendingFileWrites[i];
				if (handler.IsFileReady())
				{
					_pendingFileWrites.RemoveAt(i);
				}
			}
		}

		private void CheckFreeDiskSpace()
		{
			if (_minimumDiskSpaceMB > 0)
			{
				if (!IsEnoughDiskSpace())
				{
					Debug.LogWarning("[AVProMovieCapture] Free disk space getting too low.  Stopping capture.");
					StopCapture(true);
				}
			}
		}

		protected bool IsStartDelayComplete()
		{
			bool result = false;
			if (_startDelay == StartDelayMode.None)
			{
				result = true;
			}
			else if (_startDelay == StartDelayMode.GameSeconds ||
					_startDelay == StartDelayMode.RealSeconds)
			{
				result = (_startDelayTimer >= _startDelaySeconds);
			}
			return result;
		}

		protected bool IsStopTimeReached()
		{
			bool result = false;
			if (_stopMode != StopMode.None)
			{
				switch (_stopMode)
				{
					case StopMode.FramesEncoded:
						result = (_stats.NumEncodedFrames >= _stopFrames);
						break;
					case StopMode.SecondsEncoded:
						if (!_isRealTime)
						{
							// In non-realtime mode this is a more accurate way to determine encoded time
							result = (_stats.NumEncodedFrames >= _stopSeconds * _frameRate);
						}
						else
						{
							result = (_stats.TotalEncodedSeconds >= _stopSeconds);
						}
						break;
					case StopMode.SecondsElapsed:
						if (!_startPaused && !_paused)
						{
							float timeSinceLastEditorPause = (Time.realtimeSinceStartup - _captureStartTime);
							result = (timeSinceLastEditorPause + _capturePrePauseTotalTime) >= _stopSeconds;
						}
						break;
				}
			}
			return result;
		}

		public float GetProgress()
		{
			float result = 0f;
			if (_stopMode != StopMode.None)
			{
				switch (_stopMode)
				{
					case StopMode.FramesEncoded:
						result = (_stats.NumEncodedFrames / (float)_stopFrames);
						break;
					case StopMode.SecondsEncoded:
						result = ((_stats.NumEncodedFrames / _frameRate) / _stopSeconds);
						break;
					case StopMode.SecondsElapsed:
						if (!_startPaused && !_paused)
						{
							float timeSinceLastEditorPause = (Time.realtimeSinceStartup - _captureStartTime);
							result = (timeSinceLastEditorPause + _capturePrePauseTotalTime) / _stopSeconds;
						}
						break;
				}
			}
			return result;
		}

		protected float GetSecondsPerCaptureFrame()
		{
			float timelapseScale = (float)_timelapseScale;
			if (!_isRealTime)
			{
				timelapseScale = 1f;
			}
			float captureFrameRate = _frameRate / timelapseScale;
			float secondsPerFrame = 1f / captureFrameRate;
			return secondsPerFrame;
		}

		protected bool CanOutputFrame()
		{
			bool result = false;
			if (_handle >= 0)
			{
				if (_isRealTime)
				{
#if false && (UNITY_EDITOR_WIN || (!UNITY_EDITOR && UNITY_STANDALONE_WIN))
					// [MOZ] @RichT @Rueben 
					// Can't block here:
					// - affects both video capture as well as image capture
					// - ceases to be realtime output
					// - deviation from existing behaviour where dropping frames is expected
					// - won't match behaviour of other platforms

					// DONT DROP FRAMES, WAIT FOR BUFFER SPOT TO BE AVAILALBE, AND THEN ADD IT.
					// this produces the correct images (however sometimes it has the nasty
					// ability to not produce the corect amount of images (on higher encoding
					// time (aka. PNG's)))
					const int WatchDogLimit = 1000;
					int watchdog = 0;
					if (_outputTarget != OutputTarget.NamedPipe)
					{
						NativePlugin.PauseFrameTimer(_handle);
						while (_handle >= 0 && !NativePlugin.IsNewFrameDue(_handle) && watchdog < WatchDogLimit)
						{
							System.Threading.Thread.Sleep(1);
							watchdog++;
						}
						NativePlugin.ResumeFrameTimer(_handle);
					}
					result = (_handle >= 0) && (watchdog < WatchDogLimit) && _timeSinceLastFrame >= GetSecondsPerCaptureFrame();

					// DROP FRAMES IF THEY CANT BE ADDED TO THE BUFFER
					// Note: 
					//	- This does not work nice at all it drops most of the frames and produces a pretty much garbage
					//    output
					//if (NativePlugin.IsNewFrameDue(_handle))
					//	result = _timeSinceLastFrame >= GetSecondsPerCaptureFrame();
#else
					if (NativePlugin.IsNewFrameDue(_handle))
					{
						result = (_timeSinceLastFrame >= GetSecondsPerCaptureFrame());
					}
#endif
				}
				else
				{
					// will return true if their is an available slot
					const int WatchDogLimit = 1000;
					int watchdog = 0;
					if (_outputTarget != OutputTarget.NamedPipe)
					{
						// Wait for the encoder to have an available buffer
						// The watchdog prevents an infinite while loop
						// no need to pause the frame timer here as offline just uses the amount of frames
						// produced to count not the time spent
						while (_handle >= 0 && !NativePlugin.IsNewFrameDue(_handle) && watchdog < WatchDogLimit)
						{
							System.Threading.Thread.Sleep(1);
							watchdog++;
						}
					}

					// Return handle status as it may have closed elsewhere
					result = (_handle >= 0) && (watchdog < WatchDogLimit);
				}
			}
			return result;
		}

		protected void TickFrameTimer()
		{
			_timeSinceLastFrame += Time.deltaTime;//unscaledDeltaTime;
		}

		protected void RenormTimer()
		{
			float secondsPerFrame = GetSecondsPerCaptureFrame();
			if (_timeSinceLastFrame >= secondsPerFrame)
			{
				_timeSinceLastFrame -= secondsPerFrame;
			}
		}

		public virtual Texture GetPreviewTexture()
		{
			return null;
		}

		public virtual Texture GetSideBySideTexture()
		{
			return _sideBySideTexture;
		}

		protected void EncodeUnityAudio()
		{
			if (IsUsingUnityAudioComponent())
			{
				int audioDataLength = 0;
				System.IntPtr audioDataPtr = _unityAudioCapture.ReadData(out audioDataLength);
				if (audioDataLength > 0)
				{
					NativePlugin.EncodeAudio(_handle, audioDataPtr, (uint)audioDataLength);
				}
			}
		}

		public void EncodeAudio(NativeArray<float> audioData)
		{
			if (audioData.Length > 0)
			{
#if UNITY_NATIVEARRAY_UNSAFE_SUPPORT
				unsafe
				{
					System.IntPtr pointer = (System.IntPtr)Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(audioData);
					NativePlugin.EncodeAudio(_handle, pointer, (uint)audioData.Length);
				}
#else
				EncodeAudio(audioData.ToArray());
#endif
			}
		}

		public void EncodeAudio(float[] audioData)
		{
			if (audioData.Length > 0)
			{
				int byteCount = Marshal.SizeOf(audioData[0]) * audioData.Length;

				// Copy the array to unmanaged memory.
				System.IntPtr pointer = Marshal.AllocHGlobal(byteCount);
				Marshal.Copy(audioData, 0, pointer, audioData.Length);

				// Encode
				NativePlugin.EncodeAudio(_handle, pointer, (uint)audioData.Length);

				// Free the unmanaged memory.
				Marshal.FreeHGlobal(pointer);
			}
		}

		public virtual void PreUpdateFrame()
		{
			#if AVPRO_MOVIECAPTURE_PLAYABLES_SUPPORT
			if (IsCapturing() && !IsPaused() && !_isRealTime && _timelineController != null)
			{
				_timelineController.UpdateFrame();
			}
			#endif
			#if AVPRO_MOVIECAPTURE_VIDEOPLAYER_SUPPORT
			if (IsCapturing() && !IsPaused() && !_isRealTime && _videoPlayerController != null)
			{
				_videoPlayerController.UpdateFrame();
			}
			#endif
		}

		public virtual void UpdateFrame()
		{
			// NOTE: Unlike other CaptureFrom components, CaptureFromScreen uses a coroutine, so when it calls base.UpdateFrame() is could still process the frame afterwards
			if (_handle >= 0 && !_paused)
			{
				_stats.NumDroppedFrames = NativePlugin.GetNumDroppedFrames(_handle);
				_stats.NumDroppedEncoderFrames = NativePlugin.GetNumDroppedEncoderFrames(_handle);
				_stats.NumEncodedFrames = NativePlugin.GetNumEncodedFrames(_handle);
				_stats.TotalEncodedSeconds = NativePlugin.GetEncodedSeconds(_handle);
#if UNITY_EDITOR_WIN || (!UNITY_EDITOR && UNITY_STANDALONE_WIN)
				_stats.NumBufferedFrames = NativePlugin.GetBufferedFrames(_handle);
				_stats.TotalBuffersAvailable = NativePlugin.GetBufferSize(_handle); // does not need updating once each frame - RM
				// Below used to see the current active worker threads
				//_stats.ActiveWorkers = NativePlugin.GetActiveWorkers(_handle);
				//_stats.MaxActiveWorkers = NativePlugin.GetMaxWorkers(_handle); // also does not need updating once each frame - RM
#endif
                if (IsStopTimeReached())
				{
					_queuedStopCapture = true;
				}
			}
		}

		protected bool InitialiseSideBySideTransparency( int width, int height, bool screenFlip = false, int antiAliasing = 1 )
		{
			bool reInitialised = false;

			if( _Transparency == Transparency.LeftRight || _Transparency == Transparency.TopBottom )
			{
				if( !_sideBySideMaterial )
				{
					_sideBySideMaterial = new Material(Shader.Find("Hidden/AVProMovieCapture/SideBySideAlpha"));

					switch (_Transparency)
					{
						case Transparency.TopBottom: _sideBySideMaterial.DisableKeyword("ALPHA_LEFT_RIGHT"); _sideBySideMaterial.EnableKeyword("ALPHA_TOP_BOTTOM"); break;
						case Transparency.LeftRight: _sideBySideMaterial.EnableKeyword("ALPHA_LEFT_RIGHT"); _sideBySideMaterial.DisableKeyword("ALPHA_TOP_BOTTOM"); break;
					}

					if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal      ||
						SystemInfo.graphicsDeviceType == GraphicsDeviceType.Vulkan     ||
						SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D11 ||
						SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D12)
					{
						_sideBySideMaterial.DisableKeyword( screenFlip ? "FLIPPED" : "SCREEN_FLIPPED" );
						_sideBySideMaterial.EnableKeyword( screenFlip ? "SCREEN_FLIPPED" : "FLIPPED" );
					}
				}

				int createWidth = width;
				int createHeight = height;
				switch( _Transparency )
				{
					case Transparency.TopBottom:	createHeight *= 2;	break;
					case Transparency.LeftRight:	createWidth *= 2;	break;
				}
				if( !_sideBySideTexture || (createWidth != _sideBySideTexture.width || createHeight != _sideBySideTexture.height) )
				{
					if( _sideBySideTexture )
					{
						RenderTexture.ReleaseTemporary( _sideBySideTexture );
						_sideBySideTexture = null;
					}

					_sideBySideTexture = RenderTexture.GetTemporary(createWidth,
																	createHeight,
																	0,
																	RenderTextureFormat.ARGB32,
																	RenderTextureReadWrite.sRGB,	// Is this correct for gamma colourspace?
																	antiAliasing);
					_sideBySideTexture.name = "[AVProMovieCapture] SideBySide Transparency Target";
					_sideBySideTexture.Create();

					reInitialised = true;
				}
			}

			return reInitialised;
		}

		protected RenderTexture UpdateForSideBySideTransparency( Texture sourceTexture, bool screenFlip = false, int antiAliasing = 1)
		{
			if( sourceTexture )
			{
				// Ensure things are setup
				InitialiseSideBySideTransparency( sourceTexture.width, sourceTexture.height, screenFlip, antiAliasing );

				if ( _sideBySideTexture )
				{
					_sideBySideTexture.DiscardContents();

					if( sourceTexture && _sideBySideMaterial )
					{
						Graphics.Blit( sourceTexture, _sideBySideTexture, _sideBySideMaterial );
					}
				}
			}

			return _sideBySideTexture;
		}


		protected void ResetFPS()
		{
			_stats.ResetFPS();
		}

		public void UpdateFPS()
		{
			_stats.UpdateFPS();
		}

		protected int GetCameraAntiAliasingLevel(Camera camera)
		{
			int aaLevel = QualitySettings.antiAliasing;
			if (aaLevel == 0)
			{
				aaLevel = 1;
			}

			if (_renderAntiAliasing > 0)
			{
				aaLevel = _renderAntiAliasing;
			}

			if (aaLevel != 1 && aaLevel != 2 && aaLevel != 4 && aaLevel != 8)
			{
				Debug.LogWarning("[AVProMovieCapture] Invalid antialiasing value, must be 1, 2, 4 or 8.  Defaulting to 1. >> " + aaLevel);
				aaLevel = 1;
			}

			if (aaLevel != 1)
			{
				if ((camera.actualRenderingPath == RenderingPath.DeferredShading)
				#if !UNITY_2022_1_OR_NEWER
					|| (camera.actualRenderingPath == RenderingPath.DeferredLighting)
				#endif
				) {
					Debug.LogWarning("[AVProMovieCapture] Not using antialiasing because MSAA is not supported by camera render path " + camera.actualRenderingPath);
					aaLevel = 1;
				}
			}
			return aaLevel;
		}

		public long GetCaptureFileSize()
		{
			long result = 0;
#if UNITY_EDITOR_OSX || (!UNITY_EDITOR && (UNITY_STANDALONE_OSX || UNITY_IOS || UNITY_ANDROID))
			result = NativePlugin.GetFileSize(_handle);
#elif !UNITY_WEBPLAYER
			if (_handle >= 0 && _outputTarget == OutputTarget.VideoFile)
			{
				if (_fileInfo == null && File.Exists(_filePath))
				{
					_fileInfo = new System.IO.FileInfo(_filePath);
				}
				if (_fileInfo != null)
				{
					_fileInfo.Refresh();
					result = _fileInfo.Length;
				}
			}
#endif
			return result;
		}

		public static void GetResolution(Resolution res, ref int width, ref int height)
		{
			switch (res)
			{
				case Resolution.POW2_8192x8192:
					width = 8192; height = 8192;
					break;
				case Resolution.POW2_8192x4096:
					width = 8192; height = 4096;
					break;
				case Resolution.POW2_4096x4096:
					width = 4096; height = 4096;
					break;
				case Resolution.POW2_4096x2048:
					width = 4096; height = 2048;
					break;
				case Resolution.POW2_2048x4096:
					width = 2048; height = 4096;
					break;
				case Resolution.UHD_3840x2160:
					width = 3840; height = 2160;
					break;
				case Resolution.UHD_3840x2048:
					width = 3840; height = 2048;
					break;
				case Resolution.UHD_3840x1920:
					width = 3840; height = 1920;
					break;
				case Resolution.UHD_2560x1440:
					width = 2560; height = 1440;
					break;
				case Resolution.POW2_2048x2048:
					width = 2048; height = 2048;
					break;
				case Resolution.POW2_2048x1024:
					width = 2048; height = 1024;
					break;
				case Resolution.HD_1920x1080:
					width = 1920; height = 1080;
					break;
				case Resolution.HD_1280x720:
					width = 1280; height = 720;
					break;
				case Resolution.SD_1024x768:
					width = 1024; height = 768;
					break;
				case Resolution.SD_800x600:
					width = 800; height = 600;
					break;
				case Resolution.SD_800x450:
					width = 800; height = 450;
					break;
				case Resolution.SD_640x480:
					width = 640; height = 480;
					break;
				case Resolution.SD_640x360:
					width = 640; height = 360;
					break;
				case Resolution.SD_320x240:
					width = 320; height = 240;
					break;
			}
		}

		// Returns the next multiple of 4 or the same value if it's already a multiple of 4
		protected static int NextMultipleOf4(int value)
		{
			return (value + 3) & ~0x03;
		}

		// Audio capture support

		/// <summary>
		/// Hints to the system that we want to capture audio from the microphone.
		/// <para>
		/// <b>iOS</b><br/>
		/// Call this with <c>enabled = true</c> before you begin recording to prevent any unwanted stalls or pops when
		/// the recording begins due to changing the audio session. Call with <c>enabled = false</c> to restore the
		/// audio session.<br/>
		/// </para>
		/// </summary>
		/// <param name="enabled">Enables or disables audio recording from the microphone</param>
		/// <param name="options">Options to apply when enabling or disabling audio recording</param>
		public void SetMicrophoneRecordingHint(bool enabled, MicrophoneRecordingOptions options = MicrophoneRecordingOptions.Defaults)
		{
#if UNITY_EDITOR_OSX || (!UNITY_EDITOR && (UNITY_STANDALONE_OSX || UNITY_IOS || UNITY_ANDROID))
			NativePlugin.MicrophoneRecordingOptions nativeOptions = NativePlugin.MicrophoneRecordingOptions.None;
			if (options == MicrophoneRecordingOptions.Defaults)
			{
	#if AVPMC_MICROPHONE_RECORDING_HINT_MIX_WITH_OTHERS
				nativeOptions |= NativePlugin.MicrophoneRecordingOptions.MixWithOthers;
	#endif
			}
			else
			{
				if ((options & MicrophoneRecordingOptions.MixWithOthers) == MicrophoneRecordingOptions.MixWithOthers)
					nativeOptions |= NativePlugin.MicrophoneRecordingOptions.MixWithOthers;
				if ((options & MicrophoneRecordingOptions.DefaultToSpeaker) == MicrophoneRecordingOptions.DefaultToSpeaker)
					nativeOptions |= NativePlugin.MicrophoneRecordingOptions.DefaultToSpeaker;
				if ((options & MicrophoneRecordingOptions.AllowBluetoothMicrophone) == MicrophoneRecordingOptions.AllowBluetoothMicrophone)
					nativeOptions |= NativePlugin.MicrophoneRecordingOptions.AllowBluetooth;
			}
			NativePlugin.SetMicrophoneRecordingHint(enabled, nativeOptions);
#endif
		}

		/// <summary>
		/// Authorisation for audio capture.
		/// </summary>
		public enum AudioCaptureDeviceAuthorisationStatus
		{
			/// <summary>Audio capture is unavailable.</summary>
			Unavailable = -1,
			/// <summary>Authorisation is still to be requested.</summary>
			NotDetermined,
			/// <summary>Authorisation has been denied.</summary>
			Denied,
			/// <summary>Authorisation has been granted.</summary>
			Authorised
		};

#if UNITY_EDITOR || UNITY_STANDALONE_WINDOWS
		// No support for permission handling in the editor or standalone windows builds
		public static AudioCaptureDeviceAuthorisationStatus HasUserAuthorisationToCaptureAudio()
		{
			return AudioCaptureDeviceAuthorisationStatus.Unavailable;
		}

		public static CustomYieldInstruction RequestAudioCaptureDeviceUserAuthorisation()
		{
			// Nothing to on Windows or in the editor
			return null;
		}

#elif UNITY_STANDALONE_OSX || UNITY_IOS
		private static bool _hasCheckedAudioCaptureDeviceAuthorisationStatus = false;
		private static bool _waitingForAudioCaptureDeviceAuthorisation = true;
		private static AudioCaptureDeviceAuthorisationStatus _audioCaptureDeviceAuthorisationStatus = AudioCaptureDeviceAuthorisationStatus.NotDetermined;

		// macOS and iOS handle permission natively
		public static AudioCaptureDeviceAuthorisationStatus HasUserAuthorisationToCaptureAudio()
		{
			if (!_hasCheckedAudioCaptureDeviceAuthorisationStatus ||
				_audioCaptureDeviceAuthorisationStatus == AudioCaptureDeviceAuthorisationStatus.NotDetermined)
			{
				_audioCaptureDeviceAuthorisationStatus = (AudioCaptureDeviceAuthorisationStatus)NativePlugin.AudioCaptureDeviceAuthorisationStatus();
				_hasCheckedAudioCaptureDeviceAuthorisationStatus = true;
			}
			return _audioCaptureDeviceAuthorisationStatus;
		}

		private class WaitForAudioCaptureDeviceAuthorisation : CustomYieldInstruction
		{
			public override bool keepWaiting
			{
				get
				{
					return CaptureBase._waitingForAudioCaptureDeviceAuthorisation;
				}
			}
		}

		// Callback for the native permission request
	#if ENABLE_IL2CPP
		[MonoPInvokeCallback(typeof(NativePlugin.RequestAudioCaptureDeviceAuthorisationDelegate))]
	#endif
		private static void RequestUserAuthorisationToCaptureAudioCallback(int authorisation)
		{
			_audioCaptureDeviceAuthorisationStatus = (AudioCaptureDeviceAuthorisationStatus)authorisation;
			_hasCheckedAudioCaptureDeviceAuthorisationStatus = true;
			_waitingForAudioCaptureDeviceAuthorisation = false;
		}

		public static CustomYieldInstruction RequestAudioCaptureDeviceUserAuthorisation()
		{
			NativePlugin.RequestAudioCaptureDeviceAuthorisationDelegate callback =
					new NativePlugin.RequestAudioCaptureDeviceAuthorisationDelegate(RequestUserAuthorisationToCaptureAudioCallback);
			System.IntPtr ptr = Marshal.GetFunctionPointerForDelegate(callback);
			NativePlugin.RequestAudioCaptureDeviceAuthorisation(ptr);
			return new WaitForAudioCaptureDeviceAuthorisation();
		}

#elif UNITY_ANDROID
		private static bool _hasRequestedAudioCaptureDeviceAuthorisation = false;
		private static AudioCaptureDeviceAuthorisationStatus _audioCaptureDeviceAuthorisationStatus = AudioCaptureDeviceAuthorisationStatus.NotDetermined;

		// Android uses Unity's permissions API as a native implementation is not possible
		public static AudioCaptureDeviceAuthorisationStatus HasUserAuthorisationToCaptureAudio()
		{
			bool authorised = Permission.HasUserAuthorizedPermission(Permission.Microphone);
			if (authorised)
			{
				_audioCaptureDeviceAuthorisationStatus = AudioCaptureDeviceAuthorisationStatus.Authorised;
				_hasRequestedAudioCaptureDeviceAuthorisation = true;
			}
			else if (_hasRequestedAudioCaptureDeviceAuthorisation)
			{
				_audioCaptureDeviceAuthorisationStatus = AudioCaptureDeviceAuthorisationStatus.Denied;
			}
			return _audioCaptureDeviceAuthorisationStatus;
		}

	#if UNITY_2020_2_OR_NEWER
		private static bool _waitingForAudioCaptureDeviceAuthorisation = true;

		private static void PermissionCallback_PermissionDenied(string permission)
		{
			_audioCaptureDeviceAuthorisationStatus = AudioCaptureDeviceAuthorisationStatus.Denied;
			_hasRequestedAudioCaptureDeviceAuthorisation = false;
			_waitingForAudioCaptureDeviceAuthorisation = false;
		}

		private static void PermissionCallback_PermissionGranted(string permission)
		{
			_audioCaptureDeviceAuthorisationStatus = AudioCaptureDeviceAuthorisationStatus.Authorised;
			_hasRequestedAudioCaptureDeviceAuthorisation = true;
			_waitingForAudioCaptureDeviceAuthorisation = false;
		}

		private static void PermissionCallback_PermissionDeniedAndDontAskAgain(string permission)
		{
			_audioCaptureDeviceAuthorisationStatus = AudioCaptureDeviceAuthorisationStatus.Denied;
			_hasRequestedAudioCaptureDeviceAuthorisation = true;
			_waitingForAudioCaptureDeviceAuthorisation = false;
		}

		private class WaitForAudioCaptureDeviceAuthorisation : CustomYieldInstruction
		{
			public override bool keepWaiting
			{
				get
				{
					return CaptureBase._waitingForAudioCaptureDeviceAuthorisation;
				}
			}
		}

		public static CustomYieldInstruction RequestAudioCaptureDeviceUserAuthorisation()
		{
			if (_audioCaptureDeviceAuthorisationStatus == AudioCaptureDeviceAuthorisationStatus.Authorised)
			{
				// Already authorised
				return null;
			}

			if (_hasRequestedAudioCaptureDeviceAuthorisation &&
			    _audioCaptureDeviceAuthorisationStatus == AudioCaptureDeviceAuthorisationStatus.Denied)
			{
				// Already been denied, nothing to do
				return null;
			}

			PermissionCallbacks callbacks = new PermissionCallbacks();
			callbacks.PermissionDenied += PermissionCallback_PermissionDenied;
			callbacks.PermissionGranted += PermissionCallback_PermissionGranted;
#if !UNITY_2023_1_OR_NEWER
			callbacks.PermissionDeniedAndDontAskAgain += PermissionCallback_PermissionDeniedAndDontAskAgain;
#endif
			Permission.RequestUserPermission(Permission.Microphone, callbacks);
			return new WaitForAudioCaptureDeviceAuthorisation();
		}
	#else
		public static CustomYieldInstruction RequestAudioCaptureDeviceUserAuthorisation()
		{
			if (_audioCaptureDeviceAuthorisationStatus == AudioCaptureDeviceAuthorisationStatus.Authorised)
			{
				// Already authorised
				return null;
			}

			Permission.RequestUserPermission(Permission.Microphone);

			// Unfortunately there is no way to know when the dialog has been closed so no way
			// to determine if permission has been denied as there is nothing to distinguish
			// the result of the permission check before, during and after the request unless
			// permission has been authorised.

			return null;
		}
	#endif

#endif

		// Photo library support

		/// <summary>
		/// Level of access for the photos library.
		///</summary>
		public enum PhotoLibraryAccessLevel
		{
			/// <summary>Can only add photos to the photo library, cannot create albums or read back images.</summary>
			AddOnly,
			/// <summary>Full access, can add photos, create albums and read back images.</summary>
			ReadWrite
		};

		/// <summary>
		/// Authorisation for access to the photos library.
		/// </summary>
		public enum PhotoLibraryAuthorisationStatus
		{
			/// <summary>The photo library is unavailable.</summary>
			Unavailable = -1,
			/// <summary>Authorisation to the photo library is still to be requested.</summary>
			NotDetermined,
			/// <summary>Access to the photo library has been denied.</summary>
			Denied,
			/// <summary>Access to the photo library has been granted.</summary>
			Authorised
		};

#if UNITY_EDITOR || UNITY_STANDALONE_WINDOWS
		// No support for permission handling in the editor or standalone windows builds
		public static PhotoLibraryAuthorisationStatus HasUserAuthorisationToAccessPhotos()
		{
			return PhotoLibraryAuthorisationStatus.Unavailable;
		}

		public static CustomYieldInstruction RequestUserAuthorisationToAccessPhotos()
		{
			// Nothing to on Windows or in the editor
			return null;
		}

#elif UNITY_STANDALONE_OSX || UNITY_IOS

		/// <summary>
		/// Check to see if authorisation has been given to access the photo library.
		/// </summary>
		private static bool _waitingForAuthorisationToAccessPhotos = true;
		private static bool _hasCheckedPhotoLibraryAuthorisationStatus = false;
		private static PhotoLibraryAuthorisationStatus _photoLibraryAuthorisation = PhotoLibraryAuthorisationStatus.NotDetermined;

		public static PhotoLibraryAuthorisationStatus HasUserAuthorisationToAccessPhotos(PhotoLibraryAccessLevel accessLevel)
		{
			if (!_hasCheckedPhotoLibraryAuthorisationStatus || _photoLibraryAuthorisation == PhotoLibraryAuthorisationStatus.NotDetermined)
			{
				_hasCheckedPhotoLibraryAuthorisationStatus = true;
				_photoLibraryAuthorisation = (PhotoLibraryAuthorisationStatus)NativePlugin.PhotoLibraryAuthorisationStatus((int)accessLevel);
			}
			return _photoLibraryAuthorisation;
		}


#if ENABLE_IL2CPP && (UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX || (UNITY_IOS && !UNITY_EDITOR))
		[MonoPInvokeCallback(typeof(NativePlugin.RequestPhotoLibraryAuthorisationDelegate))]
#endif
		private static void RequestUserAuthorisationToAccessPhotosCallback(int authorisation)
		{
			_photoLibraryAuthorisation = (PhotoLibraryAuthorisationStatus)authorisation;
			_hasCheckedPhotoLibraryAuthorisationStatus = true;
			_waitingForAuthorisationToAccessPhotos = false;
		}

		private class WaitForAuthorisationToAccessPhotos: CustomYieldInstruction
		{
			public override bool keepWaiting { get { return CaptureBase._waitingForAuthorisationToAccessPhotos; } }
		}

		/// <summary>
		/// Request authorisation to access the photo library.
		/// </summary>
		public static CustomYieldInstruction RequestUserAuthorisationToAccessPhotos(PhotoLibraryAccessLevel accessLevel)
		{
			NativePlugin.RequestPhotoLibraryAuthorisationDelegate callback = new NativePlugin.RequestPhotoLibraryAuthorisationDelegate(RequestUserAuthorisationToAccessPhotosCallback);
			System.IntPtr ptr = Marshal.GetFunctionPointerForDelegate(callback);
			NativePlugin.RequestPhotoLibraryAuthorisation((int)accessLevel, ptr);
			return new WaitForAuthorisationToAccessPhotos();
		}

#elif UNITY_ANDROID

		private static bool _hasRequestedPhotosAccessAuthorisation = false;
		private static PhotoLibraryAuthorisationStatus _photoLibraryAuthorisationStatus = PhotoLibraryAuthorisationStatus.NotDetermined;
		private static string AndroidPermissionWriteExternalStorage = "android.permission.WRITE_EXTERNAL_STORAGE";

		private static int _androidSdkLevel = -1;
		private static int GetAndroidSDKLevel()
		{
			if (_androidSdkLevel == -1)
			{
    			var clazz = AndroidJNI.FindClass("android/os/Build$VERSION");
    			var fieldID = AndroidJNI.GetStaticFieldID(clazz, "SDK_INT", "I");
    			_androidSdkLevel = AndroidJNI.GetStaticIntField(clazz, fieldID);
			}
    		return _androidSdkLevel;
		}

		// Android uses Unity's permissions API as a native implementation is not possible (without a lot of grief)
		public static PhotoLibraryAuthorisationStatus HasUserAuthorisationToAccessPhotos(PhotoLibraryAccessLevel accessLevel)
		{
			if (GetAndroidSDKLevel() > 29)
			{
				_photoLibraryAuthorisationStatus = PhotoLibraryAuthorisationStatus.Authorised;
			}

			if (_photoLibraryAuthorisationStatus == PhotoLibraryAuthorisationStatus.NotDetermined)
			{
				bool authorised = Permission.HasUserAuthorizedPermission(AndroidPermissionWriteExternalStorage);
				if (authorised)
				{
					_photoLibraryAuthorisationStatus = PhotoLibraryAuthorisationStatus.Authorised;
					_hasRequestedPhotosAccessAuthorisation = true;
				}
				else if (_hasRequestedPhotosAccessAuthorisation)
				{
					_photoLibraryAuthorisationStatus = PhotoLibraryAuthorisationStatus.Denied;
				}
			}

			Debug.Log($"HasUserAuthorisationToAccessPhotos - {_photoLibraryAuthorisationStatus}");

			return _photoLibraryAuthorisationStatus;
		}

	#if UNITY_2020_2_OR_NEWER

		private static bool _waitingForPhotosAccessAuthorisation = true;

		private class WaitForPhotosAccessAuthorisation : CustomYieldInstruction
		{
			public override bool keepWaiting
			{
				get
				{
					return CaptureBase._waitingForPhotosAccessAuthorisation;
				}
			}
		}

		public static CustomYieldInstruction RequestUserAuthorisationToAccessPhotos(PhotoLibraryAccessLevel accessLevel)
		{
			if (_photoLibraryAuthorisationStatus == PhotoLibraryAuthorisationStatus.Authorised)
			{
				// Already authorised
				return null;
			}

			if (_hasRequestedPhotosAccessAuthorisation &&
			    _photoLibraryAuthorisationStatus == PhotoLibraryAuthorisationStatus.Denied)
			{
				// Already been denied, nothing to do
				return null;
			}

			PermissionCallbacks callbacks = new PermissionCallbacks();

			callbacks.PermissionDenied += (permission) => {
				_photoLibraryAuthorisationStatus = PhotoLibraryAuthorisationStatus.Denied;
				_hasRequestedPhotosAccessAuthorisation = false;
				_waitingForPhotosAccessAuthorisation = false;
			};

			callbacks.PermissionGranted += (permission) => {
				_photoLibraryAuthorisationStatus = PhotoLibraryAuthorisationStatus.Authorised;
				_hasRequestedPhotosAccessAuthorisation = true;
				_waitingForPhotosAccessAuthorisation = false;
			};

#if !UNITY_2023_1_OR_NEWER
			callbacks.PermissionDeniedAndDontAskAgain += (permission) => {
				_photoLibraryAuthorisationStatus = PhotoLibraryAuthorisationStatus.Denied;
				_hasRequestedPhotosAccessAuthorisation = true;
				_waitingForPhotosAccessAuthorisation = false;
			};
#endif

			Permission.RequestUserPermission(AndroidPermissionWriteExternalStorage, callbacks);
			return new WaitForPhotosAccessAuthorisation();
		}

	#else

		public static CustomYieldInstruction RequestUserAuthorisationToAccessPhotos(PhotoLibraryAccessLevel accessLevel)
		{
			if (_photoLibraryAuthorisationStatus == PhotoLibraryAuthorisationStatus.Authorised)
			{
				// Already authorised
				return null;
			}

			Permission.RequestUserPermission(AndroidPermissionWriteExternalStorage);

			// Unfortunately there is no way to know when the dialog has been closed so no way
			// to determine if permission has been denied as there is nothing to distinguish
			// the result of the permission check before, during and after the request unless
			// permission has been authorised.

			return null;
		}
	#endif

#endif
	}
}
