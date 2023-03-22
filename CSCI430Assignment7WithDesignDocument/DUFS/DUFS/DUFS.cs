using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DUFS
{
    public class DUFS
    {

        private static FileStream m_file = null;
        private static DUFS m_fs = null;
        public static Byte ETX = 0x03;
        public static Byte NULL = 0x00;


        private DUFS(String VolumeName)
        {
            m_file = new FileStream(VolumeName, FileMode.Open);
        }

        public static DUFS Mount(String VolumeName)
        {
            if (File.Exists(VolumeName))
                m_fs = new DUFS(VolumeName);
            else
            {
                Console.WriteLine("File does not exist!");
                return null;
            }
            return m_fs;
        }

        public static int TotalSize(String VolumeName)
        {
            int totaldata = 0;
            FileStream file = new FileStream(VolumeName, FileMode.Open);
            int b = file.ReadByte();

            file.Position += 451;
            while (file.Length != file.Position)
            {
                if (b == 0)
                {
                    totaldata += 1;
                }
                b = file.ReadByte();
            }
            
            file.Position = 0;
            file.Close();
            return totaldata;
        }

        public static Boolean Allocate(String VolumeName, int Size) 
        {
            Boolean allocated = true;
            int etxs = 0;

            FileStream file = new FileStream(VolumeName, FileMode.Create);
            if (Size > 5076)
            {
                Console.WriteLine("Volume cannot be more than 5076 bytes!");
            }
            else if (Size >= 1000)
            {
                for (int i = 0; i <= 449; i++)
                {
                    file.WriteByte(NULL);
                }
                Size -= 450;
                int sectors = Convert.ToInt32(Math.Ceiling(Convert.ToDouble((double)Size / 512f)));
                for (int i = 0; i < sectors; i++)
                {
                    file.WriteByte((byte)'0');
                }
                file.WriteByte(ETX);
                etxs += 1;
                Size -= (sectors + 1);
                for (int i = 0; i < sectors; i++)
                {
                    if (Size > 0 && Size >= 512)
                    {
                        for (int j = 0; j <= 511; j++)
                        {
                            file.WriteByte(NULL);
                        }
                        file.WriteByte(ETX);
                        etxs += 1;
                        Size -= 513;
                    }
                    else if (Size > 0 && Size < 512)
                    {
                        for (int j = 0; j <= Size - 1; j++)
                        {
                            file.WriteByte(NULL);
                        }
                        Size -= Size;
                    }
                    else break;
                }
            }
            else
            {
                Console.WriteLine("Volume must be bigger than 1000 bytes!");
                file.Close();
                return allocated = false;
            }

            file.Close();

            return allocated;
        }

        public static Boolean Deallocate(String VolumeName)
        {
            Boolean deallocated = true;

            if (File.Exists(VolumeName))
            {
                File.Delete(VolumeName);
                Console.WriteLine($"Volume {VolumeName} was deleted!");
            }
            else
            {
                Console.WriteLine("File does not exist!");
                return false;
            }

            return deallocated;
        }

        public static Boolean Truncate(String VolumeName)
        {

            if (File.Exists(VolumeName))
            {
                FileStream file = new FileStream(VolumeName, FileMode.Open);
                int filesize = (int)file.Length;
                file.Close();
                Deallocate(VolumeName);
                Allocate(VolumeName, filesize);
            }
            else
            {
                Console.WriteLine("File does not exist!");
                return false;
            }
            return true;
        }

        public static void Dump(String VolumeName)
        {
            if (File.Exists(VolumeName))
            {
                FileStream file = new FileStream(VolumeName, FileMode.Open);

                for (int i = 0; i < file.Length; i++)
                {
                    Char c = Convert.ToChar(file.ReadByte());
                    Console.Write("{0}", c.ToString());
                }

                file.Close();
            }
        }
        public static void Catalog()
        {
            m_file.Position = 0;
            for (int i = 0; i <= 449; i++)
            {
                if (((i % 50) == 0 || i == 0) && i != 439)
                {
                    m_file.Position = i;
                    for (int j = 0; j < 40; j++)
                    {
                        Console.Write(Convert.ToChar(m_file.ReadByte()));
                    }
                    Console.Write("\n");
                }
                else
                {
                    m_file.Position = i;
                }
            }
        }

        public Boolean isMounted
        {
            get
            {
                return (m_fs != null);
            }
        }

        public static void Info(String VolumeName)
        {
            if (File.Exists(VolumeName))
            {
                int files = 0;
                int size = TotalSize(VolumeName);
                FileStream file = new FileStream(VolumeName, FileMode.Open);

                Console.WriteLine($"The size of volume {VolumeName} is {file.Length}");

                file.Position = 450;

                for (int i = 0; i < 300; i++)
                {
                    byte b = (byte)file.ReadByte();
                    if (b == ETX)
                    {
                        break;
                    }
                    else if (b == 49)
                    {
                        files += 1;
                    }
                }
                Console.WriteLine($"The number of files in volume {VolumeName} is {files}");
                Console.WriteLine($"The amount of free space in volume {VolumeName} is {size}");
            }
            else
            {
                Console.WriteLine("File does not exist!");
            }
        }

        public Boolean Create(String FileName)
        {
            Boolean created = true;
            DateTime dtm_now = DateTime.Now;
            String dir_entry = String.Format("{0}", FileName);
            if (FileName.Length <= 40)
            {
                int location = FreeSector();
                m_file.Position = 0;
                for (int i = 0; i <= 400; i++)
                {
                    if (location == 0)
                    {
                        Console.WriteLine("File storage full!");
                        return false;
                    }
                    else
                    {
                        m_file.Position = i;
                        char c = Convert.ToChar(m_file.ReadByte());
                        if (c == Convert.ToChar(NULL))
                        {
                            m_file.Position = i;

                            for (int j = 0; j < FileName.Length; j++)
                            {
                                m_file.WriteByte((byte)FileName[j]);
                            }
                            m_file.Position += 40 - FileName.Length;
                            for (int k = 0; k < 9; k++)
                            {
                                m_file.WriteByte(48);
                            }
                            m_file.WriteByte((byte)Convert.ToChar(location.ToString()));
                            m_file.Position = 449 + location;
                            m_file.WriteByte(49);
                            return created;
                        }
                        else if (c != Convert.ToChar(NULL))
                        {
                            m_file.Position = i;
                            string match = null;
                            match += c;
                            m_file.Position += 1;
                            char t = Convert.ToChar(m_file.ReadByte());
                            while (t != Convert.ToChar(NULL))
                            {
                                match += t;
                                t = Convert.ToChar(m_file.ReadByte());
                            }
                            if (match == FileName)
                            {
                                Console.WriteLine("File already exists!");
                                return false;
                            }
                            else
                            {
                                i += 49;
                            }
                        }
                    }
                }
                Console.WriteLine("FILE CREATION ERROR");
                return false;
            }
            else
            {
                Console.WriteLine("File name length cannot be over 40 characters!");
                return false;
            }
        }

        public Boolean Delete(string FileName)
        {
            bool deleted = true;

            m_file.Position = 0;
            for (int i = 0; i <= 400; i++)
            {
                m_file.Position = i;
                char c = Convert.ToChar(m_file.ReadByte());
                if (c != Convert.ToChar(NULL))
                {
                    m_file.Position = i;
                    string match = null;
                    match += c;
                    m_file.Position += 1;
                    char t = Convert.ToChar(m_file.ReadByte());
                    for (int j = 0; j < 39; j++)
                    {
                        match += t;
                        t = Convert.ToChar(m_file.ReadByte());
                    }
                    char[] nulla = { '\0' };
                    match = match.TrimEnd(nulla);
                    if (match == FileName)
                    {
                        int etxs = 0;
                        m_file.Position -= 41;
                        for (int j = 0; j < 49; j++)
                        {
                            m_file.WriteByte(NULL);
                        }
                        int sectorlocation = (int)Char.GetNumericValue((char)m_file.ReadByte());
                        m_file.Position -= 1;
                        m_file.WriteByte(NULL);
                        int sectornumber = NumberofSectors();
                        m_file.Position = 449 + sectorlocation;
                        m_file.WriteByte(48);
                        m_file.Position = 450;
                        while (m_file.Length != m_file.Position)
                        {
                            if (etxs == sectorlocation)
                            {
                                if (sectornumber == etxs)
                                {
                                    for (int k = 0; k < (m_file.Length - 450 - ((sectornumber - 1) * 512) - sectornumber * 2); k++)
                                    {
                                        m_file.WriteByte(NULL);
                                    }
                                    break;
                                }
                                else
                                {
                                    for (int k = 0; k < 512; k++)
                                    {
                                        m_file.WriteByte(NULL);
                                    }
                                    break;
                                }
                            }
                            else
                            {
                                byte b = (byte)m_file.ReadByte();
                                if (b == ETX)
                                {
                                    etxs += 1;
                                }
                            }
                        }
                        return deleted;
                    }
                    else
                    {
                        i += 49;
                    }
                }
                else
                {
                    i += 49;
                }
            }
            Console.WriteLine("No file by such name to be deleted!");
            return false;
        }

        public Boolean Read(String FileName, String Start, String End)
        {
            Boolean read = true;
            for (int i = 0; i <= 400; i++)
            {
                m_file.Position = i;
                char c = Convert.ToChar(m_file.ReadByte());
                int start;
                int end;
                int.TryParse(Start, out start);
                int.TryParse(End, out end);
                if (start > end)
                {
                    Console.WriteLine("You cannot start after the end!");
                    return false;
                }
                if (start > 512 || end > 512)
                {
                    Console.WriteLine("Start positon cannot be bigger than file sector size!");
                    return false;
                }
                if (start < 0 || end < 0)
                {
                    Console.WriteLine("Start/end positon cannot be under 0!");
                    return false;
                }
                if (c != Convert.ToChar(NULL))
                {
                    m_file.Position = i;
                    string match = null;
                    match += c;
                    m_file.Position += 1;
                    char t = Convert.ToChar(m_file.ReadByte());
                    for (int j = 0; j < 39; j++)
                    {
                        match += t;
                        t = Convert.ToChar(m_file.ReadByte());
                    }
                    char[] nulla = { '\0' };
                    match = match.TrimEnd(nulla);
                    if (match == FileName)
                    {
                        int etxs = 0;
                        m_file.Position += 8;
                        int sectorlocation = (int)Char.GetNumericValue((char)m_file.ReadByte());
                        int sectornumber = NumberofSectors();
                        m_file.Position = 450;
                        while (m_file.Length != m_file.Position)
                        {
                            byte b = (byte)m_file.ReadByte();
                            if (etxs == sectorlocation)
                            {
                                m_file.Position -= 1;
                                m_file.Position += start;
                                string bytestring = "";
                                byte[] data = new byte[512];
                                for (int k = 0; k <= end; k++)
                                {
                                    b = (byte)m_file.ReadByte();
                                    m_file.Position -= 1;
                                    if (b == ETX)
                                    {
                                        Console.WriteLine("end of sector");
                                        break;
                                    }
                                    else
                                    {
                                        data[k] = (byte)m_file.ReadByte();
                                    }
                                }
                                int a = data.Length - 1;
                                while (data[a] == 0)
                                {
                                    a--;
                                    if (a < 0)
                                    {
                                        Console.WriteLine("No data found to read");
                                        return false;
                                    }
                                }
                                byte[] data2 = new byte[a + 1];
                                Array.Copy(data, data2, a + 1);
                                foreach (byte character in data2)
                                {
                                    bytestring += (char)character;
                                }
                                Console.WriteLine(bytestring);
                                break;
                            }
                            else if (b == ETX)
                            {
                                etxs += 1;
                            }
                        }
                        return read;
                    }
                    else
                    {
                        i += 49;
                    }
                }
                else
                {
                    i += 49;
                }
            }
            Console.WriteLine("No file by such name to read from!");
            return false;
        }

        public Boolean Write(String FileName, String Start, String Data)
        {
            bool written = true;
            int start;
            int.TryParse(Start, out start);
            if (start > 512) 
            {
                Console.WriteLine("Start positon cannot be bigger than file sector size!");
                return false;
            }
            if (start < 0)
            {
                Console.WriteLine("Start positon cannot be under 0!");
                return false;
            }
            for (int i = 0; i <= 400; i++)
            {
                m_file.Position = i;
                char c = Convert.ToChar(m_file.ReadByte());
                if (c != Convert.ToChar(NULL))
                {
                    m_file.Position = i;
                    string match = null;
                    match += c;
                    m_file.Position += 1;
                    char t = Convert.ToChar(m_file.ReadByte());
                    for (int j = 0; j < 39; j++)
                    {
                        match += t;
                        t = Convert.ToChar(m_file.ReadByte());
                    }
                    char[] nulla = { '\0' };
                    match = match.TrimEnd(nulla);
                    if (match == FileName)
                    {
                        int etxs = 0;
                        byte[] size = new byte[8];
                        string sizestring = "";
                        int sizeout;
                        for (int h = 0; h < 8; h++)
                        {
                            size[h] = (byte)m_file.ReadByte();
                        }
                        foreach (byte character in size)
                        {
                            sizestring += (char)character;
                        }
                        int.TryParse(sizestring, out sizeout);
                        sizeout += Data.Length;
                        sizestring = sizeout.ToString("00000000");
                        m_file.Position -= 8;
                        for (int h = 0; h < 8; h++)
                        {
                            m_file.WriteByte((byte)sizestring[h]);
                        }
                        int sectorlocation = (int)Char.GetNumericValue((char)m_file.ReadByte());
                        int sectornumber = NumberofSectors();
                        m_file.Position = 450;
                        byte[] data = Encoding.ASCII.GetBytes(Data);
                        while (m_file.Length != m_file.Position)
                        {
                            byte b = (byte)m_file.ReadByte();
                            if (etxs == sectorlocation)
                            {
                                m_file.Position -= 1;
                                m_file.Position += start;
                                for (int k = 0; k < data.Length; k++)
                                {
                                    b = (byte)m_file.ReadByte();
                                    m_file.Position -= 1;
                                    if (b == ETX)
                                    {
                                        Console.WriteLine("Wrote, but reach end of sector. Data might have not been written all the way");
                                        break;
                                    }
                                    else
                                    {
                                        try
                                        {
                                            if (m_file.Length != m_file.Position)
                                            {
                                                m_file.WriteByte(data[k]);
                                            }
                                            else
                                            {
                                                m_file.WriteByte(data[k]);
                                                break;
                                            }
                                        }
                                        catch (ObjectDisposedException e)
                                        {
                                            Console.WriteLine("End of volume has been reached");
                                            break;
                                        }
                                    }
                                }
                                break;
                            }
                            else if (b == ETX)
                            {
                                etxs += 1;
                            }
                        }
                        return written;
                    }
                    else
                    {
                        i += 49;
                    }
                }
                else
                {
                    i += 49;
                }
            }
            Console.WriteLine("No file by such name to write to!");
            return false;
        }

        

        public int FreeSector() //Finds first free sector in file if no sectors available return 0
        {
            m_file.Position = 450;
            char c = Convert.ToChar(m_file.ReadByte());
            int location = 0;
            while (c != (char)ETX)
            {
                if (c != char.Parse("0"))
                {
                    location += 1;
                    c = Convert.ToChar(m_file.ReadByte());
                }
                else
                {
                    location += 1;
                    m_file.Position = 0;
                    return location;
                }
            }
            m_file.Position = 0;
            return 0;
        }
        public int NumberofSectors() //returns number of sectors in the file
        {
            m_file.Position = 450;
            int sectors = 0;
            byte b = (byte)m_file.ReadByte();
            while (b != ETX)
            {
                sectors += 1;
                b = (byte)m_file.ReadByte();
            }
            m_file.Position = 0;
            return sectors;
        }
        public Boolean TruncateFile(String FileName)
        {
            bool truncated = true;
            Delete(FileName);
            Create(FileName);
            return truncated;
        }
        public void UnMount()
        {
            if (m_file != null)
            {
                m_file.Close();
                m_file.Dispose();
                m_file = null;

                m_fs = null;
            }
        }
    }
}
