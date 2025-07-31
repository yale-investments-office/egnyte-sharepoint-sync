#!/bin/bash

# Test script for mock API endpoints
echo "Testing Egnyte SharePoint Sync Mock API"
echo "======================================"

BASE_URL="http://localhost:7071/api/mock"

echo ""
echo "1. Testing SharePoint Config..."
curl -s "$BASE_URL/sharepoint/config" | jq '.'

echo ""
echo "2. Testing SharePoint Sites..."
curl -s "$BASE_URL/sharepoint/sites" | jq '.'

echo ""
echo "3. Testing SharePoint Libraries..."
curl -s "$BASE_URL/sharepoint/libraries?siteId=contoso.sharepoint.com,12345678-1234-1234-1234-123456789012,87654321-4321-4321-4321-210987654321" | jq '.'

echo ""
echo "4. Testing Egnyte Auth URL..."
curl -s "$BASE_URL/egnyte/auth-url" | jq '.'

echo ""
echo "5. Testing Egnyte Files (Root)..."
curl -s "$BASE_URL/egnyte/files" | jq '.'

echo ""
echo "6. Testing Egnyte Files (Documents folder)..."
curl -s "$BASE_URL/egnyte/files?path=/Shared/Documents" | jq '.'

echo ""
echo "7. Testing Sync Operation..."
curl -s -X POST "$BASE_URL/sharepoint/sync" \
  -H "Content-Type: application/json" \
  -d '{
    "files": [
      {"name": "Company Overview.pdf", "path": "/Shared/Company Overview.pdf"},
      {"name": "Contact List.xlsx", "path": "/Shared/Contact List.xlsx"}
    ],
    "targetSiteId": "contoso.sharepoint.com,12345678-1234-1234-1234-123456789012,87654321-4321-4321-4321-210987654321",
    "targetLibraryId": "lib-12345678-1234-1234-1234-123456789012"
  }' | jq '.'

echo ""
echo "Test completed!"
