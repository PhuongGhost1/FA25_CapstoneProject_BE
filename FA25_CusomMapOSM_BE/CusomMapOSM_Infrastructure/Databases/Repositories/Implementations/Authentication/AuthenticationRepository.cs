using CusomMapOSM_Domain.Entities.Users;
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

    public async Task<User?> GetUserByEmail(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
    }

    public async Task<User?> GetUserById(Guid? userId)
    {
        return await _context.Users.FindAsync(userId);
    }

    public async Task<bool> IsEmailExists(string email)
    {
        return await _context.Users.AnyAsync(x => x.Email == email);
    }

    public async Task<User?> Login(string email, string pwd)
    {
        return await _context.Users.FirstOrDefaultAsync(x => x.Email == email && x.PasswordHash == pwd);
    }

    public async Task<bool> Register(User user)
    {
        await _context.Users.AddAsync(user);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateUser(User user)
    {
        _context.Users.Update(user);
        return await _context.SaveChangesAsync() > 0;
    }
}