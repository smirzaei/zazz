﻿using System.Threading.Tasks;
using Zazz.Core.Interfaces;
using Zazz.Data.Repositories;

namespace Zazz.Data
{
    public class UoW : IUoW
    {
        private ZazzDbContext _dbContext;

        private IOAuthAccountRepository _oAuthAccountRepository;
        public IOAuthAccountRepository OAuthAccountRepository
        {
            get { return _oAuthAccountRepository ?? (_oAuthAccountRepository = new OAuthAccountRepository(GetContext())); }
        }

        private ICommentRepository _commentRepository;
        public ICommentRepository CommentRepository
        {
            get { return _commentRepository ?? (_commentRepository = new CommentRepository(GetContext())); }
        }

        private IEventRepository _eventRepository;
        public IEventRepository EventRepository
        {
            get { return _eventRepository ?? (_eventRepository = new EventRepository(GetContext())); }
        }

        private IFollowRepository _followRepository;
        public IFollowRepository FollowRepository
        {
            get { return _followRepository ?? (_followRepository = new FollowRepository(GetContext())); }
        }

        private IFollowRequestRepository _followRequestRepository;
        public IFollowRequestRepository FollowRequestRepository
        {
            get { return _followRequestRepository ?? (_followRequestRepository = new FollowRequestRepository(GetContext())); }
        }

        private IAlbumRepository _albumRepository;
        public IAlbumRepository AlbumRepository
        {
            get { return _albumRepository ?? (_albumRepository = new AlbumRepository(GetContext())); }
        }

        private IPhotoRepository _photoRepository;
        public IPhotoRepository PhotoRepository
        {
            get { return _photoRepository ?? (_photoRepository = new PhotoRepository(GetContext())); }
        }

        private IUserRepository _userRepository;
        public IUserRepository UserRepository
        {
            get { return _userRepository ?? (_userRepository = new UserRepository(GetContext())); }
        }

        private IValidationTokenRepository _validationTokenRepository;
        public IValidationTokenRepository ValidationTokenRepository
        {
            get { return _validationTokenRepository ?? (_validationTokenRepository = new ValidationTokenRepository(GetContext())); }
        }

        private IFacebookSyncRetryRepository _facebookSyncRetryRepository;
        public IFacebookSyncRetryRepository FacebookSyncRetryRepository
        {
            get
            {
                return _facebookSyncRetryRepository ?? (_facebookSyncRetryRepository = new FacebookSyncRetryRepository(GetContext()));
            }
        }

        private IFeedRepository _feedRepository;
        public IFeedRepository FeedRepository
        {
            get { return _feedRepository ?? (_feedRepository = new FeedRepository(GetContext())); }
        }

        private IPostRepository _postRepository;

        public IPostRepository PostRepository
        {
            get { return _postRepository ?? (_postRepository = new PostRepository(GetContext())); }
        }

        private ZazzDbContext GetContext()
        {
            if (_dbContext == null)
                _dbContext = new ZazzDbContext();

            return _dbContext;
        }

        public Task SaveAsync()
        {
            return Task.Run(() => _dbContext.SaveChanges());
        }

        public void Dispose()
        {
            if (_dbContext != null)
                _dbContext.Dispose();
        }
    }
}