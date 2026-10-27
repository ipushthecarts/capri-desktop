# Downloads and installation

Releases: https://github.com/ipushthecarts/capri-desktop/releases

Original project: https://github.com/lachydotmcg/capri-desktop

These builds use local speech without ElevenLabs credits. Pocket TTS is optional; its Python environment and downloaded weights are installed separately.

| System | Download | Installation |
| --- | --- | --- |
| Windows 10/11, x64 | `Capri-Local.exe` or `Capri-Local-Windows.zip` | Open the EXE; use the ZIP for the optional Pocket setup scripts. Requires .NET Framework 4.8. |
| Debian/Ubuntu, x64 | `capri-local_0.6.2+local.1_amd64.deb` | `sudo apt install ./capri-local_0.6.2+local.1_amd64.deb` |
| Arch, x64 | `capri-local-0.6.2.local.1-1-x86_64.pkg.tar.zst` | `sudo pacman -U ./capri-local-0.6.2.local.1-1-x86_64.pkg.tar.zst` |
| Other Linux, x64 | `Capri-Local-linux-x64.tar.gz` | Extract, then run `bash capri-local/install.sh`, or run `capri-local/Capri` directly. |
| Mac, Apple Silicon | `Capri-Local-mac-arm64.dmg` | Open the disk image and drag Capri Local into Applications. |
| Mac, Intel | `Capri-Local-mac-x64.dmg` | Open the disk image and drag Capri Local into Applications. |

Linux and Mac packages bundle the .NET runtime. Mac builds require macOS 15 or newer, matching the upstream app. Linux packages target Debian 12+, Ubuntu 22.04+, and current Arch. Other distributions need compatible system libraries. These distribution targets are not a claim of testing on every release.

## Local speech

- Windows: select **Configure local voice** in the Mainframe. Installed Windows voices work immediately.
- Linux: open **Local voice settings** in the tray/right-click menu, or click the wave button. The system voice uses `espeak-ng`, installed by the Debian/Arch packages.
- Mac: use the same local voice settings. The system voice uses macOS `say`.
- All three: select Pocket TTS for reference-based custom voices. Use a preset such as `alba`, or supply the full local path to a reference sample.

Linux/Mac support typed questions and spoken replies. Microphone chat remains Windows-only. Wayland limits desktop positioning/window-perching features; an X11 session provides the fuller Linux desktop behavior. No voice quality or latency guarantee is made for an X1 Gen 9.

## Optional Pocket server

Windows: run `Setup-PocketVoice.cmd`, then `Start-PocketVoice.cmd` in the extracted Windows ZIP.

Debian/Arch packages:

```sh
bash /opt/capri-local/Setup-PocketVoice.sh
bash /opt/capri-local/Start-PocketVoice.sh
```

Debian/Ubuntu may need `sudo apt install python3-venv pulseaudio-utils`. Arch needs Python and `libpulse`. The supported Python range is 3.10-3.14. Use `CAPRI_PYTHON=python3.12` before the setup command if your default Python is newer. For example:

```sh
CAPRI_PYTHON=python3.12 bash /opt/capri-local/Setup-PocketVoice.sh
```

Mac, after moving the app into Applications:

```sh
bash "/Applications/Capri Local.app/Contents/Resources/Setup-PocketVoice.sh"
bash "/Applications/Capri Local.app/Contents/Resources/Start-PocketVoice.sh"
```

Use a supported Python installation; the environment is stored under your user account, outside the signed app bundle. Initial dependency/model/preset downloads require internet. Keep the server open while using Pocket TTS. The default port is 8765.

## Build and signing notes

The release workflow builds Windows on Windows, Linux on Ubuntu 22.04, and Mac on a macOS runner. It runs protocol tests, package checks, and available desktop smoke tests. Linux package installation on Arch/Debian, live audio devices, microphone input and custom-voice quality still need testing on the target systems.

The Windows executable is unsigned. Mac apps are ad-hoc signed for local integrity and are not Apple-notarized. They are not presented as certified or vendor-signed software. SHA-256 checksums accompany release downloads.
