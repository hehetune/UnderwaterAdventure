using _Scripts.PlayerScripts;
using UnityEngine;

namespace _Scripts.Game
{
    public class App : MonoBehaviour
    {
        private static App _instance;

        public static App Instance => _instance;
        
        public GameInput GameInput;

        public Player Player;
        
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
                if( m_bPaused )
                    Time.timeScale = 0.0f; 
                else Time.timeScale = 1.0f;
            } 
        }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this.gameObject);
                return;
            }

            _instance = this;

            Player = FindObjectOfType<Player>();
        }

        

    }
}