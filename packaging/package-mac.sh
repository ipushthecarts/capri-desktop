#!/usr/bin/env bash
set -euo pipefail
arch="${1:?Pass arm64 or x64}"
app="$PWD/stage/mac-$arch/Capri Local.app"
mkdir -p "$app/Contents/MacOS" "$app/Contents/Resources" release
cp -a "out/osx-$arch/." "$app/Contents/MacOS/"
sed 's/@VERSION@/0.6.2.1/g' cross/mac/Info.plist > "$app/Contents/Info.plist"
mkdir -p stage/Capri.iconset
for s in 16 32 128 256 512; do
  sips -z "$s" "$s" cross/icon.png --out "stage/Capri.iconset/icon_${s}x${s}.png" >/dev/null
  d=$((s * 2)); sips -z "$d" "$d" cross/icon.png --out "stage/Capri.iconset/icon_${s}x${s}@2x.png" >/dev/null
done
iconutil -c icns stage/Capri.iconset -o "$app/Contents/Resources/Capri.icns"
cp Setup-PocketVoice.sh Start-PocketVoice.sh "$app/Contents/Resources/"
cp -a local-voice "$app/Contents/Resources/"
find "$app" -name __pycache__ -type d -prune -exec rm -r {} +
cp PACKAGES.md README.md LICENSE.md assets/fonts/OFL-ShareTechMono.txt "$app/Contents/Resources/"
chmod +x "$app/Contents/MacOS/Capri" "$app/Contents/Resources/"*.sh
codesign --force --deep --sign - "$app"
codesign --verify --deep --strict "$app"
ln -s /Applications "stage/mac-$arch/Applications"
hdiutil create -volname 'Capri Local' -srcfolder "stage/mac-$arch" -ov -format UDZO "release/Capri-Local-mac-$arch.dmg"
ditto -c -k --keepParent "$app" "release/Capri-Local-mac-$arch.zip"
