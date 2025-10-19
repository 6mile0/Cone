#!/bin/bash
set -e

# Check for root privileges
if [ "$EUID" -ne 0 ]; then
  echo "Please run as root or use sudo."
  exit 1
fi

# Define variables
APP_DIR="/opt/app"
APP_FILE="main.py"
SERVICE_NAME="deploy_service"
USER_NAME=$(whoami)  # systemdで使用するユーザー
SERVICE_PORT=9000

# Create application directory
echo "Creating application directory..."
sudo mkdir -p $APP_DIR
sudo chown $USER_NAME:$USER_NAME $APP_DIR

# Copy application code
echo "Copying application code..."
cp $APP_FILE $APP_DIR/$APP_FILE

# Set up Python virtual environment and install dependencies
echo "Setting up Python virtual environment..."
cd $APP_DIR
python3 -m venv venv
source venv/bin/activate
pip install --upgrade pip
pip install fastapi uvicorn[standard]

# Create systemd service file
echo "Creating systemd service..."
SERVICE_FILE="/etc/systemd/system/${SERVICE_NAME}.service"

sudo tee $SERVICE_FILE > /dev/null <<EOF
[Unit]
Description=Cone Deploy Service
After=network.target

[Service]
User=$USER_NAME
Group=$USER_NAME
WorkingDirectory=$APP_DIR
ExecStart=$APP_DIR/venv/bin/uvicorn $APP_FILE:app --host 127.0.0.1 --port $SERVICE_PORT --workers 1
Restart=always
RestartSec=5s

[Install]
WantedBy=multi-user.target
EOF

# Reload systemd, enable and start the service
echo "Reloading systemd, enabling and starting service..."
sudo systemctl daemon-reload
sudo systemctl enable $SERVICE_NAME.service
sudo systemctl start $SERVICE_NAME.service

echo "Setup complete. Check service status with:"
echo "  sudo systemctl status $SERVICE_NAME.service"
echo "  journalctl -u $SERVICE_NAME.service -f"
