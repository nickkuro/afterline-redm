# ⚠️ TEST BUILD ONLY. NOT AN OFFICIAL AFTERLINE RELEASE ⚠️

**This is an unofficial test build of Afterline with RedM support. It is only for a small group of friends helping to test it.**

- Shared with the permission of Afterline's author, **for testing only**.
- **Please don't re-upload it, post it publicly or pass it on.**
- **It may contain bugs.** Afterline's author is also working on big chat-capture changes, so this build will be replaced. Don't treat it as stable.
- For the official app, use the real Afterline releases at https://github.com/infamiera/afterline/releases.

## Before you run it

1. **Close the official Afterline first.** This build uses the same data folder (`%LocalAppData%\Afterline`) and the same chatlog folder. Back up `%LocalAppData%\Afterline` if you want to be safe.
2. **Say NO to any update prompt.** Accepting an update replaces this test build with the official Afterline, which doesn't have RedM support yet.
3. Download the `.exe` below and run it. Windows SmartScreen may warn about an unknown app. You can check the file against the `.sha256` checksum.

## What's being tested

- RedM is detected as well as FiveM, and the status shows which game is running.
- Chat is captured live from the default Cfx chat and other common chat layouts.
- Optional **Settings → Capture & processing → Custom chat selector** for servers whose chat isn't picked up.
- Screenshots (Ctrl+Shift+F12) work in RedM/RDR2 and are saved as `Afterline_RedM_*`.
- `[OOC]`, `(OOC)`, `OOC:` and `LOOC` lines count as out-of-character.
- FiveM should work exactly as before. Please tell me if it doesn't.

## Reporting problems

Message me (nickkuro / Awucard) with what happened. If chat isn't captured, include the end of `%LocalAppData%\Afterline\Logs\afterline.log`.
