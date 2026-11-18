import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import { agentClient, AgentHealth } from '../services/agentClient';

interface AgentContextType {
  isOnline: boolean;
  health: AgentHealth | null;
  checking: boolean;
  refreshAgentStatus: () => Promise<boolean>;
}

const AgentContext = createContext<AgentContextType | undefined>(undefined);

export const AgentProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [isOnline, setIsOnline] = useState(false);
  const [health, setHealth] = useState<AgentHealth | null>(null);
  const [checking, setChecking] = useState(false);

  const refreshAgentStatus = useCallback(async () => {
    setChecking(true);
    try {
      const data = await agentClient.checkHealth();
      const ok = data?.status === 'online';
      setIsOnline(ok);
      setHealth(ok ? data : null);
      return ok;
    } catch {
      setIsOnline(false);
      setHealth(null);
      return false;
    } finally {
      setChecking(false);
    }
  }, []);

  useEffect(() => {
    refreshAgentStatus();
    const interval = setInterval(() => {
      refreshAgentStatus();
    }, 5000);
    return () => clearInterval(interval);
  }, [refreshAgentStatus]);

  return (
    <AgentContext.Provider value={{ isOnline, health, checking, refreshAgentStatus }}>
      {children}
    </AgentContext.Provider>
  );
};

export const useOsAgent = (): AgentContextType => {
  const ctx = useContext(AgentContext);
  if (!ctx) {
    throw new Error('useOsAgent must be used within an AgentProvider');
  }
  return ctx;
};
