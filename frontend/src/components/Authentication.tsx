import React from 'react';
import { apiService } from '../services/api';
import { useAuth } from '../hooks/useAuth';

const IS_MOCK_MODE = true; // Set to true for mock testing

interface AuthenticationProps {
  onAuthComplete: () => void;
}

export const Authentication: React.FC<AuthenticationProps> = ({ onAuthComplete }) => {
  const { authState, updateAuthState } = useAuth();

  // Mock authentication for testing
  const handleMockAuth = () => {
    updateAuthState({ 
      egnyteToken: 'mock_egnyte_token',
      sharePointToken: 'mock_sharepoint_token'
    });
  };

  const handleEgnyteAuth = async () => {
    if (IS_MOCK_MODE) {
      updateAuthState({ egnyteToken: 'mock_egnyte_token' });
      return;
    }

    try {
      const response = await apiService.getEgnyteAuthUrl();
      if (response.success && response.data) {
        window.location.href = response.data.authUrl;
      } else {
        alert(`Failed to get Egnyte auth URL: ${response.error}`);
      }
    } catch (error) {
      alert('Error initiating Egnyte authentication');
    }
  };

  const handleSharePointAuth = async () => {
    if (IS_MOCK_MODE) {
      updateAuthState({ sharePointToken: 'mock_sharepoint_token' });
      return;
    }

    try {
      const response = await apiService.getSharePointAuthUrl();
      if (response.success && response.data) {
        window.location.href = response.data.authUrl;
      } else {
        alert(`Failed to get SharePoint auth URL: ${response.error}`);
      }
    } catch (error) {
      alert('Error initiating SharePoint authentication');
    }
  };

  React.useEffect(() => {
    // Handle OAuth callbacks
    const urlParams = new URLSearchParams(window.location.search);
    const code = urlParams.get('code');
    const state = urlParams.get('state');
    
    if (code && state) {
      if (state === 'egnyte') {
        handleEgnyteCallback(code);
      } else if (state === 'sharepoint') {
        handleSharePointCallback(code);
      }
    }
  }, []);

  const handleEgnyteCallback = async (code: string) => {
    try {
      const response = await apiService.exchangeEgnyteCode(code);
      if (response.success && response.data) {
        updateAuthState({ egnyteToken: response.data.token });
        // Clear URL parameters
        window.history.replaceState({}, document.title, window.location.pathname);
      } else {
        alert(`Failed to exchange Egnyte code: ${response.error}`);
      }
    } catch (error) {
      alert('Error exchanging Egnyte code');
    }
  };

  const handleSharePointCallback = async (code: string) => {
    try {
      const response = await apiService.exchangeSharePointCode(code);
      if (response.success && response.data) {
        updateAuthState({ sharePointToken: response.data.token });
        // Clear URL parameters
        window.history.replaceState({}, document.title, window.location.pathname);
      } else {
        alert(`Failed to exchange SharePoint code: ${response.error}`);
      }
    } catch (error) {
      alert('Error exchanging SharePoint code');
    }
  };

  React.useEffect(() => {
    if (authState.isEgnyteAuthenticated && authState.isSharePointAuthenticated) {
      onAuthComplete();
    }
  }, [authState, onAuthComplete]);

  return (
    <div className="authentication-container">
      <div className="auth-card">
        <h1>Egnyte SharePoint Sync</h1>
        <p>Connect to both Egnyte and SharePoint to begin syncing files.</p>
        
        {IS_MOCK_MODE && (
          <div className="mock-mode-banner">
            <h3>🧪 Mock Mode Enabled</h3>
            <p>Testing with mock data</p>
            <button onClick={handleMockAuth} className="auth-button mock">
              Use Mock Authentication
            </button>
          </div>
        )}
        
        <div className="auth-buttons">
          <div className="auth-section">
            <h3>Step 1: Connect to Egnyte</h3>
            {authState.isEgnyteAuthenticated ? (
              <div className="auth-success">✅ Egnyte Connected</div>
            ) : (
              <button onClick={handleEgnyteAuth} className="auth-button egnyte">
                Connect to Egnyte
              </button>
            )}
          </div>
          
          <div className="auth-section">
            <h3>Step 2: Connect to SharePoint</h3>
            {authState.isSharePointAuthenticated ? (
              <div className="auth-success">✅ SharePoint Connected</div>
            ) : (
              <button onClick={handleSharePointAuth} className="auth-button sharepoint">
                Connect to SharePoint
              </button>
            )}
          </div>
        </div>
        
        {authState.isEgnyteAuthenticated && authState.isSharePointAuthenticated && (
          <div className="auth-complete">
            <h3>🎉 Both services connected!</h3>
            <p>You can now browse and sync files.</p>
          </div>
        )}
      </div>
    </div>
  );
};
