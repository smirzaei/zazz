﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
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
    public class EventServiceShould
    {
        private Mock<IUoW> _uow;
        private EventService _sut;
        private int _userId;
        private ZazzEvent _zazzEvent;
        private Mock<INotificationService> _notificationService;
        private Mock<IStringHelper> _stringHelper;
        private Mock<IStaticDataRepository> _staticDataRepo;

        [SetUp]
        public void Init()
        {
            _uow = new Mock<IUoW>();
            _notificationService = new Mock<INotificationService>();
            _stringHelper = new Mock<IStringHelper>();
            _staticDataRepo = new Mock<IStaticDataRepository>();
            _sut = new EventService(_uow.Object, _notificationService.Object,
                _stringHelper.Object, _staticDataRepo.Object);
            _userId = 21;
            _zazzEvent = new ZazzEvent { UserId = _userId };

            _uow.Setup(x => x.SaveChanges());
        }

        [Test]
        public void ThrowIfUserIdIs0_OnGetUserEvents()
        {
            //Act & Assert
            Assert.Throws<ArgumentException>(() => _sut.GetUserEvents(0, 5, null));
        }

        [Test]
        public void ThrowIfTakeIs0_OnGetUserEvents()
        {
            //Act & Assert
            Assert.Throws<ArgumentException>(() => _sut.GetUserEvents(5, 0, null));
        }

        [Test]
        public void ThrowExceptionWhenUserIdIs0_OnCreateEvent()
        {
            //Arrange
            _zazzEvent.UserId = 0;
            //Act
            try
            {
                _sut.CreateEvent(_zazzEvent);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (ArgumentException e)
            {
                //Assert
            }
        }

        [Test]
        public void InsertAndSaveAndCreateFeed_OnCreateEvent()
        {
            //Arrange
            _uow.Setup(x => x.EventRepository.InsertGraph(_zazzEvent));
            _uow.Setup(x => x.FeedRepository.InsertGraph(It.IsAny<Feed>()));
            _uow.Setup(x => x.UserRepository.GetUserAccountType(_zazzEvent.UserId))
                .Returns(AccountType.User);

            //Act
            _sut.CreateEvent(_zazzEvent);

            //Assert
            _uow.Verify(x => x.EventRepository.InsertGraph(_zazzEvent), Times.Once());
            _uow.Verify(x => x.FeedRepository.InsertGraph(It.IsAny<Feed>()), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Exactly(2));
        }

        [Test]
        public void NotCreateNotificationIfUserIsNotClubAdmin_OnCreateEvent()
        {
            //Arrange
            _uow.Setup(x => x.EventRepository.InsertGraph(_zazzEvent));
            _uow.Setup(x => x.FeedRepository.InsertGraph(It.IsAny<Feed>()));
            _uow.Setup(x => x.UserRepository.GetUserAccountType(_zazzEvent.UserId))
                .Returns(AccountType.User);

            //Act
            _sut.CreateEvent(_zazzEvent);

            //Assert
            _notificationService.Verify(x => x.CreateNewEventNotification(It.IsAny<int>(), It.IsAny<int>(), false),
                                        Times.Never());
        }

        [Test]
        public void CreateNotificationIfUserIsClubAdmin_OnCreateEvent()
        {
            //Arrange
            _uow.Setup(x => x.EventRepository.InsertGraph(_zazzEvent));
            _uow.Setup(x => x.FeedRepository.InsertGraph(It.IsAny<Feed>()));
            _uow.Setup(x => x.UserRepository.GetUserAccountType(_zazzEvent.UserId))
                .Returns(AccountType.Club);
            _notificationService.Setup(x => x.CreateNewEventNotification(_zazzEvent.UserId, _zazzEvent.Id, false));

            //Act
            _sut.CreateEvent(_zazzEvent);

            //Assert
            _notificationService.Verify(x => x.CreateNewEventNotification(_zazzEvent.UserId, _zazzEvent.Id, false),
                                        Times.Once());
        }

        [Test]
        public void ExtractTagsFromDescriptionAndAddThem_OnCreateEvent()
        {
            //Arrange
            _zazzEvent.Description = "sample description";

            var tag1 = "#tag1";
            var tag2 = "#tag2";
            var unavailableTag = "#dsadas";

            var tag1Object = new Category { Id = 1 };
            var tag2Object = new Category { Id = 2 };

            _stringHelper.Setup(x => x.ExtractTags(_zazzEvent.Description))
                         .Returns(new List<string> { tag1, tag2, unavailableTag });
            _staticDataRepo.Setup(x => x.GetTagIfExists(tag1.Replace("#", "")))
                           .Returns(tag1Object);
            _staticDataRepo.Setup(x => x.GetTagIfExists(tag2.Replace("#", "")))
                           .Returns(tag2Object);
            _staticDataRepo.Setup(x => x.GetTagIfExists(unavailableTag.Replace("#", "")))
                           .Returns(() => null);

            _uow.Setup(x => x.EventRepository.InsertGraph(_zazzEvent));
            _uow.Setup(x => x.UserRepository.GetUserAccountType(_zazzEvent.UserId))
                .Returns(AccountType.User);
            _uow.Setup(x => x.FeedRepository.InsertGraph(It.IsAny<Feed>()));

            //Act
            _sut.CreateEvent(_zazzEvent);

            //Assert
            Assert.AreEqual(2, _zazzEvent.Tags.Count);
            Assert.IsTrue(_zazzEvent.Tags.Any(t => t.CategoryId == tag1Object.Id));
            Assert.IsTrue(_zazzEvent.Tags.Any(t => t.CategoryId == tag2Object.Id));
        }

        [Test]
        public void NotAddDuplicateTags_OnCreateEvent()
        {
            //Arrange
            _zazzEvent.Description = "sample description";

            var tag1 = "#tag1";
            var tag1Duplicate = tag1;
            var tag2 = "#tag2";
            var unavailableTag = "#dsadas";

            var tag1Object = new Category { Id = 1 };
            var tag2Object = new Category { Id = 2 };

            _stringHelper.Setup(x => x.ExtractTags(_zazzEvent.Description))
                         .Returns(new List<string> { tag1, tag2, tag1Duplicate, unavailableTag });
            _staticDataRepo.Setup(x => x.GetTagIfExists(tag1.Replace("#", "")))
                           .Returns(tag1Object);
            _staticDataRepo.Setup(x => x.GetTagIfExists(tag2.Replace("#", "")))
                           .Returns(tag2Object);
            _staticDataRepo.Setup(x => x.GetTagIfExists(unavailableTag.Replace("#", "")))
                           .Returns(() => null);

            _uow.Setup(x => x.EventRepository.InsertGraph(_zazzEvent));
            _uow.Setup(x => x.UserRepository.GetUserAccountType(_zazzEvent.UserId))
                .Returns(AccountType.User);
            _uow.Setup(x => x.FeedRepository.InsertGraph(It.IsAny<Feed>()));

            //Act
            _sut.CreateEvent(_zazzEvent);

            //Assert
            Assert.AreEqual(2, _zazzEvent.Tags.Count);
            Assert.IsTrue(_zazzEvent.Tags.Any(t => t.CategoryId == tag1Object.Id));
            Assert.IsTrue(_zazzEvent.Tags.Any(t => t.CategoryId == tag2Object.Id));
        }

        [Test]
        public void NotBeCaseSensitiveWhenCheckingForDuplicates_OnCreateEvent()
        {
            //Arrange
            _zazzEvent.Description = "sample description";

            var tag1 = "#tag1";
            var tag1Duplicate = "#TAG1";
            var tag2 = "#tag2";
            var unavailableTag = "#dsadas";

            var tag1Object = new Category { Id = 1 };
            var tag2Object = new Category { Id = 2 };

            _stringHelper.Setup(x => x.ExtractTags(_zazzEvent.Description))
                         .Returns(new List<string> { tag1, tag2, tag1Duplicate, unavailableTag });
            _staticDataRepo.Setup(x => x.GetTagIfExists(tag1.Replace("#", "")))
                           .Returns(tag1Object);
            _staticDataRepo.Setup(x => x.GetTagIfExists(tag1Duplicate.Replace("#", "")))
                           .Returns(tag1Object);
            _staticDataRepo.Setup(x => x.GetTagIfExists(tag2.Replace("#", "")))
                           .Returns(tag2Object);
            _staticDataRepo.Setup(x => x.GetTagIfExists(unavailableTag.Replace("#", "")))
                           .Returns(() => null);

            _uow.Setup(x => x.EventRepository.InsertGraph(_zazzEvent));
            _uow.Setup(x => x.UserRepository.GetUserAccountType(_zazzEvent.UserId))
                .Returns(AccountType.User);
            _uow.Setup(x => x.FeedRepository.InsertGraph(It.IsAny<Feed>()));

            //Act
            _sut.CreateEvent(_zazzEvent);

            //Assert
            Assert.AreEqual(2, _zazzEvent.Tags.Count);
            Assert.IsTrue(_zazzEvent.Tags.Any(t => t.CategoryId == tag1Object.Id));
            Assert.IsTrue(_zazzEvent.Tags.Any(t => t.CategoryId == tag2Object.Id));
        }

        [Test]
        public void CallGetById_OnGetEvent()
        {
            //Arrange
            var id = 123;
            var zazzEvent = new ZazzEvent();
            _uow.Setup(x => x.EventRepository.GetById(id))
                .Returns(zazzEvent);

            //Act
            var result = _sut.GetEvent(id);

            //Assert
            Assert.AreSame(zazzEvent, result);
            _uow.Verify(x => x.EventRepository.GetById(id), Times.Once());
        }

        [Test]
        public void ThrowNotFoundIfEventDoesntExists_OnGetEvent()
        {
            //Arrange
            var id = 123;
            _uow.Setup(x => x.EventRepository.GetById(id))
                .Returns(() => null);

            //Act
            Assert.Throws<NotFoundException>(() => _sut.GetEvent(id));

            //Assert
            _uow.Verify(x => x.EventRepository.GetById(id), Times.Once());
        }

        [Test]
        public void ThrownIfEventIdIs0_OnUpdateEvent()
        {
            //Arrange
            //Act & Assert
            Assert.Throws<ArgumentException>(() => _sut.UpdateEvent(_zazzEvent, _userId));

        }

        [Test]
        public void ThrowIfEventDoesntExists_OnUpdateEvent()
        {
            //Arrange
            _zazzEvent.Id = 444;
            _uow.Setup(x => x.EventRepository.GetById(_zazzEvent.Id))
                .Returns(() => null);

            //Act & Assert
            Assert.Throws<NotFoundException>(() => _sut.UpdateEvent(_zazzEvent, _userId));
            _uow.Verify(x => x.EventRepository.GetById(_zazzEvent.Id), Times.Once());
        }

        [Test]
        public void ThrowIfCurrentUserDoesntMatchTheOwner_OnUpdateEvent()
        {
            //Arrange
            _zazzEvent.Id = 444;
            _zazzEvent.UserId = 10;
            _uow.Setup(x => x.EventRepository.GetById(_zazzEvent.Id))
                .Returns(_zazzEvent);

            //Act & Assert
            Assert.Throws<SecurityException>(() => _sut.UpdateEvent(_zazzEvent, _userId));
            _uow.Verify(x => x.EventRepository.GetById(_zazzEvent.Id), Times.Once());
        }

        [Test]
        public void SaveUpdatedEvent_OnUpdateEvent()
        {
            //Arrange
            _zazzEvent.Id = 444;
            _uow.Setup(x => x.EventRepository.GetById(_zazzEvent.Id))
                .Returns(() => _zazzEvent);

            //Act
            _sut.UpdateEvent(_zazzEvent, _userId);

            //Assert
            _uow.Verify(x => x.SaveChanges(), Times.Once());

        }

        [Test]
        public void UpdateTags_OnUpdateEvent()
        {
            //Arrange
            _zazzEvent.Id = 5;
            _zazzEvent.Description = "sample description";
            _zazzEvent.Tags = new List<EventCategory> { new EventCategory(), new EventCategory(), new EventCategory() };
            var updatedEvent = new ZazzEvent
            {
                Id = _zazzEvent.Id,
                Description = "new Description"
            };

            var tag1 = "#tag1";
            var tag2 = "#tag2";
            var unavailableTag = "#dsadas";

            var tag1Object = new Category { Id = 1 };
            var tag2Object = new Category { Id = 2 };

            _stringHelper.Setup(x => x.ExtractTags(updatedEvent.Description))
                         .Returns(new List<string> { tag1, tag2, unavailableTag });
            _staticDataRepo.Setup(x => x.GetTagIfExists(tag1.Replace("#", "")))
                           .Returns(tag1Object);
            _staticDataRepo.Setup(x => x.GetTagIfExists(tag2.Replace("#", "")))
                           .Returns(tag2Object);
            _staticDataRepo.Setup(x => x.GetTagIfExists(unavailableTag.Replace("#", "")))
                           .Returns(() => null);
            _uow.Setup(x => x.EventRepository.GetById(_zazzEvent.Id))
                .Returns(_zazzEvent);

            //Act
            _sut.UpdateEvent(updatedEvent, _zazzEvent.UserId);

            //Assert
            Assert.AreEqual(2, _zazzEvent.Tags.Count);
            Assert.IsTrue(_zazzEvent.Tags.Any(t => t.CategoryId == tag1Object.Id));
            Assert.IsTrue(_zazzEvent.Tags.Any(t => t.CategoryId == tag2Object.Id));
        }

        [Test]
        public void NotAddDuplicate_OnUpdateEvent()
        {
            //Arrange
            _zazzEvent.Id = 5;
            _zazzEvent.Description = "sample description";
            _zazzEvent.Tags = new List<EventCategory> { new EventCategory(), new EventCategory(), new EventCategory() };
            var updatedEvent = new ZazzEvent
            {
                Id = _zazzEvent.Id,
                Description = "new Description"
            };

            var tag1 = "#tag1";
            var tag1Duplicate = tag1;
            var tag2 = "#tag2";
            var unavailableTag = "#dsadas";

            var tag1Object = new Category { Id = 1 };
            var tag2Object = new Category { Id = 2 };

            _stringHelper.Setup(x => x.ExtractTags(updatedEvent.Description))
                         .Returns(new List<string> { tag1, tag2, unavailableTag, tag1Duplicate });
            _staticDataRepo.Setup(x => x.GetTagIfExists(tag1.Replace("#", "")))
                           .Returns(tag1Object);
            _staticDataRepo.Setup(x => x.GetTagIfExists(tag2.Replace("#", "")))
                           .Returns(tag2Object);
            _staticDataRepo.Setup(x => x.GetTagIfExists(unavailableTag.Replace("#", "")))
                           .Returns(() => null);
            _uow.Setup(x => x.EventRepository.GetById(_zazzEvent.Id))
                .Returns(_zazzEvent);

            //Act
            _sut.UpdateEvent(updatedEvent, _zazzEvent.UserId);

            //Assert
            Assert.AreEqual(2, _zazzEvent.Tags.Count);
            Assert.IsTrue(_zazzEvent.Tags.Any(t => t.CategoryId == tag1Object.Id));
            Assert.IsTrue(_zazzEvent.Tags.Any(t => t.CategoryId == tag2Object.Id));
        }

        [Test]
        public void IgnoreCaseForTagsDuplicateCheck_OnUpdateEvent()
        {
            //Arrange
            _zazzEvent.Id = 5;
            _zazzEvent.Description = "sample description";
            _zazzEvent.Tags = new List<EventCategory> { new EventCategory(), new EventCategory(), new EventCategory() };
            var updatedEvent = new ZazzEvent
            {
                Id = _zazzEvent.Id,
                Description = "new Description"
            };

            var tag1 = "#tag1";
            var tag1Duplicate = "#TAG1";
            var tag2 = "#tag2";
            var unavailableTag = "#dsadas";

            var tag1Object = new Category { Id = 1 };
            var tag2Object = new Category { Id = 2 };

            _stringHelper.Setup(x => x.ExtractTags(updatedEvent.Description))
                         .Returns(new List<string> { tag1, tag2, unavailableTag, tag1Duplicate });
            _staticDataRepo.Setup(x => x.GetTagIfExists(tag1.Replace("#", "")))
                           .Returns(tag1Object);
            _staticDataRepo.Setup(x => x.GetTagIfExists(tag1Duplicate.Replace("#", "")))
                           .Returns(tag1Object);
            _staticDataRepo.Setup(x => x.GetTagIfExists(tag2.Replace("#", "")))
                           .Returns(tag2Object);
            _staticDataRepo.Setup(x => x.GetTagIfExists(unavailableTag.Replace("#", "")))
                           .Returns(() => null);
            _uow.Setup(x => x.EventRepository.GetById(_zazzEvent.Id))
                .Returns(_zazzEvent);

            //Act
            _sut.UpdateEvent(updatedEvent, _zazzEvent.UserId);

            //Assert
            Assert.AreEqual(2, _zazzEvent.Tags.Count);
            Assert.IsTrue(_zazzEvent.Tags.Any(t => t.CategoryId == tag1Object.Id));
            Assert.IsTrue(_zazzEvent.Tags.Any(t => t.CategoryId == tag2Object.Id));
        }

        [Test]
        public void ShouldThrowIfEventIdIs0_OnDelete()
        {
            //Arrange
            _uow.Setup(x => x.EventRepository.GetOwnerId(_zazzEvent.Id))
                .Returns(() => 123);

            //Act
            try
            {
                _sut.DeleteEvent(0, _zazzEvent.UserId);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (ArgumentException)
            {
            }

            //Assert
            _uow.Verify(x => x.EventRepository.GetOwnerId(_zazzEvent.Id), Times.Never());
            _uow.Verify(x => x.EventRepository.Remove(_zazzEvent.Id), Times.Never());
            _uow.Verify(x => x.SaveChanges(), Times.Never());
        }

        [Test]
        public void ThrowIfUserIdDoesntMatchTheOwnerId_OnDelete()
        {
            //Arrange
            _zazzEvent.Id = 444;
            _uow.Setup(x => x.EventRepository.GetOwnerId(_zazzEvent.Id))
                .Returns(() => 123);

            //Act

            try
            {
                _sut.DeleteEvent(_zazzEvent.Id, _zazzEvent.UserId);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (SecurityException)
            {
            }

            //Assert
            _uow.Verify(x => x.EventRepository.GetOwnerId(_zazzEvent.Id), Times.Once());
            _uow.Verify(x => x.EventRepository.Remove(_zazzEvent.Id), Times.Never());
            _uow.Verify(x => x.SaveChanges(), Times.Never());
        }

        [Test]
        public void Delete_OnDelete()
        {
            //Arrange
            _zazzEvent.Id = 444;
            _uow.Setup(x => x.EventRepository.GetOwnerId(_zazzEvent.Id))
                .Returns(() => _zazzEvent.UserId);
            _uow.Setup(x => x.EventRepository.Remove(_zazzEvent.Id));

            //Act
            _sut.DeleteEvent(_zazzEvent.Id, _userId);

            //Assert
            _uow.Verify(x => x.EventRepository.Remove(_zazzEvent.Id), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }
    }
}