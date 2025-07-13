using ProfuselyViolentProgression.PalacePrivileges.Models;
using ProjectM;

namespace ProfuselyViolentProgression.PalacePrivileges.Services;

// knows about doors. maps them to privileges
public class DoorService
{
    // todo

    public DoorPrivs PrivsWithPermission(Door door)
    {
        var privs = door.CanBeOpenedByServant ? DoorPrivs.NotServantLocked : DoorPrivs.ServantLocked;

        // todo: check door model

        return privs;
    }
}
