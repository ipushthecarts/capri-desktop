using System;
using System.IO;
using System.Net;
using System.Speech.AudioFormat;
using System.Speech.Synthesis;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace CapriDesktop
{
    // Kept separate from Voice's shared codec/playback helpers and legacy API code.
    internal static class LocalVoice
    {
        internal static bool Pocket(Preferences p) { return p.LocalVoiceProvider == "pocket"; }
        internal static int Port(Preferences p) { return p.LocalPocketPort >= 1024 && p.LocalPocketPort <= 65535 ? p.LocalPocketPort : 8765; }
        internal static string Name(Preferences p) { return Pocket(p) ? "Pocket TTS (local)" : "Windows voice (local)"; }

        internal static byte[] Speak(string text, Preferences p)
        {
            text = Voice.Sayable(Voice.Spoken(text));
            if (text.Length == 0) throw new VoiceException(Voice.Silent);
            if (Pocket(p)) return LocalVoiceTransport.Request(text, p.LocalPocketVoice, Port(p));
            using (SpeechSynthesizer synth = new SpeechSynthesizer())
            using (MemoryStream output = new MemoryStream())
            {
                if (!string.IsNullOrEmpty(p.LocalWindowsVoice)) synth.SelectVoice(p.LocalWindowsVoice);
                synth.SetOutputToAudioStream(output, new SpeechAudioFormatInfo(24000, AudioBitsPerSample.Sixteen, AudioChannel.Mono));
                synth.Speak(text);
                byte[] bytes = output.ToArray();
                short[] samples = new short[bytes.Length / 2];
                Buffer.BlockCopy(bytes, 0, samples, 0, samples.Length * 2);
                return Voice.Wave(samples, 24000);
            }
        }

        internal static bool Configure(IWin32Window owner, Preferences p)
        {
            using (Form dialog = new Form())
            {
                dialog.Text = "Capri - local voice";
                dialog.ClientSize = new Size(490, 340);
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = dialog.MinimizeBox = false;
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.AutoScaleMode = AutoScaleMode.Dpi;
                dialog.Controls.Add(new Label { Text = "Speech engine", Bounds = new Rectangle(18, 18, 450, 20) });
                ComboBox provider = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Bounds = new Rectangle(18, 42, 450, 26) };
                provider.Items.AddRange(new object[] { "Windows voice - no extra installation", "Pocket TTS - custom voice on your CPU" });
                provider.SelectedIndex = Pocket(p) ? 1 : 0;
                dialog.Controls.Add(provider);
                dialog.Controls.Add(new Label { Text = "Installed Windows voice", Bounds = new Rectangle(18, 80, 450, 20) });
                ComboBox windows = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Bounds = new Rectangle(18, 104, 450, 26) };
                windows.Items.Add("Windows default");
                windows.SelectedIndex = 0;
                using (SpeechSynthesizer synth = new SpeechSynthesizer())
                    foreach (InstalledVoice installed in synth.GetInstalledVoices())
                        if (installed.Enabled) { int i = windows.Items.Add(installed.VoiceInfo.Name); if (installed.VoiceInfo.Name == p.LocalWindowsVoice) windows.SelectedIndex = i; }
                dialog.Controls.Add(windows);
                dialog.Controls.Add(new Label { Text = "Pocket voice: preset name or full path to a voice sample", Bounds = new Rectangle(18, 144, 450, 20) });
                TextBox sample = new TextBox { Text = p.LocalPocketVoice ?? "alba", Bounds = new Rectangle(18, 168, 366, 26) };
                Button browse = new Button { Text = "Browse", Bounds = new Rectangle(390, 166, 78, 28) };
                browse.Click += delegate { using (OpenFileDialog file = new OpenFileDialog { Filter = "Voice sample|*.wav;*.mp3;*.flac;*.safetensors|All files|*.*" }) if (file.ShowDialog(dialog) == DialogResult.OK) sample.Text = file.FileName; };
                dialog.Controls.Add(sample); dialog.Controls.Add(browse);
                dialog.Controls.Add(new Label { Text = "Pocket server port (on this computer only)", Bounds = new Rectangle(18, 208, 360, 20) });
                NumericUpDown port = new NumericUpDown { Minimum = 1024, Maximum = 65535, Value = Port(p), Bounds = new Rectangle(378, 206, 90, 26) };
                dialog.Controls.Add(port);
                Label hint = new Label { Text = "No API key or credits. Start Start-PocketVoice.cmd before using Pocket TTS. Model files download on first use.", Bounds = new Rectangle(18, 244, 450, 40) };
                dialog.Controls.Add(hint);
                Action sync = delegate { bool pocket = provider.SelectedIndex == 1; windows.Enabled = !pocket; sample.Enabled = browse.Enabled = port.Enabled = pocket; };
                provider.SelectedIndexChanged += delegate { sync(); }; sync();
                Button save = new Button { Text = "Save", DialogResult = DialogResult.OK, Bounds = new Rectangle(300, 296, 80, 28) };
                Button cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Bounds = new Rectangle(388, 296, 80, 28) };
                dialog.Controls.Add(save); dialog.Controls.Add(cancel); dialog.AcceptButton = save; dialog.CancelButton = cancel;
                if (dialog.ShowDialog(owner) != DialogResult.OK) return false;
                p.LocalVoiceProvider = provider.SelectedIndex == 1 ? "pocket" : "windows";
                p.LocalWindowsVoice = windows.SelectedIndex > 0 ? windows.SelectedItem.ToString() : null;
                p.LocalPocketVoice = sample.Text.Trim();
                p.LocalPocketPort = (int)port.Value;
                return true;
            }
        }
    }
}
