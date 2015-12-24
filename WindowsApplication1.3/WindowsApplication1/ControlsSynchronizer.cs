using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.SystemUI;
using System.Windows;

namespace WindowsApplication1
{
    public class ControlsSynchronizer
    {
        //����
        private IMapControl3 m_mapControl = null;
        private IPageLayoutControl2 m_pageLayoutControl = null;
        private ITool m_mapActiveTool = null;
        private ITool m_pageLayoutActiveTool = null;
        private bool m_IsMapCtrlactive = true;
        private ArrayList m_frameworkControls = null;

        //���캯��
        public ControlsSynchronizer()
        {
            //��ʼ��ArrayList
            m_frameworkControls = new ArrayList();
        }

        public ControlsSynchronizer(IMapControl3 mapControl, IPageLayoutControl2 pageLayoutControl)
        {
            m_frameworkControls = new ArrayList();
            this.m_mapControl = mapControl;
            this.m_pageLayoutControl = pageLayoutControl;
        }

        //ȡ�ú�����MapControl
        public IMapControl3 MapControl
        {
            get {
                return m_mapControl; 
            }
            set { 
                m_mapControl = value; 
            }
        }

        //ȡ�û�����PageLayoutControl
        public IPageLayoutControl2 PageLayoutControl
        {
            get { 
                return m_pageLayoutControl; 
            }
            set { 
                m_pageLayoutControl = value; 
            }
        }

        //ȡ�õ�ǰActiveView������
        public string ActiveViewType
        {
            get
            {
                if (m_IsMapCtrlactive)
                    return "MapControl";
                else
                    return "PageLayoutControl";
            }
        }

        //ȡ�õ�ǰ���Control
        public Control ActiveControl
        {
            get
            {
                if (null == m_mapControl || null == m_pageLayoutControl)
                    throw new Exception("ControlsSynchronizer::ActiveControl:\r\nEither MapControl or PageLayoutControl are not initialized!");
                if (m_IsMapCtrlactive)
                    return (Control)m_mapControl.Object;
                else
                    return (Control)m_pageLayoutControl.Object;
            }
        }

        //����MapControl�����the PagleLayoutControl
        public void ActivateMap()
        {
            try {
                if (null == m_mapControl || null == m_pageLayoutControl)
                    throw new Exception("ControlsSynchronizer::ActiveControl:\r\nEither MapControl or PageLayoutControl are not initialized!");
                //���浱ǰPageLayout��CurrentTool
                if (null != m_pageLayoutControl.CurrentTool)
                    m_pageLayoutActiveTool = m_pageLayoutControl.CurrentTool;

                //���PagleLayout
                m_pageLayoutControl.ActiveView.Deactivate();

                //����MapControl
                m_mapControl.ActiveView.Activate(m_mapControl.hWnd);

                //��֮ǰMapControl���ʹ�õ�tool����Ϊ���tool������MapControl��CurrentTool
                if (null != m_mapActiveTool)
                    m_mapControl.CurrentTool = m_mapActiveTool;

                m_IsMapCtrlactive = true;

                //Ϊÿһ����framework controls,����Buddy controlΪMapControl
                this.SetBuddies(m_mapControl.Object);
            }
            catch(Exception ex){
                throw new Exception(string.Format("ControlsSynchronizer::ActivateMap:\r\n{0}", ex.Message));
            }
        }

        //����PagleLayoutControl������MapCotrol
        public void ActivatePageLayout()
        {
            try{
                if (null == m_mapControl || null == m_pageLayoutControl)
                    throw new Exception("ControlsSynchronizer::ActiveControl:\r\nEither MapControl or PageLayoutControl are not initialized!");

                //���浱ǰMapControl��CurrentTool
                if (m_mapControl.CurrentTool != null) m_mapActiveTool = m_mapControl.CurrentTool;

                //���MapControl
                m_mapControl.ActiveView.Deactivate();

                //����PageLayoutControl
                m_pageLayoutControl.ActiveView.Activate(m_pageLayoutControl.hWnd);

                //��֮ǰPageLayoutControl���ʹ�õ�tool����Ϊ���tool������PageLayoutControl��CurrentTool
                if (m_pageLayoutActiveTool != null) 
                    m_pageLayoutControl.CurrentTool = m_pageLayoutActiveTool;

                m_IsMapCtrlactive = false;
                //Ϊÿһ����framework controls,����Buddy controlΪPageLayoutControl
                this.SetBuddies(m_pageLayoutControl.Object);
            }
            catch(Exception ex){
                throw new Exception(string.Format("ControlsSynchronizer::ActivatePageLayout:\r\n{0}", ex.Message));
            }
        }

        //����һ����ͼ, �û�PageLayoutControl��MapControl��focus map
        public void ReplaceMap(IMap newMap)
        {
            if (null == newMap)
                throw new Exception("ControlsSynchronizer::ReplaceMap:\r\nNew map for replacement is not initialized!");

            if (null == m_mapControl || null == m_pageLayoutControl)
                throw new Exception("ControlsSynchronizer::ActiveControl:\r\nEither MapControl or PageLayoutControl are not initialized!");

            //����һ��PageLayout��Ҫ�õ���,�µ�IMaps collection��ʵ��
            IMaps maps = new Maps();
            //���µ�Mapʵ������Maps collection
            maps.Add(newMap);

            if (m_IsMapCtrlactive)
            {
                m_mapControl.Map = newMap;
                m_mapControl.ActiveView.Refresh();
                m_pageLayoutControl.PageLayout.ReplaceMaps(maps);
            }
            else
            {
                m_pageLayoutControl.PageLayout.ReplaceMaps(maps);
                m_pageLayoutControl.ActiveView.Refresh();
                m_mapControl.Map = newMap;
            }
            //����active tools
            m_mapActiveTool = null;
            m_pageLayoutActiveTool = null;
        }

        //ָ����ͬ��Map����MapControl��PageLayoutControl����һ��
        public void BindControls(IMapControl3 mapControl, IPageLayoutControl2 pageLayoutControl, bool activateMapFirst)
        {
            if (null == m_mapControl || null == m_pageLayoutControl)
                throw new Exception("ControlsSynchronizer::ActiveControl:\r\nEither MapControl or PageLayoutControl are not initialized!");
            this.m_mapControl = mapControl;
            this.m_pageLayoutControl = pageLayoutControl;
            this.BindControls(activateMapFirst);
        }

        public void BindControls(bool activateMapFirst)
        {
            if (null == m_mapControl || null == m_pageLayoutControl)
                throw new Exception("ControlsSynchronizer::ActiveControl:\r\nEither MapControl or PageLayoutControl are not initialized!");

            //����һ��IMapʵ��
            IMap newMap = new MapClass();
            newMap.Name = "Map";

            //����һ��PageLayout��Ҫ�õ���,�µ�IMaps collection��ʵ��
            IMaps maps = new Maps();
            //���µ�Mapʵ������Maps collection
            maps.Add(newMap);

            m_pageLayoutControl.PageLayout.ReplaceMaps(maps);
            m_mapControl.Map = newMap;

            //����active tools
            m_pageLayoutActiveTool = null;
            m_mapActiveTool = null;

            //ȷ�������control������
            if (activateMapFirst)
                this.ActivateMap();
            else
                this.ActivatePageLayout();
        }

        //����FrameworkControl
        public void AddFrameworkControl(object control)
        { 
            if(null == control)
                throw new Exception("ControlsSynchronizer::AddFrameworkControl:\r\nAdded control is not initialized!");

            m_frameworkControls.Add(control);
        }

        //�Ƴ�FrameworkControl
        public void RemoveFrameworkControl(object control)
        { 
            if(null == control)
                throw new Exception("ControlsSynchronizer::RemoveFrameworkControl:\r\nControl to be removed is not initialized!");

            m_frameworkControls.Remove(control);
        }

        public void RemoveFrameworkControlAt(int index)
        { 
            if(m_frameworkControls.Count < index)
                throw new Exception("ControlsSynchronizer::RemoveFrameworkControlAt:\r\nIndex is out of range!");

            m_frameworkControls.RemoveAt(index);
        }

        private void SetBuddies(object buddy)
        {
            try{
                if (buddy == null)
                    throw new Exception("ControlsSynchronizer::SetBuddies:\r\nTarget Buddy Control is not initialized!");

                foreach (object obj in m_frameworkControls)
                {
                    if (obj is IToolbarControl)
                        ((IToolbarControl)obj).SetBuddyControl(buddy);
                    else if (obj is ITOCControl)
                        ((ITOCControl)obj).SetBuddyControl(buddy);
                }
            }
            catch (Exception ex){
                throw new Exception(string.Format("ControlsSynchronizer::SetBuddies:\r\n{0}", ex.Message));
            }
        }
    }
}
