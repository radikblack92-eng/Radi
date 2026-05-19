const BOT_TOKEN = process.env.TELEGRAM_BOT_TOKEN;
const WEB_APP_URL = process.env.WEB_APP_URL;
const BOT_MENU_TEXT = process.env.BOT_MENU_TEXT ?? 'Ragdoll';
const BOT_DESCRIPTION =
  process.env.BOT_DESCRIPTION ?? 'Open the Ragdoll Sandbox Telegram Mini App.';

if (!BOT_TOKEN) {
  throw new Error('TELEGRAM_BOT_TOKEN is required');
}

if (!WEB_APP_URL || !WEB_APP_URL.startsWith('https://')) {
  throw new Error('WEB_APP_URL must be an HTTPS URL');
}

async function callTelegram(method, payload) {
  const response = await fetch(`https://api.telegram.org/bot${BOT_TOKEN}/${method}`, {
    body: JSON.stringify(payload),
    headers: {
      'Content-Type': 'application/json',
    },
    method: 'POST',
  });

  const result = await response.json();

  if (!response.ok || !result.ok) {
    throw new Error(`${method} failed: ${JSON.stringify(result)}`);
  }

  return result;
}

await callTelegram('setChatMenuButton', {
  menu_button: {
    text: BOT_MENU_TEXT,
    type: 'web_app',
    web_app: {
      url: WEB_APP_URL,
    },
  },
});

await callTelegram('setMyCommands', {
  commands: [
    {
      command: 'start',
      description: 'Open Ragdoll Sandbox',
    },
    {
      command: 'play',
      description: 'Play the ragdoll sandbox',
    },
  ],
});

await callTelegram('setMyShortDescription', {
  short_description: BOT_DESCRIPTION.slice(0, 120),
});

console.log(`Bot menu configured for ${WEB_APP_URL}`);
