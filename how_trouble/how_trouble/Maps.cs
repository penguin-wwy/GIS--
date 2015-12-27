using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Carto;


namespace how_trouble
{
    public class Maps : IMaps, IDisposable
    {
        private ArrayList m_array = null;

        public Maps()
        {
            m_array = new ArrayList();
        }

        public void Dispose()
        {
            if (null != m_array)
            {
                m_array.Clear();
                m_array = null;
            }
        }

        public void Remove(IMap Map)
        {
            m_array.Remove(Map);
        }

        public void RemoveAt(int Index)
        {
            if (Index > m_array.Count || Index < 0)
                throw new Exception("Maps::RemoveAt:\r\nIndex is out of range!");

            m_array.RemoveAt(Index);
        }

        public void Reset()
        {
            m_array.Clear();
        }

        public int Count
        {
            get
            {
                return m_array.Count;
            }
        }

        public IMap get_Item(int Index)
        {
            if (Index > m_array.Count || Index < 0)
                throw new Exception("Maps::get_Item:\r\nIndex is out of range!");
            return m_array[Index] as IMap;
        }

        public IMap Create()
        {
            IMap newMap = new MapClass();
            m_array.Add(newMap);
            return newMap;
        }

        public void Add(IMap Map)
        {
            if (Map == null)
                throw new Exception("Maps::Add:\r\nNew Map is mot initialized!");
            m_array.Add(Map);
        }
    }
}

