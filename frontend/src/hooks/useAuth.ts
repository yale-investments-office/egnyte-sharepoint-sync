import { useState, useEffect } from 'react';
import { AuthState } from '../types';

const AUTH_STORAGE_KEY = 'egnyte-sharepoint-auth';

export const useAuth = () => {
  const [authState, setAuthState] = useState<AuthState>({
    isEgnyteAuthenticated: false,
    isSharePointAuthenticated: false,
  });

  useEffect(() => {
    // Load auth state from localStorage on mount
    const savedAuth = localStorage.getItem(AUTH_STORAGE_KEY);
    if (savedAuth) {
      try {
        const parsedAuth = JSON.parse(savedAuth);
        setAuthState({
          ...parsedAuth,
          isEgnyteAuthenticated: !!parsedAuth.egnyteToken,
          isSharePointAuthenticated: !!parsedAuth.sharePointToken,
        });
      } catch (error) {
        console.error('Failed to parse saved auth state:', error);
      }
    }
  }, []);

  const updateAuthState = (updates: Partial<AuthState>) => {
    const newState = {
      ...authState,
      ...updates,
      isEgnyteAuthenticated: !!(updates.egnyteToken ?? authState.egnyteToken),
      isSharePointAuthenticated: !!(updates.sharePointToken ?? authState.sharePointToken),
    };
    
    setAuthState(newState);
    localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(newState));
  };

  const clearAuth = () => {
    const emptyState: AuthState = {
      isEgnyteAuthenticated: false,
      isSharePointAuthenticated: false,
    };
    setAuthState(emptyState);
    localStorage.removeItem(AUTH_STORAGE_KEY);
  };

  return {
    authState,
    updateAuthState,
    clearAuth,
  };
};
