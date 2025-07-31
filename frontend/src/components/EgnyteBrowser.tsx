import React, { useState, useEffect } from 'react';
import { EgnyteFile, EgnyteFolder } from '../types';
import { apiService } from '../services/api';

interface EgnyteBrowserProps {
  token: string;
  onFileSelect: (files: EgnyteFile[]) => void;
  selectedFiles: EgnyteFile[];
}

export const EgnyteBrowser: React.FC<EgnyteBrowserProps> = ({
  token,
  onFileSelect,
  selectedFiles,
}) => {
  const [currentFolder, setCurrentFolder] = useState<EgnyteFolder | null>(null);
  const [currentPath, setCurrentPath] = useState('/');
  const [loading, setLoading] = useState(false);
  const [pathHistory, setPathHistory] = useState<string[]>(['/']);

  useEffect(() => {
    loadFolder('/');
  }, [token]);

  const loadFolder = async (path: string) => {
    setLoading(true);
    try {
      const response = await apiService.getEgnyteFiles(path, token);
      if (response.success && response.data) {
        setCurrentFolder(response.data);
        setCurrentPath(path);
      } else {
        alert(`Failed to load folder: ${response.error}`);
      }
    } catch (error) {
      alert('Error loading folder');
    } finally {
      setLoading(false);
    }
  };

  const navigateToFolder = (folder: EgnyteFolder) => {
    setPathHistory(prev => [...prev, folder.path]);
    loadFolder(folder.path);
  };

  const navigateBack = () => {
    if (pathHistory.length > 1) {
      const newHistory = pathHistory.slice(0, -1);
      const previousPath = newHistory[newHistory.length - 1];
      setPathHistory(newHistory);
      loadFolder(previousPath);
    }
  };

  const isFileSelected = (file: EgnyteFile) => {
    return selectedFiles.some(f => f.path === file.path);
  };

  const handleFileToggle = (file: EgnyteFile) => {
    if (isFileSelected(file)) {
      onFileSelect(selectedFiles.filter(f => f.path !== file.path));
    } else {
      onFileSelect([...selectedFiles, file]);
    }
  };

  const formatFileSize = (bytes?: number) => {
    if (!bytes) return 'Unknown';
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(1024));
    return Math.round(bytes / Math.pow(1024, i) * 100) / 100 + ' ' + sizes[i];
  };

  if (loading) {
    return <div className="loading">Loading Egnyte files...</div>;
  }

  return (
    <div className="egnyte-browser">
      <div className="browser-header">
        <h3>Browse Egnyte Files</h3>
        <div className="navigation">
          <button 
            onClick={navigateBack} 
            disabled={pathHistory.length <= 1}
            className="nav-button"
          >
            ← Back
          </button>
          <span className="current-path">{currentPath}</span>
        </div>
      </div>

      {currentFolder && (
        <div className="file-list">
          {/* Folders */}
          {currentFolder.folders?.map((folder) => (
            <div 
              key={folder.path} 
              className="file-item folder"
              onClick={() => navigateToFolder(folder)}
            >
              <span className="file-icon">📁</span>
              <span className="file-name">{folder.name}</span>
              <span className="file-type">Folder</span>
            </div>
          ))}

          {/* Files */}
          {currentFolder.files?.map((file) => (
            <div key={file.path} className="file-item file">
              <input
                type="checkbox"
                checked={isFileSelected(file)}
                onChange={() => handleFileToggle(file)}
                className="file-checkbox"
                aria-label={`Select ${file.name}`}
              />
              <span className="file-icon">📄</span>
              <span className="file-name">{file.name}</span>
              <span className="file-size">{formatFileSize(file.size)}</span>
              <span className="file-modified">
                {file.lastModified ? new Date(file.lastModified).toLocaleDateString() : ''}
              </span>
            </div>
          ))}

          {(!currentFolder.folders?.length && !currentFolder.files?.length) && (
            <div className="empty-folder">This folder is empty</div>
          )}
        </div>
      )}

      {selectedFiles.length > 0 && (
        <div className="selection-summary">
          <h4>Selected Files ({selectedFiles.length})</h4>
          <ul>
            {selectedFiles.map(file => (
              <li key={file.path}>{file.name}</li>
            ))}
          </ul>
        </div>
      )}
    </div>
  );
};
