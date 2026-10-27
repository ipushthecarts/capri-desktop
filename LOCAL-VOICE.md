# Local voice notes

Main repository: https://github.com/lachydotmcg/capri-desktop

## Getting started

If this folder contains `Capri-Local.exe`, close the original Capri app and open `Capri-Local.exe`. Set up the chat model, then press **Test** under **VOICE**. Windows speech works without Python or a speech API key.

For a custom voice, install Python 3.12, run `Setup-PocketVoice.cmd` once, and run `Start-PocketVoice.cmd`. In Capri, open **Configure local voice**, select Pocket TTS, choose `alba` or browse to your reference audio, save, and test. Keep the server console open. The first request downloads model files; if the desktop times out while downloads continue, wait for them to finish and test again.

Reference audio is not the full voice model. The server needs its downloaded Pocket TTS weights as well. Model weights, Python dependencies, and personal recordings are not bundled in this download.

The wave button listens through an installed Windows offline speech recognizer and sends recognized words through Capri's normal chat flow. Install your speech language pack in Windows Settings if prompted. You can always type instead. Keep **Speak replies** checked to hear replies.

## Validation record

Prepared October 27, 2026, from upstream commit `1165ff963e230d55704699894db8becd641f5e63`.

- Windows x64 application compiled successfully against .NET Framework 4.8 reference assemblies with C# 5 compatibility.
- Ten Python tests passed: HTTP request validation, loopback/browser restrictions, local reference paths, WAV responses, inference errors, concurrency rejection, cached model/voice state, and float-to-PCM conversion. Model inference was mocked for these tests.
- Twelve C# transport/audio checks passed using the actual local request code and shared WAV helpers, including successful loopback speech requests, escaped Windows paths, malformed and empty WAV rejection, and server failure handling.
- `git diff --check` passed.

Not verified here: launching the Windows desktop UI, installed Windows voice synthesis/playback, microphone recognition, live Pocket TTS model downloads/inference, reference-voice quality, CPU usage, battery impact, or latency on an X1 Gen 9. The included Windows executable is a compiled preview, not a signed installer or a release tested on the target laptop. It requires 64-bit Windows and .NET Framework 4.8.

## Windows acceptance checklist

1. Open the app and confirm that VOICE shows **Configure local voice**, with no ElevenLabs key field.
2. Select a Windows voice, save, and press Test. Confirm audible speech and mouth animation.
3. Type a question and confirm the same engine speaks the reply.
4. Turn Speak replies off and confirm replies stay silent, including a speech request already in progress.
5. Start the Pocket server, select `alba`, and test after the initial download finishes.
6. Choose a local reference file, test, then restart Capri and check that the choice persists.
7. Stop the Pocket server and ask another question. Confirm that text still appears and VOICE explains the failure.
8. Use the wave button. Confirm microphone recognition, mute/unmute, and that Capri does not respond to her own voice. Offline recognition is less capable than modern neural transcription.
9. Stop voice chat while a reply is pending and confirm that late audio does not play.
10. Check the update button. This build should keep the local voice modification rather than install an upstream update.

The Python launcher is optional. If a Pocket download requires authentication or license acceptance, follow the message in its console; there is no ElevenLabs credential involved. Keep a clean reference recording and choose a voice whose use you have permission for.

## Development checks

Python:

```bat
py -3.12 -m unittest discover -s tests -v
```

Windows C# transport tests:

```powershell
.\Test-Local.ps1
```

The fork includes a Windows GitHub Actions workflow to build and run these checks after it is pushed. It has not been run on GitHub in this session. The original upstream smoke test includes legacy paid-voice expectations and is not the local build's acceptance suite.
