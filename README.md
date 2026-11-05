# Capri Local Voice 0.8.3

Capri for Windows, Linux and macOS, with local speech and no required ElevenLabs key or speech credits.

**Downloads:** https://github.com/ipushthecarts/capri-desktop/releases/latest

**Original Capri project:** https://github.com/lachydotmcg/capri-desktop

This release incorporates upstream **0.7.0 through 0.8.3**, including its intermediate fixes. The original author's copyright, artwork credits, and [PolyForm Noncommercial license](LICENSE.md) remain in place. This is an unofficial fork, not an upstream release.

## Install

See [PACKAGES.md](PACKAGES.md) for commands and platform requirements.

| Platform | Package |
| --- | --- |
| Windows x64 | `Capri-Local-Setup.exe`, standalone `Capri-Local.exe`, and Windows ZIP |
| Debian/Ubuntu x64 | `.deb` |
| Arch x64 | `.pkg.tar.zst` |
| Other Linux x64 | Portable `.tar.gz` with user-level installer |
| Mac Apple Silicon / Intel | Separate `.dmg` installers and app ZIPs; macOS 15+ |

## Changes in this update

- Merged upstream history from the previous 0.7.8 fork through 0.8.3, including 0.7.9, 0.7.10 and 0.8.0–0.8.2.
- Imported the new Windows call mode and microphone routing, interruption handling, call apps, smoother Settings, live reactions, mods, provider controls and Code/Husk Mode.
- Imported the new Mac/Linux Mainframe, mods, provider keys, hooks and desktop improvements. System voice has its own selectable row in that Mainframe, alongside custom local and cloud speech.
- Kept System voice, Pocket TTS launchers, direct Pocket model files, legacy settings migration and the existing Capri-Local data folders.
- Preserved the fork's update links, separate installer identity, CPU-only brain marker and local release version comparisons.
- Updated native builds, package versions and platform smoke checks. The Windows ZIP excludes smoke-test recordings, screenshots and settings.

The separate Quest project is imported as upstream source under `quest/`. This desktop release does not build or bundle a Quest APK. Upstream Quest downloads remain at https://github.com/lachydotmcg/capri-desktop/releases .

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

See [UPSTREAM.md](UPSTREAM.md) for the imported versions, source commit and fork-specific adaptations.

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
