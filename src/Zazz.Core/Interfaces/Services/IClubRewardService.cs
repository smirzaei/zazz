using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Core.Interfaces.Services
{
    public interface IClubRewardService
    {
        void AddRewardScenario(ClubPointRewardScenario scenario);

        void ChangeRewardAmount(ClubPointRewardScenario updatedScenario, int currentUserId);

        void RemoveRewardScenario(int scenarioId, int currentUserId);


        void AddClubReward(ClubReward reward);

        void UpdateClubReward(int rewardId, int currentUserId, ClubReward newReward);

        void DisableClubReward(int rewardId, int currentUserId);

        void EnableClubReward(int rewardId, int currentUserId);


        void AwardUserPoints(int userId, int clubId, int amount, PointRewardScenario scenario);

        UserReward RedeemPoints(int userId, int rewardId);

        void RemoveUserReward(int rewardId, int currentUserId);
    }
}