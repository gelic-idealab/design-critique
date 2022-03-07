using System;
using System.IO;
using UnityEngine;
using TiltBrushToolkit;
//using Siccity.GLTFUtility;

namespace Komodo.AssetImport
{
    public class TiltBrushAndSiccityLoader : ModelDownloaderAndLoader
    {
        public override void LoadLocalFile(string localFilename, System.Action<GameObject> callback)
        {
            if (isTiltBrushFile(localFilename))
            {
                Debug.Log("Using Tilt Brush Loader.");
                if (callback != null)
                {
                    callback(LoadFileWithTiltBrushToolkit(localFilename));
                    return;
                }
                LoadFileWithTiltBrushToolkit(localFilename);
            }
            else
            {
                Debug.Log("Using Siccity GLTFUtility Loader. ");
                if (callback != null)
                {
                    //callback(Importer.LoadFromFile(localFilename););
                    return;
                }
                //Importer.LoadFromFile(localFilename);
            }
        }

        /* 
        * Reads through file contents to check if GLB is Tilt Brush-specific.
        * Reads line-by-line and stops after finding the beginning of the binary section.
        */
        private bool isTiltBrushFile(string filename)
        {
            int timeOut = 5000; // length of time in milliseconds to allow file scan
            int minNumCharactersToRead = 1000;

            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            string tiltBrushString1 = "\"generator\": \"Tilt Brush";
            string tiltBrushString2 = "tiltbrush.com/shaders/";
            string tiltBrushString3 = "GOOGLE_tilt_brush_material";
            int lengthOfLongestString = tiltBrushString1.Length;

            //for the first line, read in the substring we want at its full length.
            int startIndex = 0;
            int numCharactersToRead = lengthOfLongestString;

            bool isFirstLine = true;

            using (StreamReader reader = new StreamReader(filename))
            {
                var buffer = new char[numCharactersToRead];
                int numCharactersRead = numCharactersToRead;
                while (reader.Peek() > -1)
                {
                    reader.ReadBlock(buffer, startIndex, numCharactersToRead);

                    string bufferAsString = new string(buffer);

                    if (numCharactersRead >= minNumCharactersToRead && watch.ElapsedMilliseconds > timeOut)
                    {
                        watch.Stop();
                        Debug.Log($"Hit time-out of {timeOut} ms before finding string. Read {numCharactersRead} total characters.");
                        return false;
                    }

                    if (bufferAsString.Contains("}") && bufferAsString.Contains("BIN"))
                    { // reached the end of the JSON section
                        watch.Stop();
                        Debug.Log($"Encountered BIN section of file before finding Tilt Brush string. Took {watch.ElapsedMilliseconds} ms.");
                        return false;
                    }

                    if (bufferAsString.Contains(tiltBrushString1) ||
                        bufferAsString.Contains(tiltBrushString2) ||
                        bufferAsString.Contains(tiltBrushString3))
                    {
                        watch.Stop();
                        Debug.Log($"Detected Tilt Brush file in {watch.ElapsedMilliseconds} ms.");
                        return true;
                    }

                    if (isFirstLine)
                    {
                        startIndex = lengthOfLongestString - 1;
                        numCharactersToRead = 1;
                        isFirstLine = false;
                        continue;
                    }

                    // after the first line, shift buffer to the left one character so we read the next substring.
                    Array.Copy(buffer, 1, buffer, 0, buffer.Length - 1);
                    numCharactersRead += 1;
                }

                watch.Stop();
                Debug.Log($"Did not find Tilt Brush string in entire file. Took {watch.ElapsedMilliseconds} ms");
                return false;
            }
        }

        private GameObject LoadFileWithTiltBrushToolkit(string localFilename)
        {
            return Glb2Importer.ImportTiltBrushAsset(localFilename);
        }

    }
}