using System;
using System.IO;
#if CROSS
using System.Diagnostics;
#else
using System.Speech.AudioFormat;
using System.Speech.Synthesis;
#endif

namespace CapriDesktop
{
    internal static class SystemVoice
    {
        internal static byte[] Speak(string text, string name)
        {
            text = Voice.Spoken(text);
            if (text.Length == 0) throw new VoiceException(Voice.Silent);
#if CROSS
            bool mac = OperatingSystem.IsMacOS();
            string file = mac ? Path.Combine(Path.GetTempPath(), "capri-system-" + Guid.NewGuid().ToString("N") + ".wav") : null;
            try
            {
                ProcessStartInfo start = new ProcessStartInfo(mac ? "/usr/bin/say" : "espeak-ng");
                start.UseShellExecute = false;
                start.CreateNoWindow = true;
                start.RedirectStandardInput = start.RedirectStandardOutput = start.RedirectStandardError = true;
                if (mac)
                {
                    start.ArgumentList.Add("-o"); start.ArgumentList.Add(file);
                    start.ArgumentList.Add("--file-format=WAVE");
                    start.ArgumentList.Add("--data-format=LEI16@24000");
                }
                else { start.ArgumentList.Add("--stdout"); start.ArgumentList.Add("--stdin"); }
                if (!string.IsNullOrWhiteSpace(name)) { start.ArgumentList.Add("-v"); start.ArgumentList.Add(name); }
                using (Process process = Process.Start(start))
                using (MemoryStream sound = new MemoryStream())
                {
                    var output = process.StandardOutput.BaseStream.CopyToAsync(sound);
                    var errors = process.StandardError.ReadToEndAsync();
                    process.StandardInput.Write(text);
                    process.StandardInput.Close();
                    if (!process.WaitForExit(60000)) { process.Kill(true); throw new VoiceException("The system voice timed out."); }
                    output.GetAwaiter().GetResult();
                    string problem = errors.GetAwaiter().GetResult();
                    if (process.ExitCode != 0) throw new VoiceException("System voice failed: " + Voice.Short(problem));
                    byte[] wav = mac ? File.ReadAllBytes(file) : sound.ToArray();
                    int rate;
                    if (Voice.Samples(wav, out rate).Length == 0) throw new VoiceException("The system voice returned no sound.");
                    return Voice.Clean(wav);
                }
            }
            catch (System.ComponentModel.Win32Exception) { throw new VoiceException("Install espeak-ng for the system voice, or choose a local speech server."); }
            finally { if (file != null) { try { File.Delete(file); } catch (IOException) { } } }
#else
            using (SpeechSynthesizer synth = new SpeechSynthesizer())
            using (MemoryStream sound = new MemoryStream())
            {
                if (!string.IsNullOrWhiteSpace(name)) synth.SelectVoice(name);
                synth.SetOutputToAudioStream(sound, new SpeechAudioFormatInfo(24000, AudioBitsPerSample.Sixteen, AudioChannel.Mono));
                synth.Speak(text);
                byte[] bytes = sound.ToArray();
                short[] samples = new short[bytes.Length / 2];
                Buffer.BlockCopy(bytes, 0, samples, 0, samples.Length * 2);
                return Voice.Wave(samples, 24000);
            }
#endif
        }
    }
}
