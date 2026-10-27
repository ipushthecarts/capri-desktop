Local voice builds for Windows, Linux and macOS. No ElevenLabs API key or credits required.

- Windows x64: standalone EXE and ZIP with optional Pocket TTS setup scripts.
- Debian/Ubuntu x64: `.deb` package.
- Arch x64: `.pkg.tar.zst` package.
- Linux x64: portable tarball with a user-level installer.
- Mac Apple Silicon and Intel: separate app ZIPs and DMG installers, macOS 15+.

Default speech uses Windows voices, Linux espeak-ng, or macOS say. Pocket TTS is available on every platform for custom reference voices and requires a separate Python/model setup. Linux/Mac support typed questions with spoken replies; microphone chat remains Windows-only.

Read PACKAGES.md in the repository for installation instructions. SHA256SUMS.txt covers the downloads. These preview binaries are not vendor-signed or Apple-notarized; Mac apps use ad-hoc signatures. Desktop smoke tests do not establish voice quality, audio-device behavior, or performance on every target machine.

Original Capri project: https://github.com/lachydotmcg/capri-desktop
Modified source and instructions: https://github.com/ipushthecarts/capri-desktop

Original copyright and PolyForm Noncommercial license remain applicable.
