﻿using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Newtonsoft.Json;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.UnitTests.Web.Controllers.Api
{
    [TestFixture]
    public class WeekliesControllerShould : BaseHMACTests
    {
        private int _weeklyId;
        private Mock<IWeeklyService> _weeklyService;

        public override void Init()
        {
            base.Init();
            
            _weeklyId = 21;
            ControllerAddress = "/api/v1/weeklies/" + _weeklyId;

            _weeklyService = MockRepo.Create<IWeeklyService>();
            IocContainer.Configure(x =>
                                   {
                                       x.For<IWeeklyService>().Use(_weeklyService.Object);
                                   });
        }

        [Test]
        public async Task Return400IfIdIs0_OnGet()
        {
            //Arrange
            ControllerAddress = "/api/v1/weeklies/0";

            AddValidHMACHeaders("GET");
            SetupMocksForHMACAuth();

            //Act
            var result = await Client.GetAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return404IfWeeklyNotExists_OnGet()
        {
            //Arrange
            AddValidHMACHeaders("GET");
            SetupMocksForHMACAuth();

            _weeklyService.Setup(x => x.GetWeekly(_weeklyId))
                          .Throws<NotFoundException>();

            //Act
            var result = await Client.GetAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task ReturnWeekly_OnGet()
        {
            //Arrange
            AddValidHMACHeaders("GET");
            SetupMocksForHMACAuth();

            var weekly = new Weekly
                         {
                             Id = _weeklyId,
                             Description = "desc",
                             Name = "name",
                         };

            _weeklyService.Setup(x => x.GetWeekly(_weeklyId))
                          .Returns(weekly);

            //Act
            var result = await Client.GetAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return403IfUserIsNotClubAdmin_OnPost()
        {
            //Arrange
            ControllerAddress = "/api/v1/weeklies";

            var weekly = new Weekly
            {
                Description = "desc",
                Name = "name",
            };

            var json = JsonConvert.SerializeObject(weekly);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _weeklyService.Setup(x => x.CreateWeekly(It.IsAny<Weekly>()))
                          .Throws<InvalidOperationException>();

            AddValidHMACHeaders("POST", ControllerAddress, json);
            SetupMocksForHMACAuth();

            //Act
            var result = await Client.PostAsync(ControllerAddress, content);

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, result.StatusCode);
        }
    }
}