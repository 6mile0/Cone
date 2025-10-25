using Ice.Areas.Admin.Dtos.Req;
using Ice.Areas.Student.Dtos.Req;
using Ice.Areas.Student.Dtos.Res;
using Ice.Db;
using Ice.Db.Models;
using Ice.Enums;
using Ice.Exception;
using Ice.Services.NotificationService;
using Microsoft.EntityFrameworkCore;

namespace Ice.Services.TicketService;

public class TicketService(IceDbContext iceDbContext, INotificationService notificationService) : ITicketService
{
    public async Task<IReadOnlyList<Tickets>> GetAllTicketsAsync(CancellationToken cancellationToken)
    {
        return await iceDbContext.Tickets
            .Include(t => t.StudentGroup)
            .Include(t => t.TicketAdminUser)
            .ThenInclude(tau => tau!.AdminUser)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Tickets?> GetTicketByIdAsync(long ticketId, CancellationToken cancellationToken)
    {
        return await iceDbContext.Tickets
            .Include(t => t.StudentGroup)
            .Include(t => t.TicketAdminUser)
            .ThenInclude(tau => tau!.AdminUser)
            .FirstOrDefaultAsync(t => t.Id == ticketId, cancellationToken);
    }

    public async Task<IReadOnlyList<Tickets>> GetTicketsByStudentGroupIdAsync(long studentGroupId,
        CancellationToken cancellationToken)
    {
        return await iceDbContext.Tickets
            .Where(t => t.StudentGroupId == studentGroupId)
            .Include(t => t.TicketAdminUser)
            .ThenInclude(tau => tau!.AdminUser)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<AddTicketResDto> CreateTicketAsync(AddTicketDto addTicketDto, CancellationToken cancellationToken)
    {
        var transaction = await iceDbContext.Database.BeginTransactionAsync(cancellationToken);

        // StudentGroupが存在するか確認
        var studentGroup = await iceDbContext.StudentGroups
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

        iceDbContext.Tickets.Add(ticket);
        await iceDbContext.SaveChangesAsync(cancellationToken);

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
            targetTutor.FullName ?? "未割当"
        );

        return new AddTicketResDto
        {
            Ticket = ticket,
            Admin = targetTutor
        };
    }

    public async Task<Tickets> UpdateTicketAsync(UpdateTicketReqDto req, CancellationToken cancellationToken)
    {
        var ticket = await iceDbContext.Tickets
            .FirstOrDefaultAsync(t => t.Id == req.TicketId, cancellationToken);

        if (ticket == null)
        {
            throw new EntityNotFoundException($"チケットID {req.TicketId} のチケットが見つかりません。");
        }

        ticket.Title = req.Title;
        ticket.Status = req.Status;
        ticket.Remark = req.Remark;
        ticket.UpdatedAt = DateTime.UtcNow;

        iceDbContext.Tickets.Update(ticket);
        await iceDbContext.SaveChangesAsync(cancellationToken);

        return ticket;
    }

    public async Task DeleteTicketAsync(long ticketId, CancellationToken cancellationToken)
    {
        var ticket = await iceDbContext.Tickets
            .FirstOrDefaultAsync(t => t.Id == ticketId, cancellationToken);

        if (ticket == null)
        {
            throw new EntityNotFoundException($"チケットID {ticketId} のチケットが見つかりません。");
        }

        iceDbContext.Tickets.Remove(ticket);
        await iceDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Tickets?> IsAbleAddTicketAsync(long studentGroupId, CancellationToken cancellationToken)
    {
        // 未解決のチケットが存在するか確認
        return await iceDbContext.Tickets
            .Include(t => t.TicketAdminUser)
            .ThenInclude(tau => tau!.AdminUser)
            .FirstOrDefaultAsync(t => t.StudentGroupId == studentGroupId && t.Status == TicketStatus.InProgress,
                cancellationToken);
    }

    public async Task<Tickets> AssignTicketAsync(AssignTicketReqDto req, CancellationToken cancellationToken)
    {
        await using var transaction = await iceDbContext.Database.BeginTransactionAsync(cancellationToken);

        var ticket = await iceDbContext.Tickets
            .FirstOrDefaultAsync(t => t.Id == req.TicketId, cancellationToken);

        if (ticket == null)
        {
            throw new EntityNotFoundException($"チケットID {req.TicketId} のチケットが見つかりません。");
        }

        var adminUser = await iceDbContext.AdminUsers
            .FirstOrDefaultAsync(a => a.Id == req.AdminUserId, cancellationToken);

        if (adminUser == null)
        {
            throw new EntityNotFoundException($"管理者ID {req.AdminUserId} の管理者が見つかりません。");
        }

        // 既存の担当者レコードを削除
        var existingAssignment = await iceDbContext.TicketAdminUsers
            .FirstOrDefaultAsync(tau => tau.TicketId == req.TicketId, cancellationToken);

        if (existingAssignment != null)
        {
            iceDbContext.TicketAdminUsers.Remove(existingAssignment);
        }

        // 新しい担当者レコードを追加
        var newAssignment = new TicketAdminUsers
        {
            TicketId = req.TicketId,
            AdminUserId = req.AdminUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        iceDbContext.TicketAdminUsers.Add(newAssignment);
        ticket.UpdatedAt = DateTime.UtcNow;
        await iceDbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return ticket;
    }

    private async Task<AdminUsers> AssignTutorToTicketAsync(long ticketId, CancellationToken cancellationToken)
    {
        // 対応中のチケットとその担当講師を取得
        var inProgressTickets = await iceDbContext.Tickets
            .Where(t => t.Status == TicketStatus.InProgress)
            .Include(t => t.TicketAdminUser)
            .ToListAsync(cancellationToken);

        // 講師ごとの対応中チケット数をカウント
        var inProgressTicketsByTutor = inProgressTickets
            .Where(t => t.TicketAdminUser != null)
            .GroupBy(t => t.TicketAdminUser!.AdminUserId)
            .ToDictionary(g => g.Key, g => g.Count());

        // すべての講師を取得
        var allTutors = await iceDbContext.AdminUsers.ToListAsync(cancellationToken);

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

        // 対応できる講師の中で最も総チケット数が少ない講師を選択
        AdminUsers targetUser;
        if (availableTutors.Count == allTutors.Count)
        {
            // 全員が対応中でない場合、総チケット数が最も少ない講師を選択
            var ticketCounts = await iceDbContext.TicketAdminUsers
                .GroupBy(tau => tau.AdminUserId)
                .Select(g => new { AdminUserId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.AdminUserId, x => x.Count, cancellationToken
                );
            var minTicketCount = ticketCounts.Values.Min();
            var candidates = availableTutors
                .Where(tutor => ticketCounts.GetValueOrDefault(tutor.Id, 0) == minTicketCount)
                .ToList();
            targetUser = candidates.First();
        }
        else
        {
            // 対応中でない講師の中からランダムに選択
            var random = new Random();
            var randomIndex = random.Next(availableTutors.Count);
            targetUser = availableTutors[randomIndex];
        }
        
        var adminUserTicket = new TicketAdminUsers
        {
            TicketId = ticketId,
            AdminUserId = targetUser.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        iceDbContext.TicketAdminUsers.Add(adminUserTicket);
        await iceDbContext.SaveChangesAsync(cancellationToken);

        return targetUser;
    }

    private async Task LinkAssignmentToTicketAsync(long ticketId, long assignmentId, long studentGroupId,
        CancellationToken cancellationToken)
    {
        var assignmentExists = await iceDbContext.Assignments
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

        iceDbContext.TicketAssignments.Add(ticketAssignment);
        await iceDbContext.SaveChangesAsync(cancellationToken);
    }
}