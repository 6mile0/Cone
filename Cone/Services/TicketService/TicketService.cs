using Cone.Areas.Admin.Dtos.Req;
using Cone.Areas.Student.Dtos.Req;
using Cone.Areas.Student.Dtos.Res;
using Cone.Db;
using Cone.Db.Models;
using Cone.Enums;
using Cone.Exception;
using Cone.Services.NotificationService;
using Microsoft.EntityFrameworkCore;

namespace Cone.Services.TicketService;

public class TicketService(ConeDbContext coneDbContext, INotificationService notificationService) : ITicketService
{
    public async Task<IReadOnlyList<Tickets>> GetAllTicketsAsync(CancellationToken cancellationToken)
    {
        return await coneDbContext.Tickets
            .Include(t => t.StudentGroup)
            .Include(t => t.TicketAdminUser)
            .ThenInclude(tau => tau!.AdminUser)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Tickets?> GetTicketByIdAsync(long ticketId, CancellationToken cancellationToken)
    {
        return await coneDbContext.Tickets
            .Include(t => t.StudentGroup)
            .Include(t => t.TicketAdminUser)
            .ThenInclude(tau => tau!.AdminUser)
            .FirstOrDefaultAsync(t => t.Id == ticketId, cancellationToken);
    }

    public async Task<IReadOnlyList<Tickets>> GetTicketsByStudentGroupIdAsync(long studentGroupId,
        CancellationToken cancellationToken)
    {
        return await coneDbContext.Tickets
            .Where(t => t.StudentGroupId == studentGroupId)
            .Include(t => t.TicketAdminUser)
            .ThenInclude(tau => tau!.AdminUser)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<AddTicketResDto> CreateTicketAsync(AddTicketDto addTicketDto, CancellationToken cancellationToken)
    {
        var transaction = await coneDbContext.Database.BeginTransactionAsync(cancellationToken);

        // StudentGroupが存在するか確認
        var studentGroup = await coneDbContext.StudentGroups
            .FirstAsync(sg => sg.Id == addTicketDto.StudentGroupId, cancellationToken);


        if (studentGroup == null)
        {
            throw new EntityNotFoundException($"学生グループID {addTicketDto.StudentGroupId} の学生グループが見つかりません。");
        }

        var ticket = new Tickets
        {
            Title = addTicketDto.Title,
            StudentGroupId = addTicketDto.StudentGroupId,
            Status = TicketStatus.InProgress, // 初期はInProgress
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        coneDbContext.Tickets.Add(ticket);
        await coneDbContext.SaveChangesAsync(cancellationToken);

        // 講師を割り当てる
        var targetTutor = await AssignTutorToTicketAsync(ticket.Id, cancellationToken);

        // 課題とチケットの関連付け
        await LinkAssignmentToTicketAsync(ticket.Id, addTicketDto.AssignmentId, addTicketDto.StudentGroupId,
            cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        // SSE通知を送信
        await notificationService.NotifyTicketCreatedAsync(
            ticket.Id,
            ticket.Title,
            studentGroup.GroupName,
            targetTutor.FullName
        );

        return new AddTicketResDto
        {
            Ticket = ticket,
            Admin = targetTutor
        };
    }

    public async Task<Tickets> UpdateTicketAsync(UpdateTicketReqDto req, CancellationToken cancellationToken)
    {
        var ticket = await coneDbContext.Tickets
            .FirstOrDefaultAsync(t => t.Id == req.TicketId, cancellationToken);

        if (ticket == null)
        {
            throw new EntityNotFoundException($"チケットID {req.TicketId} のチケットが見つかりません。");
        }

        ticket.Title = req.Title;
        ticket.Status = req.Status;
        ticket.Remark = req.Remark;
        ticket.UpdatedAt = DateTime.UtcNow;

        coneDbContext.Tickets.Update(ticket);
        await coneDbContext.SaveChangesAsync(cancellationToken);

        return ticket;
    }

    public async Task DeleteTicketAsync(long ticketId, CancellationToken cancellationToken)
    {
        var ticket = await coneDbContext.Tickets
            .FirstOrDefaultAsync(t => t.Id == ticketId, cancellationToken);

        if (ticket == null)
        {
            throw new EntityNotFoundException($"チケットID {ticketId} のチケットが見つかりません。");
        }

        coneDbContext.Tickets.Remove(ticket);
        await coneDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Tickets?> IsAbleAddTicketAsync(long studentGroupId, CancellationToken cancellationToken)
    {
        // 未解決のチケットが存在するか確認
        return await coneDbContext.Tickets
            .Include(t => t.TicketAdminUser)
            .ThenInclude(tau => tau!.AdminUser)
            .FirstOrDefaultAsync(t => t.StudentGroupId == studentGroupId && t.Status == TicketStatus.InProgress,
                cancellationToken);
    }

    public async Task<Tickets> AssignTicketAsync(AssignTicketReqDto req, CancellationToken cancellationToken)
    {
        await using var transaction = await coneDbContext.Database.BeginTransactionAsync(cancellationToken);

        var ticket = await coneDbContext.Tickets
            .FirstOrDefaultAsync(t => t.Id == req.TicketId, cancellationToken);

        if (ticket == null)
        {
            throw new EntityNotFoundException($"チケットID {req.TicketId} のチケットが見つかりません。");
        }

        var adminUser = await coneDbContext.AdminUsers
            .FirstOrDefaultAsync(a => a.Id == req.AdminUserId, cancellationToken);

        if (adminUser == null)
        {
            throw new EntityNotFoundException($"管理者ID {req.AdminUserId} の管理者が見つかりません。");
        }

        // 既存の担当者レコードを削除
        var existingAssignment = await coneDbContext.TicketAdminUsers
            .FirstOrDefaultAsync(tau => tau.TicketId == req.TicketId, cancellationToken);

        if (existingAssignment != null)
        {
            coneDbContext.TicketAdminUsers.Remove(existingAssignment);
        }

        // 新しい担当者レコードを追加
        var newAssignment = new TicketAdminUsers
        {
            TicketId = req.TicketId,
            AdminUserId = req.AdminUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        coneDbContext.TicketAdminUsers.Add(newAssignment);
        ticket.UpdatedAt = DateTime.UtcNow;
        await coneDbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return ticket;
    }

    private async Task<AdminUsers> AssignTutorToTicketAsync(long ticketId, CancellationToken cancellationToken)
    {
        // 対応中のチケットとその担当講師を取得
        var inProgressTickets = await coneDbContext.Tickets
            .Where(t => t.Status == TicketStatus.InProgress)
            .Include(t => t.TicketAdminUser)
            .ToListAsync(cancellationToken);

        // 講師ごとの対応中チケット数をカウント
        var inProgressTicketsByTutor = inProgressTickets
            .Where(t => t.TicketAdminUser != null)
            .GroupBy(t => t.TicketAdminUser!.AdminUserId)
            .ToDictionary(g => g.Key, g => g.Count());

        // すべての講師を取得（不在の講師を除外）
        var allTutors = await coneDbContext.AdminUsers
            .Where(u => !u.IsAbsent)
            .ToListAsync(cancellationToken);

        if (allTutors.Count == 0)
        {
            throw new InvalidOperationException("講師が登録されていません。");
        }

        // 対応中のチケットを持たない講師を検索
        var availableTutors = allTutors
            .Where(tutor => !inProgressTicketsByTutor.ContainsKey(tutor.Id))
            .ToList();

        // 全員が対応中の場合はエラー
        if (availableTutors.Count == 0)
        {
            throw new AllStaffCurrentlyAssistingException("TA/SAの全員が対応中です。しばらく経ってから再度チケットを作成してください。");
        }

        // 利用可能な講師の中からランダムに選ぶ
        var targetUser = availableTutors[Random.Shared.Next(availableTutors.Count)];
        
        var adminUserTicket = new TicketAdminUsers
        {
            TicketId = ticketId,
            AdminUserId = targetUser.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        coneDbContext.TicketAdminUsers.Add(adminUserTicket);
        await coneDbContext.SaveChangesAsync(cancellationToken);

        return targetUser;
    }

    private async Task LinkAssignmentToTicketAsync(long ticketId, long assignmentId, long studentGroupId,
        CancellationToken cancellationToken)
    {
        var assignmentExists = await coneDbContext.Assignments
            .AnyAsync(a => a.Id == assignmentId, cancellationToken);

        if (!assignmentExists)
        {
            throw new EntityNotFoundException($"課題ID {assignmentId} の課題が見つかりません。");
        }

        var ticketAssignment = new TicketAssignments
        {
            TicketId = ticketId,
            AssignmentId = assignmentId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            StudentGroupId = studentGroupId
        };

        coneDbContext.TicketAssignments.Add(ticketAssignment);
        await coneDbContext.SaveChangesAsync(cancellationToken);
    }
}