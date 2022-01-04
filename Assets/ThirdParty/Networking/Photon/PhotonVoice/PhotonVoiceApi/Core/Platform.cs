using System;
using System.Collections;
using System.Diagnostics;

namespace Photon.Voice
{
    public static class Platform
    {
        static public IEncoder CreateDefaultAudioEncoder<T>(ILogger logger, VoiceInfo info)
        {
            switch (info.Codec)
            {
                case Codec.AudioOpus:
                    return OpusCodec.Factory.CreateEncoder<T[]>(info, logger);
                default:
                    throw new UnsupportedCodecException("Platform.CreateDefaultAudioEncoder", info.Codec, logger);
            }
        }
#if PHOTON_VOICE_VIDEO_ENABLE

        static public IEncoder CreateDefaultVideoEncoder(ILogger logger, VoiceInfo info)
        {
            switch (info.Codec)
            {
                case Codec.VideoVP8:
                case Codec.VideoVP9:
                    return new VPxCodec.Encoder(logger, info);
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                case Codec.VideoH264:
                    //return new FFmpegCodec.Encoder(logger, info);
                    return new Windows.MFTCodec.VideoEncoder(logger, info);
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                case Codec.VideoH264:
                    //return new FFmpegCodec.Encoder(logger, info);
                    return new MacOS.VideoEncoder(logger, info);
#endif
                default:
                    throw new UnsupportedCodecException("Platform.CreateDefaultEncoder", info.Codec, logger);
            }
        }
#endif

#if PHOTON_VOICE_VIDEO_ENABLE
        static public IVideoRecorder CreateDefaultVideoRecorder(ILogger logger, PreviewManager previewManager, VoiceInfo info, string camDevice, Action<IVideoRecorder> onReady)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            var ve = new Unity.UnityAndroidVideoEncoder(logger, previewManager, info);
            return new Unity.UnityAndroidVideoRecorder(ve, ve.Preview, onReady);
#elif UNITY_IOS && !UNITY_EDITOR
            var ve = new IOS.VideoEncoder(logger, info);
            return new IOS.VideoRecorder(ve, ve.Preview, onReady);
#elif WINDOWS_UWP || (UNITY_WSA && !UNITY_EDITOR)
            var ve = new UWP.VideoEncoder(logger, info);
            return new UWP.VideoRecorder(ve, ve.Preview, onReady);
#else
            IEncoderDirect<ImageInputBuf> ve;
            switch (info.Codec)
            {
                case Codec.VideoVP8:
                case Codec.VideoVP9:
                    ve = new VPxCodec.Encoder(logger, info);
                    break;
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                case Codec.VideoH264:
                    //ve = new FFmpegCodec.Encoder(logger, info);
                    ve = new Windows.MFTCodec.VideoEncoder(logger, info);
                    break;
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                case Codec.VideoH264:
                    //ve = new FFmpegCodec.Encoder(logger, info);
                    ve = new MacOS.VideoEncoder(logger, info);
                    break;
#endif
                default:
                    throw new UnsupportedCodecException("Platform.CreateDefaultVideoRecorder", info.Codec, logger);
            }
#if UNITY_5_3_OR_NEWER // #if UNITY
            return new Unity.VideoRecorderUnity(ve, null, camDevice, info.Width, info.Height, info.FPS, onReady);
#else
            return new VideoRecorder(ve, null);
#endif

#endif
            }

        static public IVideoPlayer CreateDefaultVideoPlayer(ILogger logger, PreviewManager previewManager, VoiceInfo info)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            var vd = new Unity.UnityAndroidVideoDecoder(logger, previewManager, info.Codec);
            return new VideoPlayer(vd, vd.Preview, info.Width, info.Height);
#elif UNITY_IOS && !UNITY_EDITOR
            var vd = new IOS.VideoDecoder(logger);
            return new VideoPlayer(vd, vd.Preview, info.Width, info.Height);
#elif UNITY_WSA && !UNITY_EDITOR
            var vd = new UWP.VideoDecoder(logger, info);
            return new VideoPlayer(vd, vd.Preview, info.Width, info.Height);
#else
            IDecoderQueuedOutputImageNative vd;
            switch (info.Codec)
            {
                case Codec.VideoVP8:
                case Codec.VideoVP9:
                    vd = new VPxCodec.Decoder(logger);
                    break;
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                case Codec.VideoH264:
                    //vd = new FFmpegCodec.Decoder(logger);
                    vd = new Windows.MFTCodec.VideoDecoder(logger, info);
                    break;
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                case Codec.VideoH264:
                    //vd = new FFmpegCodec.Decoder(logger);
                    vd = new MacOS.VideoDecoder(logger, info);
                    break;
#endif
                default:
                    throw new UnsupportedCodecException("Platform.CreateDefaultVideoDecoder", info.Codec, logger);
            }
#if UNITY_5_3_OR_NEWER // #if UNITY
            var vp = new Unity.VideoPlayerUnity(vd);
            // assign Draw method copying Image to Unity texture as software decoder Output
            vd.Output = vp.Draw;
            return vp;
#else
            return new VideoPlayer(vd, null, 0, 0);
#endif

#endif
        }

        public static PreviewManager CreateDefaultPreviewManager(ILogger logger)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return new Unity.UnityAndroidPreviewManager(logger);
#elif UNITY_IOS && !UNITY_EDITOR
            return new IOS.PreviewManager(logger);
#elif UNITY_WSA && !UNITY_EDITOR
            return new UWP.PreviewManager(logger);
#elif UNITY_5_3_OR_NEWER // #if UNITY
            return new Unity.PreviewManagerUnityGUI();
#else
            return null;
#endif
        }

#endif
    }
}
