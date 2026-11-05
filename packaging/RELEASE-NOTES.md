# Capri Local 0.8.3

Updated from the original Capri repo through **v0.8.3**, including all intervening releases since the previous 0.7.8 fork. This is a full release with newly compiled packages.

## Changes

- Imports 0.8.0–0.8.3 call mode, microphone routing, smooth settings, live reactions, mods, provider controls and Code/Husk Mode.
- Imports the new Mac/Linux Mainframe, mods, providers and desktop improvements; System voice remains selectable in the new Mainframe.
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

Quest source is included in the repository. A separate Quest APK is not part of these desktop packages.

Original project: https://github.com/lachydotmcg/capri-desktop

Updated fork: https://github.com/ipushthecarts/capri-desktop

Original copyright, art credits and PolyForm Noncommercial license remain applicable.
