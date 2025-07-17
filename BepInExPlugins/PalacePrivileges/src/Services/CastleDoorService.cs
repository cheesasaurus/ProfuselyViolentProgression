using System.Collections.Generic;
using ProfuselyViolentProgression.Core.Utilities;
using ProfuselyViolentProgression.PalacePrivileges.Models;
using ProjectM;
using Stunlock.Core;
using Unity.Entities;

namespace ProfuselyViolentProgression.PalacePrivileges.Services;

// knows about doors. decides what privileges are acceptable for opening a door
public class CastleDoorService
{

    private EntityManager _entityManager = WorldUtil.Server.EntityManager;
    private CastleService _castleService;
    private Dictionary<PrefabGUID, DoorPrivs> _privsByPrefabGuid = [];

    public CastleDoorService(CastleService castleService)
    {
        _castleService = castleService;
        InitPrivsByPrefabGUID();
    }

    public bool TryGetDoorModel(Entity doorEntity, out CastleDoorModel doorModel)
    {
        doorModel = default;

        if (!_entityManager.TryGetComponentData<PrefabGUID>(doorEntity, out var prefabGUID))
        {
            LogUtil.LogWarning("door: no PrefabGUID");
            return false;
        }

        if (!_entityManager.TryGetComponentData<Door>(doorEntity, out var door))
        {
            LogUtil.LogWarning("door: no Door");
            return false;
        }

        if (!_entityManager.TryGetComponentData<Team>(doorEntity, out var team))
        {
            LogUtil.LogWarning("door: no Team");
            return false;
        }

        if (!_castleService.TryGetCastleModel_ForConnectedEntity(doorEntity, out var castleModel))
        {
            LogUtil.LogWarning("door: could not build CastleModel");
            return false;
        }

        doorModel = new CastleDoorModel
        {
            PrefabGUID = prefabGUID,
            Castle = castleModel,
            Team = team,
            PermissiblePrivsToOpen = new CastlePrivileges
            {
                Door = AssociatedPrivileges(door, prefabGUID),
            },
        };
        return true;
    }

    public DoorPrivs AssociatedPrivileges(Door door, PrefabGUID prefabGUID)
    {
        LogUtil.LogDebug(DebugUtil.LookupPrefabName(prefabGUID));

        var privs = door.CanBeOpenedByServant ? DoorPrivs.NotServantLocked : DoorPrivs.ServantLocked;

        if (_privsByPrefabGuid.TryGetValue(prefabGUID, out var doorPrivs))
        {
            privs |= doorPrivs;
        }

        // todo: check color of oakveil doors

        return privs;
    }

    private void InitPrivsByPrefabGUID()
    {
        _privsByPrefabGuid = new()
        {
            ///////////////////////////////////////////////////////////////////////
            // wide castle doors //////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////

            // portcullis with bars
            {
                PrefabGuids.TM_Castle_Wall_Door_Metal_Wide_Tier02_Standard,
                DoorPrivs.WideBars | DoorPrivs.NotServantLocked
            },
            {
                PrefabGuids.TM_Castle_Wall_Door_Metal_Wide_Tier02_ServantLock,
                DoorPrivs.WideBars | DoorPrivs.ServantLocked
            },

            // portcullis with planks
            {
                PrefabGuids.TM_Castle_Wall_Door_Wood_Wide_Tier02_Standard,
                DoorPrivs.WidePlanks | DoorPrivs.NotServantLocked
            },
            {
                PrefabGuids.TM_Castle_Wall_Door_Wood_Wide_Tier02_ServantLock,
                DoorPrivs.WidePlanks | DoorPrivs.ServantLocked
            },

            /////////////////////////////////////////////////////////////////////////
            // thin fences //////////////////////////////////////////////////////////
            /////////////////////////////////////////////////////////////////////////
            
            // "Nocturne Garden Gate"
            {
                PrefabGuids.TM_Castle_Fence_IronGate01,
                DoorPrivs.ThinFence | DoorPrivs.NotServantLocked
            },

            // "Noble Garden Gate"
            {
                PrefabGuids.TM_Castle_Fence_IronGate02,
                DoorPrivs.ThinFence | DoorPrivs.NotServantLocked
            },

            // "Verdant Garden Gate"
            {
                PrefabGuids.TM_Castle_Fence_IronGate03,
                DoorPrivs.ThinFence | DoorPrivs.NotServantLocked
            },

            // "Rural Garden Gate" - light color
            {
                PrefabGuids.TM_Castle_Fence_WoodGate01,
                DoorPrivs.ThinFence | DoorPrivs.NotServantLocked
            },

            // "Rural Garden Gate" - dark color
            {
                PrefabGuids.TM_Castle_Fence_WoodGate02,
                DoorPrivs.ThinFence | DoorPrivs.NotServantLocked
            },

            // "Stables Gate"
            {
                PrefabGuids.TM_Castle_Fence_StableGate01,
                DoorPrivs.ThinFence | DoorPrivs.NotServantLocked
            },

            // "Ancient Symphony Garden gate"
            {
                PrefabGuids.TM_Castle_Fence_ProjectK01_Gate,
                DoorPrivs.ThinFence | DoorPrivs.NotServantLocked
            },

            //////////////////////////////////////////////////////////////////////////
            // thin castle doors /////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////////////////////
             
            // "Palisade Gate"
            {
                PrefabGuids.TM_Castle_Wall_Door_Palisade_Tier01,
                DoorPrivs.ThinPalisade | DoorPrivs.NotServantLocked
            },

            // "Castle Gate"
            {
                PrefabGuids.TM_Castle_Wall_Door_Wood_Tier02_Standard,
                DoorPrivs.ThinBasic | DoorPrivs.NotServantLocked
            },
            {
                PrefabGuids.TM_Castle_Wall_Door_Wood_Tier02_ServantLock,
                DoorPrivs.ThinBasic | DoorPrivs.ServantLocked
            },

            // "Battlement Gate"
            {
                PrefabGuids.TM_Castle_Wall_Door_Wood_Tier02_Standard01_Standard,
                DoorPrivs.ThinBattlement | DoorPrivs.NotServantLocked
            },
            {
                PrefabGuids.TM_Castle_Wall_Door_Wood_Tier02_Standard01_ServantLock,
                DoorPrivs.ThinBattlement | DoorPrivs.ServantLocked
            },

            // "Cordial Gate"
            {
                PrefabGuids.TM_Castle_Wall_Door_Wood_Tier02_Standard02,
                DoorPrivs.ThinCordial | DoorPrivs.NotServantLocked
            },
            {
                PrefabGuids.TM_Castle_Wall_Door_Wood_Tier02_Standard02_ServantLock,
                DoorPrivs.ThinCordial | DoorPrivs.ServantLocked
            },

            // "Verdant Gate"
            {
                PrefabGuids.TM_Castle_Wall_Door_Wood_Tier02_Standard03_Standard,
                DoorPrivs.ThinVerdant | DoorPrivs.NotServantLocked
            },
            {
                PrefabGuids.TM_Castle_Wall_Door_Wood_Tier02_Standard03_ServantLock,
                DoorPrivs.ThinVerdant | DoorPrivs.ServantLocked
            },

            // "Royal Gate"
            {
                PrefabGuids.TM_Castle_Wall_Door_Wood_Tier02_DLC01Variant_Standard,
                DoorPrivs.ThinRoyal | DoorPrivs.NotServantLocked
            },
            {
                PrefabGuids.TM_Castle_Wall_Door_Wood_Tier02_DLC01Variant_ServantLock,
                DoorPrivs.ThinRoyal | DoorPrivs.ServantLocked
            },

            // "Plague Sanctum Gate"
            {
                PrefabGuids.TM_Castle_Wall_Door_Wood_Tier02_DLC02Variant_Standard,
                DoorPrivs.ThinPlagueSanctum | DoorPrivs.NotServantLocked
            },
            {
                PrefabGuids.TM_Castle_Wall_Door_Wood_Tier02_DLC02Variant_ServantLock,
                DoorPrivs.ThinPlagueSanctum | DoorPrivs.ServantLocked
            },

            // "Ancient Symphony Gate" first one, darker blue with angels
            {
                PrefabGuids.TM_Castle_Wall_Door_Wood_Tier02_ProjectK01Variant_Standard,
                DoorPrivs.ThinAncientSymphony1 | DoorPrivs.NotServantLocked
            },
            {
                PrefabGuids.TM_Castle_Wall_Door_Wood_Tier02_ProjectK01Variant_ServantLock,
                DoorPrivs.ThinAncientSymphony1 | DoorPrivs.ServantLocked
            },

            // "Ancient Symphony Gate" second one, blue with gold trim
            {
                PrefabGuids.TM_Castle_Wall_Door_Wood_Tier02_ProjectK02Variant_Standard,
                DoorPrivs.ThinAncientSymphony2 | DoorPrivs.NotServantLocked
            },
            {
                PrefabGuids.TM_Castle_Wall_Door_Wood_Tier02_ProjectK02Variant_ServantLock,
                DoorPrivs.ThinAncientSymphony2 | DoorPrivs.ServantLocked
            },

            // "Prison Gate"
            {
                PrefabGuids.TM_Castle_Wall_Door_Tier02_PrisonStyle01_Standard,
                DoorPrivs.ThinPrison | DoorPrivs.NotServantLocked
            },
            {
                PrefabGuids.TM_Castle_Wall_Door_Tier02_PrisonStyle01_ServantLock,
                DoorPrivs.ThinPrison | DoorPrivs.ServantLocked
            },

            // "Barrier Gate"
            {
                PrefabGuids.TM_Castle_Wall_Door_Tier02_PrisonStyle02_Standard,
                DoorPrivs.ThinBarrier | DoorPrivs.NotServantLocked
            },
            {
                PrefabGuids.TM_Castle_Wall_Door_Tier02_PrisonStyle02_ServantLock,
                DoorPrivs.ThinBarrier | DoorPrivs.ServantLocked
            },

            // oakveil dlc stuff
            {
                PrefabGuids.TM_Castle_Wall_Door_Wood_Tier02_StrongbladeDLC01Variant_Standard,
                DoorPrivs.ThinNocturnalOpulence | DoorPrivs.NotServantLocked
            },
            {
                PrefabGuids.TM_Castle_Wall_Door_Wood_Tier02_StrongbladeDLC01Variant_ServantLock,
                DoorPrivs.ThinNocturnalOpulence | DoorPrivs.ServantLocked
            },

        };
    }


}
