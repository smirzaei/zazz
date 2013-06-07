﻿using System;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Infrastructure.Services
{
    public class VoteService : IVoteService
    {
        private readonly IUoW _uow;

        public VoteService(IUoW uow)
        {
            _uow = uow;
        }

        public int GetPhotoVoteCounts(int photoId)
        {
            throw new NotImplementedException();
        }

        public bool IsUserVotedOnPhoto(int photoId, int userId)
        {
            throw new NotImplementedException();
        }

        public void AddPhotoVote(int photoId, int currentUserId)
        {
            if (photoId == 0)
                throw new ArgumentOutOfRangeException("photoId");

            if (currentUserId == 0)
                throw new ArgumentOutOfRangeException("currentUserId");

            var photo = _uow.PhotoRepository.GetPhotoWithMinimalData(photoId);
            if (photo == null)
                throw new NotFoundException();

            if (_uow.PhotoVoteRepository.Exists(photoId, currentUserId))
                throw new AlreadyVotedException();

            var vote = new PhotoVote { PhotoId = photoId, UserId = currentUserId };
            
            _uow.PhotoVoteRepository.InsertGraph(vote);
            _uow.UserReceivedVotesRepository.Increment(photo.UserId);

            _uow.SaveChanges();
        }

        public void RemovePhotoVote(int photoId, int currentUserId)
        {
            if (photoId == 0)
                throw new ArgumentOutOfRangeException("photoId");

            if (currentUserId == 0)
                throw new ArgumentOutOfRangeException("currentUserId");

            var photo = _uow.PhotoRepository.GetPhotoWithMinimalData(photoId);
            if (photo == null)
                return;

            // This check is required because if we continue we'll decrement a vote count, and if the current user hasn't voted we want to stop right here.
            if (!_uow.PhotoVoteRepository.Exists(photoId, currentUserId))
                return;

            _uow.PhotoVoteRepository.Remove(photoId, currentUserId);
            _uow.UserReceivedVotesRepository.Decrement(photo.UserId);

            _uow.SaveChanges();
        }
    }
}