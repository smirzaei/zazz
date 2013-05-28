﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data.Enums;
using Zazz.Web.Interfaces;
using Zazz.Web.Models;
using Zazz.Web.Models.Api;

namespace Zazz.UnitTests.Web.Controllers.Api
{
    [TestFixture]
    public class PhotoCommentsControllerShould : BaseHMACTests
    {
        private Mock<ICommentService> _commentService;
        private ApiComment _comment;
        private int _commentId;
        private Mock<IFeedHelper> _feedHelper;
        private int _photoId;

        public override void Init()
        {
            base.Init();

            _commentId = 99;
            _photoId = 444;
            ControllerAddress = "/api/v1/photocomments/" + _photoId;

            _comment = new ApiComment
                       {
                           CommentId =  _commentId,
                           CommentText = "message"
                       };

            _commentService = MockRepo.Create<ICommentService>();
            _feedHelper = MockRepo.Create<IFeedHelper>();
            IocContainer.Configure(x =>
                                   {
                                       x.For<ICommentService>().Use(_commentService.Object);
                                       x.For<IFeedHelper>().Use(_feedHelper.Object);
                                   });
        }

        [Test]
        public async Task Return400IfIdIs0_OnGet()
        {
            //Arrange
            ControllerAddress = "/api/v1/photocomments/" + 0;

            AddValidHMACHeaders("GET");
            SetupMocksForHMACAuth();

            //Act
            var response = await Client.GetAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task GetCommentsUsingFeedHelper_OnGet()
        {
            //Arrange
            AddValidHMACHeaders("GET");
            SetupMocksForHMACAuth();

            _feedHelper.Setup(x => x.GetComments(_photoId, CommentType.Photo, User.Id, 0, 5))
                       .Returns(new List<CommentViewModel> {new CommentViewModel()});

            _feedHelper.Setup(x => x.CommentViewModelToApiModel(It.IsAny<CommentViewModel>()))
                       .Returns(new ApiComment());

            //Act
            var response = await Client.GetAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            MockRepo.VerifyAll();
        }


    }
}