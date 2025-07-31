export interface EgnyteFile {
  name: string;
  path: string;
  is_folder: boolean;
  size?: number;
  lastModified?: string;
  checksum?: string;
}

export interface EgnyteFolder {
  name: string;
  path: string;
  folders?: EgnyteFolder[];
  files?: EgnyteFile[];
}

export interface SharePointSite {
  id: string;
  displayName: string;
  webUrl: string;
}

export interface SharePointDocumentLibrary {
  id: string;
  name: string;
  displayName: string;
  webUrl: string;
}

export interface SyncJob {
  id: string;
  status: 'pending' | 'running' | 'completed' | 'failed';
  sourceFiles: EgnyteFile[];
  targetLibrary: SharePointDocumentLibrary;
  progress: number;
  message?: string;
  startTime: Date;
  endTime?: Date;
}

export interface AuthState {
  egnyteToken?: string;
  sharePointToken?: string;
  isEgnyteAuthenticated: boolean;
  isSharePointAuthenticated: boolean;
}

export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  error?: string;
}
