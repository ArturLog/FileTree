
using System;
using System.IO;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

/// <summary>
/// Program shows tree of files and directories with their DOS attibutes (rahs), starting in path given as argument
/// </summary>
public class Program
{
    private static int numbersOfTab = 0;
    public static void Main(string[] args)
    {
        if (args.Length == 0) return;

        foreach(string path in args)
        {
            if (File.Exists(path))
            {
                //path to directory
                ProcessFile(path);
            }
            else if (Directory.Exists(path))
            {
                //path to directory
                ProcessDirectory(path);
            }
            else
            {
                Console.WriteLine("{0} is not a valid file or directory.");
                System.Environment.Exit(1);
            }

            //oldest file
            DirectoryInfo dir = new DirectoryInfo(path);
            dir.TheOldestEntity();
            dir.WriteOldest();

            //kolekcje
            SortedList<string, long>  SList = createDirectRootElements(path);
            
            Serialize(SList);
            Deserialize();
        }
    }
    /// <summary>
    /// Processing directory to find and print all elements and their attirbutes, if element is a folder,
    /// recursive getting inside folder.
    /// </summary>
    /// <param name="directoryPath">The path of directory (string)</param>
    public static void ProcessDirectory(string directoryPath)
    {
        //set tables of files and directories
        string[] fileEntries = Directory.GetFiles(directoryPath);
        string[] subdirectoryEntries = Directory.GetDirectories(directoryPath);
        int numberOfEntity = fileEntries.Length + subdirectoryEntries.Length;
        FileSystemInfo fileSI = new FileInfo(directoryPath);

        // print folder which variable directoryPath indicates
        WriteTab();
        Console.WriteLine(Path.GetFileNameWithoutExtension(directoryPath) + " (" + numberOfEntity + ")" + fileSI.GetRahs());
        numbersOfTab++;

        // processing files
        foreach (string fileName in fileEntries)
            ProcessFile(fileName);

        // processing directories
        foreach (string subdirectoryName in subdirectoryEntries)
            ProcessDirectory(subdirectoryName);

        numbersOfTab--;
    }

    /// <summary>
    /// Taking information about file (length, attributes) and printing it
    /// </summary>
    /// <param name="fileName">The path of file (string)</param>
    public static void ProcessFile(string fileName)
    {
        WriteTab();
        FileSystemInfo fileSI = new FileInfo(fileName);
        FileInfo fileF = new FileInfo(fileName);
        Console.WriteLine(Path.GetFileName(fileName) + " " + fileF.Length + " bajtow" + fileSI.GetRahs());
    }

    /// <summary>
    /// Print tabulators to make tree looks clear
    /// </summary>
    public static void WriteTab()
    {
        for (int i = 0; i < numbersOfTab; i++) Console.Write("\t");
    }

    /// <summary>
    ///     /// <summary>
    /// Find, create collection and print direct root elemement in face key -> value
    /// key - name
    /// value - size in bytes
    /// </summary>
    /// <param name="directoryPath">The path of directory (string)</param>
    /// <returns>Sorted list of parameters <key(string), value(long)></returns>
    public static SortedList<string, long> createDirectRootElements(string directoryPath)
    {
        Comparer a = new Comparer();
        SortedList<string, long> SList = new SortedList<string, long>(a);

        string[] fileEntries = Directory.GetFiles(directoryPath);
        foreach (string fileName in fileEntries)
        {
            FileInfo fileF = new FileInfo(fileName);
            SList.Add(Path.GetFileName(fileName), fileF.Length);
        }

        string[] subdirectoryEntries = Directory.GetDirectories(directoryPath);
        foreach (string subdirectoryName in subdirectoryEntries)
        {
            SList.Add(Path.GetFileNameWithoutExtension(subdirectoryName), getLengthOfDirectory(subdirectoryName));
        }

        WriteCollection(SList);

        return SList;
    }

    /// <summary>
    /// Getting length of directory
    /// </summary>
    /// <param name="directoryPath">Directory path (string)</param>
    /// <returns>Lenght of directory (long)</returns>
    public static long getLengthOfDirectory(string directoryPath)
    {
        string[] fileEntries = Directory.GetFiles(directoryPath);
        string[] subdirectoryEntries = Directory.GetDirectories(directoryPath);
        return fileEntries.Length + subdirectoryEntries.Length;
    }


    /// <summary>
    /// Serializing sorted list in DataFile.dat
    /// </summary>
    /// <param name="SList">Sorted list of parameters <key(string), value(long)></param>
    public static void Serialize(SortedList<string, long> SList)
    {
        FileStream fs = new FileStream("DataFile.dat", FileMode.Create);

        BinaryFormatter formatter = new BinaryFormatter();
        try
        {
            formatter.Serialize(fs, SList);
        }
        catch (SerializationException e)
        {
            Console.WriteLine("Failed to serialize. Reason: " + e.Message);
            throw;
        }
        finally
        {
            fs.Close();
        }
    }

    /// <summary>
    /// Deserialize specific variable - Sorted list of parameters <key(string), value(long)>
    /// and printing list 
    /// </summary>
    public static void Deserialize()
    {

        SortedList<string, long> SList = null;
        // open the file containing the data that you want to deserialize
        FileStream fs = new FileStream("DataFile.dat", FileMode.Open);
        try
        {
            BinaryFormatter formatter = new BinaryFormatter();

            // deserialize the hashtable from the file and
            // assign the reference to the local variable
            SList = (SortedList<string, long>)formatter.Deserialize(fs);
        }
        catch (SerializationException e)
        {
            Console.WriteLine("Failed to deserialize. Reason: " + e.Message);
            throw;
        }
        finally
        {
            fs.Close();
        }
        WriteCollection(SList);
    }

    /// <summary>
    /// Write collection in specific way key -> value
    /// key - name
    /// value - size in bytes
    /// </summary>
    /// <param name="SList">Sorted list of parameters <key(string), value(long)></param>
    public static void WriteCollection(SortedList<string, long> SList)
    {
        Console.WriteLine("\n");
        foreach (KeyValuePair<string, long> sl in SList)
        {
            Console.WriteLine("{0} -> {1}", sl.Key, sl.Value);
        }
    }
}

/// <summary>
/// Class to compare
/// </summary>
[Serializable]
public class Comparer : IComparer<string>
{
    /// <summary>
    /// Comparing method
    /// </summary>
    /// <param name="x">First value</param>
    /// <param name="y">Second value</param>
    /// <returns>0 - x == y, 1 when x contain y, -1 when x not contain y</returns>
    int IComparer<string>.Compare(string x, string y)
    {
        if (x == y) return 0;
        if (x.Contains(y)) return 1;
        else return -1;

    }
}

/// <summary>
/// Extension to FileSystemInfo to getting rahs
/// </summary>
public static class FileSystemInfoExtensions
{
   /// <summary>
   /// Get rahs - dos attributes Read only, Arcihve, Hidden, System from FileSystemInfo
   /// </summary>
   /// <param name="fsi">System information about file</param>
   /// <returns>String of rahs information, 
   /// example:" ra-h"</returns>
    public static string GetRahs(this FileSystemInfo fsi)
    {
        string rahs = fsi.Attributes.ToString();
        string dos = " ";
        if (System.Text.RegularExpressions.Regex.IsMatch(rahs, "Read Only", System.Text.RegularExpressions.RegexOptions.IgnoreCase)) dos += "r";
        else dos += "-";
        if (System.Text.RegularExpressions.Regex.IsMatch(rahs, "Archive", System.Text.RegularExpressions.RegexOptions.IgnoreCase)) dos += "a";
        else dos += "-";
        if (System.Text.RegularExpressions.Regex.IsMatch(rahs, "Hidden", System.Text.RegularExpressions.RegexOptions.IgnoreCase)) dos += "h";
        else dos += "-";
        if (System.Text.RegularExpressions.Regex.IsMatch(rahs, "System", System.Text.RegularExpressions.RegexOptions.IgnoreCase)) dos += "s";
        else dos += "-";

        return dos;
    }
}

/// <summary>
/// Extension to DirectoryInfo to find the oldest file
/// </summary>
public static class DirectoryInfoExtensions
{
    public static DateTime oldestDirectoryCreationTime = DateTime.Now;
    /// <summary>
    /// Function to search the odldest file
    /// </summary>
    /// <param name="entity">Not to fill, just to use method and variables of class</param>
    public static void TheOldestEntity(this DirectoryInfo entity)
    {
        if (entity.CreationTime < oldestDirectoryCreationTime) oldestDirectoryCreationTime = entity.CreationTime;

        //search files and check date
        string[] fileEntries = Directory.GetFiles(entity.FullName);
        foreach (string fileName in fileEntries)
        {
            DirectoryInfo tmp = new DirectoryInfo(fileName);
            if (tmp.CreationTime < oldestDirectoryCreationTime) oldestDirectoryCreationTime = tmp.CreationTime;
        }

        // search subdirectories 
        string[] subdirectoryEntries = Directory.GetDirectories(entity.FullName);
        foreach (string subdirectoryName in subdirectoryEntries)
            TheOldestEntity(new DirectoryInfo(subdirectoryName));

    }

    /// <summary>
    /// Print the date of the oldest file
    /// </summary>
    /// <param name="entity"></param>
    public static void WriteOldest(this DirectoryInfo entity)
    {
        Console.WriteLine("Najstarszy plik: " + oldestDirectoryCreationTime);
    }
}