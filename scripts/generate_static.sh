#!/bin/bash
set -e

# Starts the ASP.NET server, spiders it, and saves to dist/

echo "Building and starting ASP.NET Core server..."
ASPNETCORE_ENVIRONMENT=Development dotnet run --configuration Release --urls "http://localhost:5076" &
SERVER_PID=$!

echo "Waiting for server to start..."
timeout 30 bash -c 'while ! curl -s -k -L http://localhost:5076 > /dev/null; do sleep 1; done' || false

echo "Spidering the site using wget..."
mkdir -p dist
cd dist

wget --recursive \
     --page-requisites \
     --html-extension \
     --convert-links \
     --restrict-file-names=windows \
     --domains localhost \
     --no-parent \
     --no-check-certificate \
     --quiet \
     --show-progress \
     http://localhost:5076/ || true

echo "Copying scraped files to dist root..."
if [ -d "localhost+5076" ]; then
    cp -r localhost+5076/* ./
    rm -rf localhost+5076
fi

cd ..

echo "Stopping ASP.NET Core server..."
kill $SERVER_PID
wait $SERVER_PID 2>/dev/null || true

echo "Static generation completed in dist/"
