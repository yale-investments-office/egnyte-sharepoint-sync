import { useState } from 'react';
import { Authentication } from './components/Authentication';
import { EgnyteBrowser } from './components/EgnyteBrowser';
import { SharePointSelector } from './components/SharePointSelector';
import { SyncInterface } from './components/SyncInterface';
import { useAuth } from './hooks/useAuth';
import { EgnyteFile, SharePointSite, SharePointDocumentLibrary } from './types';
import './App.css';

type AppStep = 'auth' | 'browse' | 'sync';

function App() {
  const { authState } = useAuth();
  const [currentStep, setCurrentStep] = useState<AppStep>('auth');
  const [selectedFiles, setSelectedFiles] = useState<EgnyteFile[]>([]);
  const [selectedSite, setSelectedSite] = useState<SharePointSite | null>(null);
  const [selectedLibrary, setSelectedLibrary] = useState<SharePointDocumentLibrary | null>(null);

  const handleAuthComplete = () => {
    setCurrentStep('browse');
  };

  const handleLibrarySelect = (site: SharePointSite, library: SharePointDocumentLibrary) => {
    setSelectedSite(site);
    setSelectedLibrary(library);
  };

  const canProceedToSync = selectedFiles.length > 0 && selectedSite && selectedLibrary;

  const handleStartSync = () => {
    if (canProceedToSync) {
      setCurrentStep('sync');
    }
  };

  const handleSyncComplete = () => {
    // Reset state and go back to browse
    setSelectedFiles([]);
    setSelectedSite(null);
    setSelectedLibrary(null);
    setCurrentStep('browse');
  };

  const goBackToBrowse = () => {
    setCurrentStep('browse');
  };

  return (
    <div className="App">
      <header className="app-header">
        <h1>Egnyte SharePoint Sync</h1>
        <div className="step-indicator">
          <span className={`step ${currentStep === 'auth' ? 'active' : 'completed'}`}>
            1. Authentication
          </span>
          <span className={`step ${currentStep === 'browse' ? 'active' : currentStep === 'sync' ? 'completed' : ''}`}>
            2. Browse & Select
          </span>
          <span className={`step ${currentStep === 'sync' ? 'active' : ''}`}>
            3. Sync
          </span>
        </div>
      </header>

      <main className="app-main">
        {currentStep === 'auth' && (
          <Authentication onAuthComplete={handleAuthComplete} />
        )}

        {currentStep === 'browse' && authState.egnyteToken && authState.sharePointToken && (
          <div className="browse-container">
            <div className="browse-section">
              <EgnyteBrowser
                token={authState.egnyteToken}
                onFileSelect={setSelectedFiles}
                selectedFiles={selectedFiles}
              />
            </div>
            
            <div className="browse-section">
              <SharePointSelector
                token={authState.sharePointToken}
                onLibrarySelect={handleLibrarySelect}
                selectedLibrary={selectedLibrary || undefined}
              />
            </div>

            {canProceedToSync && (
              <div className="proceed-section">
                <button onClick={handleStartSync} className="proceed-button">
                  Proceed to Sync ({selectedFiles.length} files)
                </button>
              </div>
            )}
          </div>
        )}

        {currentStep === 'sync' && selectedSite && selectedLibrary && authState.egnyteToken && authState.sharePointToken && (
          <div className="sync-container">
            <button onClick={goBackToBrowse} className="back-button">
              ← Back to Browse
            </button>
            <SyncInterface
              selectedFiles={selectedFiles}
              selectedSite={selectedSite}
              selectedLibrary={selectedLibrary}
              egnyteToken={authState.egnyteToken}
              sharePointToken={authState.sharePointToken}
              onSyncComplete={handleSyncComplete}
            />
          </div>
        )}
      </main>
    </div>
  );
}

export default App;
