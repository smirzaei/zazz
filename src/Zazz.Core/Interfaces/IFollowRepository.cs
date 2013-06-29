﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IFollowRepository
    {
        void InsertGraph(Follow follow);

        /// <summary>
        /// Returns a list of users that follow the user
        /// </summary>
        /// <returns></returns>
        IQueryable<Follow> GetUserFollowers(int toUserId);

        /// <summary>
        /// Returns a list of users that the user follows
        /// </summary>
        /// <param name="fromUserId"></param>
        /// <returns></returns>
        IQueryable<Follow> GetUserFollows(int fromUserId);

        IEnumerable<int> GetFollowsUserIds(int fromUserId);

        int GetFollowersCount(int userId);

        bool Exists(int fromUserId, int toUserId);

        void Remove(int fromUserId, int toUserId);
    }
}