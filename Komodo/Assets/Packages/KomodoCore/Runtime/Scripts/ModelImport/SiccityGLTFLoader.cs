﻿//using Siccity.GLTFUtility;
using UnityEngine;

namespace Komodo.AssetImport
{
    public class SiccityGLTFLoader : ModelDownloaderAndLoader
    {
        public override void LoadLocalFile(string localFilename, System.Action<GameObject> callback)
        {
            if (callback != null)
            {
                //callback(null);
                return;
            }
            //return Importer.LoadFromFile(localFilename);
        }
    }
}
