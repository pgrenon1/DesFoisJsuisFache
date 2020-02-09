﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CognitiveServicesTTS;
using System.IO;
using System.Threading;
using System.Net;
using System.Threading.Tasks;
using UnityEditor;
#if UNITY_EDITOR || !UNITY_WSA
using System.Security.Cryptography.X509Certificates;
#endif

// IMPORTANT: THIS CODE ONLY WORKS WITH THE .NET 4.6 SCRIPTING RUNTIME

public class SpeechManager : MonoBehaviour
{

    [Tooltip("The audio source where speech will be played.")]
    public AudioSource audioSource = null;

    public VoiceName voiceName = VoiceName.enUSJessaRUS;

    // Access token used to make calls against the Cognitive Services Speech API
    string accessToken;

    // Allows callers to make sure the SpeechManager is authenticated and ready before using it
    [HideInInspector]
    public bool isReady = false;

#if UNITY_EDITOR || !UNITY_WSA
    /// <summary>
    /// This class is required to circumvent a TLS bug in Unity, otherwise Unity will throw
    /// an error stating the certificate is invalid. This is supposed to be fixed in Unity
    /// 2018.2. Note that UWP doesn't have this bug, only Mono, hence the conditional code.
    /// </summary>
    private class CustomCertificatePolicy : ICertificatePolicy
    {
        public bool CheckValidationResult(ServicePoint sp,
            X509Certificate certificate, WebRequest request, int error)
        {
            // We force Unity to always validate the certificate as "true".
            return true;
        }
    }
#endif

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    // Use this for initialization
    void Start()
    {
#if UNITY_EDITOR || !UNITY_WSA
        // Unity will complain that the following statement is deprecated, however it's still working :)
        ServicePointManager.CertificatePolicy = new CustomCertificatePolicy();

        // This 'workaround' seems to work for the .NET Storage SDK, but not here. Leaving this for clarity
        //ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
#endif

        // FOR MORE INFO ON AUTHENTICATION AND HOW TO GET YOUR API KEY, PLEASE VISIT
        // https://docs.microsoft.com/en-us/azure/cognitive-services/speech/how-to/how-to-authentication
        Authentication auth = new Authentication();
        Task<string> authenticating = auth.Authenticate("https://westus.api.cognitive.microsoft.com/sts/v1.0/issueToken",
                                                 "d45cc06b8c56493eabe503754505beb0"); // INSERT-YOUR-BING-SPEECH-API-KEY-HERE
        // Don't use the key above, it's mine and I reserve the right to invalidate it if/when I want, 
        // use the link above and go get your own. The free tier gives you 5,000 free API transactions / month.

        // Since the authentication process needs to run asynchronously, we run the code in a coroutine to
        // avoid blocking the main Unity thread.
        // Make sure you have successfully obtained a token before making any Text-to-Speech API calls.
        StartCoroutine(AuthenticateSpeechService(authenticating));
    }

    /// <summary>
    /// CoRoutine that checks to see if the async authentication process has completed. Once it completes,
    /// retrieves the token that will be used for subsequent Cognitive Services Text-to-Speech API calls.
    /// </summary>
    /// <param name="authenticating"></param>
    /// <returns></returns>
    private IEnumerator AuthenticateSpeechService(Task<string> authenticating)
    {
        // Yield control back to the main thread as long as the task is still running
        while (!authenticating.IsCompleted)
        {
            yield return null;
        }

        try
        {
            accessToken = authenticating.Result;
            isReady = true;
            Debug.Log($"Token: {accessToken}\n");
        }
        catch (Exception ex)
        {
            Debug.Log("Failed authentication.");
            Debug.Log(ex.ToString());
            Debug.Log(ex.Message);
        }
    }

    /// <summary>
    /// This method is called once by the Unity coroutine once the speech is successfully synthesized.
    /// It will then attempt to play that audio file.
    /// Note that the playback will fail if the output audio format is not pcm encoded.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="args">The <see cref="GenericEventArgs{Stream}"/> instance containing the event data.</param>
    //private void PlayAudio(object sender, GenericEventArgs<Stream> args)
    private AudioClip CreateClip(Stream audioStream, string title)
    {
        //Debug.Log("Playing audio stream");

        AudioClip clip = null;
        // Play the audio using Unity AudioSource, allowing us to benefit from effects,
        // spatialization, mixing, etc.

        // Get the size of the original stream
        var size = audioStream.Length;

        // Don't playback if the stream is empty
        if (size > 0)
        {
            try
            {
                Debug.Log($"Creating new byte array of size {size}");
                // Create buffer
                byte[] buffer = new byte[size];

                Debug.Log($"Reading stream to the end and putting in bytes array.");
                buffer = ReadToEnd(audioStream);

                // Convert raw WAV data into Unity audio data
                Debug.Log($"Converting raw WAV data of size {buffer.Length} into Unity audio data.");
                int sampleCount = 0;
                int frequency = 0;
                var unityData = ToUnityAudio(buffer, out sampleCount, out frequency);

                // Convert data to a Unity audio clip
                Debug.Log($"Converting audio data of size {unityData.Length} to Unity audio clip with {sampleCount} samples at frequency {frequency}.");
                clip = ToClip(title, unityData, sampleCount, frequency);

                // Set the source on the audio clip
                //audioSource.clip = clip;

                //Debug.Log($"Trigger playback of audio clip on AudioSource.");
                // Play audio
                //audioSource.Play();
            }
            catch (Exception ex)
            {
                Debug.Log("An error occurred during audio stream conversion and playback."
                           + Environment.NewLine + ex.Message);
            }
        }

        return clip;
    }

    /// <summary>
    /// Unity Coroutine that monitors the Task used to synthesize speech from a text string.
    /// Once completed, it starts audio playback using the assigned audio source.
    /// </summary>
    /// <param name="speakTask"></param>
    /// <returns></returns>
    private IEnumerator WaitAndWriteRoutine(Task<Stream> speakTask, string title)
    {
        // Yield control back to the main thread as long as the task is still running
        while (!speakTask.IsCompleted)
        {
            yield return null;
        }

        // Get audio stream result send it to play TTS audio
        MemoryStream resultStream = new MemoryStream();
        speakTask.Result.CopyTo(resultStream);
        if (resultStream != null)
        {
            RegisterClip(title, resultStream);
        }
    }

    private void RegisterClip(string title, MemoryStream resultStream)
    {
        var size = resultStream.Length;

        byte[] buffer = new byte[size];

        buffer = ReadToEnd(resultStream);


        //WriteFile(resultStream, title);

        WWUtils.Audio.WAV wav = new WWUtils.Audio.WAV(buffer);
        AudioClip clip = AudioClip.Create(title, wav.SampleCount, 1, wav.Frequency, false);
        clip.SetData(wav.LeftChannel, 0);

        //AudioClip clip = CreateClip(resultStream, title);

        Extensions.Play(clip); // THIS PLAYS THE CLIP IN PLAY MODE. IT WORKS FINE

        AssetDatabase.CreateAsset(clip, "Assets/Resources/Clips/" + title.RemoveDiacritics() + ".wav");
        //SavWav.Save(title.RemoveDiacritics(), clip);
        EditorUtility.SetDirty(clip);
        AssetDatabase.Refresh();

        var writer = FindObjectOfType<TTSWriter>();
        var clipCollectionAsset = writer.clipCollection;
        clipCollectionAsset.allClips.Add(title, clip);
        EditorUtility.SetDirty(clipCollectionAsset);

        //AssetDatabase.SaveAssets();

        //var audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Resources/Clips/" + title.RemoveDiacritics() + ".wav");
        //var audioClip = Resources.Load<AudioClip>("Assets/Resources/Clips/" + title.RemoveDiacritics() + ".wav");

        //using (var www = new WWW("Assets/Resources/Clips/" + title.RemoveDiacritics() + ".wav"))
        //{
        //    yield return www;
        //    var audioClip = www.GetAudioClip();

        //}
        //WWW audioLoader = new WWW("Assets/Resources/Clips/" + title.RemoveDiacritics() + ".wav");
        //var audioClip = audioLoader.GetAudioClip();


        //Extensions.Play(clip);

        //WriteFile(resultStream, title);
    }

    private void WriteFile(Stream audioStream, string title)
    {
        audioStream.Seek(0, SeekOrigin.Begin);
        FileStream fs = File.Create("Assets/Resources/Clips/" + title.RemoveDiacritics() + ".wav");
        byte[] buf = new byte[65536];
        int len = 0;
        while ((len = audioStream.Read(buf, 0, 65536)) > 0)
        {
            fs.Write(buf, 0, len);
        }
        fs.Close();
    }

    /// <summary>
    /// Converts a text string into synthesized speech using Microsoft Cognitive Services, then
    /// starts audio playback using the assigned audio source.
    /// </summary>
    /// <param name="message"></param>
    public void Speak(string message)
    {
        try
        {
            Debug.Log("Starting Cognitive Services Speech API synthesize request code execution.");
            // Synthesis endpoint for old Bing Speech API: https://speech.platform.bing.com/synthesize
            // For new unified SpeechService API: https://westus.tts.speech.microsoft.com/cognitiveservices/v1
            // Note: new unified SpeechService API synthesis endpoint is per region
            string requestUri = "https://westus.tts.speech.microsoft.com/cognitiveservices/v1";
            Synthesize cortana = new Synthesize();

            // Reuse Synthesize object to minimize latency
            Task<Stream> Speaking = cortana.Speak(CancellationToken.None, new Synthesize.InputOptions()
            {
                RequestUri = new Uri(requestUri),
                // Text to be spoken.
                Text = message,
                VoiceType = Gender.Male,
                // Refer to the documentation for complete list of supported locales.
                Locale = cortana.GetVoiceLocale(voiceName),
                // You can also customize the output voice. Refer to the documentation to view the different
                // voices that the TTS service can output.
                VoiceName = voiceName,

                // Service can return audio in different output format.
                OutputFormat = AudioOutputFormat.Riff24Khz16BitMonoPcm,
                AuthorizationToken = "Bearer " + accessToken,
            });

            // We can't await the task without blocking the main Unity thread, so we'll call a coroutine to
            // monitor completion and play audio when it's ready.
            StartCoroutine(WaitAndWriteRoutine(Speaking, message));
        }
        catch (Exception ex)
        {
            Debug.Log("An error occurred when attempting to synthesize speech audio."
                       + Environment.NewLine + ex.Message);
        }
    }

    /// <summary>
    /// Reads a stream from beginning to end, returning an array of bytes
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static byte[] ReadToEnd(Stream stream)
    {
        long originalPosition = 0;

        if (stream.CanSeek)
        {
            originalPosition = stream.Position;
            stream.Position = 0;
        }

        try
        {
            byte[] readBuffer = new byte[4096];

            int totalBytesRead = 0;
            int bytesRead;

            while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
            {
                totalBytesRead += bytesRead;

                if (totalBytesRead == readBuffer.Length)
                {
                    int nextByte = stream.ReadByte();
                    if (nextByte != -1)
                    {
                        byte[] temp = new byte[readBuffer.Length * 2];
                        Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                        Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                        readBuffer = temp;
                        totalBytesRead++;
                    }
                }
            }

            byte[] buffer = readBuffer;
            if (readBuffer.Length != totalBytesRead)
            {
                buffer = new byte[totalBytesRead];
                Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
            }
            return buffer;
        }
        finally
        {
            if (stream.CanSeek)
            {
                stream.Position = originalPosition;
            }
        }
    }

    private float[] ConvertByteToFloat(byte[] array)
    {
        float[] floatArr = new float[array.Length / 4];
        for (int i = 0; i < floatArr.Length; i++)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(array, i * 4, 4);
            floatArr[i] = BitConverter.ToSingle(array, i * 4);
        }
        return floatArr;
    }

    /// <summary>
    /// Converts two bytes to one float in the range -1 to 1.
    /// </summary>
    /// <param name="firstByte">The first byte.</param>
    /// <param name="secondByte"> The second byte.</param>
    /// <returns>The converted float.</returns>
    private static float BytesToFloat(byte firstByte, byte secondByte)
    {
        // Convert two bytes to one short (little endian)
        short s = (short)((secondByte << 8) | firstByte);

        // Convert to range from -1 to (just below) 1
        return s / 32768.0F;
    }

    /// <summary>
    /// Converts an array of bytes to an integer.
    /// </summary>
    /// <param name="bytes"> The byte array.</param>
    /// <param name="offset"> An offset to read from.</param>
    /// <returns>The converted int.</returns>
    private static int BytesToInt(byte[] bytes, int offset = 0)
    {
        int value = 0;
        for (int i = 0; i < 4; i++)
        {
            value |= bytes[offset + i] << (i * 8);
        }
        return value;
    }

    /// <summary>
    /// Dynamically creates an <see cref="AudioClip"/> that represents raw Unity audio data.
    /// </summary>
    /// <param name="name"> The name of the dynamically generated clip.</param>
    /// <param name="audioData">Raw Unity audio data.</param>
    /// <param name="sampleCount">The number of samples in the audio data.</param>
    /// <param name="frequency">The frequency of the audio data.</param>
    /// <returns>The <see cref="AudioClip"/>.</returns>
    private static AudioClip ToClip(string name, float[] audioData, int sampleCount, int frequency)
    {
        var clip = AudioClip.Create(name, sampleCount, 1, frequency, false);
        clip.SetData(audioData, 0);
        return clip;
    }

    /// <summary>
    /// Converts raw WAV data into Unity formatted audio data.
    /// </summary>
    /// <param name="wavAudio">The raw WAV data.</param>
    /// <param name="sampleCount">The number of samples in the audio data.</param>
    /// <param name="frequency">The frequency of the audio data.</param>
    /// <returns>The Unity formatted audio data. </returns>
    private static float[] ToUnityAudio(byte[] wavAudio, out int sampleCount, out int frequency)
    {
        // Determine if mono or stereo
        int channelCount = wavAudio[22];  // Speech audio data is always mono but read actual header value for processing
        Debug.Log($"Audio data has {channelCount} channel(s).");

        // Get the frequency
        frequency = BytesToInt(wavAudio, 24);
        Debug.Log($"Audio data frequency is {frequency}.");

        // Get past all the other sub chunks to get to the data subchunk:
        int pos = 12; // First subchunk ID from 12 to 16

        // Keep iterating until we find the data chunk (i.e. 64 61 74 61 ...... (i.e. 100 97 116 97 in decimal))
        while (!(wavAudio[pos] == 100 && wavAudio[pos + 1] == 97 && wavAudio[pos + 2] == 116 && wavAudio[pos + 3] == 97))
        {
            pos += 4;
            int chunkSize = wavAudio[pos] + wavAudio[pos + 1] * 256 + wavAudio[pos + 2] * 65536 + wavAudio[pos + 3] * 16777216;
            pos += 4 + chunkSize;
        }
        pos += 8;

        // Pos is now positioned to start of actual sound data.
        sampleCount = (wavAudio.Length - pos) / 2;  // 2 bytes per sample (16 bit sound mono)
        if (channelCount == 2) { sampleCount /= 2; }  // 4 bytes per sample (16 bit stereo)
        Debug.Log($"Audio data contains {sampleCount} samples. Starting conversion");

        // Allocate memory (supporting left channel only)
        var unityData = new float[sampleCount];

        try
        {
            // Write to double array/s:
            int i = 0;
            while (pos < wavAudio.Length)
            {
                unityData[i] = BytesToFloat(wavAudio[pos], wavAudio[pos + 1]);
                pos += 2;
                if (channelCount == 2)
                {
                    pos += 2;
                }
                i++;
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"Error occurred converting audio data to float array of size {wavAudio.Length} at position {pos}.");
        }

        return unityData;
    }

    private void ExportClipData(AudioClip clip, string title)
    {
        var data = new float[clip.samples * clip.channels];
        clip.GetData(data, 0);
        //var path = Path.Combine(Application.persistentDataPath, title + ".wav");
        using (var stream = new FileStream("Assets/Resources/Clips/" + title.RemoveDiacritics() + ".wav", FileMode.CreateNew, FileAccess.Write))
        {
            // The following values are based on http://soundfile.sapp.org/doc/WaveFormat/
            var bitsPerSample = (ushort)16;
            var chunkID = "RIFF";
            var format = "WAVE";
            var subChunk1ID = "fmt ";
            var subChunk1Size = (uint)16;
            var audioFormat = (ushort)1;
            var numChannels = (ushort)clip.channels;
            var sampleRate = (uint)clip.frequency;
            var byteRate = (uint)(sampleRate * clip.channels * bitsPerSample / 8);  // SampleRate * NumChannels * BitsPerSample/8
            var blockAlign = (ushort)(numChannels * bitsPerSample / 8); // NumChannels * BitsPerSample/8
            var subChunk2ID = "data";
            var subChunk2Size = (uint)(data.Length * clip.channels * bitsPerSample / 8); // NumSamples * NumChannels * BitsPerSample/8
            var chunkSize = 36 + subChunk2Size; // 36 + SubChunk2Size
                                                // Start writing the file.
            WriteString(stream, chunkID);
            WriteInteger(stream, chunkSize);
            WriteString(stream, format);
            WriteString(stream, subChunk1ID);
            WriteInteger(stream, subChunk1Size);
            WriteShort(stream, audioFormat);
            WriteShort(stream, numChannels);
            WriteInteger(stream, sampleRate);
            WriteInteger(stream, byteRate);
            WriteShort(stream, blockAlign);
            WriteShort(stream, bitsPerSample);
            WriteString(stream, subChunk2ID);
            WriteInteger(stream, subChunk2Size);
            foreach (var sample in data)
            {
                // De-normalize the samples to 16 bits.
                var deNormalizedSample = (short)0;
                if (sample > 0)
                {
                    var temp = sample * short.MaxValue;
                    if (temp > short.MaxValue)
                        temp = short.MaxValue;
                    deNormalizedSample = (short)temp;
                }
                if (sample < 0)
                {
                    var temp = sample * (-short.MinValue);
                    if (temp < short.MinValue)
                        temp = short.MinValue;
                    deNormalizedSample = (short)temp;
                }
                WriteShort(stream, (ushort)deNormalizedSample);
            }
        }
    }

    void WriteString(Stream stream, string value)
    {
        foreach (var character in value)
            stream.WriteByte((byte)character);
    }

    void WriteInteger(Stream stream, uint value)
    {
        stream.WriteByte((byte)(value & 0xFF));
        stream.WriteByte((byte)((value >> 8) & 0xFF));
        stream.WriteByte((byte)((value >> 16) & 0xFF));
        stream.WriteByte((byte)((value >> 24) & 0xFF));
    }

    void WriteShort(Stream stream, ushort value)
    {
        stream.WriteByte((byte)(value & 0xFF));
        stream.WriteByte((byte)((value >> 8) & 0xFF));
    }
}

namespace WWUtils.Audio
{
    public class WAV
    {

        // convert two bytes to one float in the range -1 to 1
        static float bytesToFloat(byte firstByte, byte secondByte)
        {
            // convert two bytes to one short (little endian)
            short s = (short)((secondByte << 8) | firstByte);
            // convert to range from -1 to (just below) 1
            return s / 32768.0F;
        }

        static int bytesToInt(byte[] bytes, int offset = 0)
        {
            int value = 0;
            for (int i = 0; i < 4; i++)
            {
                value |= bytes[offset + i] << (i * 8);
            }
            return value;
        }

        private static byte[] GetBytes(string filename)
        {
            return File.ReadAllBytes(filename);
        }
        // properties
        public float[] LeftChannel { get; internal set; }
        public float[] RightChannel { get; internal set; }
        public int ChannelCount { get; internal set; }
        public int SampleCount { get; internal set; }
        public int Frequency { get; internal set; }

        // Returns left and right double arrays. 'right' will be null if sound is mono.
        public WAV(string filename) :
            this(GetBytes(filename))
        { }

        public WAV(byte[] wav)
        {

            // Determine if mono or stereo
            ChannelCount = wav[22];     // Forget byte 23 as 99.999% of WAVs are 1 or 2 channels

            // Get the frequency
            Frequency = bytesToInt(wav, 24);

            // Get past all the other sub chunks to get to the data subchunk:
            int pos = 12;   // First Subchunk ID from 12 to 16

            // Keep iterating until we find the data chunk (i.e. 64 61 74 61 ...... (i.e. 100 97 116 97 in decimal))
            while (!(wav[pos] == 100 && wav[pos + 1] == 97 && wav[pos + 2] == 116 && wav[pos + 3] == 97))
            {
                pos += 4;
                int chunkSize = wav[pos] + wav[pos + 1] * 256 + wav[pos + 2] * 65536 + wav[pos + 3] * 16777216;
                pos += 4 + chunkSize;
            }
            pos += 8;

            // Pos is now positioned to start of actual sound data.
            SampleCount = (wav.Length - pos) / 2;     // 2 bytes per sample (16 bit sound mono)
            if (ChannelCount == 2) SampleCount /= 2;        // 4 bytes per sample (16 bit stereo)

            // Allocate memory (right will be null if only mono sound)
            LeftChannel = new float[SampleCount];
            if (ChannelCount == 2) RightChannel = new float[SampleCount];
            else RightChannel = null;

            // Write to double array/s:
            int i = 0;
            while (pos < wav.Length)
            {
                LeftChannel[i] = bytesToFloat(wav[pos], wav[pos + 1]);
                pos += 2;
                if (ChannelCount == 2)
                {
                    RightChannel[i] = bytesToFloat(wav[pos], wav[pos + 1]);
                    pos += 2;
                }
                i++;
            }
        }

        public override string ToString()
        {
            return string.Format("[WAV: LeftChannel={0}, RightChannel={1}, ChannelCount={2}, SampleCount={3}, Frequency={4}]", LeftChannel, RightChannel, ChannelCount, SampleCount, Frequency);
        }
    }

}