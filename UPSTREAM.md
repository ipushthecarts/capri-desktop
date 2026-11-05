# Upstream integration

Source: https://github.com/lachydotmcg/capri-desktop

Imported release: **v0.8.3**, commit `82a5c6b8fa2a873a4d04f21131fcd76c25620dd9`.

This update merges upstream history into the existing fork. It includes the released tags after the fork's 0.6.2 base:

| Release | Included changes |
| --- | --- |
| 0.7.0 | Local voice/ears, cursor and desktop jobs, model tiers, memory, expanded Mainframe and platform features |
| 0.7.1 | Voice-off mode, mic mute fixes and job/voice corrections |
| 0.7.2 | Windows 10 local-ears archive extraction fix |
| 0.7.3 | Startup/hotkeys, check-ins, job fixes and personality corrections |
| 0.7.4 | Configurable keys, Discord status, task panel and local-ears fixes |
| 0.7.6 | Prebuilt/custom/cloud voice rows, local model/server support, HD voice, voice effects/layers, security settings and multilingual improvements |
| 0.7.7 | Mainframe redesign/scaling, animation/moves and game/job fixes |
| 0.7.8 | Streaming/game improvements, clips, Listen for Capri, additional model providers, memory/options, local voice servers and Hyprland/Linux control improvements |

| 0.7.9–0.7.10 | Check-in speech, taskbar/full-screen behavior, mic hotkeys, language and game-comment corrections |
| 0.8.0 | Throwing, voice-in-mic routing, call foundations, live reactions, editable personality, timers, window/audio tools and local-brain context fixes |
| 0.8.1 | Unified tools and spoken settings controls, audio-device handling, lag diagnostics and Linux desktop corrections |
| 0.8.2 | Smooth scrolling, call interruption handling, voice continuity, language layout, capture settings and Linux cursor fixes |
| 0.8.3 | Expanded call apps and panels, mods, Code/Husk Mode, cross-platform Mainframe, OpenAI-compatible providers and separate Quest project |

No v0.7.5 release appears in upstream's published release list; its history is included wherever it is an ancestor of v0.8.3.

The previous fork's static `LocalVoice` class conflicted with upstream's newer `LocalVoice` engine. It has been retired. `SystemVoice` now supplies OS synthesis through the current pipeline, while Pocket TTS uses the current custom-local/server interface. The old parallel cross-platform speech player and narrow transport test were retired in favor of the shared upstream audio path, upstream smoke tests, settings migration checks and the Python adapter tests.

The bundled Pocket CPU service and launchers remain. Old preferences are read from the existing fork data folders and moved to the new provider fields once, without deleting their files. The installer uses the fork's `Capri-Local` installation directory.

Upstream credits, licence, contribution notes and security policy are retained. This fork does not change its artwork or claim endorsement by the original author or the character's rights holders.
