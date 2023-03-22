using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DUFS
{
    public class DirectoryEntry
    {
        private String m_file_name;
        private long m_file_size;
        private Boolean m_read_only;
        private long m_create_date;
        private long m_modified_date;
        private long m_start_cluster;

        public DirectoryEntry()
        {
            m_file_name = String.Empty.PadRight(39);
            m_file_size = 0;
            m_read_only = false;
            m_create_date = DateTime.Now.ToBinary();
            m_modified_date = DateTime.Now.ToBinary();
            m_start_cluster = long.MinValue; 
        }

        public DirectoryEntry(Byte[] bytes)
        {
            String input = bytes.ToString();

            m_file_name = input.Substring(0, 39);
            m_start_cluster = input[40];
            m_file_size = long.Parse(input.Substring(41, 8));
            m_create_date = DateTime.Now.ToBinary();
            m_modified_date = DateTime.Now.ToBinary();
            m_read_only = false;
        }

        public String FileName
        {
            get { return m_file_name; }
            set { m_file_name = value; }
        }

        public long Size
        {
            get { return m_file_size; }
            set { m_file_size = value; }
        }
        public Boolean ReadOnly
        {
            get { return m_read_only; }
            set { m_read_only = value; }
        }

        public String CreateDate
        {
            get { return new DateTime(m_create_date).ToString("MM/dd/yyyy"); }
        }

        public String CreateTime
        {
            get { return new DateTime(m_create_date).ToString("HH:mm:ss"); }
        }
        public String ModifiedDate
        {
            get { return new DateTime(m_modified_date).ToString("MM/dd/yyyy"); }
        }

        public String ModifiedTime
        {
            get { return new DateTime(m_modified_date).ToString("HH:mm:ss"); }
        }

        public long StartCluster
        {
            get { return m_start_cluster; }
            set { m_start_cluster = value; }
        } 

        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;

            if (obj == null)
                return false;

            if (GetType() != obj.GetType() )
                return false;

            DirectoryEntry entry = (DirectoryEntry)obj;

            return this.FileName.Equals(entry.FileName, StringComparison.OrdinalIgnoreCase); 
        }

        public override int GetHashCode()
        {
            return m_file_name.GetHashCode(); 
        }

        public override string ToString()
        {
            return base.ToString();
        }

    }
}
