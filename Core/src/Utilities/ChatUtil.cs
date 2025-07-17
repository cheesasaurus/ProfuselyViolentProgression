
using ProjectM.Network;

namespace ProfuselyViolentProgression.Core.Utilities;

public static class ChatUtil
{
    public static void SendSystemMessageToClient(User user, string message)
    {
        var entityManager = WorldUtil.Server.EntityManager;
        var messageString512Bytes = new Unity.Collections.FixedString512Bytes(message.ToString());
        ProjectM.ServerChatUtils.SendSystemMessageToClient(entityManager, user, ref messageString512Bytes);
    }

    public static void SendSystemMessageToAllClients(string message)
    {
        var entityManager = WorldUtil.Server.EntityManager;
        var messageString512Bytes = new Unity.Collections.FixedString512Bytes(message.ToString());
        ProjectM.ServerChatUtils.SendSystemMessageToAllClients(entityManager, ref messageString512Bytes);
    }
    
}
