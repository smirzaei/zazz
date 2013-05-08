﻿using System;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class TagStatRepository : BaseRepository<TagStat>, ITagStatRepository
    {
        public TagStatRepository(DbContext dbContext) : base(dbContext)
        {}

        public override IQueryable<TagStat> GetAll()
        {
            return DbSet.Include(t => t.Tag);
        }

        protected override int GetItemId(TagStat item)
        {
            throw new InvalidOperationException("You must provide Id to update an entity, if you want to insert, user InsertGraph");
        }

        public TagStat GetTagStat(byte tagId)
        {
            return DbSet.SingleOrDefault(t => t.TagId == tagId);
        }

        public int GetUsersCount(int tagId)
        {
            return DbSet
                .Where(t => t.TagId == tagId)
                .Select(t => t.UsersCount)
                .FirstOrDefault();
        }
    }
}