#!/usr/bin/env bash
set -euo pipefail
base_dir=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
if [[ $(uname -s) == Darwin ]]; then
  voice_data="$HOME/Library/Application Support/Capri-Local"
else
  voice_data="${XDG_DATA_HOME:-$HOME/.local/share}/capri-local-voice"
fi
python_bin="${CAPRI_PYTHON:-python3}"
"$python_bin" -c 'import sys; assert (3,10) <= sys.version_info[:2] < (3,15), "Pocket TTS needs Python 3.10 through 3.14"'
mkdir -p "$voice_data"
"$python_bin" -m venv "$voice_data/venv"
"$voice_data/venv/bin/python" -m pip install --upgrade pip
if [[ $(uname -s) == Linux ]]; then
  "$voice_data/venv/bin/python" -m pip install torch --index-url https://download.pytorch.org/whl/cpu
fi
"$voice_data/venv/bin/python" -m pip install -r "$base_dir/local-voice/requirements.txt"
echo "Pocket TTS installed. Run Start-PocketVoice.sh and select Pocket TTS in Local voice settings."
