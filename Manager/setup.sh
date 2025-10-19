#!/bin/bash
set -e

# =========================================
# sudo/root チェック
# =========================================
if [ "$EUID" -ne 0 ]; then
  echo "Please run as root or use sudo."
  exit 1
fi


# =========================================
# 変数設定
# =========================================
APP_DIR="/opt/app"
APP_FILE="deploy_service.py"
SERVICE_NAME="deploy_service"
USER_NAME=$(whoami)  # systemdで使用するユーザー
SERVICE_PORT=9000

# =========================================
# ディレクトリ作成
# =========================================
echo "Creating application directory..."
sudo mkdir -p $APP_DIR
sudo chown $USER_NAME:$USER_NAME $APP_DIR

# =========================================
# アプリコードをコピー（ここでは同ディレクトリのdeploy_service.pyを想定）
# =========================================
echo "Copying application code..."
cp $APP_FILE $APP_DIR/$APP_FILE

# =========================================
# 仮想環境作成 & 依存インストール
# =========================================
echo "Setting up Python virtual environment..."
cd $APP_DIR
python3 -m venv venv
source venv/bin/activate
pip install --upgrade pip
pip install fastapi uvicorn[standard]

# =========================================
# systemd サービスファイル作成
# =========================================
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

# =========================================
# systemd反映 & 起動
# =========================================
echo "Reloading systemd, enabling and starting service..."
sudo systemctl daemon-reload
sudo systemctl enable $SERVICE_NAME.service
sudo systemctl start $SERVICE_NAME.service

echo "Setup complete. Check service status with:"
echo "  sudo systemctl status $SERVICE_NAME.service"
echo "  journalctl -u $SERVICE_NAME.service -f"
