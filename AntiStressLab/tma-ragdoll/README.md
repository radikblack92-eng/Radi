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
