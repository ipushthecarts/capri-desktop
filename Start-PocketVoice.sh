#!/usr/bin/env bash
set -euo pipefail
base_dir=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
if [[ $(uname -s) == Darwin ]]; then
  voice_data="$HOME/Library/Application Support/Capri-Local"
else
  voice_data="${XDG_DATA_HOME:-$HOME/.local/share}/capri-local-voice"
fi
if [[ ! -x "$voice_data/venv/bin/python" ]]; then
  echo "Run Setup-PocketVoice.sh first." >&2
  exit 1
fi
exec "$voice_data/venv/bin/python" "$base_dir/local-voice/server.py" "$@"
