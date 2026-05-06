using UnityEngine;

namespace AntiStressLab.Audio
{
    public static class ProceduralPloop
    {
        public static AudioClip Create(float durationSeconds = 0.12f, int sampleRate = 44100)
        {
            int samples = Mathf.Max(64, Mathf.RoundToInt(durationSeconds * sampleRate));
            var data = new float[samples];

            // A quick downward chirp + soft noise, with exponential decay.
            float f0 = 260f;
            float f1 = 90f;
            float phase = 0f;

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / (samples - 1);
                float freq = Mathf.Lerp(f0, f1, t * t);
                phase += 2f * Mathf.PI * freq / sampleRate;

                float env = Mathf.Exp(-8.5f * t);
                float tone = Mathf.Sin(phase) * 0.65f;
                float noise = (Random.value * 2f - 1f) * 0.12f;
                data[i] = (tone + noise) * env;
            }

            var clip = AudioClip.Create("Ploop", samples, 1, sampleRate, stream: false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}

