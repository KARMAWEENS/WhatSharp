using Microsoft.EntityFrameworkCore;
using WhatSharp.Shared.Models;
using WhatSharp.Srv.Data;

namespace WhatSharp.Srv.Services;

public class UserServiceSrv
{
    private readonly AppDbContext _dbContext;

    public UserServiceSrv(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> RegisterUserAsync(string username, string password)
    {
        if(await _dbContext.Users.AnyAsync(u => u.Login == username))
            return false;

        var user = new User
        {
            Login = username,
            PasswordHash = password
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<User?> LoginUserAsync(string username, string password)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.Login == username && u.PasswordHash == password);
    }
}