﻿using System;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.DTOs;
using Zazz.Infrastructure;
using Zazz.Infrastructure.Services;

namespace Zazz.UnitTests.Infrastructure.Services
{
    [TestFixture]
    public class PhotoServiceShould
    {
        private Mock<IUoW> _uow;
        private Mock<IFileService> _fs;
        private PhotoService _sut;
        private string _tempRootPath;
        private Mock<ICacheService> _cacheService;

        [SetUp]
        public void Init()
        {
            _uow = new Mock<IUoW>();
            _fs = new Mock<IFileService>();
            _cacheService = new Mock<ICacheService>();
            _tempRootPath = Path.GetTempPath();
            _sut = new PhotoService(_uow.Object, _fs.Object, _tempRootPath, _cacheService.Object);
            _uow.Setup(x => x.SaveChanges());
        }

        [TestCase("/picture/user/12/333.jpg", 12, 333)]
        [TestCase("/picture/user/800/1203200.jpg", 800, 1203200)]
        [TestCase("/picture/user/102/3330.jpg", 102, 3330)]
        public void GenerateCorrectPath_OnGeneratePhotoUrl(string expected, int userId, int photoId)
        {
            //Arrange
            //Act
            var result = _sut.GeneratePhotoUrl(userId, photoId);

            //Assert
            Assert.AreEqual(expected, result);
        }

        [TestCase(12, 333)]
        [TestCase(800, 1203200)]
        [TestCase(102, 3330)]
        public void GenerateCorrectPath_OnGeneratePhotoFilePath(int userId, int photoId)
        {
            //Arrange
            var expected = String.Format(@"{0}\picture\user\{1}\{2}.jpg", _tempRootPath, userId, photoId);

            //Act
            var result = _sut.GeneratePhotoFilePath(userId, photoId);

            //Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public async Task CallGetDescriptionFromRepo_OnGetPhotoDescriptionAsync()
        {
            //Arrange
            var id = 123;
            _uow.Setup(x => x.PhotoRepository.GetDescriptionAsync(id))
                .Returns(() => Task.Run(() => "description"));

            //Act
            var result = await _sut.GetPhotoDescriptionAsync(id);

            //Assert
            _uow.Verify(x => x.PhotoRepository.GetDescriptionAsync(id), Times.Once());
        }

        [Test]
        public async Task SavePhotoToDiskAndDBAndCreateAFeedRecordWhenLastFeedIsNullThenReturnPhotoId_OnSavePhoto()
        {
            //Arrange
            var photo = new Photo
            {
                Id = 1234,
                AlbumId = 12,
                Description = "desc",
                UserId = 17
            };
            _uow.Setup(x => x.PhotoRepository.InsertGraph(photo));
            _uow.Setup(x => x.FeedRepository.InsertGraph(It.IsAny<Feed>()));
            _uow.Setup(x => x.FeedRepository.GetUserLastFeed(photo.UserId))
                .Returns(() => null);

            var path = _sut.GeneratePhotoFilePath(photo.UserId, photo.Id);
            using (var ms = new MemoryStream())
            {
                _fs.Setup(x => x.SaveFileAsync(path, ms))
                   .Returns(() => Task.Run(() => { }));

                //Act

                var id = await _sut.SavePhotoAsync(photo, ms, true);

                //Assert
                _uow.Verify(x => x.PhotoRepository.InsertGraph(photo), Times.Once());
                _uow.Verify(x => x.FeedRepository.InsertGraph(It.IsAny<Feed>()), Times.Once());
                _uow.Verify(x => x.SaveChanges(), Times.Exactly(2));
                _uow.Verify(x => x.FeedRepository.GetUserLastFeed(photo.UserId), Times.Once());
                _fs.Verify(x => x.SaveFileAsync(path, ms));
                Assert.AreEqual(DateTime.UtcNow.Date, photo.UploadDate.Date);
                Assert.AreEqual(photo.Id, id);
            }
        }

        [Test]
        public async Task SavePhotoToDiskAndDBAndCreateAFeedRecordWhenLastFeedIsPhotoButItsOlderThan24HoursThenReturnPhotoId_OnSavePhoto()
        {
            //Arrange
            var lastFeed = new Feed
                           {
                               FeedType = FeedType.Picture,
                               Time = DateTime.UtcNow.AddDays(-2)
                           };

            var photo = new Photo
                            {
                                Id = 1234,
                                AlbumId = 12,
                                Description = "desc",
                                UserId = 17
                            };
            _uow.Setup(x => x.PhotoRepository.InsertGraph(photo));
            _uow.Setup(x => x.FeedRepository.InsertGraph(It.IsAny<Feed>()));
            _uow.Setup(x => x.FeedRepository.GetUserLastFeed(photo.UserId))
                .Returns(lastFeed);

            var path = _sut.GeneratePhotoFilePath(photo.UserId, photo.Id);
            using (var ms = new MemoryStream())
            {
                _fs.Setup(x => x.SaveFileAsync(path, ms))
                   .Returns(() => Task.Run(() => { }));

                //Act

                var id = await _sut.SavePhotoAsync(photo, ms, true);

                //Assert
                _uow.Verify(x => x.PhotoRepository.InsertGraph(photo), Times.Once());
                _uow.Verify(x => x.FeedRepository.InsertGraph(It.IsAny<Feed>()), Times.Once());
                _uow.Verify(x => x.SaveChanges(), Times.Exactly(2));
                _uow.Verify(x => x.FeedRepository.GetUserLastFeed(photo.UserId), Times.Once());
                _fs.Verify(x => x.SaveFileAsync(path, ms));
                Assert.AreEqual(DateTime.UtcNow.Date, photo.UploadDate.Date);
                Assert.AreEqual(photo.Id, id);
            }
        }

        [Test]
        public async Task SavePhotoToDiskAndDBAndCreateAFeedRecordWhenLastFeedIsNotPhotoThenReturnPhotoId_OnSavePhoto()
        {
            //Arrange
            var lastFeed = new Feed
            {
                FeedType = FeedType.Event,
                Time = DateTime.UtcNow.AddHours(-2)
            };

            var photo = new Photo
            {
                Id = 1234,
                AlbumId = 12,
                Description = "desc",
                UserId = 17
            };
            _uow.Setup(x => x.PhotoRepository.InsertGraph(photo));
            _uow.Setup(x => x.FeedRepository.InsertGraph(It.IsAny<Feed>()));
            _uow.Setup(x => x.FeedRepository.GetUserLastFeed(photo.UserId))
                .Returns(lastFeed);

            var path = _sut.GeneratePhotoFilePath(photo.UserId, photo.Id);
            using (var ms = new MemoryStream())
            {
                _fs.Setup(x => x.SaveFileAsync(path, ms))
                   .Returns(() => Task.Run(() => { }));

                //Act

                var id = await _sut.SavePhotoAsync(photo, ms, true);

                //Assert
                _uow.Verify(x => x.PhotoRepository.InsertGraph(photo), Times.Once());
                _uow.Verify(x => x.FeedRepository.InsertGraph(It.IsAny<Feed>()), Times.Once());
                _uow.Verify(x => x.SaveChanges(), Times.Exactly(2));
                _uow.Verify(x => x.FeedRepository.GetUserLastFeed(photo.UserId), Times.Once());
                _fs.Verify(x => x.SaveFileAsync(path, ms));
                Assert.AreEqual(DateTime.UtcNow.Date, photo.UploadDate.Date);
                Assert.AreEqual(photo.Id, id);
            }
        }

        [Test]
        public async Task SavePhotoToDiskAndDBAndAddPhotoIdToLastFeedItemWhenLastFeedIsPhotoAndLessThan24Hours_OnSavePhoto()
        {
            //Arrange
            var lastFeed = new Feed
            {
                FeedType = FeedType.Picture,
                Time = DateTime.UtcNow.AddHours(-23)
            };

            var photo = new Photo
            {
                Id = 1234,
                AlbumId = 12,
                Description = "desc",
                UserId = 17
            };
            _uow.Setup(x => x.PhotoRepository.InsertGraph(photo));
            _uow.Setup(x => x.FeedRepository.InsertGraph(It.IsAny<Feed>()));
            _uow.Setup(x => x.FeedRepository.GetUserLastFeed(photo.UserId))
                .Returns(lastFeed);

            var path = _sut.GeneratePhotoFilePath(photo.UserId, photo.Id);
            using (var ms = new MemoryStream())
            {
                _fs.Setup(x => x.SaveFileAsync(path, ms))
                   .Returns(() => Task.Run(() => { }));

                //Act

                var id = await _sut.SavePhotoAsync(photo, ms, true);

                //Assert
                _uow.Verify(x => x.PhotoRepository.InsertGraph(photo), Times.Once());
                _uow.Verify(x => x.FeedRepository.InsertGraph(It.IsAny<Feed>()), Times.Never());
                _uow.Verify(x => x.SaveChanges(), Times.Exactly(2));
                _uow.Verify(x => x.FeedRepository.GetUserLastFeed(photo.UserId), Times.Once());
                _fs.Verify(x => x.SaveFileAsync(path, ms));
                Assert.IsNotNull(lastFeed.FeedPhotoIds.Single(p => p.PhotoId == photo.Id));
                Assert.AreEqual(DateTime.UtcNow.Date, photo.UploadDate.Date);
                Assert.AreEqual(photo.Id, id);
            }
        }

        [Test]
        public async Task SavePhotoToDiskAndDBAndNotCreateAFeedWhenIsSpecifiedThenReturnPhotoId_OnSavePhoto()
        {
            //Arrange
            var photo = new Photo
            {
                Id = 1234,
                AlbumId = 12,
                Description = "desc",
                UserId = 17
            };
            _uow.Setup(x => x.PhotoRepository.InsertGraph(photo));
            _uow.Setup(x => x.FeedRepository.InsertGraph(It.IsAny<Feed>()));

            var path = _sut.GeneratePhotoFilePath(photo.UserId, photo.Id);
            using (var ms = new MemoryStream())
            {
                _fs.Setup(x => x.SaveFileAsync(path, ms))
                   .Returns(() => Task.Run(() => { }));

                //Act

                var id = await _sut.SavePhotoAsync(photo, ms, false);

                //Assert
                _uow.Verify(x => x.PhotoRepository.InsertGraph(photo), Times.Once());
                _uow.Verify(x => x.FeedRepository.InsertGraph(It.IsAny<Feed>()), Times.Never());
                _uow.Verify(x => x.SaveChanges(), Times.Once());
                _uow.Verify(x => x.FeedRepository.GetUserLastFeed(photo.UserId), Times.Never());
                _fs.Verify(x => x.SaveFileAsync(path, ms));
                Assert.AreEqual(DateTime.UtcNow.Date, photo.UploadDate.Date);
                Assert.AreEqual(photo.Id, id);
            }
        }

        [Test]
        public async Task SavePhotoRecordToDbButNotCallSaveToDiskWhenStreamIsEmpty_OnSavePhoto()
        {
            //Arrange
            var photo = new Photo
            {
                Id = 1234,
                AlbumId = 12,
                Description = "desc",
                UserId = 17
            };
            _uow.Setup(x => x.PhotoRepository.InsertGraph(photo));
            _uow.Setup(x => x.FeedRepository.InsertGraph(It.IsAny<Feed>()));

            var path = _sut.GeneratePhotoFilePath(photo.UserId, photo.Id);

            _fs.Setup(x => x.SaveFileAsync(path, Stream.Null))
                   .Returns(() => Task.Run(() => { }));

            //Act

            var id = await _sut.SavePhotoAsync(photo, Stream.Null, false);

            //Assert
            _uow.Verify(x => x.PhotoRepository.InsertGraph(photo), Times.Once());
            _uow.Verify(x => x.FeedRepository.InsertGraph(It.IsAny<Feed>()), Times.Never());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
            _fs.Verify(x => x.SaveFileAsync(It.IsAny<string>(), It.IsAny<Stream>()), Times.Never());
            Assert.AreEqual(DateTime.UtcNow.Date, photo.UploadDate.Date);
            Assert.AreEqual(photo.Id, id);
        }

        [Test]
        public async Task ThrowIfTheCurrentUserIsNotTheOwner_OnRemovePhoto()
        {
            //Arrange
            var photoId = 124;
            var userId = 12;
            var photo = new Photo
                            {
                                Id = photoId,
                                AlbumId = 123,
                                UserId = 999
                            };

            _uow.Setup(x => x.PhotoRepository.GetByIdAsync(photoId))
                .Returns(() => Task.Run(() => photo));

            //Act
            try
            {
                await _sut.RemovePhotoAsync(photoId, userId);
                Assert.Fail("Expected exception wasn't thrown");
            }
            catch (SecurityException)
            {
            }

            //Assert
            _uow.Verify(x => x.PhotoRepository.GetByIdAsync(photoId), Times.Once());
            _uow.Verify(x => x.EventRepository.ResetPhotoId(photoId), Times.Never());
            _uow.Verify(x => x.UserRepository.ResetPhotoId(photoId), Times.Never());
            _uow.Verify(x => x.CommentRepository.RemovePhotoComments(photoId), Times.Never());
        }

        [Test]
        public async Task SetCoverPhotoIdTo0IfThePhotoIsCoverPhotoIdAndResetEventPhotoId_OnRemovePhoto()
        {
            ///Arrange
            var photoId = 124;
            var userId = 12;
            var albumId = 444;
            var coverPhotoId = photoId;
            var profilePhotoId = 2;

            var photo = new Photo
            {
                Id = photoId,
                AlbumId = albumId,
                UserId = userId,
                User = new User
                {
                    UserDetail = new UserDetail
                    {
                        CoverPhotoId = coverPhotoId,
                        ProfilePhotoId = profilePhotoId
                    }
                }
            };

            _uow.Setup(x => x.PhotoRepository.GetByIdAsync(photoId))
                .Returns(() => Task.Run(() => photo));

            var filePath = _sut.GeneratePhotoFilePath(userId, photoId);

            _uow.Setup(x => x.PhotoRepository.GetOwnerIdAsync(photoId))
                .Returns(() => Task.Run(() => userId));
            _uow.Setup(x => x.PhotoRepository.RemoveAsync(photoId))
                .Returns(() => Task.Run(() => { }));
            _fs.Setup(x => x.RemoveFile(filePath));
            _uow.Setup(x => x.EventRepository.ResetPhotoId(photoId));
            _uow.Setup(x => x.UserRepository.ResetPhotoId(photoId));
            _uow.Setup(x => x.CommentRepository.RemovePhotoComments(photoId));

            //Act
            await _sut.RemovePhotoAsync(photoId, userId);

            //Assert
            Assert.AreEqual(0, photo.User.UserDetail.CoverPhotoId);
            Assert.AreEqual(profilePhotoId, photo.User.UserDetail.ProfilePhotoId);
            _uow.Verify(x => x.PhotoRepository.Remove(photo), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
            _fs.Verify(x => x.RemoveFile(filePath), Times.Once());
            _uow.Verify(x => x.EventRepository.ResetPhotoId(photoId), Times.Once());
            _uow.Verify(x => x.UserRepository.ResetPhotoId(photoId), Times.Once());
            _uow.Verify(x => x.CommentRepository.RemovePhotoComments(photoId), Times.Once());
        }

        [Test]
        public async Task SetProfilePhotoIdTo0IfThePhotoIsCoverPhotoIdAndResetEventPhotoId_OnRemovePhoto()
        {
            ///Arrange
            var photoId = 124;
            var userId = 12;
            var albumId = 444;
            var coverPhotoId = 2;
            var profilePhotoId = photoId;

            var photo = new Photo
            {
                Id = photoId,
                AlbumId = albumId,
                UserId = userId,
                User = new User
                {
                    UserDetail = new UserDetail
                    {
                        CoverPhotoId = coverPhotoId,
                        ProfilePhotoId = profilePhotoId
                    }
                }
            };

            _uow.Setup(x => x.PhotoRepository.GetByIdAsync(photoId))
                .Returns(() => Task.Run(() => photo));

            var filePath = _sut.GeneratePhotoFilePath(userId, photoId);

            _uow.Setup(x => x.PhotoRepository.GetOwnerIdAsync(photoId))
                .Returns(() => Task.Run(() => userId));
            _uow.Setup(x => x.PhotoRepository.RemoveAsync(photoId))
                .Returns(() => Task.Run(() => { }));
            _fs.Setup(x => x.RemoveFile(filePath));
            _uow.Setup(x => x.EventRepository.ResetPhotoId(photoId));
            _uow.Setup(x => x.UserRepository.ResetPhotoId(photoId));
            _uow.Setup(x => x.CommentRepository.RemovePhotoComments(photoId));

            //Act
            await _sut.RemovePhotoAsync(photoId, userId);

            //Assert
            Assert.AreEqual(coverPhotoId, photo.User.UserDetail.CoverPhotoId);
            Assert.AreEqual(0, photo.User.UserDetail.ProfilePhotoId);
            _uow.Verify(x => x.PhotoRepository.Remove(photo), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
            _fs.Verify(x => x.RemoveFile(filePath), Times.Once());
            _uow.Verify(x => x.EventRepository.ResetPhotoId(photoId), Times.Once());
            _uow.Verify(x => x.UserRepository.ResetPhotoId(photoId), Times.Once());
            _uow.Verify(x => x.CommentRepository.RemovePhotoComments(photoId), Times.Once());
        }

        [Test]
        public async Task RemoveFileAndDbAndFeedRecordAndResetEventPhotoIdAndNotTouchCoverAndProfilePhotoIdsIfTheyAreDifferent_OnRemovePhoto()
        {
            //Arrange
            var photoId = 124;
            var userId = 12;
            var albumId = 444;
            var coverPhotoId = 4;
            var profilePhotoId = 2;

            var photo = new Photo
            {
                Id = photoId,
                AlbumId = albumId,
                UserId = userId,
                User = new User
                {
                    UserDetail = new UserDetail
                    {
                        CoverPhotoId = coverPhotoId,
                        ProfilePhotoId = profilePhotoId
                    }
                }
            };
            
            _uow.Setup(x => x.PhotoRepository.GetByIdAsync(photoId))
                .Returns(() => Task.Run(() => photo));

            var filePath = _sut.GeneratePhotoFilePath(userId, photoId);

            _uow.Setup(x => x.PhotoRepository.GetOwnerIdAsync(photoId))
                .Returns(() => Task.Run(() => userId));
            _uow.Setup(x => x.PhotoRepository.RemoveAsync(photoId))
                .Returns(() => Task.Run(() => { }));
            _fs.Setup(x => x.RemoveFile(filePath));
            _uow.Setup(x => x.EventRepository.ResetPhotoId(photoId));
            _uow.Setup(x => x.UserRepository.ResetPhotoId(photoId));
            _uow.Setup(x => x.CommentRepository.RemovePhotoComments(photoId));

            //Act
            await _sut.RemovePhotoAsync(photoId, userId);

            //Assert
            Assert.AreEqual(coverPhotoId, photo.User.UserDetail.CoverPhotoId);
            Assert.AreEqual(profilePhotoId, photo.User.UserDetail.ProfilePhotoId);
            _uow.Verify(x => x.PhotoRepository.Remove(photo), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
            _fs.Verify(x => x.RemoveFile(filePath), Times.Once());
            _uow.Verify(x => x.EventRepository.ResetPhotoId(photoId), Times.Once());
            _uow.Verify(x => x.UserRepository.ResetPhotoId(photoId), Times.Once());
            _uow.Verify(x => x.CommentRepository.RemovePhotoComments(photoId), Times.Once());
        }

        [Test]
        public async Task ThrowIfUserPhotoIdIs0_OnUpdatePhoto()
        {
            //Arrange

            var photo = new Photo
            {
                Id = 0,
                UserId = 124,
                AlbumId = 123
            };
            _uow.Setup(x => x.PhotoRepository.GetOwnerIdAsync(photo.Id))
                .Returns(() => Task.Run(() => photo.UserId));

            //Act
            try
            {
                await _sut.UpdatePhotoAsync(photo, 1234);
                Assert.Fail("Expected exception wasn't thrown");
            }
            catch (ArgumentException)
            {
            }

            //Assert
            _uow.Verify(x => x.PhotoRepository.GetOwnerIdAsync(photo.Id), Times.Never());
        }

        [Test]
        public async Task ThrowIfUserIdIsNotSameAsOwnerId_OnUpdatePhoto()
        {
            //Arrange
            var photoId = 124;
            var userId = 12;
            var albumId = 444;

            var photo = new Photo
            {
                Id = photoId,
                AlbumId = albumId,
                UserId = 890
            };

            _uow.Setup(x => x.PhotoRepository.GetOwnerIdAsync(photo.Id))
                .Returns(() => Task.Run(() => photo.UserId));

            //Act
            try
            {
                await _sut.UpdatePhotoAsync(photo, userId);
                Assert.Fail("Expected exception wasn't thrown");
            }
            catch (SecurityException)
            {
            }

            //Assert
            _uow.Verify(x => x.PhotoRepository.GetOwnerIdAsync(photo.Id), Times.Once());
        }

        [Test]
        public async Task UpdateAndSave_OnUpdatePhoto()
        {
            //Arrange
            var photoId = 124;
            var userId = 12;
            var albumId = 444;

            var photo = new Photo
            {
                Id = photoId,
                AlbumId = albumId,
                UserId = userId
            };

            _uow.Setup(x => x.PhotoRepository.GetOwnerIdAsync(photo.Id))
                .Returns(() => Task.Run(() => photo.UserId));
            _uow.Setup(x => x.PhotoRepository.InsertOrUpdate(photo));

            //Act
            await _sut.UpdatePhotoAsync(photo, userId);

            //Assert
            _uow.Verify(x => x.PhotoRepository.GetOwnerIdAsync(photo.Id), Times.Once());
            _uow.Verify(x => x.PhotoRepository.InsertOrUpdate(photo), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }

        [Test]
        public void ReturnDefaultImage_WhenPhotoIdIs0AndAddToCacheIfItsNotThere_GetUserImageUrl()
        {
            //Arrange
            var userId = 12;
            var gender = Gender.Male;
            var photoId = 123;

            var photo = new PhotoMinimalDTO
                        {
                            AlbumId = 3,
                            Id = photoId,
                            UploaderId = userId
                        };

            var expected = DefaultImageHelper.GetUserDefaultImage(gender);

            _uow.Setup(x => x.UserRepository.GetUserPhotoId(userId))
                .Returns(0);
            _uow.Setup(x => x.UserRepository.GetUserGender(userId))
                .Returns(gender);
            _uow.Setup(x => x.PhotoRepository.GetPhotoWithMinimalData(photoId))
                .Returns(() => photo);
            _cacheService.Setup(x => x.GetUserPhotoUrl(userId))
                         .Returns(() => null);
            _cacheService.Setup(x => x.AddUserPhotoUrl(userId, expected));


            //Act
            var result = _sut.GetUserImageUrl(userId);

            //Assert
            Assert.AreEqual(expected, result);
            _uow.Verify(x => x.UserRepository.GetUserPhotoId(userId), Times.Once());
            _uow.Verify(x => x.UserRepository.GetUserGender(userId), Times.Once());
            _uow.Verify(x => x.PhotoRepository.GetPhotoWithMinimalData(It.IsAny<int>()), Times.Never());
            _cacheService.Verify(x => x.GetUserPhotoUrl(userId), Times.Once());
            _cacheService.Verify(x => x.AddUserPhotoUrl(userId, expected), Times.Once());
        }

        [Test]
        public void ReturnDefaultImage_WhenPhotoIdIsNot0ButPhotoIsNullAndAddToCacheIfItsNotThere_GetUserImageUrl()
        {
            //Arrange
            var userId = 12;
            var gender = Gender.Male;
            var photoId = 123;

            var photo = new PhotoMinimalDTO
            {
                AlbumId = 3,
                Id = photoId,
                UploaderId = userId
            };

            var expected = DefaultImageHelper.GetUserDefaultImage(gender);

            _uow.Setup(x => x.UserRepository.GetUserPhotoId(userId))
                .Returns(photoId);
            _uow.Setup(x => x.UserRepository.GetUserGender(userId))
                .Returns(gender);
            _uow.Setup(x => x.PhotoRepository.GetPhotoWithMinimalData(photoId))
                .Returns(() => null);
            _cacheService.Setup(x => x.GetUserPhotoUrl(userId))
                         .Returns(() => null);
            _cacheService.Setup(x => x.AddUserPhotoUrl(userId, expected));

            //Act
            var result = _sut.GetUserImageUrl(userId);

            //Assert
            Assert.AreEqual(expected, result);
            _uow.Verify(x => x.UserRepository.GetUserPhotoId(userId), Times.Once());
            _uow.Verify(x => x.UserRepository.GetUserGender(userId), Times.Once());
            _uow.Verify(x => x.PhotoRepository.GetPhotoWithMinimalData(photoId), Times.Once());
            _cacheService.Verify(x => x.GetUserPhotoUrl(userId), Times.Once());
            _cacheService.Verify(x => x.AddUserPhotoUrl(userId, expected), Times.Once());
        }

        [Test]
        public void ReturnUserImage_WhenPhotoIdIsNot0AndPhotoIsNotNullAndAddToCacheIfItsNotThere_GetUserImageUrl()
        {
            //Arrange
            var userId = 12;
            var gender = Gender.Male;
            var photoId = 123;

            var photo = new PhotoMinimalDTO
            {
                AlbumId = 3,
                Id = photoId,
                UploaderId = userId
            };

            var expected = _sut.GeneratePhotoUrl(userId, photoId);

            _uow.Setup(x => x.UserRepository.GetUserPhotoId(userId))
                .Returns(photoId);
            _uow.Setup(x => x.UserRepository.GetUserGender(userId))
                .Returns(gender);
            _uow.Setup(x => x.PhotoRepository.GetPhotoWithMinimalData(photoId))
                .Returns(() => photo);

            _cacheService.Setup(x => x.GetUserPhotoUrl(userId))
                         .Returns(() => null);
            _cacheService.Setup(x => x.AddUserPhotoUrl(userId, expected));

            
            //Act
            var result = _sut.GetUserImageUrl(userId);

            //Assert
            Assert.AreEqual(expected, result);
            _uow.Verify(x => x.UserRepository.GetUserPhotoId(userId), Times.Once());
            _uow.Verify(x => x.UserRepository.GetUserGender(It.IsAny<int>()), Times.Never());
            _uow.Verify(x => x.PhotoRepository.GetPhotoWithMinimalData(photoId), Times.Once());
            _cacheService.Verify(x => x.GetUserPhotoUrl(userId), Times.Once());
            _cacheService.Verify(x => x.AddUserPhotoUrl(userId, expected), Times.Once());
        }

        [Test]
        public void ReturnFacebookImageUrlIfThePhotoIsFromFacebookAndAddToCacheIfItsNotThere_OnGetUserImageUrl()
        {
            //Arrange
            var userId = 12;
            var gender = Gender.Male;
            var photoId = 123;

            var photo = new PhotoMinimalDTO
            {
                AlbumId = 3,
                Id = photoId,
                UploaderId = userId,
                IsFacebookPhoto = true,
                FacebookPicUrl = "pic url"
            };

            var expected = photo.FacebookPicUrl;

            _uow.Setup(x => x.UserRepository.GetUserPhotoId(userId))
                .Returns(photoId);
            _uow.Setup(x => x.UserRepository.GetUserGender(userId))
                .Returns(gender);
            _uow.Setup(x => x.PhotoRepository.GetPhotoWithMinimalData(photoId))
                .Returns(() => photo);
            _cacheService.Setup(x => x.GetUserPhotoUrl(userId))
                         .Returns(() => null);
            _cacheService.Setup(x => x.AddUserPhotoUrl(userId, expected));

            //Act
            var result = _sut.GetUserImageUrl(userId);

            //Assert
            Assert.AreEqual(expected, result);
            _uow.Verify(x => x.UserRepository.GetUserPhotoId(userId), Times.Once());
            _uow.Verify(x => x.UserRepository.GetUserGender(It.IsAny<int>()), Times.Never());
            _uow.Verify(x => x.PhotoRepository.GetPhotoWithMinimalData(photoId), Times.Once());
            _cacheService.Verify(x => x.GetUserPhotoUrl(userId), Times.Once());
            _cacheService.Verify(x => x.AddUserPhotoUrl(userId, expected), Times.Once());
        }

        [Test]
        public void ReturnUserImageFromCacheIfExists_GetUserImageUrl()
        {
            //Arrange
            var userId = 12;
            var gender = Gender.Male;
            var photoId = 123;

            var photo = new PhotoMinimalDTO
            {
                AlbumId = 3,
                Id = photoId,
                UploaderId = userId
            };

            var expected = _sut.GeneratePhotoUrl(userId, photoId);

            _uow.Setup(x => x.UserRepository.GetUserPhotoId(userId))
                .Returns(photoId);
            _uow.Setup(x => x.UserRepository.GetUserGender(userId))
                .Returns(gender);
            _uow.Setup(x => x.PhotoRepository.GetPhotoWithMinimalData(photoId))
                .Returns(() => photo);

            _cacheService.Setup(x => x.GetUserPhotoUrl(userId))
                         .Returns(expected);
            _cacheService.Setup(x => x.AddUserPhotoUrl(userId, expected));


            //Act
            var result = _sut.GetUserImageUrl(userId);

            //Assert
            Assert.AreEqual(expected, result);
            _uow.Verify(x => x.UserRepository.GetUserPhotoId(userId), Times.Never());
            _uow.Verify(x => x.UserRepository.GetUserGender(It.IsAny<int>()), Times.Never());
            _uow.Verify(x => x.PhotoRepository.GetPhotoWithMinimalData(photoId), Times.Never());
            _cacheService.Verify(x => x.GetUserPhotoUrl(userId), Times.Once());
            _cacheService.Verify(x => x.AddUserPhotoUrl(userId, expected), Times.Never());
        }
    }
}