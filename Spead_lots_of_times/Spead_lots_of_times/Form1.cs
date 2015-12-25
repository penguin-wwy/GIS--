using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.SystemUI;

namespace Spread_lots_of_times
{
    public partial class Form1 : Form
    {
        private ESRI.ArcGIS.Controls.IMapControl3 m_mapControl = null;
        private ESRI.ArcGIS.Controls.IPageLayoutControl2 m_pageLayoutControl = null;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            m_mapControl = (IMapControl3)this.axMapControl1.Object;
            m_pageLayoutControl = (IPageLayoutControl2)this.axPageLayoutControl1.Object;
        }

        private void New_MenuItem_Click(object sender, EventArgs e)
        {

        }

        private void Open_MenuItem_Click(object sender, EventArgs e)
        {

        }

        //添加数据
        private void Add_Data_MenuItem_Click(object sender, EventArgs e)
        {
            int currentLayerCount = this.axMapControl1.LayerCount;
            ICommand pCommand = new ControlsAddDataCommandClass();
            pCommand.OnCreate(this.axMapControl1.Object);
            pCommand.OnClick();
        }

        //退出
        private void Exit_MenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
