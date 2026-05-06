## Антистресс Вайб (Unity / Android)

A relaxing sandbox MVP: interact with a soft “slime” surface via touch (tap/drag), change its color, reset, and hear subtle audio feedback.

### What’s included
- **One-scene MVP** built entirely from code (no manual scene setup required)
- **Soft-body-ish mesh deformation** (position-based + spring return) optimized for mobile
- **Touch input**: tap = dent, drag = pull/stretch
- **Minimal UI**: RGB sliders + Reset button
- **Basic sound feedback**: one-shot on interaction with simple rate limiting
- **Clean architecture**: input, simulation, rendering, UI separated

### Unity version
Target **Unity 2022.3 LTS** (or newer LTS).

### How to run
1. Open Unity Hub → **Open** → select `AntiStressLab/`
2. Open scene `Assets/Scenes/Main.unity`
3. Press Play

### Android build (APK / AAB)
1. File → Build Settings → **Android** → Switch Platform
2. Player Settings:
   - **Company Name**: your studio
   - **Product Name**: Антистресс Вайб
   - **Package Name**: `ru.radikblack.antistressvibe`
   - **Minimum API Level**: Android 7.0 (API 24) or higher
   - **Scripting Backend**: IL2CPP
   - **Target Architectures**: ARM64 (required for Play)
   - **Active Input Handling**: Both (we use classic Touch API)
3. Build → select output location → generate APK (or AAB for Play Store)

### Privacy Policy (for stores)
- GitHub Pages (recommended): publish `AntiStressLab/docs/privacy-policy.md` and use its URL in the store console.

### Performance notes
- Default mesh is a moderate grid; tune resolution in `SlimeSettings`.
- The deformation runs on the main thread but is lightweight; keep VSync off on mobile and cap at 60.

