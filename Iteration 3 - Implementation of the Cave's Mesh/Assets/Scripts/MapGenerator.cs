using UnityEngine;
using System.Collections;
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

        //Call the method mesh generator from the other class
        //in order to generate the mesh
        MeshGenerator meshGen = GetComponent<MeshGenerator>();
        meshGen.GenerateMesh(map, 1);
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
                if (neighbourX >= 0 && neighbourX < width && neighbourY >= 0 && neighbourY < height)
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

    //Method that draws in the Gizmos Property of unity in 
    //order to see if everything is working

    void OnDrawGizmos()
    {
        /*
		if (map != null) {
			for (int x = 0; x < width; x ++) {
				for (int y = 0; y < height; y ++) {
					Gizmos.color = (map[x,y] == 1)?Color.black:Color.white;
					Vector3 pos = new Vector3(-width/2 + x + .5f,0, -height/2 + y+.5f);
					Gizmos.DrawCube(pos,Vector3.one);
				}
			}
		}
		*/
    }

}