using _Scripts.Game.Camera;

namespace _Scripts.Game.Logic
{
    public class GameLogic
    {
        private static GameCamera		ms_gameCamera = null;
        public static GameCamera		gameCamera				{get{return ms_gameCamera;}}
        
        public static float             GameTime                { get; set; }
    }
}