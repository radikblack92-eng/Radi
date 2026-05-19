import { useEffect, useState } from 'react';
import { LinearFilter, SRGBColorSpace, Texture, TextureLoader } from 'three';

export function useFaceTexture(file: File | null) {
  const [texture, setTexture] = useState<Texture | null>(null);

  useEffect(() => {
    if (!file) {
      setTexture(null);
      return;
    }

    const objectUrl = URL.createObjectURL(file);
    const loader = new TextureLoader();
    let disposed = false;
    let loadedTexture: Texture | null = null;

    loader.load(objectUrl, (nextTexture) => {
      if (disposed) {
        nextTexture.dispose();
        return;
      }

      nextTexture.colorSpace = SRGBColorSpace;
      nextTexture.minFilter = LinearFilter;
      nextTexture.magFilter = LinearFilter;
      nextTexture.generateMipmaps = false;
      nextTexture.needsUpdate = true;
      loadedTexture = nextTexture;
      setTexture(nextTexture);
      URL.revokeObjectURL(objectUrl);
    });

    return () => {
      disposed = true;
      URL.revokeObjectURL(objectUrl);
      loadedTexture?.dispose();
    };
  }, [file]);

  return texture;
}
