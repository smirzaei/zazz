﻿using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface INotificationRepository : IRepository<Notification>
    {
        void RemoveRecordsByPhotoId(int photoId);

        void RemoveRecordsByEventId(int eventId);

        void RemoveRecordsByPostId(int postId);

        void MarkUserNotificationsAsRead(int userId);

        int GetUnreadNotificationsCount(int userId);
    }
}