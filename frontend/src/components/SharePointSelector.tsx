import React, { useState, useEffect } from 'react';
import { SharePointSite, SharePointDocumentLibrary } from '../types';
import { apiService } from '../services/api';

interface SharePointSelectorProps {
  token: string;
  onLibrarySelect: (site: SharePointSite, library: SharePointDocumentLibrary) => void;
  selectedLibrary?: SharePointDocumentLibrary;
}

export const SharePointSelector: React.FC<SharePointSelectorProps> = ({
  token,
  onLibrarySelect,
  selectedLibrary,
}) => {
  const [sites, setSites] = useState<SharePointSite[]>([]);
  const [selectedSite, setSelectedSite] = useState<SharePointSite | null>(null);
  const [libraries, setLibraries] = useState<SharePointDocumentLibrary[]>([]);
  const [loadingSites, setLoadingSites] = useState(false);
  const [loadingLibraries, setLoadingLibraries] = useState(false);

  useEffect(() => {
    loadSites();
  }, [token]);

  const loadSites = async () => {
    setLoadingSites(true);
    try {
      const response = await apiService.getSharePointSites(token);
      if (response.success && response.data) {
        setSites(response.data);
      } else {
        alert(`Failed to load SharePoint sites: ${response.error}`);
      }
    } catch (error) {
      alert('Error loading SharePoint sites');
    } finally {
      setLoadingSites(false);
    }
  };

  const loadLibraries = async (site: SharePointSite) => {
    setLoadingLibraries(true);
    try {
      const response = await apiService.getSharePointLibraries(site.id, token);
      if (response.success && response.data) {
        setLibraries(response.data);
      } else {
        alert(`Failed to load document libraries: ${response.error}`);
      }
    } catch (error) {
      alert('Error loading document libraries');
    } finally {
      setLoadingLibraries(false);
    }
  };

  const handleSiteSelect = (site: SharePointSite) => {
    setSelectedSite(site);
    setLibraries([]);
    loadLibraries(site);
  };

  const handleLibrarySelect = (library: SharePointDocumentLibrary) => {
    if (selectedSite) {
      onLibrarySelect(selectedSite, library);
    }
  };

  if (loadingSites) {
    return <div className="loading">Loading SharePoint sites...</div>;
  }

  return (
    <div className="sharepoint-selector">
      <div className="selector-section">
        <h3>Select SharePoint Site</h3>
        <div className="sites-list">
          {sites.map((site) => (
            <div
              key={site.id}
              className={`site-item ${selectedSite?.id === site.id ? 'selected' : ''}`}
              onClick={() => handleSiteSelect(site)}
            >
              <span className="site-icon">🏢</span>
              <div className="site-details">
                <div className="site-name">{site.displayName}</div>
                <div className="site-url">{site.webUrl}</div>
              </div>
            </div>
          ))}
        </div>
      </div>

      {selectedSite && (
        <div className="selector-section">
          <h3>Select Document Library</h3>
          {loadingLibraries ? (
            <div className="loading">Loading document libraries...</div>
          ) : (
            <div className="libraries-list">
              {libraries.map((library) => (
                <div
                  key={library.id}
                  className={`library-item ${selectedLibrary?.id === library.id ? 'selected' : ''}`}
                  onClick={() => handleLibrarySelect(library)}
                >
                  <span className="library-icon">📚</span>
                  <div className="library-details">
                    <div className="library-name">{library.displayName}</div>
                    <div className="library-description">{library.name}</div>
                  </div>
                </div>
              ))}
              
              {libraries.length === 0 && (
                <div className="no-libraries">No document libraries found in this site</div>
              )}
            </div>
          )}
        </div>
      )}

      {selectedLibrary && (
        <div className="selection-summary">
          <h4>Selected Destination</h4>
          <div className="selected-destination">
            <div><strong>Site:</strong> {selectedSite?.displayName}</div>
            <div><strong>Library:</strong> {selectedLibrary.displayName}</div>
          </div>
        </div>
      )}
    </div>
  );
};
