using DomainUser = CusomMapOSM_Domain.Entities.Users;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Authentication;
using Microsoft.EntityFrameworkCore;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Authentication;

public class AuthenticationRepository : IAuthenticationRepository
{
    private readonly CustomMapOSMDbContext _context;
    public AuthenticationRepository(CustomMapOSMDbContext context)
    {
        _context = context;
    }

    public async Task<DomainUser.User?> GetUserByEmail(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
    }

    public async Task<DomainUser.User?> GetUserById(Guid? userId)
    {
        return await _context.Users.Include(x => x.Role).FirstOrDefaultAsync(x => x.UserId == userId);
    }

    public async Task<bool> IsEmailExists(string email)
    {
        return await _context.Users.AnyAsync(x => x.Email == email);
    }

    public async Task<DomainUser.User?> Login(string email, string pwd)
    {
        return await _context.Users.FirstOrDefaultAsync(x => x.Email == email && x.PasswordHash == pwd);
    }

    public async Task<bool> Register(DomainUser.User user)
    {
        await _context.Users.AddAsync(user);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateUser(DomainUser.User user)
    {
        _context.Users.Update(user);
        return await _context.SaveChangesAsync() > 0;
    }
}