# SharePoint Site Configuration Guide

This guide explains how to configure SharePoint sites for syncing with your Egnyte files.

## Configuration Options

### 1. **Dynamic Site Selection (Default)**
Users can browse and select any SharePoint site and document library they have access to.

**Pros:**
- Maximum flexibility
- Users can sync to different sites/libraries as needed
- No hardcoded dependencies

**Cons:**
- Requires user interaction every time
- More complex UI flow

### 2. **Pre-configured Target Site**
Set a default SharePoint site and library that all syncs will target.

**Pros:**
- Simplified user experience
- Consistent sync destination
- Can still allow site selection if needed

**Cons:**
- Less flexible
- Requires admin configuration

## How to Configure a Default SharePoint Site

### Step 1: Find Your SharePoint Site Information

1. **Navigate to your SharePoint site** in a web browser
2. **Copy the site URL** (e.g., `https://yourtenant.sharepoint.com/sites/your-site`)
3. **Get the Site ID** using one of these methods:

#### Method A: Using PowerShell
```powershell
Connect-PnPOnline -Url "https://yourtenant.sharepoint.com/sites/your-site" -Interactive
Get-PnPSite | Select Id
```

#### Method B: Using Graph Explorer
1. Go to https://developer.microsoft.com/en-us/graph/graph-explorer
2. Sign in with your Microsoft account
3. Run: `GET https://graph.microsoft.com/v1.0/sites/yourtenant.sharepoint.com:/sites/your-site`
4. Copy the `id` field from the response

#### Method C: Using the API (after authentication)
```bash
curl -H "Authorization: Bearer YOUR_TOKEN" \
  "https://graph.microsoft.com/v1.0/sites/yourtenant.sharepoint.com:/sites/your-site"
```

### Step 2: Find Document Library Information

1. **Navigate to the document library** (e.g., "Documents", "Shared Documents")
2. **Get the Library ID** using Graph API:

```bash
curl -H "Authorization: Bearer YOUR_TOKEN" \
  "https://graph.microsoft.com/v1.0/sites/YOUR_SITE_ID/lists?$filter=baseTemplate eq 101"
```

### Step 3: Update Environment Variables

Add these to your `.env` file:

```bash
# SharePoint Target Configuration
SHAREPOINT_SITE_URL=https://yourtenant.sharepoint.com/sites/your-target-site
SHAREPOINT_SITE_ID=yourtenant.sharepoint.com,12345678-1234-1234-1234-123456789012,87654321-4321-4321-4321-210987654321
SHAREPOINT_LIBRARY_NAME=Documents
SHAREPOINT_LIBRARY_ID=12345678-1234-1234-1234-123456789012
```

## Configuration Examples

### Example 1: Company Documents Site
```bash
SHAREPOINT_SITE_URL=https://contoso.sharepoint.com/sites/CompanyDocs
SHAREPOINT_SITE_ID=contoso.sharepoint.com,a1b2c3d4-e5f6-7890-1234-567890abcdef,f1e2d3c4-b5a6-9870-4321-fedcba098765
SHAREPOINT_LIBRARY_NAME=Shared Documents
SHAREPOINT_LIBRARY_ID=a1b2c3d4-e5f6-7890-1234-567890abcdef
```

### Example 2: Team Project Site
```bash
SHAREPOINT_SITE_URL=https://contoso.sharepoint.com/sites/ProjectAlpha
SHAREPOINT_SITE_ID=contoso.sharepoint.com,11111111-2222-3333-4444-555555555555,66666666-7777-8888-9999-000000000000
SHAREPOINT_LIBRARY_NAME=Project Files
SHAREPOINT_LIBRARY_ID=11111111-2222-3333-4444-555555555555
```

## Hybrid Configuration

You can combine both approaches:

1. **Set default values** for common use cases
2. **Allow user selection** when they need different destinations

The application will:
- Show the default site as the first option (marked as "Default")
- Allow users to select other sites if needed
- Use the default automatically if no selection is made

## API Endpoints for Configuration

The backend now includes these endpoints:

- `GET /api/sharepoint/config` - Get current SharePoint configuration
- `GET /api/sharepoint/sites` - Get available sites (includes default if configured)
- `POST /api/sharepoint/libraries` - Get libraries for a site (includes default if applicable)

## Testing Your Configuration

1. Set your environment variables
2. Restart the backend: `cd backend && func start`
3. Test the config endpoint: `curl http://localhost:7071/api/sharepoint/config`
4. Verify the response includes your default site information

## Troubleshooting

### Common Issues:

1. **Invalid Site ID**: Make sure the site ID format is correct (includes tenant, site guid, and web guid)
2. **Permissions**: Ensure your Azure app has proper permissions to access the SharePoint site
3. **Library Not Found**: Verify the library ID is correct and the library exists
4. **URL Format**: Ensure the site URL doesn't have trailing slashes

### Getting More Information:

Use the Graph Explorer to test your IDs:
- Site: `https://graph.microsoft.com/v1.0/sites/YOUR_SITE_ID`
- Libraries: `https://graph.microsoft.com/v1.0/sites/YOUR_SITE_ID/lists`

This configuration gives you complete control over how users interact with SharePoint sites while maintaining flexibility for different use cases!
