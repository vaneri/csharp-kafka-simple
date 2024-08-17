#!/bin/bash

# Define the path to your applications
APP_DIR="."

# Define the paths to your application folders
APP1="$APP_DIR/KafkaFanInFanOut"
APP2="$APP_DIR/KafkaPartitionConsumer"
APP3="$APP_DIR/KafkaScripting"

# Define colors for logging
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[0;33m'
NC='\033[0m' # No Color

# Create logs directory if it does not exist
mkdir -p "$APP_DIR/logs"

# Function to start an application and log its output
start_app() {
    local app_dir=$1
    local app_name=$2
    local log_file=$3
    local color=$4

    echo -e "${color}Starting $app_name...${NC}"
    (cd "$app_dir" && dotnet run 2>&1 | tee -a "$log_file") &
}

# Start each application in the background and log to a file with colors
start_app "$APP1" "KafkaFanInFanOut" "$APP_DIR/logs/KafkaFanInFanOut.log" "$GREEN"
start_app "$APP2" "KafkaPartitionConsumer" "$APP_DIR/logs/KafkaPartitionConsumer.log" "$YELLOW"
start_app "$APP3" "KafkaScripting" "$APP_DIR/logs/KafkaScripting.log" "$RED"

# Wait for all background processes to complete
wait

echo -e "${GREEN}All applications are running.${NC}"
