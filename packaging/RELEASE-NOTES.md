# Capri Local 0.7.8

Updated from the original Capri repo through **v0.7.8**, including the released updates from 0.7.0 onward. This is a full release with newly compiled packages.

## Changes

- Imports the newer Mainframe, voice providers/effects, local models, memory, games/streaming, cursor/jobs, keys and platform improvements.
- Keeps speech available with no required ElevenLabs key or credits. System speech uses Windows voices, Linux espeak-ng, or macOS say.
- Integrates the bundled Pocket TTS server with the current custom-local/provider controls.
- Migrates the previous fork's system/Pocket settings, reference path, port and mute state once, and keeps the existing fork data directory.
- Keeps optional upstream prebuilt/HD/cloud choices; cloud services have their own accounts and limits.
- Prevents upstream automatic Windows installs from replacing the fork.

## Fresh installers

- Windows: `Capri-Local-Setup.exe`, standalone EXE and ZIP.
- Debian/Ubuntu x64: `.deb`.
- Arch x64: `.pkg.tar.zst`.
- Linux x64: portable tarball with user-level installer.
- macOS 15+: separate Apple Silicon and Intel DMGs/app ZIPs.

Checksums are in `SHA256SUMS.txt`. Read `PACKAGES.md` for installation and Pocket setup. Linux/Mac support typed questions and spoken replies; microphone chat remains Windows-only. Live audio devices, GPU/HD models, Pocket inference and target-machine performance are not established by build/smoke checks. Windows binaries are unsigned; Mac apps are ad-hoc signed, not Apple-notarized.

Original project: https://github.com/lachydotmcg/capri-desktop

Updated fork: https://github.com/ipushthecarts/capri-desktop

Original copyright, art credits and PolyForm Noncommercial license remain applicable.
