import type { CSSProperties } from 'react';
import { lazy, Suspense, useMemo, useRef, useState } from 'react';

import { Toolbar } from './components/Toolbar';
import { useFaceTexture } from './hooks/useFaceTexture';
import { useTelegramMiniApp } from './hooks/useTelegramMiniApp';
import {
  captureScenePreview,
  sendSandboxResult,
  submitSandboxResultToBackend,
  type SandboxResultPayload,
} from './lib/telegram';
import { withAlpha } from './lib/colors';

const RagdollScene = lazy(() =>
  import('./components/RagdollScene').then((module) => ({ default: module.RagdollScene })),
);

function App() {
  const { isTelegram, palette, queryId, rawInitData, user } = useTelegramMiniApp();
  const [photoFile, setPhotoFile] = useState<File | null>(null);
  const [resetKey, setResetKey] = useState(0);
  const [status, setStatus] = useState('Готово к броскам');
  const canvasRef = useRef<HTMLCanvasElement | null>(null);
  const faceTexture = useFaceTexture(photoFile);

  const cssVars = useMemo(
    () =>
      ({
        '--tg-theme-bg-color': palette.bg,
        '--tg-theme-secondary-bg-color': palette.secondaryBg,
        '--tg-theme-text-color': palette.text,
        '--tg-theme-hint-color': palette.hint,
        '--tg-theme-button-color': palette.button,
        '--tg-theme-button-text-color': palette.buttonText,
      }) as CSSProperties,
    [palette],
  );

  const handleShare = async () => {
    const preview = captureScenePreview(canvasRef.current);
    const payload: SandboxResultPayload = {
      createdAt: new Date().toISOString(),
      faceTextureLoaded: Boolean(faceTexture),
      queryId,
      type: 'ragdoll_result',
      userId: user?.id,
    };

    setStatus('Готовим результат...');

    try {
      const backendResult = await submitSandboxResultToBackend({
        initDataRaw: rawInitData,
        payload,
        previewDataUrl: preview,
      });
      const compactPayload: SandboxResultPayload = {
        ...payload,
        resultId: backendResult?.resultId,
        shareUrl: backendResult?.shareUrl,
      };
      const sent = sendSandboxResult(compactPayload);

      if (backendResult?.ok) {
        setStatus(sent ? 'Результат отправлен и сохранен' : 'Результат сохранен на backend');
        return;
      }

      if (backendResult && !backendResult.ok) {
        setStatus(backendResult.error ?? 'Backend не принял результат');
        return;
      }

      setStatus(
        sent
          ? 'Результат отправлен через Telegram WebApp API'
          : preview
            ? 'Превью подготовлено: настройте VITE_RESULT_API_URL'
            : 'Canvas еще не готов для превью',
      );
    } catch (error) {
      console.warn('Unable to submit sandbox result:', error);
      setStatus('Не удалось отправить результат на backend');
    }
  };

  return (
    <main
      className="relative h-dvh w-dvw overflow-hidden"
      style={{
        ...cssVars,
        background: `radial-gradient(circle at 50% 18%, ${withAlpha(
          palette.button,
          0.22,
        )}, transparent 34%), ${palette.bg}`,
        color: palette.text,
      }}
    >
      <Suspense
        fallback={
          <div className="absolute inset-0 grid place-items-center text-sm font-semibold text-white/75">
            Загрузка 3D-сцены...
          </div>
        }
      >
        <RagdollScene
          faceTexture={faceTexture}
          palette={palette}
          resetKey={resetKey}
          onCanvasReady={(canvas) => {
            canvasRef.current = canvas;
          }}
        />
      </Suspense>

      <div className="pointer-events-none absolute inset-x-0 top-0 z-20 px-4 pt-[calc(env(safe-area-inset-top)+0.75rem)]">
        <div className="mx-auto flex max-w-md items-center justify-between rounded-3xl border border-white/10 bg-black/20 px-4 py-3 shadow-xl backdrop-blur-xl">
          <div>
            <p className="text-sm font-bold text-white">Ragdoll Sandbox</p>
            <p className="text-xs text-white/65">
              {user?.first_name ? `Игрок: ${user.first_name}` : 'Telegram Mini App MVP'}
            </p>
          </div>
          <div className="rounded-full bg-white/10 px-3 py-1 text-xs font-semibold text-white/80">
            {isTelegram ? 'TMA' : 'Web'}
          </div>
        </div>
      </div>

      <div className="pointer-events-none absolute left-1/2 top-[5.6rem] z-20 -translate-x-1/2 rounded-full bg-black/25 px-3 py-1 text-center text-xs text-white/75 backdrop-blur">
        {status}
      </div>

      <Toolbar
        hasPhoto={Boolean(faceTexture)}
        isTelegram={isTelegram}
        onPhotoSelected={(file) => {
          setPhotoFile(file);
          setStatus(file ? 'Фото загружается на лицо манекена' : 'Фото сброшено');
        }}
        onReset={() => {
          setResetKey((value) => value + 1);
          setStatus('Поза сброшена');
        }}
        onShare={handleShare}
      />
    </main>
  );
}

export default App;
