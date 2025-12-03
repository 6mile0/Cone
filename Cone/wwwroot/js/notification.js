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
                <button class="notification-close-btn" aria-label="閉じる">&times;</button>
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

        // 通知を閉じる関数
        function closeNotification() {
            if (autoCloseTimeout) {
                clearTimeout(autoCloseTimeout);
            }
            overlay.classList.remove('show');
            setTimeout(() => overlay.remove(), 300);
        }

        // 閉じるボタンのイベント
        const closeBtn = overlay.querySelector('.notification-close-btn');
        closeBtn.addEventListener('click', closeNotification);

        // オーバレイの背景をクリックしても閉じる
        overlay.addEventListener('click', (e) => {
            if (e.target === overlay) {
                closeNotification();
            }
        });

        setTimeout(() => overlay.classList.add('show'), 10);

        setTimeout(() => overlay.classList.add('flash'), 100);

        // 10秒後に自動的に閉じる
        const autoCloseTimeout = setTimeout(() => {
            overlay.classList.remove('show');
            setTimeout(() => overlay.remove(), 300);
        }, 10000);
    }

    // Staff status UI update
    function updateStaffStatus(staffStatus) {
        const totalTicketsEl = document.getElementById('total-tickets');
        const unassignedTicketsEl = document.getElementById('unassigned-tickets');
        const lastUpdateEl = document.getElementById('last-update');
        const staffStatusList = document.getElementById('staff-status-list');
        const staffStatusConnection = document.getElementById('staff-status-connection');
        const staffStatusTimestamp = document.getElementById('staff-status-timestamp');

        if (!staffStatusList) return;

        const timestamp = new Date(staffStatus.timestamp);
        const timestampStr = timestamp.toLocaleString('ja-JP');

        if (staffStatusConnection) {
            staffStatusConnection.textContent = '接続済み';
            staffStatusConnection.className = 'text-success';
        }

        if (staffStatusTimestamp) {
            staffStatusTimestamp.textContent = timestampStr;
        }

        if (totalTicketsEl) totalTicketsEl.textContent = staffStatus.totalTicketCount;
        if (unassignedTicketsEl) unassignedTicketsEl.textContent = staffStatus.unassignedTicketCount;

        if (lastUpdateEl) {
            lastUpdateEl.textContent = timestampStr;
        }

        if (staffStatus.adminUserStatuses && staffStatus.adminUserStatuses.length > 0) {
            staffStatusList.innerHTML = staffStatus.adminUserStatuses.map(staff => createStaffCard(staff)).join('');
        } else {
            staffStatusList.innerHTML = '<div class="col-12 text-center text-muted py-4">スタッフが登録されていません</div>';
        }
    }

    function createStaffCard(staff) {
        const escapedName = escapeHtml(staff.fullName);
        const statusBadge = staff.isWorking
            ? '<span class="badge bg-success">対応中</span>'
            : '<span class="badge bg-secondary">待機中</span>';

        let ticketInfo = '';
        if (staff.isWorking && staff.currentTickets && staff.currentTickets.length > 0) {
            const ticketList = staff.currentTickets.map(ticket => {
                const escapedTitle = escapeHtml(ticket.title);
                const escapedGroupName = escapeHtml(ticket.studentGroupName);
                return `
                    <div class="mb-1">
                        <a href="/admin/tickets/${ticket.id}" class="text-decoration-none">
                            <span class="badge bg-info text-dark me-1" style="font-size: 0.95rem; padding: 0.4rem 0.6rem;">${escapedGroupName}</span>
                            ${escapedTitle}
                            <i class="bi bi-box-arrow-up-right ms-1"></i>
                        </a>
                    </div>
                `;
            }).join('');

            ticketInfo = `
                <div class="mt-2">
                    <small class="text-muted">対応中のチケット:</small>
                    <div class="mt-1">
                        ${ticketList}
                    </div>
                </div>
            `;
        }

        return `
            <div class="col-md-6 col-lg-4 mb-3">
                <div class="card ${staff.isWorking ? 'border-success' : ''}">
                    <div class="card-body">
                        <div class="d-flex justify-content-between align-items-center mb-2">
                            <h5 class="card-title mb-0">${escapedName}</h5>
                            ${statusBadge}
                        </div>
                        ${ticketInfo}
                    </div>
                </div>
            </div>
        `;
    }

    function escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    // SSE接続
    function connectToNotifications() {
        const eventSource = new EventSource('/sse-endpoint');
        const statusElement = document.getElementById('connection-status');
        const staffStatusConnection = document.getElementById('staff-status-connection');
        const ticketTimestamp = document.getElementById('ticket-notification-timestamp');

        if (statusElement) {
            statusElement.textContent = '接続済み';
            statusElement.className = 'text-success';
        }

        if (ticketTimestamp) {
            const now = new Date();
            ticketTimestamp.textContent = now.toLocaleString('ja-JP');
        }

        eventSource.onmessage = function(event) {
            try {
                const message = JSON.parse(event.data);

                if (message.type === 'ticket-created') {
                    // 音を再生
                    playNotificationSound();

                    // 視覚効果を表示
                    showVisualNotification(message);

                    // タイムスタンプを更新
                    const ticketTimestamp = document.getElementById('ticket-notification-timestamp');
                    if (ticketTimestamp) {
                        const now = new Date();
                        ticketTimestamp.textContent = now.toLocaleString('ja-JP');
                    }
                } else if (message.type === 'staff-status') {
                    // スタッフステータスを更新
                    updateStaffStatus(message.data);
                }

            } catch (error) {
                console.error('通知の解析に失敗しました:', error);
            }
        };

        eventSource.onerror = function(error) {
            console.error('SSE接続エラー:', error);
            if (statusElement) {
                statusElement.textContent = '接続エラー - 再接続中...';
                statusElement.className = 'text-danger';
            }
            if (staffStatusConnection) {
                staffStatusConnection.textContent = '接続エラー - 再接続中...';
                staffStatusConnection.className = 'text-danger';
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