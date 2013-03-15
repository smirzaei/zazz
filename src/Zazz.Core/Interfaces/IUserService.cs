﻿using System;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IUserService : IDisposable
    {
        AccountType GetUserAccountType(int userId);

        int GetUserId(string username);

        Task<User> GetUserAsync(string username);

        string GetUserFullName(int userId);

        string GetUserFullName(string username);
    }
}