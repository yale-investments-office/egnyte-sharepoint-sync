# Egnyte SharePoint Sync

A complete Azure application that syncs data from the Egnyte API to a SharePoint Online Teams document library. The application features a modern React web interface for browsing Egnyte files and selecting them for synchronization to SharePoint.

## Architecture

- **Frontend**: React + TypeScript (deployed as Azure Static Web App)
- **Backend**: .NET Azure Functions (serverless API)
- **Infrastructure**: Azure Static Web Apps + Azure Functions

## Features

### Web Interface
- 🔐 **OAuth Authentication**: Secure login for both Egnyte and SharePoint
- 📁 **File Browser**: Navigate through Egnyte folders and files
- ✅ **Multi-select**: Choose multiple files for synchronization
- 🎯 **Destination Picker**: Select SharePoint site and document library
- 📊 **Sync Progress**: Real-time progress tracking with visual indicators
- 📱 **Responsive Design**: Works on desktop and mobile devices

### Backend API
- **Egnyte Integration**: OAuth flow, file listing, and download
- **SharePoint Integration**: Graph API, site/library browsing, file upload
- **Sync Engine**: Orchestrates file transfer from Egnyte to SharePoint
- **Error Handling**: Comprehensive error handling and logging

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- Node.js 18+ 
- Azure Functions Core Tools
- Azure CLI (for deployment)

### Local Development

1. **Clone and setup**:
   ```bash
   git clone <repository-url>
   cd Egnyte_SPO_AI_Sync
   ```

2. **Configure environment variables**:
   ```bash
   cp .env.example .env
   # Edit .env with your Egnyte and Azure app credentials
   ```

3. **Start the backend**:
   ```bash
   cd backend
   func start
   ```

4. **Start the frontend** (in a new terminal):
   ```bash
   cd frontend
   npm run dev
   ```

5. **Open the application**: Navigate to http://localhost:3000

### Configuration

#### Egnyte Setup
1. Create an Egnyte developer app at https://developers.egnyte.com/
2. Configure redirect URI: `http://localhost:3000` (local) or your deployed URL
3. Note down your domain, client ID, and client secret

#### Azure/SharePoint Setup
1. Register an Azure AD application in the Azure Portal
2. Configure API permissions for Microsoft Graph (Sites.Read.All, Files.ReadWrite.All)
3. Add redirect URI: `http://localhost:3000` (local) or your deployed URL
4. Note down your tenant ID, client ID, and client secret

## Deployment

### Using Azure Developer CLI (azd)

1. **Initialize azd** (if not already done):
   ```bash
   azd init
   ```

2. **Deploy to Azure**:
   ```bash
   azd up
   ```

This will:
- Create Azure Static Web App for the frontend
- Create Azure Functions App for the backend
- Configure all necessary Azure resources
- Deploy both frontend and backend

### Manual Deployment

#### Frontend (Static Web App)
```bash
cd frontend
npm run build
# Deploy dist/ folder to Azure Static Web Apps
```

#### Backend (Functions)
```bash
cd backend
func azure functionapp publish <your-function-app-name>
```

## Project Structure

```
Egnyte_SPO_AI_Sync/
├── azure.yaml                 # Azure Developer CLI configuration
├── .env.example               # Environment variables template
├── frontend/                  # React application
│   ├── src/
│   │   ├── components/        # React components
│   │   ├── hooks/            # Custom React hooks
│   │   ├── services/         # API service layer
│   │   ├── types/            # TypeScript type definitions
│   │   ├── App.tsx           # Main application component
│   │   └── main.tsx          # Application entry point
│   ├── package.json
│   ├── vite.config.ts        # Vite build configuration
│   └── tsconfig.json         # TypeScript configuration
└── backend/                   # Azure Functions
    ├── EgnyteAPI.cs          # Egnyte integration functions
    ├── SharePointSync.cs     # SharePoint integration functions
    ├── backend.csproj        # .NET project file
    ├── host.json             # Functions host configuration
    └── local.settings.json   # Local development settings
```

## API Endpoints

### Egnyte Endpoints
- `GET /api/egnyte/auth-url` - Get OAuth authorization URL
- `POST /api/egnyte/exchange-code` - Exchange authorization code for token
- `POST /api/egnyte/files` - List files and folders
- `POST /api/egnyte/download` - Download file

### SharePoint Endpoints
- `GET /api/sharepoint/auth-url` - Get OAuth authorization URL
- `POST /api/sharepoint/exchange-code` - Exchange authorization code for token
- `POST /api/sharepoint/sites` - List SharePoint sites
- `POST /api/sharepoint/libraries` - List document libraries
- `POST /api/sharepoint/upload` - Upload file to SharePoint
- `POST /api/sharepoint/sync` - Sync multiple files

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test locally
5. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For issues and questions:
1. Check the existing GitHub issues
2. Create a new issue with detailed description
3. Include logs and error messages when applicable

## Security & Configuration

### Azure Key Vault Integration (Recommended)

For production deployments, this application supports Azure Key Vault for secure credential management:

- **Secure Storage**: API keys and secrets stored in Azure Key Vault
- **Managed Identity**: Uses Azure managed identity for authentication
- **Local Development**: Supports both Key Vault and environment variables
- **Automatic Fallback**: Falls back to environment variables if Key Vault is not configured

See [KEYVAULT_SETUP.md](KEYVAULT_SETUP.md) for detailed setup instructions.

### Quick Key Vault Setup

```bash
# Run the automated setup script
./setup-keyvault.sh \
  --resource-group "egnyte-sharepoint-sync-rg" \
  --keyvault-name "egnyte-sp-sync-kv" \
  --location "East US" \
  --egnyte-client-id "your-egnyte-client-id" \
  --egnyte-client-secret "your-egnyte-client-secret" \
  --azure-client-id "your-azure-client-id" \
  --azure-client-secret "your-azure-client-secret" \
  --azure-tenant-id "your-azure-tenant-id"
```

- All API communications use HTTPS
- OAuth tokens are stored securely in browser localStorage
- No sensitive credentials are stored in the frontend code
- Backend uses Azure Key Vault for production secrets (when deployed)
