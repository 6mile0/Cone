(function() {
    'use strict';

    let notificationAudio = null;
    let audioInitialized = false;
    
    function initializeAudio() {
        if (!audioInitialized) {
            notificationAudio = new Audio('/audio/notify.mp3');
            notificationAudio.load();
            audioInitialized = true;
            document.removeEventListener('click', initializeAudio);
            document.removeEventListener('keydown', initializeAudio);

            const interactionAlert = document.getElementById('interaction-alert');
            if (interactionAlert) {
                interactionAlert.style.display = 'none';
            }
        }
    }

    document.addEventListener('click', initializeAudio, { once: true });
    document.addEventListener('keydown', initializeAudio, { once: true });

    function playNotificationSound() {
        if (!audioInitialized || !notificationAudio) {
            console.warn('音声が初期化されていません。ページ内をクリックしてください。');
            return;
        }
        notificationAudio.currentTime = 0;
        notificationAudio.play().catch(error => {
            console.error('通知音の再生に失敗しました:', error);
        });
    }

    // 視覚効果 - 全画面オーバレイ
    function showVisualNotification(data) {
        const overlay = document.createElement('div');
        overlay.className = 'ticket-notification-overlay';
        overlay.innerHTML = `
            <div class="ticket-notification">
                <div class="notification-content">
                    <h2>🔔 新しいチケット</h2>
                    <div class="notification-info">
                        <div class="notification-info-item">
                            <span class="notification-info-label">グループ</span>
                            <div class="notification-info-value">${data.studentGroupName}</div>
                        </div>
                        <div class="notification-info-item">
                            <span class="notification-info-label">タイトル</span>
                            <div class="notification-info-value">${data.title}</div>
                        </div>
                        <div class="notification-info-item">
                            <span class="notification-info-label">担当者</span>
                            <div class="notification-info-value">${data.assignedStaffName}</div>
                        </div>
                    </div>
                </div>
            </div>
        `;

        document.body.appendChild(overlay);
        
        setTimeout(() => overlay.classList.add('show'), 10);
        
        setTimeout(() => overlay.classList.add('flash'), 100);
        
        setTimeout(() => {
            overlay.classList.remove('show');
            setTimeout(() => overlay.remove(), 300);
        }, 10000);
    }

    // SSE接続
    function connectToNotifications() {
        const eventSource = new EventSource('/sse-endpoint');
        const statusElement = document.getElementById('connection-status');

        if (statusElement) {
            statusElement.textContent = '接続済み';
            statusElement.className = 'text-success';
        }

        eventSource.onmessage = function(event) {
            try {
                const data = JSON.parse(event.data);

                // 音を再生
                playNotificationSound();

                // 視覚効果を表示
                showVisualNotification(data);

            } catch (error) {
                console.error('通知の解析に失敗しました:', error);
            }
        };

        eventSource.onerror = function(error) {
            console.error('SSE接続エラー:', error);
            if (statusElement) {
                statusElement.textContent = '接続エラー - 再接続中...';
                statusElement.className = 'text-warning';
            }
            eventSource.close();
            // 5秒後に再接続を試みる
            setTimeout(connectToNotifications, 5000);
        };
    }
    
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', connectToNotifications);
    } else {
        connectToNotifications();
    }
})();