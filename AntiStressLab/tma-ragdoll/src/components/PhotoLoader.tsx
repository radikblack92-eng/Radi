import type { ChangeEvent } from 'react';

type PhotoLoaderProps = {
  onPhotoSelected: (file: File | null) => void;
  hasPhoto: boolean;
};

export function PhotoLoader({ onPhotoSelected, hasPhoto }: PhotoLoaderProps) {
  const handleChange = (event: ChangeEvent<HTMLInputElement>) => {
    onPhotoSelected(event.target.files?.[0] ?? null);
    event.target.value = '';
  };

  return (
    <label className="flex min-h-12 flex-1 cursor-pointer items-center justify-center rounded-2xl bg-white/10 px-4 text-sm font-semibold text-white shadow-lg ring-1 ring-white/15 backdrop-blur active:scale-[0.98]">
      <input
        accept="image/*"
        className="sr-only"
        type="file"
        onChange={handleChange}
      />
      {hasPhoto ? 'Заменить лицо' : 'Загрузить лицо'}
    </label>
  );
}
