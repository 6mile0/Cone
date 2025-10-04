using Ice.Areas.Admin.Dtos.Req;
using Ice.Areas.Student.Dtos.Req;
using Ice.Areas.Student.Dtos.Res;
using Ice.Db;
using Ice.Db.Models;
using Ice.Enums;
using Ice.Exception;
using Microsoft.EntityFrameworkCore;

namespace Ice.Services.TicketService;

public class TicketService(IceDbContext iceDbContext): ITicketService
{
    public async Task<IReadOnlyList<Tickets?>> GetAllTicketsAsync(CancellationToken cancellationToken)
    {
        return await iceDbContext.Tickets.ToListAsync(cancellationToken);
    }

    public async Task<Tickets?> GetTicketByIdAsync(long ticketId, CancellationToken cancellationToken)
    {
        return await iceDbContext.Tickets
            .Include(t => t.StudentGroup)
            .Include(t => t.TicketAdminUser)
            .ThenInclude(tau => tau!.AdminUser)
            .FirstOrDefaultAsync(t => t.Id == ticketId, cancellationToken);
    }

    public async Task<IReadOnlyList<Tickets>> GetTicketsByStudentGroupIdAsync(long studentGroupId, CancellationToken cancellationToken)
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
        var studentGroupExists = await iceDbContext.StudentGroups
            .AnyAsync(sg => sg.Id == addTicketDto.StudentGroupId, cancellationToken);

        if (!studentGroupExists)
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
        var targetTutor = await AssignTutorToTicketAsync(cancellationToken);
        
        var adminUserTicket = new TicketAdminUsers
        {
            TicketId = ticket.Id,
            AdminUserId = targetTutor.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        iceDbContext.TicketAdminUsers.Add(adminUserTicket);
        await iceDbContext.SaveChangesAsync(cancellationToken);
        
        await transaction.CommitAsync(cancellationToken);

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
            .FirstOrDefaultAsync(t => t.StudentGroupId == studentGroupId && t.Status == TicketStatus.InProgress, cancellationToken);
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

    private async Task<AdminUsers> AssignTutorToTicketAsync(CancellationToken cancellationToken)
    {
        // 講師ごとの担当チケット数を一度のクエリで取得
        var tutorTicketCounts = await iceDbContext.AdminUsers
            .GroupJoin(
                iceDbContext.TicketAdminUsers,
                admin => admin.Id,
                tau => tau.AdminUserId,
                (admin, tickets) => new
                {
                    TutorId = admin.Id,
                    TicketCount = tickets.Count()
                })
            .ToListAsync(cancellationToken);

        if (tutorTicketCounts.Count == 0)
        {
            throw new InvalidOperationException("講師が登録されていません。");
        }

        // 最も少ないチケット数の講師を取得
        var minTicketCount = tutorTicketCounts.Min(t => t.TicketCount);
        var leastBusyTutorId = tutorTicketCounts
            .First(t => t.TicketCount == minTicketCount)
            .TutorId;

        return await iceDbContext.AdminUsers
            .FirstAsync(u => u.Id == leastBusyTutorId, cancellationToken);
    }
}