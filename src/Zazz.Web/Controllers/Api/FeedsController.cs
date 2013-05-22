﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Filters;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data.Enums;
using Zazz.Web.Filters;
using Zazz.Web.Models;
using Zazz.Web.Models.Api;

namespace Zazz.Web.Controllers.Api
{
    [HMACAuthorize]
    public class FeedsController : BaseApiController
    {
        private readonly IUoW _uow;
        private readonly IUserService _userService;
        private readonly IPhotoService _photoService;

        public FeedsController(IUoW uow, IUserService userService, IPhotoService photoService)
        {
            _uow = uow;
            _userService = userService;
            _photoService = photoService;
        }

        // GET api/v1/feeds
        public IEnumerable<object> GetHomeFeeds()
        {
            throw new NotImplementedException();
        }

        // GET api/v1/feeds?lastFeed=
        public IEnumerable<object> GetHomeFeeds(int lastFeed)
        {
            throw new NotImplementedException();
        }

        // GET api/v1/feeds?id=
        public IEnumerable<object> GetUserFeeds(int id)
        {
            throw new NotImplementedException();
        }

        // GET api/v1/feeds?id=&lastFeed=
        public IEnumerable<object> GetUserFeeds(int id, int lastFeed)
        {
            throw new NotImplementedException();
        }

        private FeedApiResponse FeedViewModelToApiModel(FeedViewModel feed)
        {
            return new FeedApiResponse
                   {
                       FeedType = feed.FeedType,
                       UserId = feed.UserId,
                       UserDisplayName = feed.UserDisplayName,
                       UserDisplayPhoto = feed.UserImageUrl,
                       CanCurrentUserRemoveFeed = feed.CurrentUserCanRemoveFeed,
                       Time = feed.Time,

                       Comments = feed.CommentsViewModel.Comments
                       .Select(c => new CommentApiModel
                                    {
                                        CommentId = c.CommentId,
                                        CommentText = c.CommentText,
                                        IsFromCurrentUser = c.IsFromCurrentUser,
                                        UserId = c.UserId,
                                        UserDisplayName = c.UserDisplayName,
                                        UserDisplayPhoto = c.UserPhotoUrl,
                                        Time = c.Time.ToString("s")
                                    }),

                       Photos = feed.FeedType == FeedType.Picture
                       ? feed.PhotoViewModel.Select(p => new PhotoApiModel
                                                         {
                                                             PhotoId = p.PhotoId,
                                                             Description = p.PhotoDescription,
                                                             UserId = p.FromUserId,
                                                             UserDisplayName = p.FromUserDisplayName,
                                                             UserDisplayPhoto = p.FromUserPhotoUrl,
                                                             PhotoLinks = p.PhotoUrl
                                                         })
                       : null,

                       Post = feed.FeedType == FeedType.Post
                       ? new PostApiModel
                         {
                             Message = feed.PostViewModel.PostText,
                             PostId = feed.PostViewModel.PostId,
                             ToUserDisplayName = feed.PostViewModel.ToUserDisplayName,
                             ToUserId = feed.PostViewModel.ToUserId,
                             ToUserDisplayPhoto = feed.PostViewModel.ToUserPhotoUrl
                         }
                       : null,

                       Event = feed.FeedType == FeedType.Event
                       ? new EventApiModel
                         {
                             City = feed.EventViewModel.City,
                             CreatedDate = feed.EventViewModel.CreatedDate.HasValue 
                                ? feed.EventViewModel.CreatedDate.Value.ToString("s")
                                : null,
                            Description = feed.EventViewModel.Description,
                            EventId = feed.EventViewModel.Id,
                            FacebookLink = feed.EventViewModel.FacebookEventId.HasValue
                                ? "https://www.facebook.com/events/" + feed.EventViewModel.FacebookEventId.Value
                                :  null,
                            ImageUrl = feed.EventViewModel.ImageUrl,
                            IsDateOnly = feed.EventViewModel.IsDateOnly,
                            IsFacebookEvent = feed.EventViewModel.IsFacebookEvent,
                            IsFromCurrentUser = feed.EventViewModel.IsOwner,
                            Latitude = feed.EventViewModel.Latitude,
                            Longitude = feed.EventViewModel.Longitude,
                            Location = feed.EventViewModel.Location,
                            Name = feed.EventViewModel.Name,
                            Price = feed.EventViewModel.Price,
                            Street = feed.EventViewModel.Street,
                            Time = feed.EventViewModel.Time.ToString("s"),
                            UtcTime = DateTime.Parse(feed.EventViewModel.UtcTime).ToString("s")
                         } 
                       : null
                   };
        }
    }
}
