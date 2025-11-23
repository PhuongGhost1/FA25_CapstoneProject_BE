using CusomMapOSM_Domain.Entities.Groups;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Groups;
using Microsoft.EntityFrameworkCore;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Groups;

public class GroupSubmissionRepository : IGroupSubmissionRepository
{
    private readonly CustomMapOSMDbContext _context;

    public GroupSubmissionRepository(CustomMapOSMDbContext context)
    {
        _context = context;
    }

    public async Task<GroupSubmission?> GetSubmissionById(Guid submissionId)
    {
        return await _context.GroupSubmissions
            .Include(s => s.Group)
            .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);
    }

    public async Task<List<GroupSubmission>> GetSubmissionsByGroup(Guid groupId)
    {
        return await _context.GroupSubmissions
            .Where(s => s.GroupId == groupId)
            .OrderByDescending(s => s.SubmittedAt)
            .ToListAsync();
    }

    public async Task<List<GroupSubmission>> GetSubmissionsBySession(Guid sessionId)
    {
        return await _context.GroupSubmissions
            .Include(s => s.Group)
            .Where(s => s.Group!.SessionId == sessionId)
            .OrderByDescending(s => s.SubmittedAt)
            .ToListAsync();
    }

    public async Task<bool> CreateSubmission(GroupSubmission submission)
    {
        _context.GroupSubmissions.Add(submission);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateSubmission(GroupSubmission submission)
    {
        _context.GroupSubmissions.Update(submission);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteSubmission(Guid submissionId)
    {
        var submission = await _context.GroupSubmissions.FindAsync(submissionId);
        if (submission == null) return false;

        _context.GroupSubmissions.Remove(submission);
        return await _context.SaveChangesAsync() > 0;
    }
}
