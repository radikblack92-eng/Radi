import { sendData } from '@telegram-apps/sdk-react';

export type SandboxResultPayload = {
  type: 'ragdoll_result';
  createdAt: string;
  userId?: number;
  queryId?: string;
  faceTextureLoaded: boolean;
  resultId?: string;
  shareUrl?: string;
};

export type BackendResultResponse = {
  ok: boolean;
  resultId?: string;
  shareUrl?: string;
  error?: string;
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

export async function submitSandboxResultToBackend({
  initDataRaw,
  payload,
  previewDataUrl,
}: {
  initDataRaw?: string;
  payload: SandboxResultPayload;
  previewDataUrl: string | null;
}): Promise<BackendResultResponse | null> {
  const endpoint = import.meta.env.VITE_RESULT_API_URL;

  if (!endpoint) {
    return null;
  }

  const response = await fetch(endpoint, {
    body: JSON.stringify({
      initDataRaw,
      payload,
      previewDataUrl,
    }),
    headers: {
      'Content-Type': 'application/json',
    },
    method: 'POST',
  });

  const result = (await response.json()) as BackendResultResponse;

  if (!response.ok) {
    return {
      ok: false,
      error: result.error ?? `Backend responded with ${response.status}`,
    };
  }

  return result;
}
