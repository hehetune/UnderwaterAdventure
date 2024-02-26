using UnityEngine;

namespace _Scripts.Game.Spawner
{
    public class SpeciesSpawner : MonoBehaviour
    {
        public enum State
        {
            invalid,
            active,
            waiting,
            sleeping,
            dead,
        }
        
        // editor vars
        public GameObject	        m_spawnObject = null;
        public string				m_loadSpawnObject;
        public int					m_minSpawn = 1;
        public int					m_maxSpawn = 1;
        public float				m_randomSpawnRadius = 4.0f;
        public float				m_extraBoundsSizeX = 0.0f;
        public float				m_extraBoundsSizeY = 0.0f;
        
        // accessible vars 
        protected State				m_state = State.invalid;
        private Bounds2D		m_bounds;

        protected virtual void Awake()
        {
            enabled = false;
            
            m_state = State.waiting;
            
            // make a Bounds for this spawner, for activating and culling purposes
            Vector3 spawnBoundsSize = new Vector3(m_randomSpawnRadius*2.0f + m_extraBoundsSizeX, m_randomSpawnRadius*2.0f + m_extraBoundsSizeY, 1.0f);

            // Make shoals have an even bigger spawn bounds as they spawn in a coroutine and therefore need more time to fully spawn
            if(m_maxSpawn > 1 )
            {
                spawnBoundsSize *= 2f;
            }
            
            m_bounds = new Bounds2D(transform.position, spawnBoundsSize);
            
            // App.spawnerManager.AddSpawner(this);
            
            // Check for loading prefabs here.
            // Most spawned things are referenced directly in m_spawnObject, but they can now be loaded as resources, by name.
            if((m_spawnObject == null) && (m_state != State.dead))
            {
                m_spawnObject = Resources.Load<GameObject>(m_loadSpawnObject);
            }
            
            if(!m_spawnObject)
            {
                Debug.Log("Spawner has no spawnObject!", gameObject);
            }
        }
    }
}