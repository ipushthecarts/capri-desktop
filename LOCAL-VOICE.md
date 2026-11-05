# Local voice integration notes

This build is based on upstream Capri v0.8.3 plus the local fork changes described in README.md and UPSTREAM.md.

## Existing settings

Windows keeps `%LOCALAPPDATA%\Capri-Local\settings.json`. Mac/Linux keep the existing `Capri-Local/cross.json` under the OS application-data folder. Old files are read in place; nothing is deleted for this migration.

On the first upgraded launch, the previous system/Pocket choice is mapped to the new provider fields. A Pocket reference path, custom port and muted state are retained. A migration marker prevents a later launch from undoing newly selected settings. Records that already have a current custom server/provider are preserved.

Windows uses the current custom-local row and voice pipeline. Mac/Linux use the provider interface. Upstream prebuilt local, custom model/server, HD, voice style/layers and optional cloud choices remain available. Cloud voices remain optional and can use their providers' credits; the default system voice does not.

## Validation

The install-packages workflow builds the new Windows app and setup executable, self-contained Linux app, and Intel/Apple Silicon Mac apps. The release requires all platform jobs to succeed before publishing.

Checks include upstream desktop smoke tests, Windows installer-payload verification, legacy voice settings migration, the ten bundled Pocket-adapter tests, native OS voice synthesis into WAV without speaker playback, and Debian/Arch package installation. See the actual release workflow results for the completed checks.

These checks do not establish live speaker/microphone behavior, GPU/HD model compatibility, Pocket model quality, game support on every system, or latency on an X1 Gen 9. Model/dependency downloads need internet before subsequent local inference. Windows binaries are unsigned and Mac apps are ad-hoc signed, not Apple-notarized.

## Manual checks after installing

1. Confirm the version is 0.8.3 and old position/model/voice settings remain.
2. Test the system voice without a speech API key, including the new Mac/Linux Mainframe System voice row.
3. Start the Pocket service and test a preset and an existing local reference file.
4. Toggle speech off and on, including during a pending reply.
5. Stop the local service and confirm text replies remain available with an explanatory voice error.
6. Test Windows microphone chat, mute and hotkeys with the installed recognition model.
7. Check current voice effects, model selection and desktop behavior on the target system.

Original source and credit: https://github.com/lachydotmcg/capri-desktop

Fork releases: https://github.com/ipushthecarts/capri-desktop/releases

## 0.8 migration

The new call-mode, mic-routing and provider settings are retained from upstream. Local speech still uses the selected provider. Existing Pocket reference paths, ports and mute state are not reset by this update. The 0.8 cross-platform backup settings file is retained alongside the fork migration marker. CPU-only local-brain markers continue to use the existing engine folder.
