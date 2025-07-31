import React, { useState } from 'react';
import { EgnyteFile, SharePointSite, SharePointDocumentLibrary, SyncJob } from '../types';
import { apiService } from '../services/api';

interface SyncInterfaceProps {
  selectedFiles: EgnyteFile[];
  selectedSite: SharePointSite;
  selectedLibrary: SharePointDocumentLibrary;
  egnyteToken: string;
  sharePointToken: string;
  onSyncComplete: () => void;
}

export const SyncInterface: React.FC<SyncInterfaceProps> = ({
  selectedFiles,
  selectedSite,
  selectedLibrary,
  egnyteToken,
  sharePointToken,
  onSyncComplete,
}) => {
  const [syncJob, setSyncJob] = useState<SyncJob | null>(null);
  const [syncing, setSyncing] = useState(false);

  const startSync = async () => {
    setSyncing(true);
    try {
      const response = await apiService.syncFiles(
        selectedFiles,
        selectedSite.id,
        selectedLibrary.id,
        egnyteToken,
        sharePointToken
      );

      if (response.success && response.data) {
        setSyncJob(response.data);
        // For demo purposes, simulate progress updates
        simulateProgress(response.data);
      } else {
        alert(`Failed to start sync: ${response.error}`);
        setSyncing(false);
      }
    } catch (error) {
      alert('Error starting sync');
      setSyncing(false);
    }
  };

  const simulateProgress = (_job: SyncJob) => {
    // In a real implementation, you would poll the backend for progress updates
    let progress = 0;
    const interval = setInterval(() => {
      progress += Math.random() * 20;
      if (progress >= 100) {
        progress = 100;
        clearInterval(interval);
        setSyncJob(prev => prev ? { ...prev, status: 'completed', progress: 100 } : null);
        setSyncing(false);
        setTimeout(() => {
          onSyncComplete();
        }, 2000);
      } else {
        setSyncJob(prev => prev ? { ...prev, progress: Math.round(progress) } : null);
      }
    }, 1000);
  };

  const formatFileSize = (bytes?: number) => {
    if (!bytes) return 'Unknown';
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(1024));
    return Math.round(bytes / Math.pow(1024, i) * 100) / 100 + ' ' + sizes[i];
  };

  const totalSize = selectedFiles.reduce((sum, file) => sum + (file.size || 0), 0);

  return (
    <div className="sync-interface">
      <div className="sync-summary">
        <h3>Sync Summary</h3>
        
        <div className="sync-details">
          <div className="detail-section">
            <h4>Source (Egnyte)</h4>
            <div className="file-count">{selectedFiles.length} files selected</div>
            <div className="total-size">Total size: {formatFileSize(totalSize)}</div>
          </div>
          
          <div className="sync-arrow">→</div>
          
          <div className="detail-section">
            <h4>Destination (SharePoint)</h4>
            <div className="site-name">{selectedSite.displayName}</div>
            <div className="library-name">{selectedLibrary.displayName}</div>
          </div>
        </div>

        <div className="selected-files">
          <h4>Files to Sync:</h4>
          <ul>
            {selectedFiles.map(file => (
              <li key={file.path}>
                <span className="file-name">{file.name}</span>
                <span className="file-size">({formatFileSize(file.size)})</span>
              </li>
            ))}
          </ul>
        </div>
      </div>

      {!syncing && !syncJob && (
        <button onClick={startSync} className="sync-button">
          Start Sync
        </button>
      )}

      {syncJob && (
        <div className="sync-progress">
          <h4>Sync in Progress</h4>
          <div className="progress-bar">
            <div 
              className="progress-fill" 
              data-progress={syncJob.progress}
            ></div>
          </div>
          <div className="progress-text">{syncJob.progress}% complete</div>
          
          {syncJob.status === 'completed' && (
            <div className="sync-success">
              ✅ Sync completed successfully!
            </div>
          )}
          
          {syncJob.status === 'failed' && (
            <div className="sync-error">
              ❌ Sync failed: {syncJob.message}
            </div>
          )}
        </div>
      )}
    </div>
  );
};
