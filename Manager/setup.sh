#!/bin/bash
set -e

# =========================================
# python3 / venv チェック
# =========================================
if ! command -v python3 >/dev/null 2>&1; then
    echo "Python3 is not installed. Please install Python 3 and try again."
    exit 1
fi

PYTHON_VENV_CHECK=$(python3 -m venv /tmp/test_venv 2>&1 || true)
if [ -d "/tmp/test_venv" ]; then rm -rf /tmp/test_venv; fi
if echo "$PYTHON_VENV_CHECK" | grep -q "ensurepip"; then
    echo "python3-venv is not installed. Installing..."
    sudo apt update
    sudo apt install -y python3-venv
fi

# =========================================
# 変数設定
# =========================================
APP_DIR="/opt/app"
SERVICE_NAME="cone_deploy_service"
USER_NAME=$(whoami)   # systemdで使用するユーザー（通常ユーザー）
SERVICE_PORT=9000

# =========================================
# ディレクトリ作成
# =========================================
echo "Creating application directory..."
sudo mkdir -p "$APP_DIR"
sudo chown "$USER_NAME:$USER_NAME" "$APP_DIR"

# =========================================
# アプリコードコピー
# =========================================
echo "Copying application code..."
cp -r ./app/* "$APP_DIR/"

# =========================================
# 仮想環境作成 & 依存インストール
# =========================================
echo "Setting up Python virtual environment..."
cd "$APP_DIR"
python3 -m venv venv
source venv/bin/activate
pip install --upgrade pip
pip install -r requirements.txt

# =========================================
# systemd サービス作成
# =========================================
echo "Creating systemd service..."
SERVICE_FILE="/etc/systemd/system/${SERVICE_NAME}.service"

sudo tee "$SERVICE_FILE" > /dev/null <<EOF
[Unit]
Description=Cone Deploy Service
After=network.target

[Service]
User=$USER_NAME
Group=$USER_NAME
WorkingDirectory=$APP_DIR
ExecStart=$APP_DIR/venv/bin/uvicorn main:app --host 127.0.0.1 --port $SERVICE_PORT --workers 1
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
sudo systemctl enable "$SERVICE_NAME.service"
sudo systemctl start "$SERVICE_NAME.service"

echo "Setup complete. Check service status with:"
echo "  systemctl status $SERVICE_NAME.service"
echo "  journalctl -u $SERVICE_NAME.service -f"
