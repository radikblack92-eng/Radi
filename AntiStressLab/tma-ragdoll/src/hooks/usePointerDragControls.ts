import type { ThreeEvent } from '@react-three/fiber';
import type { PublicApi } from '@react-three/cannon';
import type { RefObject } from 'react';
import { useCallback, useRef, useState } from 'react';
import { Object3D, Vector3 } from 'three';

export type DraggableBody = {
  api: PublicApi;
  ref: RefObject<Object3D | null>;
  impulseScale?: number;
};

type DragState = {
  body: DraggableBody;
  pointerId: number;
  offset: Vector3;
  lastPoint: Vector3;
  lastVelocity: Vector3;
  lastTime: number;
};

function capturePointer(event: ThreeEvent<PointerEvent>) {
  const target = event.target as EventTarget & {
    setPointerCapture?: (pointerId: number) => void;
    releasePointerCapture?: (pointerId: number) => void;
  };

  target.setPointerCapture?.(event.pointerId);
}

function releasePointer(event: ThreeEvent<PointerEvent>) {
  const target = event.target as EventTarget & {
    releasePointerCapture?: (pointerId: number) => void;
  };

  target.releasePointerCapture?.(event.pointerId);
}

export function usePointerDragControls() {
  const drag = useRef<DragState | null>(null);
  const [isDragging, setIsDragging] = useState(false);

  const startDrag = useCallback(
    (event: ThreeEvent<PointerEvent>, body: DraggableBody) => {
      event.stopPropagation();
      capturePointer(event);

      const worldPosition = new Vector3();
      body.ref.current?.getWorldPosition(worldPosition);

      drag.current = {
        body,
        pointerId: event.pointerId,
        offset: worldPosition.clone().sub(event.point),
        lastPoint: worldPosition,
        lastVelocity: new Vector3(),
        lastTime: performance.now(),
      };

      body.api.wakeUp();
      body.api.velocity.set(0, 0, 0);
      body.api.angularVelocity.set(0, 0, 0);
      setIsDragging(true);
    },
    [],
  );

  const moveDrag = useCallback((event: ThreeEvent<PointerEvent>) => {
    const current = drag.current;

    if (!current || current.pointerId !== event.pointerId) {
      return;
    }

    event.stopPropagation();
    const now = performance.now();
    const target = event.point.clone().add(current.offset);
    const deltaTime = Math.max((now - current.lastTime) / 1000, 1 / 120);
    const velocity = target.clone().sub(current.lastPoint).divideScalar(deltaTime);

    current.body.api.position.set(target.x, target.y, target.z);
    current.body.api.velocity.set(velocity.x * 0.55, velocity.y * 0.55, velocity.z * 0.55);
    current.body.api.angularVelocity.set(0, 0, 0);
    current.lastPoint.copy(target);
    current.lastVelocity.copy(velocity);
    current.lastTime = now;
  }, []);

  const endDrag = useCallback((event?: ThreeEvent<PointerEvent>) => {
    const current = drag.current;

    if (!current || (event && current.pointerId !== event.pointerId)) {
      return;
    }

    event?.stopPropagation();
    if (event) {
      releasePointer(event);
    }

    const impulse = current.lastVelocity.multiplyScalar(current.body.impulseScale ?? 0.18);
    const point = new Vector3();
    current.body.ref.current?.getWorldPosition(point);
    current.body.api.applyImpulse([impulse.x, impulse.y, impulse.z], [point.x, point.y, point.z]);
    current.body.api.wakeUp();
    drag.current = null;
    setIsDragging(false);
  }, []);

  return {
    isDragging,
    startDrag,
    moveDrag,
    endDrag,
  };
}
