import {
  bindMiniAppCssVars,
  bindThemeParamsCssVars,
  init,
  initDataUser,
  isTMA,
  miniAppReady,
  mountMiniAppSync,
  restoreInitData,
  setMiniAppBackgroundColor,
  setMiniAppBottomBarColor,
  setMiniAppHeaderColor,
  themeParamsBackgroundColor,
  themeParamsButtonColor,
  themeParamsButtonTextColor,
  themeParamsHintColor,
  themeParamsSecondaryBackgroundColor,
  themeParamsTextColor,
  useSignal,
} from '@telegram-apps/sdk-react';
import { useEffect, useMemo, useState } from 'react';

import { fallbackPalette, softenHex, type TelegramPalette } from '../lib/colors';

export function useTelegramMiniApp() {
  const backgroundColor = useSignal(themeParamsBackgroundColor);
  const secondaryBackgroundColor = useSignal(themeParamsSecondaryBackgroundColor);
  const textColor = useSignal(themeParamsTextColor);
  const hintColor = useSignal(themeParamsHintColor);
  const buttonColor = useSignal(themeParamsButtonColor);
  const buttonTextColor = useSignal(themeParamsButtonTextColor);
  const user = useSignal(initDataUser);
  const [isTelegram, setIsTelegram] = useState(false);

  useEffect(() => {
    let cleanupSdk: VoidFunction | undefined;
    let cleanupThemeVars: VoidFunction | undefined;
    let cleanupMiniAppVars: VoidFunction | undefined;

    try {
      cleanupSdk = init();
      const insideTelegram = isTMA();
      setIsTelegram(insideTelegram);

      if (!insideTelegram) {
        return cleanupSdk;
      }

      if (mountMiniAppSync.isAvailable()) {
        mountMiniAppSync();
      }

      restoreInitData();

      if (bindThemeParamsCssVars.isAvailable()) {
        cleanupThemeVars = bindThemeParamsCssVars();
      }

      if (bindMiniAppCssVars.isAvailable()) {
        cleanupMiniAppVars = bindMiniAppCssVars();
      }

      if (miniAppReady.isAvailable()) {
        miniAppReady();
      }
    } catch (error) {
      console.warn('Telegram Mini App SDK initialization skipped:', error);
    }

    return () => {
      cleanupMiniAppVars?.();
      cleanupThemeVars?.();
      cleanupSdk?.();
    };
  }, []);

  const palette = useMemo<TelegramPalette>(() => {
    const bg = backgroundColor ?? fallbackPalette.bg;

    return {
      bg,
      secondaryBg: secondaryBackgroundColor ?? softenHex(bg, 0.08),
      text: textColor ?? fallbackPalette.text,
      hint: hintColor ?? fallbackPalette.hint,
      button: buttonColor ?? fallbackPalette.button,
      buttonText: buttonTextColor ?? fallbackPalette.buttonText,
    };
  }, [
    backgroundColor,
    buttonColor,
    buttonTextColor,
    hintColor,
    secondaryBackgroundColor,
    textColor,
  ]);

  useEffect(() => {
    if (!isTelegram) {
      return;
    }

    if (setMiniAppBackgroundColor.isAvailable()) {
      setMiniAppBackgroundColor(palette.bg);
    }

    if (setMiniAppHeaderColor.isAvailable() && setMiniAppHeaderColor.supports.rgb()) {
      setMiniAppHeaderColor(palette.bg);
    }

    if (setMiniAppBottomBarColor.isAvailable()) {
      setMiniAppBottomBarColor(palette.secondaryBg);
    }
  }, [isTelegram, palette.bg, palette.secondaryBg]);

  return {
    isTelegram,
    palette,
    user,
  };
}
