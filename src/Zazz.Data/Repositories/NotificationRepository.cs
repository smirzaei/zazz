﻿using System.Data.Entity;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class NotificationRepository : BaseLongRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(DbContext dbContext) : base(dbContext)
        {}

        public void RemoveRecordsByPhotoId(int photoId)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveRecordsByEventId(int eventId)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveRecordsByPostId(int postId)
        {
            throw new System.NotImplementedException();
        }

        public void MarkUserNotificationsAsRead(int userId)
        {
            throw new System.NotImplementedException();
        }

        public int GetUnreadNotificationsCount(int userId)
        {
            throw new System.NotImplementedException();
        }
    }
}