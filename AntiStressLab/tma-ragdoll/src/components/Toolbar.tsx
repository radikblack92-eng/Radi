import { PhotoLoader } from './PhotoLoader';

type ToolbarProps = {
  hasPhoto: boolean;
  isTelegram: boolean;
  onPhotoSelected: (file: File | null) => void;
  onReset: () => void;
  onShare: () => void;
};

export function Toolbar({
  hasPhoto,
  isTelegram,
  onPhotoSelected,
  onReset,
  onShare,
}: ToolbarProps) {
  return (
    <div className="pointer-events-none absolute inset-x-0 bottom-0 z-20 flex flex-col gap-3 px-4 pb-[calc(env(safe-area-inset-bottom)+1rem)]">
      <div className="rounded-3xl border border-white/10 bg-black/25 p-3 shadow-2xl backdrop-blur-xl">
        <div className="pointer-events-auto flex gap-2">
          <PhotoLoader hasPhoto={hasPhoto} onPhotoSelected={onPhotoSelected} />
          <button
            className="min-h-12 rounded-2xl bg-white/10 px-4 text-sm font-semibold text-white shadow-lg ring-1 ring-white/15 active:scale-[0.98]"
            type="button"
            onClick={onReset}
          >
            Сброс
          </button>
        </div>
        <button
          className="pointer-events-auto mt-2 min-h-12 w-full rounded-2xl px-4 text-sm font-bold text-[var(--tg-theme-button-text-color,#fff)] shadow-lg active:scale-[0.99]"
          style={{ background: 'var(--tg-theme-button-color, #2ea6ff)' }}
          type="button"
          onClick={onShare}
        >
          {isTelegram ? 'Отправить результат' : 'Подготовить результат'}
        </button>
      </div>
      <p className="mx-auto max-w-[22rem] text-center text-xs leading-snug text-white/70 drop-shadow">
        Потяните манекен за голову, руки или ноги и отпустите, чтобы бросить его.
      </p>
    </div>
  );
}
