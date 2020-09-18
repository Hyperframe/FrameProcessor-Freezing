using System;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using System.Threading.Tasks;
using System.Threading;

#if !UNITY_EDITOR
using Windows.Devices.Enumeration;
using Windows.Media.Capture.Frames;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
#endif

public class FrameProcessor : MonoBehaviour
{
    public PressableButtonHoloLens2 button;

    private bool scanningActive = false;
    private int framesProcessed = 0;

#if !UNITY_EDITOR
    private MediaCapture mediaCapture;
    private MediaFrameReader mediaFrameReader;
#endif

    private const int DESIRED_WIDTH = 640;
    private const int DESIRED_HEIGHT = 360;
    private const int DESIRED_FRAMERATE = 15;

    public void ButtonPressed()
    {
        SetScanState(!scanningActive);
    }

    private void SetScanState(bool scanning)
    {
#if !UNITY_EDITOR
        if (scanning)
        {
            StartCapture();
        }
        else
        {
            StopCapture();
        }
#endif

        var labels = button.GetComponentsInChildren<TextMeshPro>();
        foreach (var label in labels)
        {
            label.text = scanning ? "Stop Capture" : "Start Capture";
        }
        scanningActive = scanning;
    }

#if !UNITY_EDITOR
    public async void StartCapture()
    {
        System.Diagnostics.Debug.WriteLine("Starting capture.");

        var videoDevice = await GetBestVideoDevice();
        if (videoDevice == null)
        {
            System.Diagnostics.Debug.WriteLine("Failed to find video device.");
            return;
        }

        MediaCaptureVideoProfile profile;
        MediaCaptureVideoProfileMediaDescription description;
        if (!GetBestProfileAndDescription(videoDevice, out profile, out description))
        {
            System.Diagnostics.Debug.WriteLine("Failed to find profile and description.");
            return;
        }

        var settings = new MediaCaptureInitializationSettings
        {
            MemoryPreference = MediaCaptureMemoryPreference.Cpu,
            VideoDeviceId = videoDevice.Id,
            VideoProfile = profile,
            RecordMediaDescription = description,
        };

        var mediaCapture = new MediaCapture();
        await mediaCapture.InitializeAsync(settings);

        MediaFrameSource source = null;
        MediaFrameFormat format = null;
        if (!GetBestSourceAndFormat(mediaCapture, out source, out format))
        {
            System.Diagnostics.Debug.WriteLine("Failed to find source and format.");
            return;
        }

        System.Diagnostics.Debug.WriteLine(string.Format("Selected Video Format: Width: {0}, Height: {1}, Major Type: {2}, Subtype: {3}, Frame Rate: {4}/{5}",
            format.VideoFormat.Width,
            format.VideoFormat.Height,
            format.MajorType,
            format.Subtype,
            format.FrameRate == null ? "null" : format.FrameRate.Numerator.ToString(),
            format.FrameRate == null ? "null" : format.FrameRate.Denominator.ToString()));

        await source.SetFormatAsync(format);

        mediaFrameReader = await mediaCapture.CreateFrameReaderAsync(source, MediaEncodingSubtypes.Bgra8);
        if (await mediaFrameReader.StartAsync() != MediaFrameReaderStartStatus.Success)
        {
            System.Diagnostics.Debug.WriteLine("Failed to start media frame reader.");
            return;
        }

        mediaFrameReader.FrameArrived += MediaFrameReader_FrameArrived;
        System.Diagnostics.Debug.WriteLine("Capture started.");
    }

    private async void StopCapture()
    {
        System.Diagnostics.Debug.WriteLine("Stopping capture.");
        await mediaFrameReader.StopAsync();
        mediaFrameReader.FrameArrived -= MediaFrameReader_FrameArrived;
        mediaCapture.Dispose();
        mediaCapture = null;
        System.Diagnostics.Debug.WriteLine("Capture stopped.");
    }

    private void MediaFrameReader_FrameArrived(MediaFrameReader sender, MediaFrameArrivedEventArgs args)
    {
        var mediaFrameReference = sender.TryAcquireLatestFrame();

        Interlocked.Increment(ref framesProcessed);
        System.Diagnostics.Debug.WriteLine($"{framesProcessed} frames processed.");

        mediaFrameReference?.Dispose();
    }

    private static async Task<DeviceInformation> GetBestVideoDevice()
    {
        DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
        foreach (var device in devices)
        {
            if (MediaCapture.IsVideoProfileSupported(device.Id) && device.EnclosureLocation.Panel == Panel.Back)
            {
                return device;
            }
        }

        return null;
    }
    private static bool GetBestProfileAndDescription(
        DeviceInformation videoDevice,
        out MediaCaptureVideoProfile mediaProfile,
        out MediaCaptureVideoProfileMediaDescription mediaDescription)
    {
        var profiles = MediaCapture.FindAllVideoProfiles(videoDevice.Id);
        foreach (var profile in profiles)
        {
            foreach (var description in profile.SupportedRecordMediaDescription)
            {
                System.Diagnostics.Debug.WriteLine("Supported MF Video Profile Description (width: {0}) (height: {1}) (sub-type: {2}) (frame rate: {3})",
                    description.Width,
                    description.Height,
                    description.Subtype,
                    description.FrameRate);

                if (description.Width == DESIRED_WIDTH && description.Height == DESIRED_HEIGHT && description.FrameRate == DESIRED_FRAMERATE)
                {
                    mediaProfile = profile;
                    mediaDescription = description;
                    return true;
                }
            }
        }

        mediaProfile = null;
        mediaDescription = null;
        return false;
    }

    private static bool GetBestSourceAndFormat(
        MediaCapture mediaCapture,
        out MediaFrameSource frameSource,
        out MediaFrameFormat frameFormat)
    {
        foreach (var source in mediaCapture.FrameSources.Values)
        {
            foreach (var format in source.SupportedFormats)
            {
                if (format.VideoFormat.Width == DESIRED_WIDTH && format.VideoFormat.Height == DESIRED_HEIGHT && format.FrameRate.Numerator == DESIRED_FRAMERATE)
                {
                    frameSource = source;
                    frameFormat = format;
                    return true;
                }
            }
        }

        frameSource = null;
        frameFormat = null;
        return false;
    }
#endif
}
