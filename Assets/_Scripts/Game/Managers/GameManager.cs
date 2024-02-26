using _Scripts.PlayerScripts;
using UnityEngine;

namespace _Scripts.Game
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;

        public static GameManager Instance => _instance;
        
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