using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.SystemUI;

namespace Spread_lots_of_times
{
    public class ControlsSynchronizer
    {
        //属性
        private IMapControl3 m_mapControl = null;
        private IPageLayoutControl2 m_pageLayoutControl = null;
        private ITool m_mapActiveTool = null;
        private ITool m_pageLayoutActiveTool = null;
        private bool m_IsMapCtrlactive = true;
        private ArrayList m_frameworkControls = null;

        //构造函数
        public ControlsSynchronizer()
        {
            //初始化ArrayList
            m_frameworkControls = new ArrayList();
        }

        public ControlsSynchronizer(IMapControl3 mapControl, IPageLayoutControl2 pageLayoutControl)
        {
            m_frameworkControls = new ArrayList();
            this.m_mapControl = mapControl;
            this.m_pageLayoutControl = pageLayoutControl;
        }

        //取得和设置MapControl
        public IMapControl3 MapControl
        {
            get
            {
                return m_mapControl;
            }
            set
            {
                m_mapControl = value;
            }
        }

        //取得或设置PageLayoutControl
        public IPageLayoutControl2 PageLayoutControl
        {
            get
            {
                return m_pageLayoutControl;
            }
            set
            {
                m_pageLayoutControl = value;
            }
        }

        //取得当前ActiveView的类型
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

        //取得当前活动的Control
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

        //激活MapControl并解除the PagleLayoutControl
        public void ActivateMap()
        {
            try
            {
                if (null == m_mapControl || null == m_pageLayoutControl)
                    throw new Exception("ControlsSynchronizer::ActiveControl:\r\nEither MapControl or PageLayoutControl are not initialized!");
                //缓存当前PageLayout的CurrentTool
                if (null != m_pageLayoutControl.CurrentTool)
                    m_pageLayoutActiveTool = m_pageLayoutControl.CurrentTool;

                //解除PagleLayout
                m_pageLayoutControl.ActiveView.Deactivate();

                //激活MapControl
                m_mapControl.ActiveView.Activate(m_mapControl.hWnd);

                //将之前MapControl最后使用的tool，作为活动的tool，赋给MapControl的CurrentTool
                if (null != m_mapActiveTool)
                    m_mapControl.CurrentTool = m_mapActiveTool;

                m_IsMapCtrlactive = true;

                //为每一个的framework controls,设置Buddy control为MapControl
                this.SetBuddies(m_mapControl.Object);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("ControlsSynchronizer::ActivateMap:\r\n{0}", ex.Message));
            }
        }

        //激活PagleLayoutControl并减活MapCotrol
        public void ActivatePageLayout()
        {
            try
            {
                if (null == m_mapControl || null == m_pageLayoutControl)
                    throw new Exception("ControlsSynchronizer::ActiveControl:\r\nEither MapControl or PageLayoutControl are not initialized!");

                //缓存当前MapControl的CurrentTool
                if (m_mapControl.CurrentTool != null) m_mapActiveTool = m_mapControl.CurrentTool;

                //解除MapControl
                m_mapControl.ActiveView.Deactivate();

                //激活PageLayoutControl
                m_pageLayoutControl.ActiveView.Activate(m_pageLayoutControl.hWnd);

                //将之前PageLayoutControl最后使用的tool，作为活动的tool，赋给PageLayoutControl的CurrentTool
                if (m_pageLayoutActiveTool != null)
                    m_pageLayoutControl.CurrentTool = m_pageLayoutActiveTool;

                m_IsMapCtrlactive = false;
                //为每一个的framework controls,设置Buddy control为PageLayoutControl
                this.SetBuddies(m_pageLayoutControl.Object);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("ControlsSynchronizer::ActivatePageLayout:\r\n{0}", ex.Message));
            }
        }

        //给予一个地图, 置换PageLayoutControl和MapControl的focus map
        public void ReplaceMap(IMap newMap)
        {
            if (null == newMap)
                throw new Exception("ControlsSynchronizer::ReplaceMap:\r\nNew map for replacement is not initialized!");

            if (null == m_mapControl || null == m_pageLayoutControl)
                throw new Exception("ControlsSynchronizer::ActiveControl:\r\nEither MapControl or PageLayoutControl are not initialized!");

            //创建一个PageLayout需要用到的,新的IMaps collection的实例
            IMaps maps = new Maps();
            //把新的Map实例赋给Maps collection
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
            //重设active tools
            m_mapActiveTool = null;
            m_pageLayoutActiveTool = null;
        }

        //指定共同的Map来把MapControl和PageLayoutControl绑在一起
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

            //创造一个IMap实例
            IMap newMap = new MapClass();
            newMap.Name = "Map";

            //创建一个PageLayout需要用到的,新的IMaps collection的实例
            IMaps maps = new Maps();
            //把新的Map实例赋给Maps collection
            maps.Add(newMap);

            m_pageLayoutControl.PageLayout.ReplaceMaps(maps);
            m_mapControl.Map = newMap;

            //重设active tools
            m_pageLayoutActiveTool = null;
            m_mapActiveTool = null;

            //确定最后活动的control被激活
            if (activateMapFirst)
                this.ActivateMap();
            else
                this.ActivatePageLayout();
        }

        //增加FrameworkControl
        public void AddFrameworkControl(object control)
        {
            if (null == control)
                throw new Exception("ControlsSynchronizer::AddFrameworkControl:\r\nAdded control is not initialized!");

            m_frameworkControls.Add(control);
        }

        //移出FrameworkControl
        public void RemoveFrameworkControl(object control)
        {
            if (null == control)
                throw new Exception("ControlsSynchronizer::RemoveFrameworkControl:\r\nControl to be removed is not initialized!");

            m_frameworkControls.Remove(control);
        }

        public void RemoveFrameworkControlAt(int index)
        {
            if (m_frameworkControls.Count < index)
                throw new Exception("ControlsSynchronizer::RemoveFrameworkControlAt:\r\nIndex is out of range!");

            m_frameworkControls.RemoveAt(index);
        }

        private void SetBuddies(object buddy)
        {
            try
            {
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
            catch (Exception ex)
            {
                throw new Exception(string.Format("ControlsSynchronizer::SetBuddies:\r\n{0}", ex.Message));
            }
        }
    }
}
