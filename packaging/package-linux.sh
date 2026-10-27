#!/usr/bin/env bash
set -euo pipefail
# Invoke from the repository root after dotnet publish has produced out/linux.
version="${1:-0.6.2+local.1}"
arch_version="${version/+/\.}"
root="$PWD/stage/linux"
mkdir -p "$root/opt/capri-local" "$root/usr/bin" "$root/usr/share/applications" "$root/usr/share/icons/hicolor/256x256/apps" "$root/usr/share/licenses/capri-local" release
cp -a out/linux/. "$root/opt/capri-local/"
cp Setup-PocketVoice.sh Start-PocketVoice.sh "$root/opt/capri-local/"
cp -a local-voice "$root/opt/capri-local/"
find "$root/opt/capri-local" -name __pycache__ -type d -prune -exec rm -r {} +
cp README.md LOCAL-VOICE.md PACKAGES.md LICENSE.md "$root/opt/capri-local/"
cp assets/fonts/OFL-ShareTechMono.txt "$root/usr/share/licenses/capri-local/"
cp LICENSE.md "$root/usr/share/licenses/capri-local/"
cp assets/chip.png "$root/opt/capri-local/capri.png"
cp assets/chip.png "$root/usr/share/icons/hicolor/256x256/apps/capri-local.png"
cp packaging/capri-local.desktop "$root/usr/share/applications/"
printf '#!/bin/sh\nexec /opt/capri-local/Capri "$@"\n' > "$root/usr/bin/capri-local"
chmod 755 "$root/usr/bin/capri-local" "$root/opt/capri-local/Capri" "$root/opt/capri-local/"*.sh
# A portable distribution with a user-level installer, with no sudo required.
cp packaging/install-linux.sh "$root/opt/capri-local/install.sh"
chmod 755 "$root/opt/capri-local/install.sh"
tar -czf release/Capri-Local-linux-x64.tar.gz -C "$root/opt" capri-local
# Debian/Ubuntu package.
mkdir -p "$root/DEBIAN"
cat > "$root/DEBIAN/control" <<EOF
Package: capri-local
Version: $version
Architecture: amd64
Maintainer: Capri Local <130241547+ipushthecarts@users.noreply.github.com>
Section: utils
Priority: optional
Depends: libc6 (>= 2.35), libstdc++6, libgcc-s1, libicu70 | libicu72 | libicu74 | libicu76 | libicu78, libssl3 | libssl3t64, zlib1g, libfontconfig1, libice6, libsm6, libx11-6, libxrandr2, libxi6, libxcursor1, libxrender1, libgl1, espeak-ng
Recommends: pulseaudio-utils
Suggests: python3-venv
Homepage: https://github.com/ipushthecarts/capri-desktop
Description: Capri desktop companion with local speech
 Local Windows/Pocket speech fork; this Linux package uses espeak-ng
 or an optional local Pocket TTS server. No ElevenLabs credits required.
EOF
dpkg-deb --root-owner-group --build "$root" "release/capri-local_${version}_amd64.deb"
# Arch package metadata is separate from Debian's control directory.
rm -r "$root/DEBIAN"
size=$(du -sb "$root" | cut -f1)
cat > "$root/.PKGINFO" <<EOF
pkgname = capri-local
pkgbase = capri-local
pkgver = $arch_version-1
pkgdesc = Capri desktop companion with local speech
url = https://github.com/ipushthecarts/capri-desktop
builddate = $(date +%s)
packager = Capri Local
size = $size
arch = x86_64
license = LicenseRef-PolyForm-Noncommercial-1.0.0
depend = glibc
depend = gcc-libs
depend = icu
depend = openssl
depend = zlib
depend = fontconfig
depend = libice
depend = libsm
depend = libx11
depend = libxrandr
depend = libxi
depend = libxcursor
depend = libxrender
depend = libglvnd
depend = espeak-ng
depend = libpulse
optdepend = python: optional Pocket TTS server
EOF
tar --zstd --owner=0 --group=0 -cf "release/capri-local-${arch_version}-1-x86_64.pkg.tar.zst" -C "$root" .PKGINFO opt usr
# Basic packaging assertions; desktop UI is tested separately under Xvfb.
dpkg-deb --info "release/capri-local_${version}_amd64.deb"
tar --zstd -tf "release/capri-local-${arch_version}-1-x86_64.pkg.tar.zst" | grep '^opt/capri-local/Capri$' >/dev/null
