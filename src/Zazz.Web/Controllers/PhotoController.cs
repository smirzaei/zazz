﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Web.Models;

namespace Zazz.Web.Controllers
{
    public class PhotoController : BaseController
    {
        private readonly IPhotoService _photoService;
        private readonly IAlbumService _albumService;
        private readonly IUserService _userService;

        public PhotoController(IPhotoService photoService, IAlbumService albumService, IUserService userService)
        {
            _photoService = photoService;
            _albumService = albumService;
            _userService = userService;
        }

        [Authorize]
        public async Task<ActionResult> Remove(int id)
        {
            using (_photoService)
            using (_albumService)
            using (_userService)
            {
                var userId = _userService.GetUserId(User.Identity.Name);
                await _photoService.RemovePhotoAsync(id, userId);
            }

            return Redirect(HttpContext.Request.UrlReferrer.AbsolutePath);
        }

        [Authorize]
        public async Task<ActionResult> Upload(HttpPostedFileBase image, string description, int albumId)
        {
            using (_photoService)
            using (_albumService)
            using (_userService)
            {
                var errorMessage = "Image was not valid";
                if (image == null || !ImageValidator.IsValid(image, out errorMessage))
                {
                    ShowAlert(errorMessage, AlertType.Error);
                    return Redirect(HttpContext.Request.UrlReferrer.AbsolutePath);
                }

                await SaveImageAsync(image.InputStream, description, albumId);
            }

            return Redirect(HttpContext.Request.UrlReferrer.AbsolutePath);
        }

        public async Task<JsonNetResult> AjaxUpload(string description, int albumId, HttpPostedFileBase image)
        {
            using (_photoService)
            using (_albumService)
            using (_userService)
            {
                var response = new FineUploadResponse
                               {
                               };
                var errorMessage = "Image was not valid";
                if (image == null || !ImageValidator.IsValid(image, out errorMessage))
                {
                    response.Success = false;
                    response.Error = errorMessage;
                    return new JsonNetResult(response);
                }

                var photo = await SaveImageAsync(image.InputStream, description, albumId);
                response.PhotoId = photo.Id;
                response.Success = true;
                response.PhotoUrl = _photoService.GeneratePhotoUrl(photo.UploaderId, photo.Id);

                return new JsonNetResult(response);
            }
        }

        private async Task<Photo> SaveImageAsync(Stream image, string description, int albumId)
        {

            var userId = _userService.GetUserId(User.Identity.Name);
            var album = await _albumService.GetAlbumAsync(albumId);

            if (album.UserId != userId)
                throw new SecurityException();

            var photo = new Photo
                        {
                            AlbumId = albumId,
                            Description = description,
                            UploaderId = userId
                        };

            await _photoService.SavePhotoAsync(photo, image, true);
            return photo;
        }

        [Authorize]
        public async Task<ActionResult> Feed(int id)
        {
            using (_photoService)
            using (_albumService)
            using (_userService)
            {
                var userId = _userService.GetUserId(User.Identity.Name);
                var userDisplayName = _userService.GetUserDisplayName(userId);
                var userPhoto = _photoService.GetUserImageUrl(userId);
                var photo = await _photoService.GetPhotoAsync(id);

                var vm = new FeedViewModel
                         {
                             UserId = userId,
                             UserImageUrl = userPhoto,
                             UserDisplayName = userDisplayName,
                             Time = photo.UploadDate,
                             FeedType = FeedType.Picture,
                             IsFromCurrentUser = true,
                             PhotoViewModel = new PhotoViewModel
                                              {
                                                  PhotoId = photo.Id,
                                                  PhotoUrl = _photoService.GeneratePhotoUrl(userId, photo.Id),
                                                  PhotoDescription = photo.Description
                                              },
                             CommentsViewModel = new CommentsViewModel
                                                 {
                                                     Comments = new List<CommentViewModel>(),
                                                     FeedType = FeedType.Picture,
                                                     CurrentUserPhotoUrl = userPhoto,
                                                     ItemId = photo.Id
                                                 }
                         };

                return View("FeedItems/_PicturePostFeedItem", vm);
            }
        }

        [Authorize, HttpGet]
        public async Task<ActionResult> Crop(int id, string @for)
        {
            using (_photoService)
            using (_albumService)
            using (_userService)
            {
                var vm = new CropViewModel();
                var photo = await _photoService.GetPhotoAsync(id);

                if (photo.IsFacebookPhoto)
                {
                    vm.IsFacebookPhoto = true;
                    return View(vm);
                }

                var userId = _userService.GetUserId(User.Identity.Name);
                if (photo.UploaderId != userId)
                    throw new HttpException(401, "You are not authorized to crop this image.");

                vm.PhotoUrl = _photoService.GeneratePhotoUrl(userId, photo.Id);
                vm.Ratio = @for.Equals("cover", StringComparison.InvariantCultureIgnoreCase)
                               ? 10/3 : 1;

                return View(vm);   
            }
        }

        [Authorize, HttpPost]
        public async Task<ActionResult> Crop(int id, string @for, CropViewModel vm)
        {
            using (_photoService)
            using (_albumService)
            using (_userService)
            {
                var cropArea = new Rectangle((int)vm.X, (int)vm.Y, (int)vm.W, (int)vm.H);
                var photo = await _photoService.GetPhotoAsync(id);

                if (photo.IsFacebookPhoto)
                {
                    vm.IsFacebookPhoto = true;
                    return View(vm);
                }

                var userId = _userService.GetUserId(User.Identity.Name);
                if (photo.UploaderId != userId)
                    throw new HttpException(401, "You are not authorized to crop this image.");

                vm.PhotoUrl = _photoService.GeneratePhotoUrl(userId, photo.Id);
                vm.Ratio = @for.Equals("cover", StringComparison.InvariantCultureIgnoreCase)
                               ? 10 / 3 : 1;

                _photoService.CropPhoto(photo, userId, cropArea);
                return View(vm);
            }
        }
    }
}
