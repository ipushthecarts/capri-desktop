# Capri Local Voice

A personal-use modification of Capri for Windows that speaks without ElevenLabs keys, credits, or a paid speech subscription.

**Original project / main repository:** https://github.com/lachydotmcg/capri-desktop

**Pocket TTS:** https://github.com/kyutai-labs/pocket-tts

This is an unofficial modification, not an upstream Capri release. Original Capri code, art, and required copyright notices remain credited to Lachy. The original PolyForm Noncommercial license remains in [LICENSE.md](LICENSE.md). This change does not relicense Capri as MIT or Apache.

## Quick start: no Python needed

1. Extract the complete source ZIP on Windows 10/11, or clone this repository.
2. Double-click `Build-Local.cmd`. It uses the .NET Framework compiler included with normal Windows 10/11 installations; no Visual Studio project is required. If Windows PowerShell blocks the script, review `Build-Local.ps1` and unblock the downloaded files through their Properties dialog.
3. Open `dist/Capri-Local.exe`.
4. Set up Capri's chat model as usual, or use an existing Ollama instance.
5. Under **VOICE**, open **Configure local voice**, select a Windows voice, save, then press **Test**. Leave **Speak replies** checked.

A compiled `Capri-Local.exe` may also be included in the accompanying portable ZIP. Windows voices work without the Python server. They are basic system voices; a custom character voice requires Pocket TTS.

## Custom voice with Pocket TTS

1. Install 64-bit Python 3.12 with its Python launcher: https://www.python.org/downloads/windows/
2. From the same folder as `Capri-Local.exe`, run `Setup-PocketVoice.cmd` once. It creates a private Python environment and installs CPU PyTorch plus Pocket TTS 4.0.1.
3. Run `Start-PocketVoice.cmd` and leave its window open.
4. In Capri, select **Pocket TTS** under **Configure local voice**.
5. Start with the preset `alba`, or browse to a clean local WAV recording of the voice you want. MP3, FLAC, and exported Pocket TTS safetensors voices are also accepted.
6. Save and press **Test**. The first request downloads and loads the model and selected preset. First-time downloads may exceed the client's two-minute timeout; leave the server open, wait for it to finish, and try again.

The model and selected voice state stay in memory between replies. The server runs on CPU with two PyTorch threads by default, leaving room for the rest of your desktop. Exact latency on an X1 Carbon Gen 9 has not been measured. Running a chat model on the same CPU will affect responsiveness.

To change the port or thread count, run from a terminal:

```bat
Start-PocketVoice.cmd --port 8766 --threads 2
```

Choose the same port in Capri. The default is 8765.

You can create a reference sample separately with Qwen3-TTS VoiceDesign and choose that sample in Capri. This project does not bundle Qwen, provide an ElevenLabs voice-ID importer, or guarantee an exact match to the original character's voice. Pocket TTS synthesizes new speech from the reference; it does not play a fixed collection of recordings.

## What changed

| Area | Change |
| --- | --- |
| Speech output | Windows offline speech is the default; optional Pocket TTS supports custom reference voices. |
| Voice settings | Replaced the visible ElevenLabs key, voice ID, and model controls with a local voice configuration dialog. Provider, Windows voice, reference path, and port are saved. |
| Voice chat | The wave button uses the existing local chat/recognition pipeline and now speaks replies through the selected local engine. No ElevenLabs conversational agent is created. |
| Microphone | Uses Windows' installed offline recognition language packs in this local build; online Windows dictation is bypassed. Missing recognizers produce a setup message. |
| Animation | Generated speech is normalized to mono PCM16 WAV and sent through Capri's existing playback and mouth animation code. |
| Failure behavior | Failed speech leaves the text response visible. The microphone is gated while generating and playing speech to reduce feedback. There is no paid speech fallback. |
| Settings | Uses `%LOCALAPPDATA%\Capri-Local` so it does not overwrite a normal Capri installation's preferences. Close normal Capri before opening this build; it keeps upstream's single-instance identity. |
| Updates | Upstream automatic updates are disabled for the local version, preventing replacement by an unmodified ElevenLabs build. |
| Packaging | Added Windows build scripts and separate, optional CPU Pocket TTS setup/start launchers. |

## How it works

`app/LocalVoice.cs` implements the local synthesis providers and settings dialog. Windows speech is generated directly through `System.Speech.Synthesis`. Pocket speech uses a JSON request to `http://127.0.0.1:8765/v1/audio/speech` and receives a WAV file.

`local-voice/server.py` adapts the Pocket TTS Python API to that endpoint. It loads the model once, caches the most recently selected voice, serializes model use, converts floating-point samples to PCM16, and returns audio. The server binds only to loopback, accepts preset names or local files, rejects web-browser-originated requests, and makes no ElevenLabs calls. The desktop client refuses HTTP redirects. Initial dependency/model/preset downloads require internet; subsequent inference uses local model files. Other programs running on your machine can access this local server.

`app/Capri.cs` selects the local synthesizer for typed and microphone replies and bypasses the paid agent path. `app/Mainframe.cs` presents the local configuration. `app/Ears.cs` explains offline recognition setup. `LOCAL_BUILD` excludes the Windows online-dictation implementation from the Windows build, avoiding a Windows SDK/WinRT dependency.

Upstream ElevenLabs classes remain in the source for reference and its old smoke tests; they are not used by the normal local voice flow. The shared `Voice` class still supplies audio encoding, lip-sync analysis, and playback. Cloud chat providers remain optional upstream features: select Ollama or the built-in local chat model if you also want chat inference on your PC.

## Validation and limitations

See [LOCAL-VOICE.md](LOCAL-VOICE.md) for the validation record and Windows acceptance checklist. The Python adapter has automated HTTP and audio-conversion tests with mocked model inference. A successful Windows-target compilation does not establish that audio playback, microphone recognition, custom voice quality, or performance work on your particular laptop. The full Windows UI/audio flow needs a Windows machine for final testing.

## Development

```bat
py -3.12 -m unittest discover -s tests -v
```

The server's protocol tests need only Python's standard library. Its model-adapter tests additionally need NumPy, which the Pocket TTS setup installs.

Build on Windows with `Build-Local.cmd`. The accompanying source ZIP contains the modified sources and scripts; downloaded models and the Python environment are deliberately excluded. Do not commit personal recordings, API keys, or preferences.

Based on upstream commit `1165ff963e230d55704699894db8becd641f5e63`.
