import { createServer } from 'node:http';
import { randomUUID } from 'node:crypto';

const PORT = Number(process.env.PORT ?? 8787);
const BOT_TOKEN = process.env.TELEGRAM_BOT_TOKEN;
const PUBLIC_RESULT_BASE_URL = process.env.PUBLIC_RESULT_BASE_URL;
const MAX_BODY_BYTES = 1_000_000;

function writeJson(response, statusCode, payload) {
  response.writeHead(statusCode, {
    'Access-Control-Allow-Headers': 'Content-Type',
    'Access-Control-Allow-Methods': 'POST, OPTIONS',
    'Access-Control-Allow-Origin': process.env.CORS_ORIGIN ?? '*',
    'Content-Type': 'application/json',
  });
  response.end(JSON.stringify(payload));
}

async function readJsonBody(request) {
  const chunks = [];
  let size = 0;

  for await (const chunk of request) {
    size += chunk.length;

    if (size > MAX_BODY_BYTES) {
      throw new Error('Request body is too large');
    }

    chunks.push(chunk);
  }

  return JSON.parse(Buffer.concat(chunks).toString('utf8'));
}

async function answerWebAppQuery({ payload, resultId, shareUrl }) {
  if (!BOT_TOKEN || !payload.queryId) {
    return false;
  }

  const messageText = [
    'Ragdoll Sandbox result',
    `Created: ${payload.createdAt}`,
    payload.faceTextureLoaded ? 'Face photo: yes' : 'Face photo: no',
  ].join('\n');

  const telegramResponse = await fetch(`https://api.telegram.org/bot${BOT_TOKEN}/answerWebAppQuery`, {
    body: JSON.stringify({
      result: {
        id: resultId,
        input_message_content: {
          message_text: messageText,
        },
        reply_markup: shareUrl
          ? {
              inline_keyboard: [[{ text: 'Open result', url: shareUrl }]],
            }
          : undefined,
        title: 'Ragdoll Sandbox result',
        type: 'article',
      },
      web_app_query_id: payload.queryId,
    }),
    headers: {
      'Content-Type': 'application/json',
    },
    method: 'POST',
  });

  if (!telegramResponse.ok) {
    const errorText = await telegramResponse.text();
    throw new Error(`Telegram Bot API error: ${errorText}`);
  }

  return true;
}

createServer(async (request, response) => {
  if (request.method === 'OPTIONS') {
    writeJson(response, 204, {});
    return;
  }

  if (request.method !== 'POST' || request.url !== '/api/tma/ragdoll-result') {
    writeJson(response, 404, { ok: false, error: 'Not found' });
    return;
  }

  try {
    const body = await readJsonBody(request);
    const resultId = randomUUID();
    const shareUrl = PUBLIC_RESULT_BASE_URL ? `${PUBLIC_RESULT_BASE_URL}/${resultId}` : undefined;

    // Persist body.previewDataUrl in object storage here, then use the public URL in shareUrl.
    // Verify body.initDataRaw with TELEGRAM_BOT_TOKEN before trusting user identity in production.
    const answered = await answerWebAppQuery({
      payload: body.payload,
      resultId,
      shareUrl,
    });

    writeJson(response, 200, {
      ok: true,
      answered,
      resultId,
      shareUrl,
    });
  } catch (error) {
    writeJson(response, 500, {
      ok: false,
      error: error instanceof Error ? error.message : 'Unknown error',
    });
  }
}).listen(PORT, () => {
  console.log(`Telegram result handler listening on :${PORT}`);
});
