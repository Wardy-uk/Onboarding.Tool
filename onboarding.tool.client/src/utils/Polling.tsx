import { useEffect, useRef } from 'react';

export function usePolling({
  fn,
  interval = 2000,
  shouldContinue,
  onComplete,
  enabled = true
}: {
  fn: () => Promise<any>;
  interval?: number;
  shouldContinue: (result: any) => boolean;
  onComplete?: (result: any) => void;
  enabled?: boolean;
}) {
  const timer = useRef<NodeJS.Timeout | null>(null);
  const stopped = useRef(false);

  useEffect(() => {
    if (!enabled) return;
    stopped.current = false;
    let isMounted = true;
    const poll = async () => {
      if (stopped.current) return;
      try {
        const result = await fn();
        if (!shouldContinue(result)) {
          stopped.current = true;
          if (onComplete) onComplete(result);
          return;
        }
        if (isMounted) {
          timer.current = setTimeout(poll, interval);
        }
      } catch {
        if (isMounted) {
          timer.current = setTimeout(poll, interval);
        }
      }
    };
    poll();
    return () => {
      isMounted = false;
      stopped.current = true;
      if (timer.current) clearTimeout(timer.current);
    };
  }, [fn, interval, shouldContinue, onComplete, enabled]);
} 