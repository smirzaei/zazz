﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using PagedList;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Web.Models;

namespace Zazz.Web.Controllers
{
    public class AlbumController : BaseController
    {
        private readonly IAlbumService _albumService;
        private readonly IUserService _userService;
        private readonly IPhotoService _photoService;

        public AlbumController(IAlbumService albumService, IUserService userService, IPhotoService photoService)
        {
            _albumService = albumService;
            _userService = userService;
            _photoService = photoService;
        }

        [Authorize]
        public ActionResult Index()
        {
            using (_photoService)
            using (_albumService)
            using (_userService)
            {
                var userId = _userService.GetUserId(User.Identity.Name);
                return RedirectToAction("List", "Album", new { id = userId, page = 1 });
            }
        }

        public async Task<ActionResult> List(int id, int page = 1)
        {
            if (page == 0)
                throw new HttpException(400, "Bad Request");

            const int PAGE_SIZE = 6;
            var skip = (page - 1) * PAGE_SIZE;

            using (_photoService)
            using (_albumService)
            using (_userService)
            {
                var totalAlbumsCount = await _albumService.GetUserAlbumsCountAsync(id);
                var albums = await _albumService.GetUserAlbumsAsync(id, skip, PAGE_SIZE);

                const string DEFAULT_IMAGE_PATH = "/Images/placeholder.gif";
                var albumsVM = new List<ImageViewModel>();
                foreach (var album in albums)
                {
                    var albumVm = new ImageViewModel
                                      {
                                          Id = album.Id,
                                          Text = album.Name
                                      };
                    var firstAlbumImageId = album.Photos.Select(p => p.Id).FirstOrDefault();
                    albumVm.ImageUrl = firstAlbumImageId == 0
                        ? DEFAULT_IMAGE_PATH
                        : _photoService.GeneratePhotoUrl(id, album.Id, firstAlbumImageId);

                    albumsVM.Add(albumVm);
                }

                var isOwner = false;
                if (User.Identity.IsAuthenticated)
                {
                    var currentUserId = _userService.GetUserId(User.Identity.Name);
                    isOwner = currentUserId == id;
                }

                var pagedList = new StaticPagedList<ImageViewModel>(albumsVM, page, PAGE_SIZE, totalAlbumsCount);

                var vm = new AlbumListViewModel
                             {
                                 IsOwner = isOwner,
                                 Albums = pagedList,
                                 UserId = id
                             };

                return View("List", vm);
            }
        }

        [Authorize, HttpPost]
        public async Task<ActionResult> CreateAlbum(string albumName)
        {
            if (albumName.Length > 50)
            {
                ShowAlert("Album name cannot be longer than 50 characters", AlertType.Error);
            }
            else
            {
                using (_photoService)
                using (_albumService)
                using (_userService)
                {
                    var userId = _userService.GetUserId(User.Identity.Name);
                    var album = new Album
                    {
                        Name = albumName,
                        UserId = userId
                    };

                    await _albumService.CreateAlbumAsync(album);
                }
            }

            return Redirect(HttpContext.Request.UrlReferrer.AbsolutePath);
        }

        [Authorize]
        public async Task<ActionResult> Remove(int id)
        {
            using (_photoService)
            using (_albumService)
            using (_userService)
            {
                var userId = _userService.GetUserId(User.Identity.Name);

                await _albumService.DeleteAlbumAsync(id, userId);
            }

            return Redirect(HttpContext.Request.UrlReferrer.AbsolutePath);
        }

        [HttpGet]
        public async Task<ActionResult> Photos(int id, int page = 1)
        {
            if (page == 0)
                throw new HttpException(400, "Bad Request");

            const int PAGE_SIZE = 20;
            var skip = (page - 1) * PAGE_SIZE;

            using (_photoService)
            using (_albumService)
            using (_userService)
            {
                var userId = 0;
                if (User.Identity.IsAuthenticated)
                    userId = _userService.GetUserId(User.Identity.Name);

                var album = await _albumService.GetAlbumAsync(id);
                var isOwner = userId == album.UserId;

                var totalPhotos = album.Photos.Count();
                var photos = album.Photos
                                  .OrderBy(p => p.Id)
                                  .Skip(skip)
                                  .Take(PAGE_SIZE);

                var photosVm = photos
                    .Select(p => new ImageViewModel
                                 {
                                     Id = p.Id,
                                     Text = p.Description,
                                     ImageUrl = _photoService.GeneratePhotoUrl(p.UploaderId, p.AlbumId, p.Id)
                                 });

                var pagedList = new StaticPagedList<ImageViewModel>(photosVm, page, PAGE_SIZE, totalPhotos);

                var vm = new AlbumPhotosViewModel
                         {
                             IsOwner = isOwner,
                             AlbumId = id,
                             Photos = pagedList
                         };

                return View(vm);
            }
        }
    }
}
