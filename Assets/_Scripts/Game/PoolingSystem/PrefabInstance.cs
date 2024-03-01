using UnityEngine;

namespace _Scripts.Game.PoolingSystem
{
    public class PrefabInstance : MonoBehaviour
    {
        private GameObject		m_prefab = null;
        public GameObject		prefab		{get{return m_prefab;}}
	
        public void Init(GameObject initPrefab)
        {
            m_prefab = initPrefab;
        }
    }
}