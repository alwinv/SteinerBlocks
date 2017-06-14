//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
//using HoloToolkit.Unity.InputModule;
using System;
using System.IO;
using System.Text;
//using HoloToolkit.Sharing;
using HoloToolkit.Sharing.Spawning;
#if !UNITY_EDITOR
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Windows.Storage;
#endif

public class BlockIO : MonoBehaviour {

    public PrefabSpawnManager spawnManager;
    public GameObject block_prefab;
    public GameObject blocks_grid;

    private string[] blocksJSONList;
    private int blocks_SideShow_CurrentListItem = 0;

    // Enum to use in re-positioning the grids
    public enum Orientation
    {
        Vertical,
        Horizontal
    }

    // Use this for initialization
    void Start () {
    }

    // Update is called once per frame
    void Update () {
		
	}

#if !UNITY_EDITOR

    // Handler to load a local file
    // Handler to load a file 
    void OnLoadFile_ForLocal(string fileName)
    {
        createBlocksFromFile(fileName, false);
    }

    // Handler to load a shared file 
    void OnLoadFile_ForSharing (string fileName)
    {
        createBlocksFromFile(fileName, true);
    }

    void OnSaveFile_ForLocal()
    {
        // get all the objects
        Transform blocks_grid = this.transform.GetChild(0);

        // extract the grid name from the first block's name up to the first "_" character
        string name = blocks_grid.GetChild(0).name;
        name = name.Substring(0, name.IndexOf("_"));

        // detect how many blocks wide and high the grid is
        // todo: 1000 should be replaced by blocks_grid.ChildCount to support arbitrary grid sizes
        // todo: but there are 2x the expected children in the grids somehow, so it's failing
        int width = 50; //Mathf.FloorToInt((blocks_grid.GetChild(1000 - 1).localPosition.x - blocks_grid.GetChild(0).localPosition.x)/Globals.BlockSpacing) + 1;
        int height = 20; //Mathf.FloorToInt((blocks_grid.GetChild(1000 - 1).localPosition.z - blocks_grid.GetChild(0).localPosition.z)/Globals.BlockSpacing) + 1;

        // create a 2D grid to place the GameObjects in
        GameObject[,] blocksGameObjectList2D = new GameObject[width, height];

        for (int nIndex = 0; nIndex < width; nIndex++)
        {
            for (int mIndex = 0; mIndex < height; mIndex++)
            {
                blocksGameObjectList2D[nIndex, mIndex] = blocks_grid.GetChild(nIndex * height + mIndex).gameObject;
            }
        }
        
        BlockDataList blocksList = new BlockDataList(name, blocksGameObjectList2D);

        // Serialze into JSON format 
        string blockListText = JSONSerializeBlockDataList(blocksList);

        // Save JSON string as file to Documents folder
        SaveBlockDataListToRoamingFolder(blockListText, name);
    }

    void createBlocksFromFile(string fileName, bool SpawnSharedObjects)
    {
        // load file from documents
        string blocksJSON = LoadBlocksFromRoamingFolder(fileName).Result;

        // if no file was found or failed, load from app resource file URI
        if (blocksJSON == null)
        {
            blocksJSON = readBlocksJSONFromAppResource(fileName).Result;
        }

        // convert JSON string to blocks data object
        BlockDataList blocksList = JSONDeserializeBlockDataList(blocksJSON);

        // create the block game objects
        createBlocksFromDataList(blocksList, SpawnSharedObjects);
    }

    private async Task<String> LoadBlocksFromRoamingFolder(string fileName)
    {
        // read blocks files and load them into a BlockDataList
        //var documentsFolder = Windows.Storage.KnownFolders.DocumentsLibrary;
        var roamingFolder = Windows.Storage.ApplicationData.Current.RoamingFolder;

        // read from local file
        try
        {
            //var file = await documentsFolder.GetFileAsync(fileName);
            var file = await roamingFolder.GetFileAsync(fileName);
            if (file != null)
            {
                return await Windows.Storage.FileIO.ReadTextAsync(file);
            }
            else
                return null;
        }
        catch (Exception ex)
        {
            Debug.Log("Failed: reading file (" + ex.Message + ")");
            return null;
        }
    }

    // Handler to load sample files
    void OnLoadFiles_ForSlideShow(string[] fileNames)
    {
        blocksJSONList = new string[fileNames.Length];

        for (int i = 0; i < fileNames.Length; i++)
        {
            // read blocks JSON string from file
            blocksJSONList[i] = readBlocksJSONFromAppResource(fileNames[i]).Result;
        }

        // convert JSON string to blocks data object
        BlockDataList blocksList = JSONDeserializeBlockDataList(blocksJSONList[blocks_SideShow_CurrentListItem]);

        // create the block game objects using the first file
        createBlocksFromDataList(blocksList, false);
    }

    void OnLoadNextBlocks_ForSlideShow()
    {
        // rotate to next item in slideshow, if at end, set it back to first item
        if(blocks_SideShow_CurrentListItem == blocksJSONList.Length)
            blocks_SideShow_CurrentListItem = 0;
        else
            blocks_SideShow_CurrentListItem += 1;

        // load the .blocks data into blocks datastructure
        BlockDataList blocksList = JSONDeserializeBlockDataList(blocksJSONList[blocks_SideShow_CurrentListItem]);

        // rotate all the SlideShow's blocks to the new orientation
        for(int i = 0; i < blocksList.BlockDataArray.Length; i++)
        {
            // note: starting at i=1 and indexing using i-1 because the blocks_grid transform itself is included
            this.transform.GetChild(0).GetChild(i).gameObject.SendMessage("OnRotateAbsolute", Quaternion.Euler(blocksList.BlockDataArray[i].Rotation));
        }
    }

    private void createBlocksFromDataList(BlockDataList blocksDataList, bool SpawnSharedObjects)
    {
        float nOffset = blocksDataList.width / 2; // sets the center of grid the grid to 0
        float mOffset = 0; // sets bottom of grid to 0 (to center grid at 0 instead, use "mRows / 2");
        Quaternion BlockRotation;
        float blockSpacing = Globals.BlockSpacing;

        // set blocks up for each column of the grid
        for (int nIndex = 0; nIndex < blocksDataList.width; nIndex++)
        {
            // set blocks up for each row of the grid
            for (int mIndex = 0; mIndex < blocksDataList.height; mIndex++)
            {
                // get rotation from input block grid if it exists
                BlockRotation = Quaternion.Euler(blocksDataList.BlockData2DArray[nIndex, mIndex].Rotation);

                if (SpawnSharedObjects)
                {
                    // instantiate a shared cube in the right spot on the grid
                    this.spawnManager.Spawn(
                        new SyncSpawnedObject(),
                        new Vector3(
                            (nIndex - nOffset) * Globals.BlockSpacing,
                            1 * blockSpacing,
                            (mIndex - mOffset + 1) * Globals.BlockSpacing),
                        BlockRotation,
                        this.transform.GetChild(0).gameObject,
                        blocksDataList.Name,
                        false);
                }
                else
                {
                    // instantiate a local cube in the right spot on the grid
                    GameObject NewBlock = Instantiate(block_prefab, this.transform.GetChild(0), false);
                    NewBlock.transform.localRotation = BlockRotation;
                    NewBlock.transform.localPosition = new Vector3(
                            (nIndex - nOffset) * Globals.BlockSpacing,
                            1 * Globals.BlockSpacing,
                            (mIndex - mOffset + 1) * Globals.BlockSpacing);
                    NewBlock.name = blocksDataList.Name + "_" + mIndex.ToString() + "-" + nIndex.ToString();
                }
            }
        }
    }

    //
    // Persistence of block grids...
    // 
    [DataContract]
    private class BlockDataList
    {
        private string name = "";
        private string size = "";
        public int width = 0;
        public int height = 0;
        private BlockData[,] blockDataList2D = null;
        private BlockData[] blockDataListOneDimension = null;
        public GameObject BlockGridParent = null;
        private GameObject[,] BlockGameObjects = null;

        public BlockDataList(string Name, GameObject[,] BlockList)
        {
            name = Name;
            width = BlockList.GetLength(0);
            height = BlockList.GetLength(1);
            size = width.ToString() + "," + height.ToString();
            BlockGridParent = BlockList[0, 0].transform.parent.parent.gameObject;
            // create new 2D and 1D block list objects to fill
            blockDataList2D = new BlockData[width, height];
            blockDataListOneDimension = new BlockData[width * height];
            BlockGameObjects = new GameObject[width, height];
            // get data from blocks for each column of the grid
            for (int nIndex = 0; nIndex < width; nIndex++)
            {
                // get data from blocks for each row of the grid
                for (int mIndex = 0; mIndex < height; mIndex++)
                {
                    blockDataList2D[nIndex, mIndex] = new BlockData(BlockList[nIndex, mIndex]);
                    blockDataListOneDimension[nIndex * height + mIndex] = blockDataList2D[nIndex, mIndex];
                    BlockGameObjects[nIndex, mIndex] = BlockList[nIndex, mIndex];
                }
            }
        }

        public void CommitChanges(GameObject blockUpdated)
        {
            // updates the datalist arrays with the current rotations

            int startIndex = blockUpdated.name.IndexOf(this.name + "_Block") + (this.name + "_Block").Length;
            int blockIndex = int.Parse(blockUpdated.name.Substring(startIndex));
            BlockData newData = new BlockData(blockUpdated);
            blockDataListOneDimension[blockIndex] = newData;
            blockDataList2D[blockIndex % blockDataList2D.GetLength(0), Mathf.FloorToInt(blockIndex / blockDataList2D.GetLength(0))] = newData;
        }

        public BlockData[,] BlockData2DArray
        {
            get
            {
                if (blockDataList2D == null
                    && size != null
                    && blockDataListOneDimension != null)
                {
                    // convert it to a 2d array as well
                    blockDataList2D = new BlockData[width, height];
                    for (int nIndex = 0; nIndex < width; nIndex++)
                    {
                        for (int mIndex = 0; mIndex < height; mIndex++)
                        {
                            blockDataList2D[nIndex, mIndex] = blockDataListOneDimension[nIndex * height + mIndex];
                        }
                    }
                }
                return blockDataList2D;
            }
        }

        [DataMember]
        public string Name
        {
            set { name = value; }
            get { return name; }
        }

        [DataMember]
        public string Size
        {
            set
            {
                size = value;
                // get the 2 sizes of the array
                string[] blocksLengthString = size.Split(",".ToCharArray());
                if (blocksLengthString.Length > 1)
                {
                    width = int.Parse(blocksLengthString[0]);
                    height = int.Parse(blocksLengthString[1]);
                }
            }
            get { return size; }
        }

        [DataMember]
        public BlockData[] BlockDataArray
        {
            set
            {
                blockDataListOneDimension = value;
            }
            get { return blockDataListOneDimension; }
        }

        public GameObject[,] Blocks2DArray
        {
            get { return BlockGameObjects; }
        }
    }

    [DataContract]
    private class BlockData
    {
        private Vector3 rotation = new Vector3(0, 0, 0);

        public BlockData(GameObject block)
        {
            if (block != null)
            {
                rotation = new Vector3(
                    Mathf.Round(block.transform.localEulerAngles.x),
                    Mathf.Round(block.transform.localEulerAngles.y),
                    Mathf.Round(block.transform.localEulerAngles.z));
            }
        }

        public Vector3 Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        [DataMember]
        public string r
        {
            set
            {
                // take a string of the format "0,90,180" and convert it to a Vector3
                string[] rotationStringSplits = value.Split(",".ToCharArray(), 3);
                rotation.x = float.Parse(rotationStringSplits[0]);
                rotation.y = float.Parse(rotationStringSplits[1]);
                rotation.z = float.Parse(rotationStringSplits[2]);
            }
            get
            {
                return rotation.x.ToString() + "," + rotation.y.ToString() + "," + rotation.z.ToString();
            }
        }
    }

    private async Task<string> readBlocksJSONFromAppResource(string fileName)
    {
        // load a .blocks file from app's /Blocks folder
        try
        {
            Uri fileURI = new Uri("ms-appx:///Blocks/" + fileName);
            StorageFile blocksFile = await StorageFile.GetFileFromApplicationUriAsync(fileURI);
            return await FileIO.ReadTextAsync(blocksFile);
        }
        catch (Exception ex)
        {
            Debug.Log("Failed: reading file (" + ex.Message + ")");
            return null;
        }
    }

    private BlockDataList JSONDeserializeBlockDataList(string JSONdata)
    {
        DataContractJsonSerializer jsonSer = new DataContractJsonSerializer(typeof(BlockDataList));
        Debug.Log(JSONdata);
        MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(JSONdata));
        BlockDataList returnBlockDataList = (BlockDataList)jsonSer.ReadObject(stream);
        return returnBlockDataList;
    }

    private async void SaveBlockDataListToRoamingFolder(string blockListText, string fileName)
    {
        // File info
        //var documentsFolder = Windows.Storage.KnownFolders.DocumentsLibrary;
        var roamingFolder = Windows.Storage.ApplicationData.Current.RoamingFolder;
        fileName = fileName + ".blocks";

        try
        {
            // save to local file
            //var file = await documentsFolder.CreateFileAsync(fileName, Windows.Storage.CreationCollisionOption.ReplaceExisting);
            var file = await roamingFolder.CreateFileAsync(fileName, Windows.Storage.CreationCollisionOption.ReplaceExisting);
            await Windows.Storage.FileIO.WriteTextAsync(file, blockListText);
        }
        catch (Exception ex)
        {
            Debug.Log("Failed: writing file (" + ex.Message + ")");
        }
    }

    private string JSONSerializeBlockDataList(object BlockDataList)
    {
        MemoryStream stream = new MemoryStream();
        DataContractJsonSerializer jsonSer = new DataContractJsonSerializer(typeof(BlockDataList));
        jsonSer.WriteObject(stream, BlockDataList);
        stream.Position = 0;
        StreamReader sr = new StreamReader(stream);
        string returnVal = sr.ReadToEnd();
        return returnVal;
    }

#endif
}
