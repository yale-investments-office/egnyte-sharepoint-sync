import { 
  EgnyteFile, 
  EgnyteFolder, 
  SharePointSite, 
  SharePointDocumentLibrary, 
  SyncJob,
  ApiResponse 
} from '../types';

const API_BASE = 'http://localhost:7071';
const IS_MOCK_MODE = true; // Set to true for mock testing

class ApiService {
  private getEndpoint(path: string, useMock: boolean = IS_MOCK_MODE): string {
    const mockPrefix = useMock ? '/mock' : '';
    return `${API_BASE}/api${mockPrefix}${path}`;
  }

  // Egnyte API methods
  async getEgnyteAuthUrl(): Promise<ApiResponse<{ authUrl: string }>> {
    try {
      const response = await fetch(this.getEndpoint('/egnyte/auth-url'));
      return await response.json();
    } catch (error) {
      return { success: false, error: 'Failed to get Egnyte auth URL' };
    }
  }

  async exchangeEgnyteCode(code: string): Promise<ApiResponse<{ token: string }>> {
    try {
      const response = await fetch(this.getEndpoint('/egnyte/exchange-code'), {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ code }),
      });
      return await response.json();
    } catch (error) {
      return { success: false, error: 'Failed to exchange Egnyte code' };
    }
  }

  async getEgnyteFiles(path: string = '/', token: string): Promise<ApiResponse<EgnyteFolder>> {
    try {
      const url = IS_MOCK_MODE 
        ? `${this.getEndpoint('/egnyte/files')}?path=${encodeURIComponent(path)}`
        : this.getEndpoint('/egnyte/files');
      
      const options: RequestInit = IS_MOCK_MODE 
        ? { method: 'GET' }
        : {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ path, token }),
          };

      const response = await fetch(url, options);
      return await response.json();
    } catch (error) {
      return { success: false, error: 'Failed to get Egnyte files' };
    }
  }

  async downloadEgnyteFile(filePath: string, token: string): Promise<ApiResponse<{ downloadUrl: string }>> {
    try {
      const response = await fetch(this.getEndpoint('/egnyte/download'), {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ filePath, token }),
      });
      return await response.json();
    } catch (error) {
      return { success: false, error: 'Failed to download Egnyte file' };
    }
  }

  // SharePoint API methods
  async getSharePointAuthUrl(): Promise<ApiResponse<{ authUrl: string }>> {
    try {
      const response = await fetch(this.getEndpoint('/sharepoint/auth-url'));
      return await response.json();
    } catch (error) {
      return { success: false, error: 'Failed to get SharePoint auth URL' };
    }
  }

  async exchangeSharePointCode(code: string): Promise<ApiResponse<{ token: string }>> {
    try {
      const response = await fetch(this.getEndpoint('/sharepoint/exchange-code'), {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ code }),
      });
      return await response.json();
    } catch (error) {
      return { success: false, error: 'Failed to exchange SharePoint code' };
    }
  }

  async getSharePointSites(token: string): Promise<ApiResponse<SharePointSite[]>> {
    try {
      const url = this.getEndpoint('/sharepoint/sites');
      const options: RequestInit = IS_MOCK_MODE 
        ? { method: 'GET' }
        : {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ token }),
          };

      const response = await fetch(url, options);
      return await response.json();
    } catch (error) {
      return { success: false, error: 'Failed to get SharePoint sites' };
    }
  }

  async getSharePointLibraries(siteId: string, token: string): Promise<ApiResponse<SharePointDocumentLibrary[]>> {
    try {
      const url = IS_MOCK_MODE 
        ? `${this.getEndpoint('/sharepoint/libraries')}?siteId=${encodeURIComponent(siteId)}`
        : this.getEndpoint('/sharepoint/libraries');
      
      const options: RequestInit = IS_MOCK_MODE 
        ? { method: 'GET' }
        : {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ siteId, token }),
          };

      const response = await fetch(url, options);
      return await response.json();
    } catch (error) {
      return { success: false, error: 'Failed to get SharePoint libraries' };
    }
  }

  async uploadToSharePoint(
    siteId: string,
    libraryId: string,
    fileName: string,
    fileContent: string,
    token: string
  ): Promise<ApiResponse<{ uploadId: string }>> {
    try {
      const response = await fetch(this.getEndpoint('/sharepoint/upload'), {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ siteId, libraryId, fileName, fileContent, token }),
      });
      return await response.json();
    } catch (error) {
      return { success: false, error: 'Failed to upload to SharePoint' };
    }
  }

  async syncFiles(
    files: EgnyteFile[],
    siteId: string,
    libraryId: string,
    egnyteToken: string,
    sharePointToken: string
  ): Promise<ApiResponse<SyncJob>> {
    try {
      const response = await fetch(this.getEndpoint('/sharepoint/sync'), {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          files,
          siteId,
          libraryId,
          egnyteToken,
          sharePointToken,
        }),
      });
      return await response.json();
    } catch (error) {
      return { success: false, error: 'Failed to start sync' };
    }
  }
}

export const apiService = new ApiService();
