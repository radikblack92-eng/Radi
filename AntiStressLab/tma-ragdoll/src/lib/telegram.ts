import { sendData } from '@telegram-apps/sdk-react';

export type SandboxResultPayload = {
  type: 'ragdoll_result';
  createdAt: string;
  userId?: number;
  faceTextureLoaded: boolean;
};

export function captureScenePreview(canvas: HTMLCanvasElement | null): string | null {
  if (!canvas) {
    return null;
  }

  // Upload this preview to your backend first, then send a tiny reference via sendData.
  return canvas.toDataURL('image/webp', 0.72);
}

export function sendSandboxResult(payload: SandboxResultPayload): boolean {
  const serialized = JSON.stringify(payload);

  if (sendData.isAvailable()) {
    sendData(serialized);
    return true;
  }

  return false;
}
