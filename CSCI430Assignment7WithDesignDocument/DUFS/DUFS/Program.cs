using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DUFS
{
    public class Program
    {
        public const int COMMAND_NAME = 0;
        // ALLOCATION
        public const int CMD_ALLOC_VOLNAME = 1;
        public const int CMD_ALLOC_SIZE = 2;
        // DEALLOCATION
        public const int CMD_DEALLOC_VOLNAME = 1;
        // TRUNCATE
        public const int CMD_TRUNC_VOLNAME = 1;
        // DUMP
        public const int CMD_DUMP_VOLNAME = 1;
        // MOUNT
        public const int CMD_MOUNT_VOLNAME = 1;
        // UNMOUNT
        // NO ARGUMENTS FOR UNMOUNT
        //INFO
        public const int CMD_INFO_VOLNAME = 1;

        public static Boolean isMounted = false;
        public static String volumeName = String.Empty;
        public static String Command = String.Empty;
        static void Main(string[] args)
        {
            DUFS dufs;

            if (args.Length == 0)
            {
                Console.WriteLine("Invalid arguments");
                return;
            }

            Command = args[COMMAND_NAME];

            if (Command.Equals("ALLOCATE"))
            {
                if (args.Length != 3)
                {
                    Console.WriteLine("Invalid number of arguments for the ALLOCATE command.");
                    return;
                }

                String VolumeName = args[CMD_ALLOC_VOLNAME];
                String Size = args[CMD_ALLOC_SIZE];

                DUFS.Allocate(VolumeName, int.Parse(Size));
            }
            else if (Command.Equals("DEALLOCATE"))
            {
                if (args.Length != 2)
                {
                    Console.WriteLine("Invalid number of arguments for the DEALLOCATE command.");
                    return;
                }

                String VolumeName = args[CMD_DEALLOC_VOLNAME];
                DUFS.Deallocate(VolumeName);

            }
            else if (Command.Equals("TRUNCATE"))
            {
                if (args.Length != 2)
                {
                    Console.WriteLine("Invalid number of arguments for the TRUNCATE command.");
                    return;
                }

                String VolumeName = args[CMD_TRUNC_VOLNAME];
                DUFS.Truncate(VolumeName);
            }
            else if (Command.Equals("DUMP"))
            {
                if (args.Length != 2)
                {
                    Console.WriteLine("Invalid number of arguments for the DUMP command.");
                    return;
                }

                String VolumeName = args[CMD_DUMP_VOLNAME];
                DUFS.Dump(VolumeName);
            }
            else if (Command.Equals("INFO"))
            {
                if (args.Length != 2)
                {
                    Console.WriteLine("Invalid number of arguments for the INFO command.");
                    return;
                }

                String VolumeName = args[CMD_INFO_VOLNAME];
                DUFS.Info(VolumeName);
            }
            else if (Command.Equals("MOUNT"))
            {
                if (args.Length != 2)
                {
                    Console.WriteLine("Invalid number of arguments for the MOUNT command.");
                    return;
                }

                if (isMounted)
                {
                    Console.WriteLine("Volume '{0}' is already mounted.  Unmount it before mounting another volume.", volumeName);
                    return;
                }

                String VolumeName = args[CMD_MOUNT_VOLNAME];
                dufs = DUFS.Mount(VolumeName);

                if (dufs != null)
                {
                    String FileName = String.Empty;
                    Boolean success = false;
                    String Start = String.Empty;
                    String End = String.Empty;
                    String Data = String.Empty;
                    Console.WriteLine("Volume '{0}' is mounted.", VolumeName);
                    while (dufs.isMounted)
                    {

                        Console.Write("DUFS:>");
                        Command = Console.ReadLine();

                        if (Command.StartsWith("CREATE", StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                //CREATE FILENAME
                                Command = Command.Trim();
                                FileName = Command.Split(' ')[1];

                                success = dufs.Create(FileName);

                                if (success)
                                    Console.WriteLine("File Created!");
                                else
                                    Console.WriteLine("The File Was Not Created!");
                            }
                            catch (IndexOutOfRangeException e)
                            {
                                Console.WriteLine(e.Message);
                            }
                        }
                        else if (Command.StartsWith("EXIT", StringComparison.OrdinalIgnoreCase))
                        {
                            dufs.UnMount();
                        }
                        else if (Command.StartsWith("CATALOG", StringComparison.OrdinalIgnoreCase))
                        {
                            DUFS.Catalog();
                        }
                        else if (Command.StartsWith("DELETE", StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                //DELETE FILENAME
                                Command = Command.Trim();
                                FileName = Command.Split(' ')[1];

                                success = dufs.Delete(FileName);

                                if (success)
                                    Console.WriteLine("File Deleted!");
                                else
                                    Console.WriteLine("The File Was Not Deleted!");
                            }
                            catch (IndexOutOfRangeException e)
                            {
                                Console.WriteLine("Invalid number of arguments!");
                                Console.WriteLine(e.Message);
                            }
                        }
                        else if (Command.StartsWith("TRUNCATE", StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                //TRUNCATE FILENAME
                                Command = Command.Trim();
                                FileName = Command.Split(' ')[1];

                                success = dufs.TruncateFile(FileName);

                                if (success)
                                    Console.WriteLine("File Emptied!");
                                else
                                    Console.WriteLine("The File Was Not Emptied!");
                            }
                            catch (IndexOutOfRangeException e)
                            {
                                Console.WriteLine("Invalid number of arguments!");
                                Console.WriteLine(e.Message);
                            }
                        }
                        else if (Command.StartsWith("READ", StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                Command = Command.Trim();
                                FileName = Command.Split(' ')[1];
                                Start = Command.Split(' ')[2];
                                End = Command.Split(' ')[3];

                                success = dufs.Read(FileName, Start, End);

                                if (success)
                                    Console.WriteLine("File Read!");
                                else
                                    Console.WriteLine("The File Was Not Read!");
                            }
                            catch (IndexOutOfRangeException e)
                            {
                                Console.WriteLine("Invalid number of arguments!");
                                Console.WriteLine(e.Message);
                            }
                        }
                        else if (Command.StartsWith("WRITE", StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                Command = Command.Trim();
                                FileName = Command.Split(' ')[1];
                                Start = Command.Split(' ')[2];
                                int number;
                                if (int.TryParse(Start, out number) == true)
                                {
                                    int index = Command.IndexOf(' ');
                                    index = Command.IndexOf(' ', index + 1);
                                    index = Command.IndexOf(' ', index + 1);
                                    Data = Command.Substring(index);
                                    Data = Data.Remove(0, 1);

                                    success = dufs.Write(FileName, Start, Data);

                                    if (success)
                                        Console.WriteLine("File Written!");
                                    else
                                        Console.WriteLine("The File Was Not Written!");
                                }
                                else
                                {
                                    Console.WriteLine("Invalid number of arguments!");
                                    Console.WriteLine("Start argument must be a number!");
                                }
                            }
                            catch (IndexOutOfRangeException e)
                            {
                                Console.WriteLine(e.Message);
                            }
                        }
                    }
                }

            }

        }
    }
}
