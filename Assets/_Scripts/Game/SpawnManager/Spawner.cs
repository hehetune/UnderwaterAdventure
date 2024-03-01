using System.Collections;
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
        private int m_numSpawned = 0; // how many did we spawn, if/when we spawned
        protected int m_numActive = 0; // how many are currently active - have not been killed or culled
        private int m_numSleeping = 0; // how many have been culled but are not dead - they want to be restored

        private int
            m_checkID = -1; // id to ensure this spawner only gets checked/updated once per frame in case it spans multiple cells

        public int checkID
        {
            get { return m_checkID; }
            set { m_checkID = value; }
        }

        public State state
        {
            get { return m_state; }
            set { m_state = value; }
        }

        public int numSpawned
        {
            get { return m_numSpawned; }
        }

        public int numActive
        {
            get { return m_numActive; }
        }

        public int numSleeping
        {
            get { return m_numSleeping; }
        }

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
        private float m_waitTime = 0.0f;

        private bool
            m_keepAlive =
                false; // if we don't get a keepAlive tick, we're out of active range so need to deactivate ourself

        private int m_numChildren = 0;
        private int m_numChildrenAlive = 0;

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
            switch (m_state)
            {
                case State.waiting:
                case State.sleeping:
                {
                    if (m_state == State.waiting)
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
            switch (m_state)
            {
                case State.active:
                {
                    m_state = State.sleeping;
                }
                    break;
            }
        }

        protected IEnumerator Spawn()
        {
            // how many things do we spawn?  If we are re/spawning for the first time, do the whole lot.  If we are restoring
            // a sleeping spawner, only restore m_numSleeping.
            int numSpawn = 0;
            if (state == State.sleeping)
            {
                numSpawn = m_numSleeping;
            }
            else if (state == State.waiting)
            {
                numSpawn = Util.RandRange(m_minSpawn, m_maxSpawn);
            }

            numSpawn = Mathf.Min(m_maxSpawn, numSpawn);

            m_state = State.active;

            // see if we have any children to attach (e.g. if this is a fishing boat, these are the blokes to stick on it)
            AttachedChildSpawner[] attachedChildren =
                (m_numChildren > 0) ? GetComponents<AttachedChildSpawner>() : null;
            float scale = (m_scaleMin == m_scaleMax) ? m_scaleMin : Random.Range(m_scaleMin, m_scaleMax);

            // spawn them
            for (int i = 0; i < numSpawn && state == State.active; i++)
            {
                Vector3 spawnPos = (Random.insideUnitCircle * m_randomSpawnRadius);
                spawnPos += transform.position;
            }

            GameObject obj = null;

            Quaternion spawnRotation = (m_inheritTransform ? transform.rotation : Quaternion.identity);

            if (m_variants.Count == 0)
            {
                obj = SpawnerManager.Spawn(m_spawnObject, this, spawnPos, spawnRotation, scale,
                    m_variant); // create the object, stick a SpawnedObject component on it
            }
            else
            {
                int index = Random.Range(0, m_variants.Count);
                obj = SpawnerManager.Spawn(m_variants[index], this, spawnPos, spawnRotation, scale,
                    m_variant); // create the object, stick a SpawnedObject component on it
            }

            if (m_inheritTransform)
            {
                obj.transform.localScale = m_transform.localScale;
            }

            if (m_attachSpawnedObjectToParent)
            {
                obj.transform.parent = transform.parent.transform;
            }

            InitSpawnedObject(obj);

            // attach children if necessary
            if ((attachedChildren != null) && (attachedChildren.Length > 0))
            {
                foreach (AttachedChildSpawner attachedChild in attachedChildren)
                {
                    if (attachedChild.state == AttachedChildSpawner.State.dead) // don't restore dead children
                        continue;

                    Transform attachPoint =
                        obj.transform.Find(attachedChild
                            .m_parentAttachPointName); // find the point that this child will be attached to
                    FGAssert.Assert(attachPoint != null);

                    // spawn the child and attach to us
                    GameObject childObj = SpawnerManager.Spawn(attachedChild.m_childObject, null, attachPoint.position,
                        attachPoint.rotation, scale, m_variant);
                    FGAssert.Assert(childObj != null);
                    childObj.transform.parent = obj.transform;

                    // add a SpawnedObject component and notify AttachedChild thing so it will restore the child object's state if necessary
                    SpawnedObject sp = childObj.AddComponent<SpawnedObject>();
                    FGAssert.Assert(sp != null);
                    sp.Init(null, attachedChild);
                    attachedChild.OnSpawned(sp);

                    if (m_spawnerOnBreakableWall != null)
                    {
                        m_spawnerOnBreakableWall.AddSpawnedObject(obj);
                    }
                }

                if (i == 0 && obj != null)
                {
                    Edible edible = obj.GetComponent<Edible>();
                    if (edible != null)
                    {
                        switch (edible.TypeID)
                        {
                            /*TODO:						case TypeAttributes.TypeID.???:
                                                     FGOLAnalytics.ReportBossSpawned(FGOLAnalytics.Boss.BadSanta);
                                                     FGOLAnalytics.ReportBossSpawned(FGOLAnalytics.Boss.BigDaddy);
                                                     break;
                            */
                            case TypeAttributes.TypeID.crabGiant:
                                FGOLAnalytics.ReportBossSpawned(FGOLAnalytics.Boss.GiantCrab);
                                break;

                            case TypeAttributes.TypeID.helicopter:
                                FGOLAnalytics.ReportBossSpawned(FGOLAnalytics.Boss.Helicopter);
                                break;

                            case TypeAttributes.TypeID.enemySharkMegalodon:
                                FGOLAnalytics.ReportBossSpawned(FGOLAnalytics.Boss.Megalodon);
                                break;
                        }
                    }
                }
            }

            yield return 0;
        }
    }
}