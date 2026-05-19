export type TelegramPalette = {
  bg: `#${string}`;
  secondaryBg: `#${string}`;
  text: `#${string}`;
  hint: `#${string}`;
  button: `#${string}`;
  buttonText: `#${string}`;
};

export const fallbackPalette: TelegramPalette = {
  bg: '#0f172a',
  secondaryBg: '#172033',
  text: '#f8fafc',
  hint: '#94a3b8',
  button: '#2ea6ff',
  buttonText: '#ffffff',
};

export function withAlpha(hex: string, alpha: number): string {
  const normalized = hex.replace('#', '');

  if (normalized.length !== 6) {
    return `rgba(15, 23, 42, ${alpha})`;
  }

  const red = parseInt(normalized.slice(0, 2), 16);
  const green = parseInt(normalized.slice(2, 4), 16);
  const blue = parseInt(normalized.slice(4, 6), 16);

  return `rgba(${red}, ${green}, ${blue}, ${alpha})`;
}

export function softenHex(hex: string, amount = 0.16): `#${string}` {
  const normalized = hex.replace('#', '');

  if (normalized.length !== 6) {
    return fallbackPalette.secondaryBg;
  }

  const channel = (start: number) => {
    const value = parseInt(normalized.slice(start, start + 2), 16);
    return Math.min(255, Math.round(value + (255 - value) * amount))
      .toString(16)
      .padStart(2, '0');
  };

  return `#${channel(0)}${channel(2)}${channel(4)}`;
}
