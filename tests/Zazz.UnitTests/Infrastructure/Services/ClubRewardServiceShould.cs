﻿using System;
using System.Security;
using Moq;
using NUnit.Framework;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
using Zazz.Infrastructure.Services;

namespace Zazz.UnitTests.Infrastructure.Services
{
    [TestFixture]
    public class ClubRewardServiceShould
    {
        private MockRepository _mockRepo;
        private Mock<IUoW> _uow;
        private ClubRewardService _sut;

        [SetUp]
        public void Init()
        {
            _mockRepo = new MockRepository(MockBehavior.Strict);
            _uow = _mockRepo.Create<IUoW>();

            _sut = new ClubRewardService(_uow.Object);
        }

        [Test]
        public void ThrowIfScenarioAlreadyExists_OnAddRewardScenario()
        {
            //Arrange
            var scenario = new ClubPointRewardScenario
                                                         {
                                                             Amount = 12,
                                                             ClubId = 22,
                                                             Scenario = PointRewardScenario.QRCodeSan
                                                         };

            _uow.Setup(x => x.ClubPointRewardScenarioRepository.Exists(scenario.ClubId, scenario.Scenario))
                .Returns(true);

            //Act & Assert

            Assert.Throws<AlreadyExistsException>(() => _sut.AddRewardScenario(scenario));
            _mockRepo.VerifyAll();
        }

        [Test]
        public void SaveNewScenario_OnAddRewardScenario()
        {
            //Arrange
            var scenario = new ClubPointRewardScenario
                           {
                               Amount = 12,
                               ClubId = 22,
                               Scenario = PointRewardScenario.QRCodeSan
                           };

            _uow.Setup(x => x.ClubPointRewardScenarioRepository.Exists(scenario.ClubId, scenario.Scenario))
                .Returns(false);

            _uow.Setup(x => x.ClubPointRewardScenarioRepository.InsertGraph(scenario));
            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.AddRewardScenario(scenario);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfScenarioNotExists_OnChangeRewardAmount()
        {
            //Arrange
            var scenario = new ClubPointRewardScenario
                           {
                               Id = 555,
                               Amount = 12,
                               ClubId = 22,
                               Scenario = PointRewardScenario.QRCodeSan
                           };

            var newAmount = 444;

            _uow.Setup(x => x.ClubPointRewardScenarioRepository.GetById(scenario.Id))
                .Returns(() => null);

            //Act
            Assert.Throws<NotFoundException>(() => _sut.ChangeRewardAmount(scenario.Id, scenario.ClubId, newAmount));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfUserIsNotAllowedToChangeScenario_OnChangeRewardAmount()
        {
            //Arrange
            var scenario = new ClubPointRewardScenario
                           {
                               Id = 555,
                               Amount = 12,
                               ClubId = 22,
                               Scenario = PointRewardScenario.QRCodeSan
                           };

            var newAmount = 444;

            _uow.Setup(x => x.ClubPointRewardScenarioRepository.GetById(scenario.Id))
                .Returns(scenario);

            //Act
            Assert.Throws<SecurityException>(() => _sut.ChangeRewardAmount(scenario.Id, 123, newAmount));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void UpdateScenario_OnChangeRewardAmount()
        {
            //Arrange
            var scenario = new ClubPointRewardScenario
                           {
                               Id = 555,
                               Amount = 12,
                               ClubId = 22,
                               Scenario = PointRewardScenario.QRCodeSan
                           };

            var newAmount = 444;

            _uow.Setup(x => x.ClubPointRewardScenarioRepository.GetById(scenario.Id))
                .Returns(scenario);

            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.ChangeRewardAmount(scenario.Id, scenario.ClubId, newAmount);

            //Assert
            Assert.AreEqual(newAmount, scenario.Amount);
            _mockRepo.VerifyAll();
        }

        [Test]
        public void NotDoAnythingIfScenarioDoesntExists_OnRemoveRewardScenario()
        {
            //Arrange
            var scenario = new ClubPointRewardScenario
                           {
                               Id = 555,
                               Amount = 12,
                               ClubId = 22,
                               Scenario = PointRewardScenario.QRCodeSan
                           };

            _uow.Setup(x => x.ClubPointRewardScenarioRepository.GetById(scenario.Id))
                .Returns(() => null);

            //Act
            _sut.RemoveRewardScenario(scenario.Id, scenario.ClubId);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfUserIsNotAllowedToRemove_OnRemoveRewardScenario()
        {
            //Arrange
            var scenario = new ClubPointRewardScenario
                           {
                               Id = 555,
                               Amount = 12,
                               ClubId = 22,
                               Scenario = PointRewardScenario.QRCodeSan
                           };

            _uow.Setup(x => x.ClubPointRewardScenarioRepository.GetById(scenario.Id))
                .Returns(scenario);

            //Act
            Assert.Throws<SecurityException>(() => _sut.RemoveRewardScenario(scenario.Id, 1));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void RemoveScenario_OnRemoveRewardScenario()
        {
            //Arrange
            var scenario = new ClubPointRewardScenario
                           {
                               Id = 555,
                               Amount = 12,
                               ClubId = 22,
                               Scenario = PointRewardScenario.QRCodeSan
                           };

            _uow.Setup(x => x.ClubPointRewardScenarioRepository.GetById(scenario.Id))
                .Returns(scenario);

            _uow.Setup(x => x.ClubPointRewardScenarioRepository.Remove(scenario));
            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.RemoveRewardScenario(scenario.Id, scenario.ClubId);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfClubIdIsMissing_OnAddClubReward()
        {
            //Arrange
            var reward = new ClubReward
                         {
                             Name = "name",
                             Description = "desc",
                             IsEnabled = true
                         };

            //Act
            Assert.Throws<ArgumentException>(() => _sut.AddClubReward(reward));

            //Assert
            _mockRepo.VerifyAll();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void ThrowIfNameIsMissing_OnAddClubReward(string name)
        {
            //Arrange
            var reward = new ClubReward
            {
                ClubId = 232,
                Name = name,
                Description = "desc",
                IsEnabled = true
            };

            //Act
            Assert.Throws<ArgumentException>(() => _sut.AddClubReward(reward));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void AddClubReward_OnAddClubReward()
        {
            //Arrange
            var reward = new ClubReward
                         {
                             ClubId = 33,
                             Name = "name",
                             Description = "desc",
                             IsEnabled = true
                         };

            _uow.Setup(x => x.ClubRewardRepository.InsertGraph(reward));
            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.AddClubReward(reward);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfRewardNotExists_OnUpdateClubReward()
        {
            //Arrange
            var oldReward = new ClubReward
                            {
                                Id = 444,
                                ClubId = 33,
                                Name = "name",
                                Description = "desc",
                                IsEnabled = true
                            };

            var newReward = new ClubReward
                                   {
                                       Cost = 4423,
                                       Description = "new desc",
                                       Name = "new name",
                                   };

            _uow.Setup(x => x.ClubRewardRepository.GetById(oldReward.Id))
                .Returns(() => null);

            //Act
            
            Assert.Throws<NotFoundException>(() => _sut.UpdateClubReward(oldReward.Id, oldReward.ClubId, newReward));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfUserIsNotAllowedToEdit_OnUpdateClubReward()
        {
            //Arrange
            var oldReward = new ClubReward
                            {
                                Id = 444,
                                ClubId = 33,
                                Name = "name",
                                Description = "desc",
                                IsEnabled = true
                            };

            var newReward = new ClubReward
                            {
                                Cost = 4423,
                                Description = "new desc",
                                Name = "new name",
                            };

            _uow.Setup(x => x.ClubRewardRepository.GetById(oldReward.Id))
                .Returns(oldReward);

            //Act
            Assert.Throws<SecurityException>(() => _sut.UpdateClubReward(oldReward.Id, 888, newReward));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void UpdateReward_OnUpdateClubReward()
        {
            //Arrange
            var oldReward = new ClubReward
                            {
                                Id = 444,
                                ClubId = 33,
                                Name = "name",
                                Description = "desc",
                                IsEnabled = true
                            };

            var newReward = new ClubReward
                            {
                                Cost = 4423,
                                Description = "new desc",
                                Name = "new name",
                            };

            _uow.Setup(x => x.ClubRewardRepository.GetById(oldReward.Id))
                .Returns(oldReward);
            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.UpdateClubReward(oldReward.Id, oldReward.ClubId, newReward);

            //Assert
            Assert.AreEqual(newReward.Name, oldReward.Name);
            Assert.AreEqual(newReward.Description, oldReward.Description);
            Assert.AreEqual(newReward.Cost, oldReward.Cost);

            _mockRepo.VerifyAll();
        }

        [Test]
        public void NotDoAnythingIfRewardNotExists_OnRemoveClubReward()
        {
            //Arrange
            var reward = new ClubReward
                            {
                                Id = 444,
                                ClubId = 33,
                                Name = "name",
                                Description = "desc",
                                IsEnabled = true
                            };

            _uow.Setup(x => x.ClubRewardRepository.GetById(reward.Id))
                .Returns(() => null);

            //Act
            _sut.RemoveClubReward(reward.Id, reward.ClubId);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfUserIsNotAllowed_OnRemoveClubReward()
        {
            //Arrange
            var reward = new ClubReward
            {
                Id = 444,
                ClubId = 33,
                Name = "name",
                Description = "desc",
                IsEnabled = true
            };

            _uow.Setup(x => x.ClubRewardRepository.GetById(reward.Id))
                .Returns(reward);

            //Act
            Assert.Throws<SecurityException>(() => _sut.RemoveClubReward(reward.Id, 99));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void DisableRewardAndNotRemoveIt_OnRemoveClubReward()
        {
            //Arrange
            var reward = new ClubReward
            {
                Id = 444,
                ClubId = 33,
                Name = "name",
                Description = "desc",
                IsEnabled = true
            };

            _uow.Setup(x => x.ClubRewardRepository.GetById(reward.Id))
                .Returns(reward);

            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.RemoveClubReward(reward.Id, reward.ClubId);

            //Assert
            Assert.IsFalse(reward.IsEnabled);
            _mockRepo.VerifyAll();
        }

        [Test]
        public void CreateAHistoryRecordAndUpdateUserPoints_OnAwardUserPoints()
        {
            //Arrange
            var userId = 22;
            var clubId = 33;
            var scenraio = PointRewardScenario.QRCodeSan;
            var amount = 25;

            _uow.Setup(x => x.UserPointRepository.ChangeUserPoints(userId, clubId, amount));
            _uow.Setup(x => x.UserPointHistoryRepository
                             .InsertGraph(It.Is<UserPointHistory>(h => h.ClubId == clubId &&
                                                                       h.UserId == userId &&
                                                                       h.RewardScenario == scenraio)));
            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.AwardUserPoints(userId, clubId, amount, scenraio);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void AddRewardAndCreateHistoryRecordAndUpdateUserPoints()
        {
            //Arrange
            var reward = new ClubReward
                         {
                             ClubId = 22,
                             Cost = 88,
                             Id = 55
                         };

            var userId = 823;

            _uow.Setup(x => x.UserRewardRepository
                             .InsertGraph(It.Is<UserReward>(r => r.PointsSepnt == reward.Cost &&
                                                                 r.RewardId == reward.Id &&
                                                                 r.UserId == userId)));

            _uow.Setup(x => x.UserPointHistoryRepository
                             .InsertGraph(It.Is<UserPointHistory>(h => h.ClubId == reward.ClubId &&
                                                                       h.ChangedAmount == reward.Cost*-1 &&
                                                                       h.RewardId == reward.Id &&
                                                                       h.UserId == userId)));

            _uow.Setup(x => x.UserPointRepository.ChangeUserPoints(userId, reward.ClubId, reward.Cost*-1));

            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.RedeemPoints(userId, reward);

            //Assert
            _mockRepo.VerifyAll();
        }


    }
}