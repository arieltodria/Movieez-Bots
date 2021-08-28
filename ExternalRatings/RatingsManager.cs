using System;
using System.Collections.Generic;
using System.Text;
using IMDBCore;
using RottenTomatoes;
using RottenTomatoes.Api;
using static System.IO.Compression.GZipStream;
using System.Net;
using System.Data;
using System.IO;
using System.IO.Compression;
using Movieez.Objects;
using System.Linq;

namespace Movieez.ExternalRatings
{
    public class RatingsManager  
    {
        private WebClient webClient;
        string DBUrl = "https://datasets.imdbws.com/";
        public Dictionary<string, string> DBCompressedFiles; // Key=FileName, Value=FileFullPath
        public Dictionary<string, string> DBDecompressedFiles; // Key=FileName, Value=FileFullPath
        public string[] DBFileNames = { "title.basics.tsv.gz", "title.ratings.tsv.gz" };
        int minMovieYear = 2020;
        List<Objects.Rating> RatingsList;
        // Logger
        public static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public RatingsManager(bool runOnStart = true)
        {
            List<Objects.Rating> ratingsList = new List<Objects.Rating>();
            DBCompressedFiles = new Dictionary<string, string>();         // Key=FileName, Value=FileFullPath
            //DBInfo = new Dictionary<string, DataTable>();       // Key=FileName, Value=DataTable obj
            foreach (string fileName in DBFileNames)
                DBCompressedFiles.Add(fileName, Movieez.Program.ResourcesPath + $"\\{fileName}");
            webClient = new WebClient();
            webClient.BaseAddress = DBUrl;
            Run();
        }

        public void Run()
        {
            GetFiles();
            ParseFiles();
        }

        public void GetFiles()
        {
            foreach (KeyValuePair<string,string> file in DBCompressedFiles)
            {
                if(!File.Exists(file.Value))
                {
                    string fileUri = DBUrl + file.Key;
                    webClient.DownloadFile(fileUri, file.Value);
                    logger.Info($"Donwloading file {fileUri}");
                }
                logger.Info($"File {file.Value} already exists");
            }
            UnzipFiles();
        }

        public void UnzipFiles()
        {
            DBDecompressedFiles = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> file in DBCompressedFiles)
            {
                string deCompressedFileName = file.Key.Remove(file.Key.LastIndexOf('.'));
                string deCompressedFilePath = file.Value.Remove(file.Value.LastIndexOf('.'));
                if (File.Exists(deCompressedFilePath))
                {
                    File.Delete(deCompressedFilePath);
                    logger.Info($"File {deCompressedFilePath} already exists. Deleting file... ");
                }
                DeCompressFile(file.Value, file.Value.Remove(file.Value.LastIndexOf('.')));
                DBDecompressedFiles.Add(deCompressedFileName, deCompressedFilePath);
            }
        }

        public void DeCompressFile(string CompressedFile, string DeCompressedFile)
        {
            byte[] buffer = new byte[1024 * 1024];

            using (System.IO.FileStream fstrmCompressedFile = System.IO.File.OpenRead(CompressedFile)) // fi.OpenRead())
            {
                using (System.IO.FileStream fstrmDecompressedFile = System.IO.File.Create(DeCompressedFile))
                {
                    using (System.IO.Compression.GZipStream strmUncompress = new System.IO.Compression.GZipStream(fstrmCompressedFile,
                            System.IO.Compression.CompressionMode.Decompress))
                    {
                        int numRead = strmUncompress.Read(buffer, 0, buffer.Length);

                        while (numRead != 0)
                        {
                            fstrmDecompressedFile.Write(buffer, 0, numRead);
                            fstrmDecompressedFile.Flush();
                            numRead = strmUncompress.Read(buffer, 0, buffer.Length);
                        } // Whend

                        numRead = 0;

                        while ((numRead = strmUncompress.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            fstrmDecompressedFile.Write(buffer, 0, numRead);
                            fstrmDecompressedFile.Flush();
                        } // Whend

                        strmUncompress.Close();
                    } // End Using System.IO.Compression.GZipStream strmUncompress 

                    fstrmDecompressedFile.Flush();
                    fstrmDecompressedFile.Close();
                } // End Using System.IO.FileStream fstrmCompressedFile 

                fstrmCompressedFile.Close();
            } // End Using System.IO.FileStream fstrmCompressedFile 

        } // End Sub DeCompressFile


        public void ParseFiles()
        {
            ParseMoviesFile();
        }
        /* */
        void ParseMoviesFile()
        {
            DataTable dt = new DataTable();
            //TextReader tr = File.OpenText(DBDecompressedFiles["title.basics.tsv"]);
            var lines = File.ReadLines(DBDecompressedFiles["title.basics.tsv"]).Reverse();
            foreach (string line in lines)
            {
                //int fufu = 0;
                string[] items = line.Split('\t');
                ReadMovieFromRow(items);
                dt.Rows.Add(items);
            }
            PrintDB(dt.Rows);
        }

        public void ReadMovieFromRow(string[] items) 
        {
            int movieIdIndex = 0;
            int primaryTitleIndex = 2;
            //int originalTitleIndex = 2;
            int yearIndex = 5;

            if (Int16.Parse(items[yearIndex]) < minMovieYear)
            {
                Objects.Rating rating = new Objects.Rating();
                rating.Id = items[movieIdIndex];
                rating.Name = items[primaryTitleIndex];
                rating.Reviewer = "IMDB";
                RatingsList.Add(rating);
            }
        }

        void ParseRatingsFile()
        {
            DataTable dt = new DataTable();
            //TextReader tr = File.OpenText(DBDecompressedFiles["title.basics.tsv"]);
            var lines = File.ReadAllLines(DBDecompressedFiles["title.ratings.tsv"]).Reverse();
            foreach(string line in lines)
            {
                //int fufu = 0;
                string[] items = line.Split('\t');
                if (dt.Columns.Count == 0)
                {
                    // Create the data columns for the data table based on the number of items
                    // on the first line of the file
                    for (int i = 0; i < items.Length; i++)
                        dt.Columns.Add(new DataColumn(items[i], typeof(string)));
                }
                else
                {
                    ReadRatingFromRow(items);
                }
                dt.Rows.Add(items);
            }
            PrintDB(dt.Rows);
        }

        public void ReadRatingFromRow(string[] items)
        {
            int movieIdIndex = 0;
            int primaryTitleIndex = 2;
            //int originalTitleIndex = 2;
            int yearIndex = 5;

            if (Int16.Parse(items[yearIndex]) < minMovieYear)
            {
                Objects.Rating rating = new Objects.Rating();
                rating.Id = items[movieIdIndex];
                rating.Name = items[primaryTitleIndex];
                rating.Reviewer = "IMDB";
                RatingsList.Add(rating);
            }
        }

        void PrintDB(DataRowCollection dt)
        {
            // Print out all the values
            foreach (DataRow dr in dt)
            {
                foreach (string s in dr.ItemArray)
                    Console.WriteLine(s + "\t");
                Console.WriteLine();
            }
        }
    }

}
