﻿using System;
using System.Data.Entity;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class UserRewardRepository : BaseRepository<UserReward>, IUserRewardRepository
    {
        public UserRewardRepository(DbContext dbContext)
            : base(dbContext)
        { }

        protected override int GetItemId(UserReward item)
        {
            throw new InvalidOperationException("You must provide the Id for updating. Use insert graph for insert");
        }
    }
}