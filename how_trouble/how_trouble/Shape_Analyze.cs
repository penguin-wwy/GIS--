using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using ESRI.ArcGIS.Geodatabase;
using System.Diagnostics;

namespace how_trouble
{
    public sealed class Shape_Analyze : BaseCommand
    {
        private IMapControl3 m_mapControl;

        private ILayer m_pLayer;

        public Shape_Analyze(ILayer pLayer)
        {
            base.m_caption = "shape analyze";
            m_pLayer = pLayer;
        }

        public override void OnClick()
        {
            set_Python_Argv();
        }

        public void set_Python_Argv()
        {
            IFeatureLayer pFeatureLayer = (IFeatureLayer)m_pLayer;
            //IFeature fc = (IFeatureClass)pFeatureLayer.FeatureClass;
            //IFeatureClass fc = pFeatureLayer.FeatureClass;
            //IFeatureDataset ds = fc.FeatureDataset;
            //IWorkspace ws = ds.Workspace;
            //string  path = ws.PathName;

            IDataLayer dl = (IDataLayer)m_pLayer;
            IWorkspaceName ws = ((IDatasetName)(dl.DataSourceName)).WorkspaceName;
            string path = ws.PathName;

            //设置脚本参数
            string sArguments = @"shape_analyze.py";//这里是python的文件名字
            string file_name = m_pLayer.Name;
            RunPythonScript(sArguments, path, file_name);
        }

        public static void RunPythonScript(string sArgName, string ws_name, string file_name)
        {//调用脚本
            string path = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + sArgName;// 获得python文件的绝对路径
            string sArguments = path;
            if (ws_name.Length > 0 || file_name.Length > 0)
            {
                sArguments =sArguments + " " + ws_name + " " + file_name;//传递参数
            }
            //设置进程并运行
            Process p = new Process();
            p.StartInfo.FileName = "F:\\ArcGIS10.2\\python.exe";
            p.StartInfo.Arguments = sArguments;
            p.Start();
            p.WaitForExit();
            //p.Close();
            //System.Diagnostics.Process.Start("F:\\ArcGIS10.2\\python.exe", sArguments);
        }


        public override void OnCreate(object hook)
        {
            
        }
    }
}
