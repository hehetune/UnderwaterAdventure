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