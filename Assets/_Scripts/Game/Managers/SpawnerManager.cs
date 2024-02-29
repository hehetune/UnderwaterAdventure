using System.Collections.Generic;
using _Scripts.Game.Logic;
using _Scripts.Game.SpawnManager;
using _Scripts.PlayerScripts;
using UnityEngine;

namespace _Scripts.Game.Managers
{
    public class SpawnerManager : MonoBehaviour
    {
        private Bounds
            m_worldBounds =
                new Bounds(Vector3.zero, Vector3.zero); // this is the extents of the world containing all spawners

        private List<Spawner>
            m_allSpawners =
                new List<Spawner>(); // a list of all spawners in the world, in whatever order Unity creates them

        public List<Spawner> allSpawners
        {
            get { return m_allSpawners; }
        }

        private const float m_cellSizeX = 15f;
        private const float m_cellSizeY = m_cellSizeX;

        private const float BackgroundGridZThreshold = 20f;

        private int m_gridSizeX;
        private int m_gridSizeY;
        private Spawner[,][] m_grid = null;
        private Spawner[,][] m_backgroundGrid = null;

        private int
            m_checkFrameID =
                0; // this ID is used to make sure a spawner doesn't get checked twice, if it spans multiple grid cells

        void Start()
        {
            CreateSpawnerGrid();
        }

        public void CreateSpawnerGrid()
        {
            m_gridSizeX = (int)(m_worldBounds.size.x / m_cellSizeX) + 1;
            m_gridSizeY = (int)(m_worldBounds.size.y / m_cellSizeY) + 1;

            // Now we know how big the grid should be, we can create it.
            // For every grid cell, we store an array of Spawners that are in that cell.
            // This uses a combination of a C-style fixed 2D array (one element per grid cell) and a Java-style jagged array (array of arrays),
            // as the grid size is fixed but the number of spawners in each cell is variable.  Declaring this requires the slightly weird syntax "Spawner[,][]"
            m_grid = new Spawner[m_gridSizeX, m_gridSizeY][];
            // Now we also have a special grid just for background spawners as they spawn on screen cos they're further back
            m_backgroundGrid = new Spawner[m_gridSizeX, m_gridSizeY][];

            int[,] counts = new int[m_gridSizeX, m_gridSizeY]; // temp array of spawner counts
            int[,] backgroundCounts = new int[m_gridSizeX, m_gridSizeY];

            // all cells empty by default
            for (int i = 0; i < m_gridSizeX; i++)
            {
                for (int j = 0; j < m_gridSizeY; j++)
                {
                    m_grid[i, j] = null;
                    m_backgroundGrid[i, j] = null;
                    counts[i, j] = 0;
                    backgroundCounts[i, j] = 0;
                }
            }

            // Run through the list of all spawners in 2 passes.
            // First, we need to count how many spawners are in each cell so we can create the arrays for them.
            // Then we can populate the grid.
            float gridX0 = m_worldBounds.min.x;
            float gridY0 = m_worldBounds.min.y;

            // first get the counts
            foreach (Spawner s in m_allSpawners)
            {
                if (s.insertIntoMultipleGridCells) // handle spawner that is inserted into multiple cells
                {
                    FastBounds2D b = s.bounds;
                    int gx0 = (int)((b.x0 - gridX0) / m_cellSizeX);
                    int gx1 = (int)((b.x1 - gridX0) / m_cellSizeX);
                    int gy0 = (int)((b.y0 - gridY0) / m_cellSizeY);
                    int gy1 = (int)((b.y1 - gridY0) / m_cellSizeY);
                    gx0 = Mathf.Max(gx0, 0);
                    gx1 = Mathf.Min(gx1, m_gridSizeX - 1);
                    gy0 = Mathf.Max(gy0, 0);
                    gy1 = Mathf.Min(gy1, m_gridSizeY - 1);
                    for (int gx = gx0; gx <= gx1; gx++)
                    {
                        for (int gy = gy0; gy <= gy1; gy++)
                        {
                            if (s.transform.position.z < BackgroundGridZThreshold)
                            {
                                counts[gx, gy]++;
                            }
                            else
                            {
                                backgroundCounts[gx, gy]++;
                            }
                        }
                    }
                }
                else // handle regular spawner, only inserted into 1 cell
                {
                    Vector3 pos = s.transform.position;
                    int gx = (int)((pos.x - gridX0) / m_cellSizeX);
                    int gy = (int)((pos.y - gridY0) / m_cellSizeY);

                    if (s.transform.position.z < BackgroundGridZThreshold)
                    {
                        counts[gx, gy]++;
                    }
                    else
                    {
                        backgroundCounts[gx, gy]++;
                    }
                }
            }

            // now create the Spawner arrays in the grid
            for (int i = 0; i < m_gridSizeX; i++)
            {
                for (int j = 0; j < m_gridSizeY; j++)
                {
                    int count = counts[i, j];
                    if (count > 0)
                    {
                        m_grid[i, j] = new Spawner[count];
                        counts[i, j] = 0; // reset count, we'll use it again when populating the grid
                    }

                    int backgroundCount = backgroundCounts[i, j];
                    if (backgroundCount > 0)
                    {
                        m_backgroundGrid[i, j] = new Spawner[backgroundCount];
                        backgroundCounts[i, j] = 0;
                    }
                }
            }

            // and finally fill in the Spawner references
            foreach (Spawner s in m_allSpawners)
            {
                if (s.insertIntoMultipleGridCells) // handle spawner that is inserted into multiple cells
                {
                    FastBounds2D b = s.bounds;
                    int gx0 = (int)((b.x0 - gridX0) / m_gridSizeX);
                    int gx1 = (int)((b.x1 - gridX0) / m_gridSizeX);
                    int gy0 = (int)((b.y0 - gridY0) / m_cellSizeY);
                    int gy1 = (int)((b.y1 - gridY0) / m_cellSizeY);
                    gx0 = Mathf.Max(gx0, 0);
                    gx1 = Mathf.Min(gx1, m_gridSizeX - 1);
                    gy0 = Mathf.Max(gy0, 0);
                    gy1 = Mathf.Min(gy1, m_gridSizeY - 1);
                    for (int gx = gx0; gx <= gx1; gx++)
                    {
                        for (int gy = gy0; gy <= gy1; gy++)
                        {
                            if (s.transform.position.z < BackgroundGridZThreshold)
                            {
                                m_grid[gx, gy][counts[gx, gy]++] = s;
                            }
                            else
                            {
                                m_backgroundGrid[gx, gy][backgroundCounts[gx, gy]++] = s;
                            }
                        }
                    }
                }
                else // handle regular spawner, only inserted into 1 cell
                {
                    Vector3 pos = s.transform.position;
                    int gx = (int)((pos.x - gridX0) / m_gridSizeX);
                    int gy = (int)((pos.y - gridY0) / m_cellSizeY);

                    if (s.transform.position.z < BackgroundGridZThreshold)
                    {
                        m_grid[gx, gy][counts[gx, gy]++] = s;
                    }
                    else
                    {
                        m_backgroundGrid[gx, gy][backgroundCounts[gx, gy]++] = s;
                    }
                }
            }
        }

        void Update()
        {
            if (App.paused)
                return;

            if (GameLogic.gameCamera.hasInitialized)
            {
                // Foreground
                UpdateSpawnerActivating(backgroundSpawners: false);

                //Background

                UpdateSpawnerActivating(backgroundSpawners: true);
            }
        }

        // Switch spawners and on off based on proximity to the main camera.
        // We don't explicitly switch spawners OFF if they are going out of range, we just stop
        // sending them a 'keep alive' tick, they will switch themselves off in their Update().
        void UpdateSpawnerActivating(bool backgroundSpawners)
        {
#if DEBUG_DRAW_BOUNDS
		//Util.DrawDebugBounds2D(m_worldBounds, Color.white);	
#endif

            // Scan all the spawners that touch the bigger ("deactivate") bounds.  Anything inside gets a "keep alive" tick to stop them from self-deactivating.
            // Then we can do an additional check for things that touch the smaller ("activate") bounds and switch them on if necessary.

            FastBounds2D keepAliveBounds = (backgroundSpawners
                ? GameLogic.gameCamera.KeepAliveBackgroundBounds
                : GameLogic.gameCamera.KeepAliveWorldBounds);
            FastBounds2D activateBounds = (backgroundSpawners
                ? GameLogic.gameCamera.ScreenBackgroundBounds
                : GameLogic.gameCamera.ScreenWorldBounds);

            // get the min and max grid cell indices to scan
            float gridX0 = m_worldBounds.min.x;
            float gridY0 = m_worldBounds.min.y;
            int gx0 = (int)((keepAliveBounds.x0 - gridX0) / m_cellSizeX) - 1; // (add a 1 cell border)
            int gy0 = (int)((keepAliveBounds.y0 - gridY0) / m_cellSizeY) - 1;
            int gx1 = (int)((keepAliveBounds.x1 - gridX0) / m_cellSizeX) + 1;
            int gy1 = (int)((keepAliveBounds.y1 - gridY0) / m_cellSizeY) + 1;

            // check if we're completely outside the range of the spawners, in which case there's no grid to check
            if ((gx0 >= m_gridSizeX) || (gy0 >= m_gridSizeY) || (gx1 < 0) || (gy1 < 0))
                return;

            // clamp in case we're partially outside the range of the spawners
            gx0 = Mathf.Clamp(gx0, 0, m_gridSizeX - 1);
            gy0 = Mathf.Clamp(gy0, 0, m_gridSizeY - 1);
            gx1 = Mathf.Clamp(gx1, 0, m_gridSizeX - 1);
            gy1 = Mathf.Clamp(gy1, 0, m_gridSizeY - 1);

            m_checkFrameID++;

            Spawner[,][] grid = (backgroundSpawners ? m_backgroundGrid : m_grid);

            // iterate through the grid, find every spawner that potentially touches one or both of the small and large bounds that we check for switching on/off
            for (int y = gy0; y <= gy1; y++)
            {
                for (int x = gx0; x <= gx1; x++)
                {
#if DEBUG_DRAW_BOUNDS
				float px0 = gridX0 + (x*m_cellSizeX);
				float py0 = gridY0 + (y*m_cellSizeY);
				float px1 = px0+m_cellSizeX;
				float py1 = py0+m_cellSizeY;
				Color gridCol = new Color(0.3f, 0.3f, 0.3f, 1.0f);
				Util.DrawDebugLine(new Vector3(px0,py0,0.0f), new Vector3(px1,py0,0.0f), gridCol);
				if(x==gx1)
					Util.DrawDebugLine(new Vector3(px1,py0,0.0f), new Vector3(px1,py1,0.0f), gridCol);
				if(y==gy1)
					Util.DrawDebugLine(new Vector3(px1,py1,0.0f), new Vector3(px0,py1,0.0f), gridCol);
				Util.DrawDebugLine(new Vector3(px0,py1,0.0f), new Vector3(px0,py0,0.0f), gridCol);
#endif
                    Spawner[] spawners = grid[x, y];
                    if (spawners != null)
                    {
                        for (int si = 0, ni = spawners.Length; si < ni; si++)
                        {
                            Spawner s = spawners[si];

                            if (s == null)
                            {
                                continue;
                            }

                            // don't check the same spawner more than once in a frame, in case it spans multiple grid cells
                            if (s.checkID == m_checkFrameID)
                                continue;
                            s.checkID = m_checkFrameID;

                            Spawner.State state = s.state;
                            if (state != Spawner.State.dead) // ignore dead spawners
                            {
                                bool canSpawn = true;

                                canSpawn &= s.CanSpawn();

                                // don't spawn if the player has gone through a portal to a different layer
                                if (canSpawn) // check for filtering out spawner based on shark type and/or credits collected
                                {
                                    FastBounds2D sb = s.bounds;
#if DEBUG_DRAW_BOUNDS
								Util.DrawDebugBounds2D(sb, Color.cyan);
#endif
                                    if (sb.Intersects(keepAliveBounds))
                                    {
                                        // this one is inside the large bounds so gets a KeepAlive tick, this stops it from deactivating itself in its update function
                                        s.KeepAliveTick();
                                        if (!s.enabled)
                                        {
                                            // it is not currently active, let's see if it should activate itself
                                            if (sb.Intersects(activateBounds))
                                            {
                                                if ((state == Spawner.State.waiting) ||
                                                    (state == Spawner.State.sleeping))
                                                {
                                                    s.OnActivated(); // yes, switch it on
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void SpawnPlayer()
        {
            GameObject obj = SpawnerManager.Spawn(stats.sharkPrefab, null, transform.position, transform.rotation, 1f);
            Player player = obj.GetComponent<Player>();
            if (player != null)
            {
                player.InitializeAttachPoints();
            }

            obj.transform.localScale *= stats.sharkScale;
        }
    }
}