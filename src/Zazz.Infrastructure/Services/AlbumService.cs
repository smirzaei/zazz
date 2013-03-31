﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Infrastructure.Services
{
    public class AlbumService : IAlbumService
    {
        private readonly IUoW _uow;
        private readonly IPhotoService _photoService;

        public AlbumService(IUoW uow, IPhotoService photoService)
        {
            _uow = uow;
            _photoService = photoService;
        }

        public Task<Album> GetAlbumAsync(int albumId)
        {
            return _uow.AlbumRepository.GetByIdAsync(albumId);
        }

        public Task<List<Album>> GetUserAlbumsAsync(int userId, int skip, int take)
        {
            return Task.Run(() => _uow.AlbumRepository.GetAll()
                                      .Where(a => a.UserId == userId)
                                      .OrderBy(a => a.Id)
                                      .Skip(skip)
                                      .Take(take).ToList());
        }

        public Task<List<Album>> GetUserAlbumsAsync(int userId)
        {
            return Task.Run(() => _uow.AlbumRepository.GetAll()
                                      .Where(a => a.UserId == userId).ToList());
        }

        public Task<int> GetUserAlbumsCountAsync(int userId)
        {
            return Task.Run(() => _uow.AlbumRepository.GetAll().Count(a => a.UserId == userId));
        }

        public async Task CreateAlbumAsync(Album album)
        {
            _uow.AlbumRepository.InsertGraph(album);
            // there is a direct call to repository in FacebookService (get page photos)
            _uow.SaveChanges();
        }

        public async Task UpdateAlbumAsync(Album album, int currentUserId)
        {
            if (album.Id == 0)
                throw new ArgumentException("Album id cannot be 0");

            var ownerId = await _uow.AlbumRepository.GetOwnerIdAsync(album.Id);
            if (ownerId != currentUserId)
                throw new SecurityException();

            _uow.AlbumRepository.InsertOrUpdate(album);
            _uow.SaveChanges();
        }

        public async Task DeleteAlbumAsync(int albumId, int currentUserId)
        {
            if (albumId == 0)
                throw new ArgumentException("Album Id cannot be 0", "albumId");

            var ownerId = await _uow.AlbumRepository.GetOwnerIdAsync(albumId);
            if (ownerId != currentUserId)
                throw new SecurityException();

            var photosIds = _uow.AlbumRepository.GetAlbumPhotoIds(albumId).ToList();

            foreach (var photoId in photosIds)
                await _photoService.RemovePhotoAsync(photoId, currentUserId);

            await _uow.AlbumRepository.RemoveAsync(albumId);
            _uow.SaveChanges();
        }

        public void Dispose()
        {
            _uow.Dispose();
        }
    }
}