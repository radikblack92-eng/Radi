# TMA Ragdoll Sandbox MVP

React/Vite Telegram Mini App prototype for a mobile 3D ragdoll sandbox.

## Stack

- React + Vite + TypeScript
- React Three Fiber
- `@react-three/cannon` physics
- `@telegram-apps/sdk-react` for Telegram launch/user/theme data
- Tailwind CSS v4 for the bottom overlay UI

## Run

```bash
npm install
npm run dev
npm run build
```

Copy `.env.example` to `.env.local` for local frontend settings. Keep real bot tokens only in
server-side environment variables.

The default public bot entrypoint is configured as `VITE_TELEGRAM_BOT_URL=https://t.me/rooommerbot`.

## Deploy

The repository includes a GitHub Pages workflow at
`.github/workflows/tma-ragdoll-pages.yml`. After this branch is merged into `main`, run the workflow
manually or push to `main`; it builds the app with:

```bash
VITE_BASE_PATH=/Radi/
VITE_TELEGRAM_BOT_URL=https://t.me/rooommerbot
```

Expected HTTPS Mini App URL:

```text
https://radikblack92-eng.github.io/Radi/
```

GitHub Pages must be enabled for the repository with **Source: GitHub Actions** in repository
settings.

## Connect the Telegram bot

Do not paste bot tokens into source code or shell commands that are saved in logs. Put the token in
your local/server environment and configure the bot menu button with:

```bash
cd AntiStressLab/tma-ragdoll
TELEGRAM_BOT_TOKEN=... WEB_APP_URL=https://radikblack92-eng.github.io/Radi/ npm run bot:configure
```

The script calls Telegram Bot API methods `setChatMenuButton`, `setMyCommands`, and
`setMyShortDescription`. If BotFather asks for a Web App URL, use the same HTTPS URL above.

## Architecture

- `src/hooks/useTelegramMiniApp.ts` initializes Telegram WebApp SDK, restores init data, binds theme CSS variables, and exposes user/theme state.
- `src/components/RagdollScene.tsx` owns the mobile-friendly Canvas and physics world.
- `src/components/Ragdoll.tsx` defines a low-poly procedural mannequin with constrained physics bodies.
- `src/hooks/usePointerDragControls.ts` implements touch-first grabbing/flinging with Pointer Events.
- `src/components/PhotoLoader.tsx` loads a phone gallery image; `useFaceTexture` maps it onto the head face with `TextureLoader`.
- `src/lib/telegram.ts` contains `sendData` and backend submission integration points for sending result metadata to the bot.

## Mobile GPU notes

- Canvas uses DPR `[1, 1.5]`, no antialiasing, no shadows, no postprocessing.
- The mannequin is procedural low-poly geometry to keep the first bundle tiny.
- Production character art should be delivered as external compressed `.glb` assets under `public/models/` and lazy-loaded by route/component. Export with Draco or Meshopt compression and keep colliders procedural, so physics does not depend on render mesh complexity.

## Sending the result to chat

`sendSandboxResult` sends compact JSON via Telegram `sendData`. If `VITE_RESULT_API_URL` is set,
the frontend first posts `{ initDataRaw, payload, previewDataUrl }` to your backend and then sends
only a compact result reference back to Telegram.

Never put a bot token in the Vite frontend. Use `TELEGRAM_BOT_TOKEN` only on the server:

```bash
cp .env.example .env.local
TELEGRAM_BOT_TOKEN=... PUBLIC_RESULT_BASE_URL=https://your-domain.example/results npm run server:result
```

`server/telegram-result-handler.mjs` is a minimal Node reference handler for
`POST /api/tma/ragdoll-result`. In production, add persistent storage for `previewDataUrl`, verify
`initDataRaw` with your bot token before trusting user identity, and deploy the handler behind HTTPS.
