using System;
using System.Threading.Tasks;
using OpenTK.Audio.OpenAL;
using Vintagestory.API.Client;

namespace VintageVoice.Client;
public class VoiceRecorder
{
    bool recording = false;
    bool busy = false;
    public IClientNetworkChannel communicationChannel;
    public IClientNetworkChannel infoChannel;

    public void Init(IClientNetworkChannel channelCommunication, IClientNetworkChannel channelInfo)
    {
        communicationChannel = channelCommunication;
        infoChannel = channelInfo;
        Debug.Log("Voice Recorder initialized");
    }

    public void StartRecording()
    {
        if (recording)
        {
            infoChannel.SendPacket("stop_recording");
            // Busy treatment
            busy = true;
            recording = false;
            Task.Delay(100).ContinueWith((_) => busy = false);
        }

        // Checking if is already recording or if the voice is busy
        if (recording || busy) return;
        recording = true;
        infoChannel.SendPacket("start_recording");

        Debug.Log("Starting recording");

        // Audio settings
        int sampleRate = 44100;
        ALFormat format = ALFormat.Mono16;
        int bufferSize = sampleRate * 1;
        string defaultDevice = ALC.GetString(ALDevice.Null, AlcGetString.DefaultDeviceSpecifier);

        // Tries to connect to the microphone
        ALCaptureDevice captureDevice = ALC.CaptureOpenDevice(defaultDevice, sampleRate, format, bufferSize);
        // Get microphone device
        ALDevice microphoneDevice = new(captureDevice);

        // Start capture
        ALC.CaptureStart(captureDevice);

        // Buffer to store the audio
        short[] buffer = new short[bufferSize];

        Debug.Log("Recording...");

        // Running on secondary thread to not freeze the game
        Task.Run(() =>
        {
            // Recording the audio and sending to the server
            while (recording)
            {
                // Get the audio samples
                ALC.GetInteger(microphoneDevice, AlcGetInteger.CaptureSamples, out int samplesAvailable);

                if (samplesAvailable > 0)
                {
                    // Receives the samples to audio into buffer
                    ALC.CaptureSamples(captureDevice, buffer, samplesAvailable);
                    // Convert the buffer into bytes
                    byte[] byteBuffer = new byte[samplesAvailable * sizeof(short)];
                    Buffer.BlockCopy(buffer, 0, byteBuffer, 0, byteBuffer.Length);

                    // Send the bytes audio to the server
                    communicationChannel.SendPacket(byteBuffer);
                    Debug.Log($"Sample recorded size: {byteBuffer.Length}");
                }
            }

            Debug.Log("Finished");

            // Close Capture
            ALC.CaptureStop(captureDevice);
            ALC.CaptureCloseDevice(captureDevice);
        });
    }
}