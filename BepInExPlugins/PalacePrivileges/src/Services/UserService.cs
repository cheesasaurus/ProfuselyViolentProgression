using ProfuselyViolentProgression.Core.Utilities;
using ProfuselyViolentProgression.PalacePrivileges.Models;
using ProjectM;
using ProjectM.Network;
using Unity.Entities;

namespace ProfuselyViolentProgression.PalacePrivileges.Services;

public class UserService
{
    private EntityManager _entityManager = WorldUtil.Server.EntityManager;

    public bool TryGetUserModel_ForCharacter(Entity characterEntity, out UserModel userModel)
    {
        userModel = default;
        if (!_entityManager.TryGetComponentData<PlayerCharacter>(characterEntity, out var playerCharacter))
        {
            return false;
        }
        return TryGetUserModel(playerCharacter.UserEntity, out userModel);
    }

    public bool TryGetUserModel_ForOwnedEntity(Entity ownedEntity, out UserModel userModel)
    {
        userModel = default;
        if (!_entityManager.TryGetComponentData<UserOwner>(ownedEntity, out var userOwner))
        {
            return false;
        }
        return TryGetUserModel(userOwner.Owner._Entity, out userModel);
    }

    public bool TryGetUserModel(Entity userEntity, out UserModel userModel)
    {
        userModel = default;
        if (!_entityManager.TryGetComponentData<User>(userEntity, out var user))
        {
            return false;
        }
        if (!_entityManager.TryGetComponentData<Team>(userEntity, out var team))
        {
            return false;
        }
        userModel.CharacterName = user.CharacterName;
        userModel.PlatformId = user.PlatformId;
        userModel.Team = team;
        userModel.User = user;
        return true;
    }
    
}
