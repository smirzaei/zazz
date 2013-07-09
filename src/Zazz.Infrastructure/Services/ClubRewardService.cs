﻿using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Infrastructure.Services
{
    public class ClubRewardService : IClubRewardService
    {
        private readonly IUoW _uow;

        public ClubRewardService(IUoW uow)
        {
            _uow = uow;
        }

        public void AddRewardScenario(ClubPointRewardScenario scenario)
        {
            throw new System.NotImplementedException();
        }

        public void ChangeRewardAmount(int scenarioId, int currentUserId, int amount)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveRewardScenario(int scenarioId, int currentUserId)
        {
            throw new System.NotImplementedException();
        }

        public void AddClubReward(ClubReward reward)
        {
            throw new System.NotImplementedException();
        }

        public void UpdateClubReward(int rewardId, int currentUserId, ClubReward newReward)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveClubReward(int rewardId, int currentUserId)
        {
            throw new System.NotImplementedException();
        }

        public UserPoint RewardUserPoints(int clubId, int userId, int amount)
        {
            throw new System.NotImplementedException();
        }

        public UserReward RedeemPoints(int userId, ClubReward reward)
        {
            throw new System.NotImplementedException();
        }
    }
}