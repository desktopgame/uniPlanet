using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace uniPlanet {
    [System.Serializable]
    public class Block
    {
        public string reference;
        public string texture;
    }

    [System.Serializable]
    public class Blocks
    {
        public Block[] blocks;
    }

    [System.Serializable]
    public class MappingRule
    {
        public string all;
        public string top;
        public string bottom;
        public string left;
        public string right;
        public string front;
        public string back;
    }

    [System.Serializable]
    public class Texture
    {
        public string baseFileName;
        public MappingRule mappingRule;
        public string reference;
    }

    [System.Serializable]
    public class Textures
    {
        public Texture[] textures;
        public string baseDirectory;
    }

    [System.Serializable]
    public class Cell
    {
        public int x;
        public int y;
        public int z;
        public string block;
    }

    [System.Serializable]
    public class WorldSize
    {
        public int x;
        public int y;
        public int z;
    }

    [System.Serializable]
    public class World
    {
        public Cell[] cell;
        public WorldSize worldSize;
    }
}