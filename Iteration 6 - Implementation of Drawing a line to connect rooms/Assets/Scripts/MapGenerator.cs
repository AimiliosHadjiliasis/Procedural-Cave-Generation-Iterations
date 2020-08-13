using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class MapGenerator : MonoBehaviour
{

    public int width;
    public int height;

    public string seed;
    public bool useRandomSeed;

    //keep track of how much will be occubied by walls 
    [Range(0, 100)]
    public int randomFillPercent;

    //Create the map (2D array of integers) which defines the a grid of integers 
    //and any tile that is equal to 0 in the map will be an empty tile
    //and any tile that is equal to 1 will be a tile that represents a wall
    int[,] map;

    void Start()
    {
        GenerateMap();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GenerateMap();
        }
    }

    void GenerateMap()
    {
        //Fill the map with random values as a starting configurations
        //This is how cecullar automata works
        map = new int[width, height];
        RandomFillMap();

        //We can use diferent number here to get different values for smoothing
        for (int i = 0; i < 5; i++)
        {
            SmoothMap();
        }

        ProcesMap();

        //Specify border of the map
        int borderSize = 1;
        int[,] borderedMap = new int[width + borderSize * 2, height + borderSize * 2];

        //Loop through border map and set everything equal to the map that we generated
        //excepr the border that we need to set it equal to wall tile = 1
        for (int x = 0; x < borderedMap.GetLength(0); x++)
        {
            for (int y = 0; y < borderedMap.GetLength(1); y++)
            {
                //not in border
                if (x >= borderSize && x < width + borderSize && y >= borderSize && y < height + borderSize)
                {
                    borderedMap[x, y] = map[x - borderSize, y - borderSize];
                }
                //in border set wall tile
                else
                {
                    
                    borderedMap[x, y] = 1;
                }
            }
        }

        //Call the method mesh generator from the other class
        //in order to generate the mesh
        MeshGenerator meshGen = GetComponent<MeshGenerator>();
        meshGen.GenerateMesh(borderedMap, 1);
    }

    //Method that process the map
    //It removes the small walls and small rooms in the cave
    void ProcesMap()
    {
        //Get the region
        List<List<Coord>> wallRegions = GetRegions(1);

        //Variable that used to delete small regions of size 50
        //in case we increase the numeber then bigger regions are going to be removed
        int wallThresholdSize = 50;

        /***************************************************************************/
        //                  REMOVES SMALL WALLS INSIDE THE MAP:
        //for every list of coords if we are inside the wall size
        //then in evey tile of that wall we set the map place to empty space
        foreach (List<Coord> wallRegion in wallRegions)
        {
            if (wallRegion.Count < wallThresholdSize)
            {
                foreach (Coord tile in wallRegion)
                {
                    map[tile.tileX, tile.tileY] = 0;
                }
            }
        }
        /***************************************************************************/

        /***************************************************************************/
        //          REMOVES SMALLER ROOMS THAT ARE CREATED INSIDE THE MAP:
        List<List<Coord>> roomRegions = GetRegions(0);
        int roomThresholdSize = 50;

        //Rooms that survive the remove process
        List<Room> survivingRooms = new List<Room>();

        foreach (List<Coord> roomRegion in roomRegions)
        {
            if (roomRegion.Count < roomThresholdSize)
            {
                foreach (Coord tile in roomRegion)
                {
                    map[tile.tileX, tile.tileY] = 1;
                }
            }
            else
            {
                survivingRooms.Add(new Room(roomRegion, map));
            }
        }
        /***************************************************************************/

        ConnectClosestRooms(survivingRooms);
    }

    //Method that connects 2 rooms together
    //It chooses the 2 closest rooms
    void ConnectClosestRooms(List<Room> allRooms)
    {
        //keep track of best distance
        int bestDistance = 0;   

        //Tiles resulted best distance:
        Coord bestTileA = new Coord();
        Coord bestTileB = new Coord();

        //Rooms that tiles come from:
        Room bestRoomA = new Room();
        Room bestRoomB = new Room();

        //Know if we found a possible connection
        bool possibleConnectionFound = false;

        //Go through all the rooms
        foreach (Room roomA in allRooms)
        {
            possibleConnectionFound = false;

            //compare room a to find the second room to connect
            foreach (Room roomB in allRooms)
            {
                //No need to find connection between the same room
                if (roomA == roomB)
                {
                    continue;
                }

                //if room a is actualy connected to room be we move to other room
                if (roomA.IsConnected(roomB))
                {
                    possibleConnectionFound = false;
                    break;
                }

                //Loop through all the tiles:
                for (int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; tileIndexA++)
                {
                    for (int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB++)
                    {
                        //Initialise the tile A and B coords
                        Coord tileA = roomA.edgeTiles[tileIndexA];
                        Coord tileB = roomB.edgeTiles[tileIndexB];
                        
                        //Formoula for finding the distance
                        int distanceBetweenRooms = (int)(Mathf.Pow(tileA.tileX - tileB.tileX, 2) 
                                                        + Mathf.Pow(tileA.tileY - tileB.tileY, 2));

                        //in case we have a smaller distance
                        if (distanceBetweenRooms < bestDistance || !possibleConnectionFound)
                        {
                            bestDistance = distanceBetweenRooms;
                            possibleConnectionFound = true;
                            bestTileA = tileA;
                            bestTileB = tileB;
                            bestRoomA = roomA;
                            bestRoomB = roomB;
                        }
                    }
                }
            }

            //In case we found a room 
            if (possibleConnectionFound)
            {
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            }
        }
    }

    //Mehtod that create a passage to the 2 rooms
    void CreatePassage(Room roomA, Room roomB, Coord tileA, Coord tileB)
    {
        Room.ConnectRooms(roomA, roomB);    //tell a b that are connected
        
        //draw line to show that they are connected
        Debug.DrawLine(CoordToWorldPoint(tileA), CoordToWorldPoint(tileB), Color.green, 100);
    }

    //used to draw the line
    Vector3 CoordToWorldPoint(Coord tile)
    {
        return new Vector3(-width / 2 + .5f + tile.tileX, 2, -height / 2 + .5f + tile.tileY);
    }

    //Method that given a certain tile type can return all the regions of that type of tile
    List<List<Coord>> GetRegions(int tileType)
    {
        List<List<Coord>> regions = new List<List<Coord>>();
        //array that shows if a tile is already looked
        int[,] mapFlags = new int[width, height];

        //Loop through every part of the map
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (mapFlags[x,y] == 0 && map[x,y] == tileType)
                {
                    List<Coord> newRegion = GetRegionTiles(x, y);
                    regions.Add(newRegion);

                    foreach(Coord tile in newRegion)
                    {
                        mapFlags[tile.tileX, tile.tileY] = 1;
                    }
                }
            }
        }
        return regions;
    }

    //Method that returns a list of coordinates of each tile
    List<Coord> GetRegionTiles(int startX, int startY)
    {
        //List of coords that store the tiles
        List<Coord> tiles = new List<Coord>();

        //array that shows if a tile is already looked
        int[,] mapFlags = new int[width, height];

        //Need to know what type we have
        //wether is a wall tile = 1 or an empty space tile = 0
        int tileType = map[startX, startY];
        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(new Coord(startX, startY));

        //set map flags to 1 so we know that we looked at that tile
        mapFlags[startX, startY] = 1;

        //if there are items in our queue:
        while(queue.Count>0)
        {
            //get first tile:
            Coord tile = queue.Dequeue(); //1st item and remove
            tiles.Add(tile);

            //Add adjacent tile:
            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    if (IsInMapRange(x,y) && (y==tile.tileY || x == tile.tileX))
                    {
                        if (mapFlags[x,y] == 0 && map[x,y] == tileType)
                        {
                            mapFlags[x, y] = 1;
                            queue.Enqueue(new Coord(x, y));
                        }
                    }
                }
            }
        }

        return tiles;
    }

    bool IsInMapRange(int x,int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    //method work by the seed
    void RandomFillMap()
    {
        if (useRandomSeed)
        {
            seed = Time.time.ToString();
        }

        //pseudo random generator
        //returns a unique hash code (an integer for the seed)
        System.Random pseudoRandom = new System.Random(seed.GetHashCode());

        //Loop through the tiles of the map
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                //Set walls = 1 // set empty space = 0
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    map[x, y] = 1;
                }
                else
                {
                    map[x, y] = (pseudoRandom.Next(0, 100) < randomFillPercent) ? 1 : 0;
                }
            }
        }
    }

    //Smoothing iteration to generate walls
    void SmoothMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                //Rules for walls:
                int neighbourWallTiles = GetSurroundingWallCount(x, y);

                if (neighbourWallTiles > 4)
                    map[x, y] = 1;
                else if (neighbourWallTiles < 4)
                    map[x, y] = 0;

            }
        }
    }

    //Method that tell us how many neighbouring tiles are walls	
    int GetSurroundingWallCount(int gridX, int gridY)
    {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                //Inside the border
                if (IsInMapRange(neighbourX,neighbourY))
                {
                    if (neighbourX != gridX || neighbourY != gridY)
                    {
                        wallCount += map[neighbourX, neighbourY];
                    }
                }
                //Outside the border	
                else
                {
                    wallCount++;
                }
            }
        }

        return wallCount;
    }

    //Struct that tell us where in the map a tile is located
    struct Coord
    {
        //Initialise x,y values of the tile
        public int tileX;
        public int tileY;

        //Constructor:
        public Coord(int x, int y)
        {
            tileX = x;
            tileY = y;
        }   
    }

    /***************************************************/
    //  Class that holds all the information of a room
    /***************************************************/
    class Room
    {
        //Store tiles and tiles that form the edge of the room
        //and a list that stores the connected rooms
        public List<Coord> tiles;
        public List<Coord> edgeTiles;
        public List<Room> connectedRooms;
        public int roomSize;    //tiles in room

        //Empty constructor in case we want to set a room equal to an empty room
        public Room()
        {
        }

        //Constructor:
        public Room(List<Coord> roomTiles, int[,] map)
        {
            tiles = roomTiles;
            roomSize = tiles.Count;

            connectedRooms = new List<Room>();
            edgeTiles = new List<Coord>();

            //Loop through the tiles and check the neighbours:
            foreach (Coord tile in tiles)
            {
                for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
                {
                    for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                    {
                        //Excluding the diagonal neighbours:
                        if (x == tile.tileX || y == tile.tileY)
                        {
                            //if its a wall tile:
                            if (map[x, y] == 1)
                            {
                                edgeTiles.Add(tile);
                            }
                        }
                    }
                }
            }
        }

        //Method that takes 2 rooms and add room a to room b list and vice versa
        public static void ConnectRooms(Room roomA, Room roomB)
        {
            roomA.connectedRooms.Add(roomB);
            roomB.connectedRooms.Add(roomA);
        }

        //Bool that check if a room is connected with ohther room
        public bool IsConnected(Room otherRoom)
        {
            return connectedRooms.Contains(otherRoom);
        }
    }
}