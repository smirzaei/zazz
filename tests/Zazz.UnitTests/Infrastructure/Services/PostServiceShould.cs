﻿using System;
using System.Security;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Infrastructure.Services;

namespace Zazz.UnitTests.Infrastructure.Services
{
    [TestFixture]
    public class PostServiceShould
    {
        private Mock<IUoW> _uow;
        private PostService _sut;
        private Post _post;

        [SetUp]
        public void Init()
        {
            _uow = new Mock<IUoW>();
            _sut = new PostService(_uow.Object);

            _uow.Setup(x => x.SaveChanges());

            _post = new Post
                    {
                        Id = 1234,
                        CreatedTime = DateTime.MinValue,
                        Message = "message",
                        UserId = 12
                    };
        }

        [Test]
        public async Task CreateNewPostAndFeedAndShouldNotSetTimeHere_OnNewPost() 
            // we should not set created time here because facebook gives us its own time.
        {
            //Arrange
            _uow.Setup(x => x.PostRepository.InsertGraph(_post));
            _uow.Setup(x => x.FeedRepository.InsertGraph(It.IsAny<Feed>()));

            //Act
            await _sut.NewPostAsync(_post);

            //Assert
            Assert.AreEqual(DateTime.MinValue, _post.CreatedTime);
            _uow.Verify(x => x.PostRepository.InsertGraph(_post), Times.Once());
            _uow.Verify(x => x.FeedRepository.InsertGraph(It.IsAny<Feed>()), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());

        }

        [Test]
        public async Task ThrowWhenCurrentUserIdIsNotSameAsPostOwner_OnRemovePost()
        {
            //Arrange
            _uow.Setup(x => x.PostRepository.GetByIdAsync(_post.Id))
                .Returns(() => Task.Run(() => _post));

            //Act
            try
            {
                await _sut.RemovePostAsync(_post.Id, 1);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (SecurityException)
            {}
            
            //Assert
            _uow.Verify(x => x.PostRepository.GetByIdAsync(_post.Id), Times.Once());
            _uow.Verify(x => x.FeedRepository.RemovePostFeeds(_post.Id), Times.Never());
            _uow.Verify(x => x.PostRepository.Remove(_post), Times.Never());
            _uow.Verify(x => x.CommentRepository.RemovePostComments(_post.Id), Times.Never());
            _uow.Verify(x => x.SaveChanges(), Times.Never());
        }

        [Test]
        public async Task RemoveAndPostAndSaveWhenEverythingIsFine_OnRemove()
        {
            //Arrange
            _uow.Setup(x => x.PostRepository.GetByIdAsync(_post.Id))
                .Returns(() => Task.Run(() => _post));
            _uow.Setup(x => x.PostRepository.Remove(_post));
            _uow.Setup(x => x.FeedRepository.RemovePostFeeds(_post.Id));
            _uow.Setup(x => x.CommentRepository.RemovePostComments(_post.Id));

            //Act
            await _sut.RemovePostAsync(_post.Id, _post.UserId);

            //Assert
            _uow.Verify(x => x.PostRepository.GetByIdAsync(_post.Id), Times.Once());
            _uow.Verify(x => x.PostRepository.Remove(_post), Times.Once());
            _uow.Verify(x => x.FeedRepository.RemovePostFeeds(_post.Id), Times.Once());
            _uow.Verify(x => x.CommentRepository.RemovePostComments(_post.Id), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }

        [Test]
        public async Task ThrowWhenPostNotExists_OnEditPost()
        {
            //Arrange
            _uow.Setup(x => x.PostRepository.GetByIdAsync(_post.Id))
                .Returns(() => Task.Factory.StartNew<Post>(() => null));

            //Act
            try
            {
                await _sut.EditPostAsync(_post.Id, "new text", _post.UserId);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (Exception)
            {
            }

            //Assert
            _uow.Verify(x => x.PostRepository.GetByIdAsync(_post.Id), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Never());
        }

        [Test]
        public async Task ThrowWhenUserIdIsDifferent_OnEditPost()
        {
            //Arrange
            _uow.Setup(x => x.PostRepository.GetByIdAsync(_post.Id))
                .Returns(() => Task.Run(() => _post));

            //Act
            try
            {
                await _sut.EditPostAsync(_post.Id, "new text", 99);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (SecurityException)
            {
            }

            //Assert
            _uow.Verify(x => x.PostRepository.GetByIdAsync(_post.Id), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Never());
        }

        [Test]
        public async Task SaveNewChanges_OnEditPost()
        {
            //Arrange
            var newText = "Edited";
            _uow.Setup(x => x.PostRepository.GetByIdAsync(_post.Id))
                .Returns(() => Task.Run(() => _post));

            //Act
            await _sut.EditPostAsync(_post.Id, newText, _post.UserId);

            //Assert
            Assert.AreEqual(newText, _post.Message);
            _uow.Verify(x => x.PostRepository.GetByIdAsync(_post.Id), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }
    }
}