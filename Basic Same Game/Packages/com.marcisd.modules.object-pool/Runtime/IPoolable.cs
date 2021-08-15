using System;

namespace MSD.Modules.ObjectPool
{
    public interface IPoolable
    {
        void OnAfterSpawn();
        void OnBeforeDespawn(Action onBeforeDespawnComplete);
    }
}
