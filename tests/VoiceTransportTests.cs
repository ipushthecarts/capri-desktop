// Standalone protocol checks; build with the actual transport and shared WAV helpers.
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace CapriDesktop
{
    // Voice's JSON error helper logs through Brain; the voice protocol needs no chat model.
    internal static class Brain { internal static void Log(string url, int status, string detail) { } }

    internal static class VoiceTransportTests
    {
        private static int checks;
        private static void Assert(bool condition, string message) { if (!condition) throw new Exception(message); checks++; }
        private static void Reject(byte[] wav)
        {
            bool rejected = false;
            try { LocalVoiceTransport.Validate(wav); } catch (VoiceException) { rejected = true; }
            Assert(rejected, "Invalid WAV accepted");
        }

        private static byte[] Reply(byte[] wav, int status, out int port, out Thread worker, Action<string> saw)
        {
            TcpListener server = new TcpListener(IPAddress.Loopback, 0);
            server.Start();
            port = ((IPEndPoint)server.LocalEndpoint).Port;
            worker = new Thread(delegate()
            {
                try
                {
                    using (TcpClient client = server.AcceptTcpClient())
                    using (NetworkStream stream = client.GetStream())
                    {
                        stream.ReadTimeout = 5000;
                        StreamReader reader = new StreamReader(stream, Encoding.UTF8, false, 1024, true);
                        string line; int length = 0;
                        StringBuilder request = new StringBuilder();
                        while (!string.IsNullOrEmpty(line = reader.ReadLine()))
                        {
                            request.AppendLine(line);
                            if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase)) length = int.Parse(line.Substring(15));
                        }
                        char[] body = new char[length];
                        int at = 0, got;
                        while (at < length && (got = reader.Read(body, at, length - at)) > 0) at += got;
                        saw(request.ToString() + new string(body, 0, at));
                        byte[] header = Encoding.ASCII.GetBytes("HTTP/1.1 " + status + " Response\r\nContent-Type: audio/wav\r\nContent-Length: " + wav.Length + "\r\nConnection: close\r\n\r\n");
                        stream.Write(header, 0, header.Length); stream.Write(wav, 0, wav.Length);
                    }
                }
                finally { server.Stop(); }
            });
            worker.IsBackground = true;
            worker.Start();
            return wav;
        }

        private static int Main()
        {
            byte[] valid = Voice.Wave(new short[] { 0, 1000, -1000 }, 24000);
            LocalVoiceTransport.Validate(valid); checks++;
            Reject(Encoding.UTF8.GetBytes("not audio"));
            byte[] stereo = (byte[])valid.Clone(); stereo[22] = 2; Reject(stereo);
            byte[] floating = (byte[])valid.Clone(); floating[20] = 3; Reject(floating);
            byte[] truncated = new byte[valid.Length - 1]; Array.Copy(valid, truncated, truncated.Length); Reject(truncated);
            Reject(Voice.Wave(new short[0], 24000));
            int port; Thread thread; string request = null;
            Reply(valid, 200, out port, out thread, delegate(string text) { request = text; });
            byte[] response = LocalVoiceTransport.Request("Hello \"choom\"", "C:\\my voice.wav", port);
            Assert(thread.Join(5000), "Fake server did not stop");
            Assert(Voice.Seconds(response) > 0, "Audio missing");
            Assert(request.Contains("POST /v1/audio/speech") && request.Contains("Hello \\\"choom\\\"") && request.Contains("C:\\\\my voice.wav"), "JSON/path incorrect");
            Assert(request.IndexOf("xi-api-key", StringComparison.OrdinalIgnoreCase) < 0, "Cloud credential sent");
            Reply(Encoding.UTF8.GetBytes("bad"), 200, out port, out thread, delegate { });
            bool failed = false;
            try { LocalVoiceTransport.Request("Hi", "alba", port); } catch (VoiceException) { failed = true; }
            Assert(failed && thread.Join(5000), "Malformed audio did not fail");
            Reply(new byte[0], 503, out port, out thread, delegate { });
            failed = false;
            try { LocalVoiceTransport.Request("Hi", "alba", port); } catch (VoiceException) { failed = true; }
            Assert(failed && thread.Join(5000), "Unavailable server did not fail");
            Console.WriteLine("PASS: " + checks + " transport/audio checks");
            return 0;
        }
    }
}
