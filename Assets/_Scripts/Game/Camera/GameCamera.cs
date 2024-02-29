using UnityEngine;

namespace _Scripts.Game.Camera
{
    public class GameCamera : MonoBehaviour
    {
        private bool m_firstTime = true;
        private bool m_requestSnap = false;

        public bool hasInitialized
        {
            get { return !m_firstTime; }
        }
        
        private FastBounds2D 	m_screenWorldBounds = null;
        private FastBounds2D 	m_keepAliveWorldBounds = null;

        private FastBounds2D    m_screenBackgroundBounds = null;
        private FastBounds2D    m_keepAliveBackgroundBounds = null;
        public FastBounds2D		ScreenWorldBounds   		{get{return m_screenWorldBounds;}}
        public FastBounds2D		KeepAliveWorldBounds		{get{return m_keepAliveWorldBounds;}}
        public FastBounds2D     ScreenBackgroundBounds      {get{return m_screenBackgroundBounds;}}
        public FastBounds2D     KeepAliveBackgroundBounds   {get{return m_keepAliveBackgroundBounds;}}
    }
}