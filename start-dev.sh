#!/bin/bash

# Egnyte SharePoint Sync - Local Development Starter

echo "🚀 Starting Egnyte SharePoint Sync Application"
echo "=============================================="

# Check if required tools are installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK is not installed. Please install .NET 6.0 or later."
    exit 1
fi

if ! command -v npm &> /dev/null; then
    echo "❌ Node.js/npm is not installed. Please install Node.js 18 or later."
    exit 1
fi

if ! command -v func &> /dev/null; then
    echo "❌ Azure Functions Core Tools is not installed."
    echo "Please install: npm install -g azure-functions-core-tools@4"
    exit 1
fi

echo "✅ All required tools are installed"
echo ""

# Check for environment variables
if [ ! -f ".env" ]; then
    echo "⚠️  No .env file found. Please copy .env.example to .env and configure your settings."
    echo "   cp .env.example .env"
    echo ""
fi

# Start backend
echo "🔧 Starting Azure Functions backend..."
cd backend
func start --port 7071 &
BACKEND_PID=$!
cd ..

# Wait a moment for backend to start
sleep 3

# Start frontend
echo "🎨 Starting React frontend..."
cd frontend
npm run dev &
FRONTEND_PID=$!
cd ..

echo ""
echo "🎉 Application started successfully!"
echo "=================================="
echo ""
echo "📱 Frontend:  http://localhost:3000"
echo "⚙️  Backend:   http://localhost:7071"
echo ""
echo "💡 Tips:"
echo "   • Configure your .env file with Egnyte and Azure credentials"
echo "   • Check the README.md for setup instructions"
echo "   • Press Ctrl+C to stop both services"
echo ""

# Function to cleanup background processes
cleanup() {
    echo ""
    echo "🛑 Stopping services..."
    kill $BACKEND_PID 2>/dev/null
    kill $FRONTEND_PID 2>/dev/null
    echo "✅ Services stopped"
    exit 0
}

# Trap Ctrl+C and cleanup
trap cleanup INT

# Wait for user to stop
echo "Press Ctrl+C to stop all services..."
wait
