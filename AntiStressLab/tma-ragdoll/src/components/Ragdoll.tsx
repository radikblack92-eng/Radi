import { useBox, useConeTwistConstraint, usePlane, useSphere } from '@react-three/cannon';
import type { PublicApi, Triplet } from '@react-three/cannon';
import type { ThreeEvent } from '@react-three/fiber';
import type { ReactNode, RefObject } from 'react';
import { useEffect } from 'react';
import type { Mesh, Object3D, Texture } from 'three';

import type { DraggableBody } from '../hooks/usePointerDragControls';

type GrabHandler = (event: ThreeEvent<PointerEvent>, body: DraggableBody) => void;

type RagdollProps = {
  faceTexture: Texture | null;
  resetKey: number;
  onGrab: GrabHandler;
};

type PartPose = {
  position: Triplet;
  rotation?: Triplet;
};

type PartMeshProps = {
  api: PublicApi;
  color: string;
  children: ReactNode;
  onGrab: GrabHandler;
  refObject: RefObject<Mesh | null>;
  impulseScale?: number;
};

const BODY_MATERIAL = 'ragdoll';
const INITIAL_POSE: Record<string, PartPose> = {
  head: { position: [0, 2.45, 0] },
  torso: { position: [0, 1.65, 0] },
  pelvis: { position: [0, 1.05, 0] },
  upperArmLeft: { position: [-0.64, 1.85, 0] },
  lowerArmLeft: { position: [-1.15, 1.78, 0] },
  handLeft: { position: [-1.52, 1.72, 0] },
  upperArmRight: { position: [0.64, 1.85, 0] },
  lowerArmRight: { position: [1.15, 1.78, 0] },
  handRight: { position: [1.52, 1.72, 0] },
  upperLegLeft: { position: [-0.24, 0.62, 0] },
  lowerLegLeft: { position: [-0.25, 0.06, 0] },
  footLeft: { position: [-0.25, -0.25, 0.13] },
  upperLegRight: { position: [0.24, 0.62, 0] },
  lowerLegRight: { position: [0.25, 0.06, 0] },
  footRight: { position: [0.25, -0.25, 0.13] },
};

function resetBody(api: PublicApi, pose: PartPose) {
  const [x, y, z] = pose.position;
  const [rx, ry, rz] = pose.rotation ?? [0, 0, 0];

  api.position.set(x, y, z);
  api.rotation.set(rx, ry, rz);
  api.velocity.set(0, 0, 0);
  api.angularVelocity.set(0, 0, 0);
  api.wakeUp();
}

function PartMesh({
  api,
  children,
  color,
  impulseScale,
  onGrab,
  refObject,
}: PartMeshProps) {
  const draggableRef = refObject as RefObject<Object3D | null>;

  return (
    <mesh
      ref={refObject}
      onPointerDown={(event) => onGrab(event, { api, ref: draggableRef, impulseScale })}
    >
      {children}
      <meshLambertMaterial color={color} />
    </mesh>
  );
}

function Ground() {
  const [groundRef] = usePlane<Mesh>(() => ({
    material: 'ground',
    position: [0, -0.44, 0],
    rotation: [-Math.PI / 2, 0, 0],
  }));

  return (
    <mesh ref={groundRef} receiveShadow={false}>
      <planeGeometry args={[12, 12]} />
      <meshBasicMaterial color="#020617" transparent opacity={0.22} />
    </mesh>
  );
}

export function Ragdoll({ faceTexture, onGrab, resetKey }: RagdollProps) {
  const [headRef, headApi] = useSphere<Mesh>(() => ({
    args: [0.31],
    angularDamping: 0.65,
    linearDamping: 0.45,
    mass: 0.75,
    material: BODY_MATERIAL,
    position: INITIAL_POSE.head.position,
  }));
  const [torsoRef, torsoApi] = useBox<Mesh>(() => ({
    args: [0.64, 0.82, 0.26],
    angularDamping: 0.58,
    linearDamping: 0.38,
    mass: 2.2,
    material: BODY_MATERIAL,
    position: INITIAL_POSE.torso.position,
  }));
  const [pelvisRef, pelvisApi] = useBox<Mesh>(() => ({
    args: [0.62, 0.34, 0.28],
    angularDamping: 0.58,
    linearDamping: 0.38,
    mass: 1.5,
    material: BODY_MATERIAL,
    position: INITIAL_POSE.pelvis.position,
  }));

  const [upperArmLeftRef, upperArmLeftApi] = useBox<Mesh>(() => ({
    args: [0.56, 0.18, 0.18],
    angularDamping: 0.52,
    linearDamping: 0.36,
    mass: 0.55,
    material: BODY_MATERIAL,
    position: INITIAL_POSE.upperArmLeft.position,
    rotation: INITIAL_POSE.upperArmLeft.rotation,
  }));
  const [lowerArmLeftRef, lowerArmLeftApi] = useBox<Mesh>(() => ({
    args: [0.52, 0.16, 0.16],
    angularDamping: 0.5,
    linearDamping: 0.34,
    mass: 0.42,
    material: BODY_MATERIAL,
    position: INITIAL_POSE.lowerArmLeft.position,
    rotation: INITIAL_POSE.lowerArmLeft.rotation,
  }));
  const [handLeftRef, handLeftApi] = useSphere<Mesh>(() => ({
    args: [0.12],
    angularDamping: 0.46,
    linearDamping: 0.32,
    mass: 0.2,
    material: BODY_MATERIAL,
    position: INITIAL_POSE.handLeft.position,
  }));

  const [upperArmRightRef, upperArmRightApi] = useBox<Mesh>(() => ({
    args: [0.56, 0.18, 0.18],
    angularDamping: 0.52,
    linearDamping: 0.36,
    mass: 0.55,
    material: BODY_MATERIAL,
    position: INITIAL_POSE.upperArmRight.position,
    rotation: INITIAL_POSE.upperArmRight.rotation,
  }));
  const [lowerArmRightRef, lowerArmRightApi] = useBox<Mesh>(() => ({
    args: [0.52, 0.16, 0.16],
    angularDamping: 0.5,
    linearDamping: 0.34,
    mass: 0.42,
    material: BODY_MATERIAL,
    position: INITIAL_POSE.lowerArmRight.position,
    rotation: INITIAL_POSE.lowerArmRight.rotation,
  }));
  const [handRightRef, handRightApi] = useSphere<Mesh>(() => ({
    args: [0.12],
    angularDamping: 0.46,
    linearDamping: 0.32,
    mass: 0.2,
    material: BODY_MATERIAL,
    position: INITIAL_POSE.handRight.position,
  }));

  const [upperLegLeftRef, upperLegLeftApi] = useBox<Mesh>(() => ({
    args: [0.22, 0.58, 0.2],
    angularDamping: 0.52,
    linearDamping: 0.36,
    mass: 0.72,
    material: BODY_MATERIAL,
    position: INITIAL_POSE.upperLegLeft.position,
  }));
  const [lowerLegLeftRef, lowerLegLeftApi] = useBox<Mesh>(() => ({
    args: [0.19, 0.52, 0.18],
    angularDamping: 0.5,
    linearDamping: 0.34,
    mass: 0.55,
    material: BODY_MATERIAL,
    position: INITIAL_POSE.lowerLegLeft.position,
  }));
  const [footLeftRef, footLeftApi] = useBox<Mesh>(() => ({
    args: [0.24, 0.16, 0.38],
    angularDamping: 0.48,
    linearDamping: 0.34,
    mass: 0.24,
    material: BODY_MATERIAL,
    position: INITIAL_POSE.footLeft.position,
    rotation: INITIAL_POSE.footLeft.rotation,
  }));

  const [upperLegRightRef, upperLegRightApi] = useBox<Mesh>(() => ({
    args: [0.22, 0.58, 0.2],
    angularDamping: 0.52,
    linearDamping: 0.36,
    mass: 0.72,
    material: BODY_MATERIAL,
    position: INITIAL_POSE.upperLegRight.position,
  }));
  const [lowerLegRightRef, lowerLegRightApi] = useBox<Mesh>(() => ({
    args: [0.19, 0.52, 0.18],
    angularDamping: 0.5,
    linearDamping: 0.34,
    mass: 0.55,
    material: BODY_MATERIAL,
    position: INITIAL_POSE.lowerLegRight.position,
  }));
  const [footRightRef, footRightApi] = useBox<Mesh>(() => ({
    args: [0.24, 0.16, 0.38],
    angularDamping: 0.48,
    linearDamping: 0.34,
    mass: 0.24,
    material: BODY_MATERIAL,
    position: INITIAL_POSE.footRight.position,
    rotation: INITIAL_POSE.footRight.rotation,
  }));

  useConeTwistConstraint(torsoRef, headRef, {
    angle: Math.PI / 5,
    collideConnected: false,
    maxForce: 1e4,
    pivotA: [0, 0.43, 0],
    pivotB: [0, -0.28, 0],
    twistAngle: Math.PI / 8,
  });
  useConeTwistConstraint(torsoRef, pelvisRef, {
    angle: Math.PI / 9,
    collideConnected: false,
    maxForce: 1e4,
    pivotA: [0, -0.43, 0],
    pivotB: [0, 0.2, 0],
    twistAngle: Math.PI / 12,
  });

  useConeTwistConstraint(torsoRef, upperArmLeftRef, {
    angle: Math.PI / 3,
    collideConnected: false,
    maxForce: 8e3,
    pivotA: [-0.38, 0.25, 0],
    pivotB: [0.28, 0, 0],
    twistAngle: Math.PI / 4,
  });
  useConeTwistConstraint(upperArmLeftRef, lowerArmLeftRef, {
    angle: Math.PI / 3,
    collideConnected: false,
    maxForce: 7e3,
    pivotA: [-0.28, 0, 0],
    pivotB: [0.26, 0, 0],
    twistAngle: Math.PI / 5,
  });
  useConeTwistConstraint(lowerArmLeftRef, handLeftRef, {
    angle: Math.PI / 5,
    collideConnected: false,
    maxForce: 4e3,
    pivotA: [-0.28, 0, 0],
    pivotB: [0.1, 0, 0],
  });

  useConeTwistConstraint(torsoRef, upperArmRightRef, {
    angle: Math.PI / 3,
    collideConnected: false,
    maxForce: 8e3,
    pivotA: [0.38, 0.25, 0],
    pivotB: [-0.28, 0, 0],
    twistAngle: Math.PI / 4,
  });
  useConeTwistConstraint(upperArmRightRef, lowerArmRightRef, {
    angle: Math.PI / 3,
    collideConnected: false,
    maxForce: 7e3,
    pivotA: [0.28, 0, 0],
    pivotB: [-0.26, 0, 0],
    twistAngle: Math.PI / 5,
  });
  useConeTwistConstraint(lowerArmRightRef, handRightRef, {
    angle: Math.PI / 5,
    collideConnected: false,
    maxForce: 4e3,
    pivotA: [0.28, 0, 0],
    pivotB: [-0.1, 0, 0],
  });

  useConeTwistConstraint(pelvisRef, upperLegLeftRef, {
    angle: Math.PI / 4,
    collideConnected: false,
    maxForce: 8e3,
    pivotA: [-0.22, -0.18, 0],
    pivotB: [0, 0.3, 0],
    twistAngle: Math.PI / 5,
  });
  useConeTwistConstraint(upperLegLeftRef, lowerLegLeftRef, {
    angle: Math.PI / 4,
    collideConnected: false,
    maxForce: 7e3,
    pivotA: [0, -0.3, 0],
    pivotB: [0, 0.27, 0],
    twistAngle: Math.PI / 6,
  });
  useConeTwistConstraint(lowerLegLeftRef, footLeftRef, {
    angle: Math.PI / 6,
    collideConnected: false,
    maxForce: 5e3,
    pivotA: [0, -0.27, 0],
    pivotB: [0, 0, -0.18],
  });

  useConeTwistConstraint(pelvisRef, upperLegRightRef, {
    angle: Math.PI / 4,
    collideConnected: false,
    maxForce: 8e3,
    pivotA: [0.22, -0.18, 0],
    pivotB: [0, 0.3, 0],
    twistAngle: Math.PI / 5,
  });
  useConeTwistConstraint(upperLegRightRef, lowerLegRightRef, {
    angle: Math.PI / 4,
    collideConnected: false,
    maxForce: 7e3,
    pivotA: [0, -0.3, 0],
    pivotB: [0, 0.27, 0],
    twistAngle: Math.PI / 6,
  });
  useConeTwistConstraint(lowerLegRightRef, footRightRef, {
    angle: Math.PI / 6,
    collideConnected: false,
    maxForce: 5e3,
    pivotA: [0, -0.27, 0],
    pivotB: [0, 0, -0.18],
  });

  useEffect(() => {
    if (resetKey === 0) {
      return;
    }

    resetBody(headApi, INITIAL_POSE.head);
    resetBody(torsoApi, INITIAL_POSE.torso);
    resetBody(pelvisApi, INITIAL_POSE.pelvis);
    resetBody(upperArmLeftApi, INITIAL_POSE.upperArmLeft);
    resetBody(lowerArmLeftApi, INITIAL_POSE.lowerArmLeft);
    resetBody(handLeftApi, INITIAL_POSE.handLeft);
    resetBody(upperArmRightApi, INITIAL_POSE.upperArmRight);
    resetBody(lowerArmRightApi, INITIAL_POSE.lowerArmRight);
    resetBody(handRightApi, INITIAL_POSE.handRight);
    resetBody(upperLegLeftApi, INITIAL_POSE.upperLegLeft);
    resetBody(lowerLegLeftApi, INITIAL_POSE.lowerLegLeft);
    resetBody(footLeftApi, INITIAL_POSE.footLeft);
    resetBody(upperLegRightApi, INITIAL_POSE.upperLegRight);
    resetBody(lowerLegRightApi, INITIAL_POSE.lowerLegRight);
    resetBody(footRightApi, INITIAL_POSE.footRight);
  }, [
    footLeftApi,
    footRightApi,
    handLeftApi,
    handRightApi,
    headApi,
    lowerArmLeftApi,
    lowerArmRightApi,
    lowerLegLeftApi,
    lowerLegRightApi,
    pelvisApi,
    resetKey,
    torsoApi,
    upperArmLeftApi,
    upperArmRightApi,
    upperLegLeftApi,
    upperLegRightApi,
  ]);

  return (
    <group>
      <Ground />
      <PartMesh api={headApi} color="#f2d0a7" impulseScale={0.12} onGrab={onGrab} refObject={headRef}>
        <sphereGeometry args={[0.31, 18, 14]} />
        <mesh
          position={[0, 0.01, 0.303]}
          onPointerDown={(event) =>
            onGrab(event, {
              api: headApi,
              impulseScale: 0.12,
              ref: headRef as RefObject<Object3D | null>,
            })
          }
        >
          <circleGeometry args={[0.18, 24]} />
          <meshBasicMaterial color="#ffe3bd" map={faceTexture ?? undefined} toneMapped={false} />
        </mesh>
      </PartMesh>
      <PartMesh api={torsoApi} color="#38bdf8" impulseScale={0.15} onGrab={onGrab} refObject={torsoRef}>
        <boxGeometry args={[0.64, 0.82, 0.26]} />
      </PartMesh>
      <PartMesh api={pelvisApi} color="#0ea5e9" impulseScale={0.16} onGrab={onGrab} refObject={pelvisRef}>
        <boxGeometry args={[0.62, 0.34, 0.28]} />
      </PartMesh>

      <PartMesh api={upperArmLeftApi} color="#7dd3fc" onGrab={onGrab} refObject={upperArmLeftRef}>
        <boxGeometry args={[0.56, 0.18, 0.18]} />
      </PartMesh>
      <PartMesh api={lowerArmLeftApi} color="#bae6fd" onGrab={onGrab} refObject={lowerArmLeftRef}>
        <boxGeometry args={[0.52, 0.16, 0.16]} />
      </PartMesh>
      <PartMesh api={handLeftApi} color="#f2d0a7" impulseScale={0.22} onGrab={onGrab} refObject={handLeftRef}>
        <sphereGeometry args={[0.12, 12, 8]} />
      </PartMesh>

      <PartMesh api={upperArmRightApi} color="#7dd3fc" onGrab={onGrab} refObject={upperArmRightRef}>
        <boxGeometry args={[0.56, 0.18, 0.18]} />
      </PartMesh>
      <PartMesh api={lowerArmRightApi} color="#bae6fd" onGrab={onGrab} refObject={lowerArmRightRef}>
        <boxGeometry args={[0.52, 0.16, 0.16]} />
      </PartMesh>
      <PartMesh api={handRightApi} color="#f2d0a7" impulseScale={0.22} onGrab={onGrab} refObject={handRightRef}>
        <sphereGeometry args={[0.12, 12, 8]} />
      </PartMesh>

      <PartMesh api={upperLegLeftApi} color="#0284c7" onGrab={onGrab} refObject={upperLegLeftRef}>
        <boxGeometry args={[0.22, 0.58, 0.2]} />
      </PartMesh>
      <PartMesh api={lowerLegLeftApi} color="#0369a1" onGrab={onGrab} refObject={lowerLegLeftRef}>
        <boxGeometry args={[0.19, 0.52, 0.18]} />
      </PartMesh>
      <PartMesh api={footLeftApi} color="#075985" impulseScale={0.22} onGrab={onGrab} refObject={footLeftRef}>
        <boxGeometry args={[0.24, 0.16, 0.38]} />
      </PartMesh>

      <PartMesh api={upperLegRightApi} color="#0284c7" onGrab={onGrab} refObject={upperLegRightRef}>
        <boxGeometry args={[0.22, 0.58, 0.2]} />
      </PartMesh>
      <PartMesh api={lowerLegRightApi} color="#0369a1" onGrab={onGrab} refObject={lowerLegRightRef}>
        <boxGeometry args={[0.19, 0.52, 0.18]} />
      </PartMesh>
      <PartMesh api={footRightApi} color="#075985" impulseScale={0.22} onGrab={onGrab} refObject={footRightRef}>
        <boxGeometry args={[0.24, 0.16, 0.38]} />
      </PartMesh>
    </group>
  );
}
