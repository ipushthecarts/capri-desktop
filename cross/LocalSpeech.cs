using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;

namespace CapriDesktop
{
    internal sealed class LocalSpeech
    {
        private readonly object gate = new object();
        private Process playback;
        private int generation;
        private bool busy;
        private int[] mouths;
        private Stopwatch elapsed;
        internal bool Busy { get { lock (gate) return busy; } }
        internal int Mouth { get { lock (gate) { if (mouths == null || elapsed == null) return -1; int at = (int)(elapsed.Elapsed.TotalSeconds * 12); return at < mouths.Length ? mouths[at] : 0; } } }

        internal void Stop()
        {
            lock (gate)
            {
                ++generation;
                busy = false;
                mouths = null;
                elapsed = null;
                try { if (playback != null && !playback.HasExited) playback.Kill(true); } catch (InvalidOperationException) { } catch (System.ComponentModel.Win32Exception) { }
                playback = null;
            }
        }

        internal void Say(string text, Settings settings, Action<string> failed)
        {
            Stop();
            text = Voice.Sayable(Voice.Spoken(text));
            if (settings.LocalVoiceEnabled == false || text.Length == 0) return;
            int ticket;
            lock (gate) { ticket = generation; busy = true; }
            string provider = settings.LocalVoiceProvider;
            string voice = settings.LocalPocketVoice;
            int port = settings.LocalPocketPort >= 1024 && settings.LocalPocketPort <= 65535 ? settings.LocalPocketPort : 8765;
            ThreadPool.QueueUserWorkItem(delegate
            {
                string file = null;
                Process process = null;
                string problem = null;
                try
                {
                    ProcessStartInfo start;
                    int[] shapes = null;
                    if (provider == "pocket")
                    {
                        byte[] wav = LocalVoiceTransport.Request(text, voice, port);
                        lock (gate) { if (ticket != generation) return; }
                        file = Path.Combine(Path.GetTempPath(), "capri-voice-" + Guid.NewGuid().ToString("N") + ".wav");
                        File.WriteAllBytes(file, wav);
                        shapes = Voice.Mouths(wav, 12);
                        string player = OperatingSystem.IsMacOS() ? "/usr/bin/afplay" : Find("paplay") ?? Find("pw-play") ?? Find("aplay");
                        if (player == null) throw new IOException("Install pulseaudio-utils (Debian/Ubuntu) or libpulse (Arch) for Pocket TTS playback.");
                        start = new ProcessStartInfo(player);
                        start.ArgumentList.Add(file);
                    }
                    else
                    {
                        string command = OperatingSystem.IsMacOS() ? "/usr/bin/say" : Find("espeak-ng");
                        if (command == null) throw new IOException("Install espeak-ng for the system voice, or select Pocket TTS.");
                        start = new ProcessStartInfo(command);
                        if (OperatingSystem.IsLinux()) start.ArgumentList.Add("--stdin");
                        start.RedirectStandardInput = true;
                    }
                    start.UseShellExecute = false;
                    start.CreateNoWindow = true;
                    start.RedirectStandardError = true;
                    process = new Process { StartInfo = start };
                    lock (gate)
                    {
                        if (ticket != generation) return;
                        process.Start();
                        playback = process;
                        mouths = shapes;
                        elapsed = Stopwatch.StartNew();
                    }
                    // Drain stderr while the player runs, so a full pipe cannot stall it.
                    var errors = process.StandardError.ReadToEndAsync();
                    if (start.RedirectStandardInput) { process.StandardInput.Write(text); process.StandardInput.Close(); }
                    if (!process.WaitForExit(180000)) { process.Kill(true); throw new IOException("Local voice playback timed out."); }
                    string detail = errors.GetAwaiter().GetResult();
                    if (process.ExitCode != 0) throw new IOException("Local voice playback failed. Check your audio output. " + Voice.Short(detail));
                }
                catch (Exception error) { problem = Voice.Short(error.Message); }
                finally
                {
                    bool current;
                    lock (gate) { current = ticket == generation; if (current) { busy = false; playback = null; mouths = null; elapsed = null; } }
                    if (process != null) process.Dispose();
                    if (file != null) { try { File.Delete(file); } catch (IOException) { } catch (UnauthorizedAccessException) { } }
                    if (current && problem != null) Dispatcher.UIThread.Post(delegate { lock (gate) { if (ticket != generation) return; } failed(problem); });
                }
            });
        }

        private static string Find(string command)
        {
            foreach (string folder in (Environment.GetEnvironmentVariable("PATH") ?? "/usr/bin:/bin").Split(Path.PathSeparator))
            {
                string path = Path.Combine(folder, command);
                if (File.Exists(path)) return path;
            }
            return null;
        }

        internal static void Configure(Pet pet)
        {
            Window dialog = new Window { Title = "Capri - local voice", Width = 470, Height = 440, CanResize = false, WindowStartupLocation = WindowStartupLocation.CenterOwner };
            StackPanel panel = new StackPanel { Margin = new Thickness(20), Spacing = 10 };
            CheckBox enabled = new CheckBox { Content = "Speak replies", IsChecked = pet.Saved.LocalVoiceEnabled != false };
            ComboBox provider = new ComboBox { ItemsSource = new[] { "System voice (no extra server)", "Pocket TTS (custom voice on CPU)" }, SelectedIndex = pet.Saved.LocalVoiceProvider == "pocket" ? 1 : 0, HorizontalAlignment = HorizontalAlignment.Stretch };
            TextBox voice = new TextBox { Text = pet.Saved.LocalPocketVoice ?? "alba", Watermark = "alba or full path to a local voice sample" };
            TextBox port = new TextBox { Text = (pet.Saved.LocalPocketPort >= 1024 ? pet.Saved.LocalPocketPort : 8765).ToString() };
            TextBlock status = new TextBlock { Text = "Pocket TTS needs Start-PocketVoice.sh running. Microphone chat is currently Windows-only.", TextWrapping = Avalonia.Media.TextWrapping.Wrap };
            Button save = new Button { Content = "Save and test", HorizontalAlignment = HorizontalAlignment.Stretch };
            save.Click += delegate
            {
                int number;
                if (!int.TryParse(port.Text, out number) || number < 1024 || number > 65535) { status.Text = "Port must be 1024-65535."; return; }
                pet.Saved.LocalVoiceEnabled = enabled.IsChecked == true;
                pet.Saved.LocalVoiceProvider = provider.SelectedIndex == 1 ? "pocket" : "system";
                pet.Saved.LocalPocketVoice = (voice.Text ?? "").Trim();
                pet.Saved.LocalPocketPort = number;
                pet.Save();
                pet.Speech.Say("Voice link online. Miss me, choom?", pet.Saved, delegate(string problem) { status.Text = problem; });
                status.Text = enabled.IsChecked == true ? "Settings saved. Testing local voice..." : "Settings saved. Replies will stay silent.";
            };
            panel.Children.Add(enabled); panel.Children.Add(provider);
            panel.Children.Add(new TextBlock { Text = "Pocket voice: preset or full local audio path" }); panel.Children.Add(voice);
            panel.Children.Add(new TextBlock { Text = "Pocket server port (this computer only)" }); panel.Children.Add(port);
            panel.Children.Add(save); panel.Children.Add(status);
            dialog.Content = panel;
            dialog.Show(pet);
        }
    }
}
