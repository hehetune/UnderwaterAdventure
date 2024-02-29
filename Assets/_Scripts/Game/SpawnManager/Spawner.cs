using _Scripts.Game.Logic;
using UnityEngine;

namespace _Scripts.Game.SpawnManager
{
    public class Spawner : MonoBehaviour
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
        public GameObject m_spawnObject = null;
        public string m_loadSpawnObject;
        public int m_minSpawn = 1;
        public int m_maxSpawn = 1;
        public float m_randomSpawnRadius = 4.0f;
        public float m_extraBoundsSizeX = 0.0f;
        public float m_extraBoundsSizeY = 0.0f;

        // accessible vars 
        protected State m_state = State.invalid;
        private FastBounds2D m_bounds;
        private int					m_checkID = -1;			// id to ensure this spawner only gets checked/updated once per frame in case it spans multiple cells
        public int					checkID					{get{return m_checkID;} set{m_checkID=value;}}
        public State state { get { return m_state; } set { m_state = value; } }

        public FastBounds2D bounds
        {
            get { return m_bounds; }
        } // maybe more efficient to just make this public

        private bool m_insertIntoMultipleGridCells = false;

        public bool insertIntoMultipleGridCells
        {
            get { return m_insertIntoMultipleGridCells; }
        }
        
        // private
        private float				m_waitTime = 0.0f;
        private bool				m_keepAlive = false;	// if we don't get a keepAlive tick, we're out of active range so need to deactivate ourself
        private int					m_numChildren = 0;
        private int					m_numChildrenAlive = 0;

        protected virtual void Awake()
        {
            enabled = false;

            m_state = State.waiting;

            // make a Bounds for this spawner, for activating and culling purposes
            Vector3 spawnBoundsSize = new Vector3(m_randomSpawnRadius * 2.0f + m_extraBoundsSizeX,
                m_randomSpawnRadius * 2.0f + m_extraBoundsSizeY, 1.0f);

            // Make shoals have an even bigger spawn bounds as they spawn in a coroutine and therefore need more time to fully spawn
            if (m_maxSpawn > 1)
            {
                spawnBoundsSize *= 2f;
            }

            m_bounds = new FastBounds2D(transform.position, spawnBoundsSize);

            GameManager.SpawnerManager.AddSpawner(this);

            // Check for loading prefabs here.
            // Most spawned things are referenced directly in m_spawnObject, but they can now be loaded as resources, by name.
            if ((m_spawnObject == null) && (m_state != State.dead))
            {
                m_spawnObject = Resources.Load<GameObject>(m_loadSpawnObject);
            }

            if (!m_spawnObject)
            {
                Debug.Log("Spawner has no spawnObject!", gameObject);
            }
        }
        
        // see if we're allowed to spawn with the current shark state
        public virtual bool CanSpawn()
        {
            if (m_state == State.waiting && m_waitTime > GameLogic.GameTime)
            {
                return false;
            }

            // was checking GetHighestUnlockedShark(), now reverted back to currentShark.
            // update: now use currentGameplaySharkType for proper behaviour with novelty sharks
            // if((App.sharkStats.currentGameplaySharkType < m_minSharkType) && !App.spawnerManager.m_debugIgnoreMinSharkLimit)
            //     return false;
            
            // maybe base on player's level, not shark type
			
            // if((GameLogic.sessionStandardCurrency < m_minCredits) && !App.spawnerManager.m_debugIgnoreMinCreditsLimit)
            //     return false;

            // CollectableItemToSpawn collectableItemToSpawn = GetComponent<CollectableItemToSpawn>();
            // if (collectableItemToSpawn != null)
            // {
            //     if (!King._Instance._CollectablesManager.IsCollectableItemReadyToBeSpawned(collectableItemToSpawn.collectableName, collectableItemToSpawn.collectableItemSku))
            //     {
            //         return false;
            //     }
            // }

            return true;
        }
        
        public void KeepAliveTick()
        {
            m_keepAlive = true;
        }
        
        public virtual void OnActivated()
        {
            switch(m_state)
            {
                case State.waiting:
                case State.sleeping:
                {
				
			
                    if(m_state==State.waiting)
                    {
                        m_numChildrenAlive = m_numChildren;
                    }
			
                    // todo: check respawn time.  Make sure respawns can't now appear on screen (maybe add another state, or can it be done with Waiting - check old code)
                    m_keepAlive = true;
                    enabled = true;

                    if (gameObject.activeSelf)
                    {
                        StartCoroutine(Spawn());
                    }
                    m_state = State.active;
                }
                    break;
            }
        }
	
        public void OnDeactivated()
        {
            switch(m_state)
            {
                case State.active:
                {
                    m_state = State.sleeping;
                }
                    break;
            }
        }
    }
}