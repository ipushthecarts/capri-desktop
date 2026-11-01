# Capri Local Voice 0.7.8

Capri for Windows, Linux and macOS, with local speech and no required ElevenLabs key or speech credits.

**Downloads:** https://github.com/ipushthecarts/capri-desktop/releases/latest

**Original Capri project:** https://github.com/lachydotmcg/capri-desktop

This release incorporates upstream **0.7.0 through 0.7.8**, including its intermediate fixes. The original author's copyright, artwork credits, and [PolyForm Noncommercial license](LICENSE.md) remain in place. This is an unofficial fork, not an upstream release.

## Install

See [PACKAGES.md](PACKAGES.md) for commands and platform requirements.

| Platform | Package |
| --- | --- |
| Windows x64 | `Capri-Local-Setup.exe`, standalone `Capri-Local.exe`, and Windows ZIP |
| Debian/Ubuntu x64 | `.deb` |
| Arch x64 | `.pkg.tar.zst` |
| Other Linux x64 | Portable `.tar.gz` with user-level installer |
| Mac Apple Silicon / Intel | Separate `.dmg` installers and app ZIPs; macOS 15+ |

## What changed from the previous fork

- Imported the complete upstream update history through tag `v0.7.8`, rather than copying selected release features.
- Kept the newer Mainframe, local/custom voice controls, voice effects/layers, memory, model providers, game support, hands, hotkeys and platform improvements.
- Replaced the old parallel speech hook with providers in upstream's current speech pipeline. The default system voice uses Windows speech, macOS `say`, or Linux `espeak-ng`. Generated audio passes through the current voice effects and playback code.
- Added a Pocket TTS preset to Windows **CUSTOM LOCAL** setup. The bundled CPU server also works with the Mac/Linux **Local server** provider.
- Migrated old system/Pocket voice choices, reference paths, ports and mute state once. Later voice selections are kept.
- Kept settings in the fork's existing `Capri-Local` directory and disabled automatic upstream Windows installs, so an upstream installer cannot replace these modifications. Mac/Linux update notices check this fork.
- Rebuilt native packages from the updated sources, with a full release and SHA-256 checksums.

## Local voice setup

Windows starts with the installed system voice. Under **VOICE > CUSTOM LOCAL > Set up**, choose **System voice · on your PC**, **Pocket TTS · on your PC**, another local server, or one of upstream's directly supported voice model files. PREBUILT LOCAL and optional cloud providers remain available. Cloud providers use their own accounts/limits; they are not required.

On Mac/Linux, open **Settings > Voice > Provider** and choose **System voice** or **Local server**. For the bundled Pocket server, use:

- Server: `http://127.0.0.1:8765/v1`
- Voice: `alba`, another supported preset, or the full local path to a reference sample
- Format: WAV
- Model: blank

Run `Setup-PocketVoice.cmd` then `Start-PocketVoice.cmd` on Windows. Use the `.sh` launchers on Mac/Linux; see PACKAGES.md for their installed paths. Setup downloads Python dependencies, and the first request downloads model files. No model weights or personal voice recordings are included in the installers.

A reference sample can be created separately with a voice-design tool such as Qwen3-TTS, or recorded with appropriate rights and consent. An ElevenLabs voice ID cannot be imported as a local model.

## Upstream updates included

See [UPSTREAM.md](UPSTREAM.md) for the imported versions and source commit. Upstream 0.7.8 already includes many local voice improvements, so this fork builds on those rather than maintaining the old 0.6.2 voice implementation.

Linux/Mac support typed questions with spoken replies; microphone chat remains Windows-only. Linux perching/window control depends on the desktop session; X11 and supported Hyprland setups have different capabilities from other Wayland sessions.

## Build and checks

Windows builds require .NET Framework 4.8 and the Windows 10/11 SDK. The upstream build script now supplies the complete source/resource list and builds both the app and installer:

```powershell
./Build-Local.ps1 -SmokeTest
```

Mac/Linux:

```sh
dotnet build cross/Capri.Cross.csproj -c Release
python3 -m unittest discover -s tests -v
```

The release workflow builds on native Windows, Ubuntu and macOS runners, exercises upstream desktop/installer smoke tests plus local settings migration checks, checks real OS speech synthesis without playing through an audio device, and verifies Debian/Arch package installation. Live microphone/audio-device behavior, HD models, Pocket model inference and laptop-specific performance still need testing on the target hardware. See [LOCAL-VOICE.md](LOCAL-VOICE.md).
