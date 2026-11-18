export const AGENT_BASE_URL = 'http://127.0.0.1:41950';

export interface CursorPoint {
  x: number;
  y: number;
}

export interface AgentHealth {
  status: string;
  agent: string;
  version: string;
  platform: string;
  capabilities: string[];
}

export interface OsAutoClickStatus {
  isRunning: boolean;
  intervalMs: number;
  button: 'left' | 'right' | 'middle';
  clickType: 'single' | 'double';
  repeatCount: number;
  clicksDone: number;
  useRecordedPoints: boolean;
  points: CursorPoint[];
  cursorX: number;
  cursorY: number;
  hotkeyToggle: string;
  hotkeyCapture: string;
}

export interface OsWindowInfo {
  handle: number;
  title: string;
  processName: string;
  processId: number;
  isTopMost: boolean;
  opacityPercent: number;
}

async function fetchAgent<T>(
  path: string,
  options: RequestInit = {},
  timeoutMs = 2000
): Promise<T> {
  const controller = new AbortController();
  const timer = setTimeout(() => controller.abort(), timeoutMs);

  try {
    const res = await fetch(`${AGENT_BASE_URL}${path}`, {
      ...options,
      signal: controller.signal,
      headers: {
        'Content-Type': 'application/json',
        ...(options.headers || {}),
      },
    });
    if (!res.ok) {
      throw new Error(`HTTP ${res.status}`);
    }
    return (await res.json()) as T;
  } finally {
    clearTimeout(timer);
  }
}

export const agentClient = {
  checkHealth: () => fetchAgent<AgentHealth>('/api/health', {}, 1200),

  getAutoClickStatus: () => fetchAgent<OsAutoClickStatus>('/api/autoclick/status', {}, 1500),

  startOsAutoClick: (payload: {
    intervalMs: number;
    button: 'left' | 'right' | 'middle';
    clickType: 'single' | 'double';
    repeatCount: number;
    useRecordedPoints: boolean;
    points?: CursorPoint[];
  }) =>
    fetchAgent<OsAutoClickStatus>('/api/autoclick/start', {
      method: 'POST',
      body: JSON.stringify(payload),
    }),

  stopOsAutoClick: () =>
    fetchAgent<OsAutoClickStatus>('/api/autoclick/stop', {
      method: 'POST',
    }),

  captureCursorPoint: (delaySeconds = 3) =>
    fetchAgent<{ captured: CursorPoint; status: OsAutoClickStatus }>(
      '/api/autoclick/capture',
      {
        method: 'POST',
        body: JSON.stringify({ delaySeconds }),
      },
      (delaySeconds + 3) * 1000
    ),

  clearCapturedPoints: () =>
    fetchAgent<OsAutoClickStatus>('/api/autoclick/points/clear', {
      method: 'POST',
    }),

  scheduleOsPower: (mode: 'shutdown' | 'restart' | 'hibernate', seconds: number) =>
    fetchAgent<{ scheduled: boolean }>('/api/system/schedule', {
      method: 'POST',
      body: JSON.stringify({ mode, seconds }),
    }),

  cancelOsPower: () =>
    fetchAgent<{ cancelled: boolean }>('/api/system/cancel', {
      method: 'POST',
    }),

  getOsWindows: () => fetchAgent<OsWindowInfo[]>('/api/windows', {}, 2500),

  pinOsWindow: (handle: number, topMost: boolean) =>
    fetchAgent<{ success: boolean }>('/api/windows/pin', {
      method: 'POST',
      body: JSON.stringify({ handle, topMost }),
    }),

  setOsWindowOpacity: (handle: number, percent: number) =>
    fetchAgent<{ success: boolean }>('/api/windows/opacity', {
      method: 'POST',
      body: JSON.stringify({ handle, percent }),
    }),

  focusOsWindow: (handle: number) =>
    fetchAgent<{ success: boolean }>('/api/windows/focus', {
      method: 'POST',
      body: JSON.stringify({ handle }),
    }),
};
