using _Scripts.Game.Managers;
using UnityEngine;

namespace _Scripts.Game
{
    public class GameManager : MonoBehaviour
    {
        // Singleton
        private static GameManager m_instance = null;
        public static GameManager Instance => m_instance;
        
        // Settings
        private static bool m_bPaused = false;
        public static bool paused
        { 
            get 
            { 
                return m_bPaused; 
            } 
            private set
            {
                // The game goes nuts if you pause while mega breaching!
                // if(value && App.gameLogic != null)
                // {
                //     GameLogic.StopMegaBreach();
                // }

                m_bPaused = value;
                Time.timeScale = m_bPaused ? 0.0f : 1.0f;
            } 
        }
        
        public GameObject				m_spawnerManagerPrefab = null;
        
        private static SpawnerManager	m_spawnerManager = null;

        public static SpawnerManager SpawnerManager => m_spawnerManager;

    }
}