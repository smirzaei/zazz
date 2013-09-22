﻿using System;
using System.Linq;
using NUnit.Framework;
using Zazz.Core.Models.Data;
using Zazz.Data;
using Zazz.Data.Repositories;

namespace Zazz.IntegrationTests.Repositories
{
    [TestFixture]
    public class FollowRepositoryShould
    {
        private ZazzDbContext _context;
        private FollowRepository _repo;
        private User _userA;
        private User _userB;
        private Follow _follow;

        [SetUp]
        public void Init()
        {
            _context = new ZazzDbContext(true);
            _repo = new FollowRepository(_context);

            _userA = Mother.GetUser();
            _userB = Mother.GetUser();

            _context.Users.Add(_userA);
            _context.Users.Add(_userB);

            _context.SaveChanges();

            _follow = new Follow { FromUserId = _userA.Id, ToUserId = _userB.Id };
            _context.Follows.Add(_follow);

            _context.SaveChanges();
        }

        [Test]
        public void ReturnFollowersCorrectly()
        {
            //Arrange
            //Act
            var result = _repo.GetUserFollowers(_userB.Id);

            //Assert
            Assert.AreEqual(1, result.Count());
        }

        [Test]
        public void ReturnFollowsCorrectly()
        {
            //Arrange
            //Act
            var result = _repo.GetUserFollows(_userA.Id);

            //Assert
            Assert.AreEqual(1, result.Count());
        }

        [Test]
        public void ReturnTrue_WhenFollowExists()
        {
            //Arrange
            //Act
            var result = _repo.Exists(_userA.Id, _userB.Id);

            //Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void ReturnFalse_WhenFollowNotExists()
        {
            //Arrange
            //Act
            var result = _repo.Exists(_userB.Id, _userA.Id);

            //Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void RemoveCorrectly_OnRemoveAsync()
        {
            //Arrange
            //Act
            _repo.Remove(_userA.Id, _userB.Id);
            _context.SaveChanges();

            var check = _repo.Exists(_userA.Id, _userB.Id);
            //Assert
            Assert.IsFalse(check);
        }

        [Test]
        public void ReturnCorrectFollowUserIds_OnGetFollowsUserIds()
        {
            //Arrange
            //Act
            var result = _repo.GetFollowsUserIds(_userA.Id);

            //Assert
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(_userB.Id, result.First());
        }


    }
}