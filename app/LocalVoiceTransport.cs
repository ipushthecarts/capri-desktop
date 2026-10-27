using System;
using System.IO;
using System.Net;
using System.Text;

namespace CapriDesktop
{
    internal static class LocalVoiceTransport
    {
        internal static byte[] Request(string text, string voice, int port)
        {
            if (port < 1024 || port > 65535) throw new VoiceException("Invalid Pocket TTS port.");
            // Fixed loopback destination: no keys, cloud fallback, proxy or redirects.
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:" + port + "/v1/audio/speech");
            request.Method = "POST";
            request.Proxy = null;
            request.AllowAutoRedirect = false;
            request.Timeout = 120000;
            request.ReadWriteTimeout = 120000;
            request.ContentType = "application/json";
            request.ServicePoint.Expect100Continue = false;
            byte[] body = Encoding.UTF8.GetBytes("{\"input\":" + Voice.Quote(text) + ",\"voice\":" + Voice.Quote(string.IsNullOrWhiteSpace(voice) ? "alba" : voice.Trim()) + "}");
            request.ContentLength = body.Length;
            try
            {
                using (Stream upload = request.GetRequestStream()) upload.Write(body, 0, body.Length);
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (MemoryStream output = new MemoryStream())
                {
                    if (response.StatusCode != HttpStatusCode.OK) throw new VoiceException("Pocket TTS returned HTTP " + (int)response.StatusCode + ".");
                    byte[] buffer = new byte[8192];
                    int count;
                    while ((count = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        if (output.Length + count > 32 * 1024 * 1024) throw new VoiceException("Pocket TTS returned too much audio.");
                        output.Write(buffer, 0, count);
                    }
                    byte[] wav = output.ToArray();
                    Validate(wav);
                    return Voice.Clean(wav);
                }
            }
            catch (WebException error)
            {
                if (error.Response != null) error.Response.Dispose();
                throw new VoiceException(error.Status == WebExceptionStatus.Timeout ? "Pocket TTS timed out. Try a shorter reply." : "Pocket TTS is unavailable. Start Start-PocketVoice.cmd and check its window.");
            }
        }

        internal static void Validate(byte[] wav)
        {
            if (wav.Length < 44 || Encoding.ASCII.GetString(wav, 0, 4) != "RIFF" || Encoding.ASCII.GetString(wav, 8, 4) != "WAVE")
                throw new VoiceException("Pocket TTS did not return a WAV file.");
            // Capri's animation and playback expect mono PCM16, not float WAV.
            bool format = false, data = false;
            for (int at = 12; at + 8 <= wav.Length; )
            {
                int size = BitConverter.ToInt32(wav, at + 4);
                long next = (long)at + 8 + size + (size & 1);
                if (size < 0 || next > wav.Length) throw new VoiceException("Pocket TTS returned a truncated WAV file.");
                string chunk = Encoding.ASCII.GetString(wav, at, 4);
                if (chunk == "fmt ")
                {
                    if (size < 16 || BitConverter.ToInt16(wav, at + 8) != 1 || BitConverter.ToInt16(wav, at + 10) != 1 || BitConverter.ToInt16(wav, at + 22) != 16 || BitConverter.ToInt32(wav, at + 12) <= 0)
                        throw new VoiceException("Pocket TTS audio must be mono PCM16 WAV.");
                    format = true;
                }
                if (chunk == "data" && size >= 2 && size % 2 == 0) data = true;
                at = (int)next;
            }
            if (!format || !data) throw new VoiceException("Pocket TTS returned an empty or invalid WAV file.");
        }

    }
}
