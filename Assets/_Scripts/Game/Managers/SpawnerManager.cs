using UnityEngine;

namespace _Scripts.Game.Managers
{
    public class SpawnerManager : MonoBehaviour
    {
        private void SpawnPlayer()
        {
            GameObject obj = SpawnerManager.Spawn(stats.sharkPrefab, null, transform.position, transform.rotation, 1f);
            Player player = obj.GetComponent<Player>();
            if (player != null)
            {
                player.InitializeAttachPoints();
            }

            obj.transform.localScale *= stats.sharkScale;
        }
    }
}