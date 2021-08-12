using System;

namespace MSD.Modules.ObjectPooling
{
    public interface IPoolable
    {
        void OnAfterSpawn();
        void OnBeforeDespawn(Action onBeforeDespawnComplete);
    }
}
