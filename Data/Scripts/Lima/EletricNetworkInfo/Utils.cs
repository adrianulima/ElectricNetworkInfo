using VRage.Game.ModAPI;
using VRage.Game;
using Sandbox.Game.Entities;

namespace Lima
{
  internal static class Utils
  {
    public static bool IsOwnerOrFactionShare(IMyCubeBlock block, IMyPlayer player)
    {
      var relation = (block.OwnerId > 0 ? player.GetRelationTo(block.OwnerId) : MyRelationsBetweenPlayerAndBlock.NoOwnership);
      if (relation == MyRelationsBetweenPlayerAndBlock.Owner || relation == MyRelationsBetweenPlayerAndBlock.NoOwnership)
        return true;

      var shareMode = Utils.GetBlockShareMode(block);
      if (shareMode == MyOwnershipShareModeEnum.All || (relation == MyRelationsBetweenPlayerAndBlock.FactionShare && shareMode == MyOwnershipShareModeEnum.Faction))
        return true;

      return false;
    }

    public static MyOwnershipShareModeEnum GetBlockShareMode(IMyCubeBlock block)
    {
      var internalBlock = block as MyCubeBlock;
      if (internalBlock != null && internalBlock.IDModule != null)
        return internalBlock.IDModule.ShareMode;
      return MyOwnershipShareModeEnum.None;
    }
  }
}