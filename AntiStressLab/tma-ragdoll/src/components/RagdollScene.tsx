import { Physics, useContactMaterial } from '@react-three/cannon';
import { Canvas } from '@react-three/fiber';
import type { ThreeEvent } from '@react-three/fiber';
import type { Texture } from 'three';

import { usePointerDragControls } from '../hooks/usePointerDragControls';
import type { TelegramPalette } from '../lib/colors';
import { Ragdoll } from './Ragdoll';

type RagdollSceneProps = {
  faceTexture: Texture | null;
  palette: TelegramPalette;
  resetKey: number;
  onCanvasReady: (canvas: HTMLCanvasElement) => void;
};

function PhysicsMaterials() {
  useContactMaterial('ragdoll', 'ground', {
    contactEquationRelaxation: 4,
    friction: 0.72,
    restitution: 0.28,
  });

  return null;
}

function DragPlane({
  onMove,
  onEnd,
}: {
  onMove: (event: ThreeEvent<PointerEvent>) => void;
  onEnd: (event: ThreeEvent<PointerEvent>) => void;
}) {
  return (
    <mesh
      position={[0, 1.1, 0]}
      renderOrder={10}
      onPointerCancel={onEnd}
      onPointerMove={onMove}
      onPointerOut={onEnd}
      onPointerUp={onEnd}
    >
      <planeGeometry args={[14, 14]} />
      <meshBasicMaterial color="#000" depthWrite={false} opacity={0} transparent />
    </mesh>
  );
}

export function RagdollScene({
  faceTexture,
  onCanvasReady,
  palette,
  resetKey,
}: RagdollSceneProps) {
  const dragControls = usePointerDragControls();

  return (
    <Canvas
      camera={{ fov: 42, position: [0, 1.18, 5.25] }}
      dpr={[1, 1.5]}
      gl={{
        alpha: false,
        antialias: false,
        powerPreference: 'high-performance',
        preserveDrawingBuffer: true,
        stencil: false,
      }}
      onCreated={({ gl }) => {
        gl.setClearColor(palette.bg);
        onCanvasReady(gl.domElement);
      }}
    >
      <color attach="background" args={[palette.bg]} />
      <fog attach="fog" args={[palette.bg, 6.5, 10]} />
      <ambientLight intensity={0.75} />
      <directionalLight intensity={1.15} position={[2.8, 4.5, 3.5]} />
      <Physics
        allowSleep
        broadphase="SAP"
        defaultContactMaterial={{ friction: 0.65, restitution: 0.18 }}
        gravity={[0, -9.82, 0]}
        iterations={8}
        size={96}
        tolerance={0.0008}
      >
        <PhysicsMaterials />
        <Ragdoll
          faceTexture={faceTexture}
          resetKey={resetKey}
          onGrab={dragControls.startDrag}
        />
      </Physics>
      {dragControls.isDragging ? (
        <DragPlane onEnd={dragControls.endDrag} onMove={dragControls.moveDrag} />
      ) : null}
    </Canvas>
  );
}
