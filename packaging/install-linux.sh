#!/usr/bin/env bash
set -euo pipefail
base_dir=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
data_dir="${XDG_DATA_HOME:-$HOME/.local/share}/capri-local"
app_dir="${XDG_DATA_HOME:-$HOME/.local/share}/applications"
mkdir -p "$data_dir" "$app_dir"
if [[ "$base_dir" != "$data_dir" ]]; then cp -a "$base_dir/." "$data_dir/"; fi
chmod +x "$data_dir/Capri" "$data_dir/Setup-PocketVoice.sh" "$data_dir/Start-PocketVoice.sh"
# Desktop Entry Exec quoting is not shell quoting. Escape its special characters.
python3 - "$data_dir" "$app_dir/capri-local.desktop" <<'PY'
import pathlib,sys
folder=pathlib.Path(sys.argv[1])
def quote(s):
    return '"'+s.replace('\\','\\\\').replace('"','\\"').replace('`','\\`').replace('$','\\$').replace('%','%%')+'"'
pathlib.Path(sys.argv[2]).write_text('[Desktop Entry]\nName=Capri Local\nExec='+quote(str(folder/'Capri'))+'\nIcon='+str(folder/'capri.png')+'\nTerminal=false\nType=Application\nCategories=Utility;\n')
PY
echo "Installed to $data_dir. Open Capri Local from your applications menu."
echo "For system speech, install espeak-ng. For Pocket playback, install paplay or pw-play."
