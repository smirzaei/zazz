﻿using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IAlbumRepository : IRepository<Album>
    {
        Task<int> GetOwnerIdAsync(int albumId);
    }
}