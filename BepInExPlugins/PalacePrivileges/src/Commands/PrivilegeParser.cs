using System;
using System.Collections.Generic;
using ProfuselyViolentProgression.PalacePrivileges.Models;

namespace ProfuselyViolentProgression.LoadoutLockdown.Commands;

public class PrivilegeParser
{
    public static PrivilegeParser Instance;

    protected Dictionary<string, CastlePrivileges> PrivsByName = new();

    public CastlePrivileges PrivilegesFromCommandString(string str)
    {
        // todo: implement
        throw new NotImplementedException();
    }

    public IEnumerable<IEnumerable<string>> NamesGrouped(CastlePrivileges str)
    {
        // todo: implement
        throw new NotImplementedException();
    }

}
