#!/bin/bash

# Script to download vox audio files from impstation PR 1448
# This script helps download the actual audio files needed for the vox sounds port

echo "Downloading vox audio files from impstation PR 1448..."

# Base URL for raw files from the PR
BASE_URL="https://github.com/impstation/imp-station-14/raw/d3c8519db563bd71a8934d607eea7ff386fc03f4/Resources/Audio/_Impstation/Voice/Vox"

# Audio files to download
FILES=(
    "voxchitter1.ogg"
    "voxchitter2.ogg"
    "voxcoo1.ogg"
    "voxcoo2.ogg"
    "voxcry.ogg"
    "voxgasp1.ogg"
    "voxgasp2.ogg"
    "voxgasp3.ogg"
    "voxhiss.ogg"
)

# Create directory if it doesn't exist
mkdir -p "Resources/Audio/_Impstation/Voice/Vox"

# Download each file
for file in "${FILES[@]}"; do
    echo "Downloading $file..."
    curl -L -o "Resources/Audio/_Impstation/Voice/Vox/$file" "$BASE_URL/$file"
    if [ $? -eq 0 ]; then
        echo "✓ Downloaded $file"
    else
        echo "✗ Failed to download $file"
    fi
done

echo "Download complete! Run 'git add Resources/Audio/_Impstation/Voice/Vox/*.ogg' to stage the audio files."