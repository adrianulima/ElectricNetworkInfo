using VRageMath;
using System.Collections.Generic;

namespace Lima
{
  public class EntityItemPooler
  {
    private int _maxSize;

    private readonly Queue<EntityItem> _pool;

    public EntityItemPooler(int maxSize)
    {
      _maxSize = maxSize;
      _pool = new Queue<EntityItem>(_maxSize);
    }

    public void PutEntityItem(EntityItem entt)
    {
      _pool.Enqueue(entt);
    }

    public EntityItem GetEntityItem(string title, Color textColor)
    {
      EntityItem entt;
      if (_pool.Count > 0)
      {
        entt = (EntityItem)_pool.Dequeue();
        entt.Title = title.TrimStart();
      }
      else
        entt = new EntityItem(title.TrimStart(), textColor);

      if (title.StartsWith(" "))
        entt.UpdateColors(Color.Goldenrod);
      else
        entt.UpdateColors(textColor);

      return entt;
    }
  }
}