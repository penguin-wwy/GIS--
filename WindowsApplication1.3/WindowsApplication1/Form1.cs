using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.SystemUI;

namespace WindowsApplication1
{
    public partial class Form1 : Form
    {
        private ControlsSynchronizer m_controlsSynchronizer = null;
        private ESRI.ArcGIS.Controls.IMapControl3 m_mapControl = null;
        private ESRI.ArcGIS.Controls.IPageLayoutControl2 m_pageLayoutControl = null;

        private IMapDocument pMapDocument;

        private string sMapUnits = null;            /*全局坐标单位变量*/

        private ITOCControl2 m_tocControl = null;    /*TOCControl控件变量*/
        private IToolbarMenu m_menuMap = null;      /*TOCControl中Map菜单*/
        private IToolbarMenu m_menuLayer = null;    /*TOCControl中图层菜单*/
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            m_mapControl = (IMapControl3)this.axMapControl1.Object;
            m_pageLayoutControl = (IPageLayoutControl2)this.axPageLayoutControl1.Object;

            //初始化controls synchronization calss
            m_controlsSynchronizer = new ControlsSynchronizer(m_mapControl, m_pageLayoutControl);

            //把MapControl和PageLayoutControl绑定起来(两个都指向同一个Map),然后设置MapControl为活动的Control
            m_controlsSynchronizer.BindControls(true);

            //为了在切换MapControl和PageLayoutControl视图同步，要添加Framework Control
            m_controlsSynchronizer.AddFrameworkControl(this.axToolbarControl1.Object);
            m_controlsSynchronizer.AddFrameworkControl(this.axTOCControl1.Object);

            // 添加打开命令按钮到工具条
            OpenNewMapDocument openMapDoc = new OpenNewMapDocument(m_controlsSynchronizer);
            axToolbarControl1.AddItem(openMapDoc, -1, 0, false, -1, esriCommandStyles.esriCommandStyleIconOnly);

            //全局坐标单位变量
            sMapUnits = "Unknown";

            m_tocControl = (ITOCControl2)this.axTOCControl1.Object;

            m_menuMap = new ToolbarMenuClass();
            m_menuLayer = new ToolbarMenuClass();

            //添加自定义菜单项到TOCCOntrol的Map菜单中
            m_menuMap.AddItem(new OpenNewMapDocument(m_controlsSynchronizer), -1, 0,
                false, esriCommandStyles.esriCommandStyleIconAndText);              /*打开文档菜单*/
            m_menuMap.AddItem(new ControlsAddDataCommandClass(), -1, 1,
                false, esriCommandStyles.esriCommandStyleIconAndText);              /*添加数据菜单*/
            m_menuMap.AddItem(new LayerVisibility(), 1, 2,
                false, esriCommandStyles.esriCommandStyleTextOnly);                 /*打开全部图层菜单*/
            m_menuMap.AddItem(new LayerVisibility(), 2, 3,
                false, esriCommandStyles.esriCommandStyleTextOnly);                 /*关闭全部图层菜单*/
            m_menuMap.AddSubMenu("esriControls.ControlsFeatureSelectionMenu",
                4, true);                                                           /*以二级菜单的形式添加内置的“选择”菜单*/
            m_menuMap.AddSubMenu("esriControls.ControlsMapViewMenu",
                5, true);                                                           /*以二级菜单的形式添加内置的“地图浏览”菜单*/

            //添加自定义菜单项到TOCCOntrol的图层菜单中
            m_menuLayer.AddItem(new RemoveLayer(), -1, 0, 
                false, esriCommandStyles.esriCommandStyleTextOnly);                 /*添加“移除图层”菜单项*/
            m_menuLayer.AddItem(new ZoomToLayer(), -1, 1,
                true, esriCommandStyles.esriCommandStyleTextOnly);                  /*添加“放大到整个图层”菜单项*/

            //设置菜单的Hook
            m_menuLayer.SetHook(m_mapControl);
            m_menuMap.SetHook(m_mapControl);
        }

        private void New_MenuItem_Click(object sender, EventArgs e)
        {
            if (this.axMapControl1.LayerCount > 0)
            {
                //询问是否保存当前地图
                DialogResult res = MessageBox.Show("是否保存当前地图?", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (DialogResult.Yes == res)
                {
                    //如果要保存，调用另存为对话框
                    ICommand command = new ControlsSaveAsDocCommandClass();
                    if (m_mapControl != null)
                        command.OnCreate(m_controlsSynchronizer.MapControl.Object);
                    else
                        command.OnCreate(m_controlsSynchronizer.PageLayoutControl.Object);
                    command.OnClick();
                }
            }
 
            //创建新的地图实例
            IMap map = new MapClass();
            map.Name = "Map";
            m_controlsSynchronizer.MapControl.DocumentFilename = null;
            //更新新建地图实例的共享地图文档
            m_controlsSynchronizer.ReplaceMap(map);

        }

        private void Open_MenuItem_Click(object sender, EventArgs e)
        {
            if (this.axMapControl1.LayerCount > 0)
            {
                DialogResult result = MessageBox.Show("是否保存当前地图？", "提示", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == DialogResult.Cancel) 
                    return;
                if (result == DialogResult.Yes)
                    this.Save_MenuItem_Click(null, null);
            }
            OpenNewMapDocument openMapDoc = new OpenNewMapDocument(m_controlsSynchronizer);
            openMapDoc.OnCreate(m_controlsSynchronizer.MapControl.Object);
            openMapDoc.OnClick();

        }

        private void AddData_MenuItem_Click(object sender, EventArgs e)
        {
            int currentLayerCount = this.axMapControl1.LayerCount;
            ICommand pCommand = new ControlsAddDataCommandClass();
            pCommand.OnCreate(this.axMapControl1.Object);
            pCommand.OnClick();
            //在添加数据AddData时，我们也要进行地图共享
            IMap pMap = this.axMapControl1.Map;
            this.m_controlsSynchronizer.ReplaceMap(pMap);

        }

        private void Save_MenuItem_Click(object sender, EventArgs e)
        {
            //确认当前文档是否有用
            if (null != m_pageLayoutControl.DocumentFilename &&
                m_mapControl.CheckMxFile(m_pageLayoutControl.DocumentFilename))
            {
                //创建一个新的文档空间class MapDocumentClass : MapDocument, IMapDocument, IDocumentVersion
                IMapDocument mapDoc = new MapDocumentClass();
                //打开当前文档
                mapDoc.Open(m_pageLayoutControl.DocumentFilename, string.Empty);
                //替换
                mapDoc.ReplaceContents((IMxdContents)m_pageLayoutControl.PageLayout);
                //保存
                mapDoc.Save(mapDoc.UsesRelativePaths, false);
                //关闭
                mapDoc.Close();
            }
        }

        private void SaveAS_MenuItem_Click(object sender, EventArgs e)
        {
            //如果当前视图为MapControl时，提示用户另存为操作将丢失PageLayoutControl中的设置
            if (m_controlsSynchronizer.ActiveControl is IMapControl3)
            {
                if (MessageBox.Show("另存为地图文档将丢失制版视图的设置\r\n您要继续吗?", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    return;
            }
            //调用另存为命令
            ICommand command = new ControlsSaveAsDocCommandClass();
            command.OnCreate(m_controlsSynchronizer.ActiveControl);
            command.OnClick();
        }

        private void Exit_MenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void tabControl2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (0 == this.tabControl2.SelectedIndex)
            {
                //激活MapControl
                m_controlsSynchronizer.ActivateMap();
            }
            else
            {
                //激活PageLayoutControl
                m_controlsSynchronizer.ActivatePageLayout();
            }
        }

        private void axToolbarControl1_OnMouseMove(object sender, IToolbarControlEvents_OnMouseMoveEvent e)
        {
            //取得鼠标所在工具的索引号
            int index = axToolbarControl1.HitTest(e.x, e.y, false);

            if (-1 != index)
            {
                //取得鼠标所在工具的 ToolbarItem
                IToolbarItem toolbarTerm = axToolbarControl1.GetItem(index);

                //设置状态栏信息
                MessageLabel.Text = toolbarTerm.Command.Message;
            }
            else
            {
                MessageLabel.Text = "就绪";
            }

            
        }

        private void axMapControl1_OnMouseMove_1(object sender, IMapControlEvents2_OnMouseMoveEvent e)
        {
            //显示当前比例尺
            ScaleLabel.Text = " 比例尺 1:" + ((long)this.axMapControl1.MapScale).ToString();

            // 显示当前坐标
            DinateLabel.Text = " 当前坐标 X = " + e.mapX.ToString() + " Y = " + e.mapY.ToString() + " " + this.axMapControl1.MapUnits;

            
        }

        private void axMapControl1_OnMapReplaced(object sender, IMapControlEvents2_OnMapReplacedEvent e)
        {
            esriUnits mapUnits = axMapControl1.MapUnits;
            switch (mapUnits)
            {
                case esriUnits.esriCentimeters:
                    sMapUnits = "Centimeters";
                    break;
                case esriUnits.esriDecimalDegrees:
                    sMapUnits = "Decimal Degrees";
                    break;
                case esriUnits.esriDecimeters:
                    sMapUnits = "Decimeters";
                    break;
                case esriUnits.esriFeet:
                    sMapUnits = "Feet";
                    break;
                case esriUnits.esriInches:
                    sMapUnits = "Inches";
                    break;
                case esriUnits.esriKilometers:
                    sMapUnits = "Kilometers";
                    break;
                case esriUnits.esriMeters:
                    sMapUnits = "Meters";
                    break;
                case esriUnits.esriMiles:
                    sMapUnits = "Miles";
                    break;
                case esriUnits.esriMillimeters:
                    sMapUnits = "Millimeters";
                    break;
                case esriUnits.esriNauticalMiles:
                    sMapUnits = "NauticalMiles";
                    break;
                case esriUnits.esriPoints:
                    sMapUnits = "Points";
                    break;
                case esriUnits.esriUnknownUnits:
                    sMapUnits = "Unknown";
                    break;
                case esriUnits.esriYards:
                    sMapUnits = "Yards";
                    break;
            }

            //在鹰眼中添加地图
            //当主地图显示控件的地图更换时，鹰眼中的地图也跟随更换
            this.axMapControl2.Map = new MapClass();

            //添加主地图控件中的所有图层到鹰眼控件中
            for (int i = 0; i < this.axMapControl1.LayerCount; i++)
            {
                this.axMapControl2.AddLayer(this.axMapControl1.get_Layer(i));
            }

            //设置 MapControl 显示范围至数据的全局范围
            this.axMapControl2.Extent = this.axMapControl1.FullExtent;

            //刷新鹰眼控件地图
            this.axMapControl2.Refresh();

            this.axMapControl2.Refresh();
        }

        private void axMapControl1_OnExtentUpdated(object sender, IMapControlEvents2_OnExtentUpdatedEvent e)
        {//绘制鹰眼的矩形框
            //得到新范围
            IEnvelope pEnv = (IEnvelope)e.newEnvelope;
            IGraphicsContainer pGra = axMapControl2.Map as IGraphicsContainer;
            IActiveView pAv = pGra as IActiveView;

            //在绘制前，清除 axMapControl2 中的任何图形元素
            pGra.DeleteAllElements();
            IRectangleElement pRectangleEle = new RectangleElementClass();
            IElement pEle = pRectangleEle as IElement;
            pEle.Geometry = pEnv;

            //设置鹰眼图中的红线框
            IRgbColor pColor = new RgbColorClass();
            pColor.Red = 255;
            pColor.Green = 0;
            pColor.Blue = 0;
            pColor.Transparency = 255;

            //产生一个线符号对象
            ILineSymbol pOutline = new SimpleLineSymbolClass();
            pOutline.Width = 2;
            pOutline.Color = pColor;

            //设置颜色属性
            pColor = new RgbColorClass();
            pColor.Red = 255;
            pColor.Green = 0;
            pColor.Blue = 0;
            pColor.Transparency = 0;

            //设置填充符号的属性
            IFillSymbol pFillSymbol = new SimpleFillSymbolClass();
            pFillSymbol.Color = pColor;
            pFillSymbol.Outline = pOutline;
            IFillShapeElement pFillShapeEle = pEle as IFillShapeElement;
            pFillShapeEle.Symbol = pFillSymbol;
            pGra.AddElement((IElement)pFillShapeEle, 0);

            //刷新
            pAv.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);

        }

        private void axMapControl2_OnMouseDown(object sender, IMapControlEvents2_OnMouseDownEvent e)
        {
            if(0 != this.axMapControl2.Map.LayerCount)
            {
                //按下鼠标左键移动矩形框
                if (1 == e.button)
                {
                    IPoint pPoint = new PointClass();
                    pPoint.PutCoords(e.mapX, e.mapY);
                    IEnvelope pEnvelope = this.axMapControl1.Extent;
                    pEnvelope.CenterAt(pPoint);
                    this.axMapControl1.Extent = pEnvelope;
                    this.axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
                }
                //按下鼠标右键绘制矩形框
                else if (2 == e.button)
                {
                    IEnvelope pEnvelop = this.axMapControl2.TrackRectangle();
                    this.axMapControl1.Extent = pEnvelop;
                    this.axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
                }
            }
        }

        private void axMapControl2_OnMouseMove(object sender, IMapControlEvents2_OnMouseMoveEvent e)
        {
            //如果不是左键按下就直接返回
            if (e.button != 1) 
                return;
            
            IPoint pPoint = new PointClass();
            pPoint.PutCoords(e.mapX, e.mapY);
            this.axMapControl1.CenterAt(pPoint);
            this.axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
        }

        //axTOCControl1右键响应函数
        private void axTOCControl1_OnMouseDown(object sender, ITOCControlEvents_OnMouseDownEvent e)
        {
            //如果不是右键按下直接返回
            if (e.button != 2) 
                return;

            esriTOCControlItem item = esriTOCControlItem.esriTOCControlItemNone;
            IBasicMap map = null;
            ILayer layer = null;
            object other = null;
            object index = null;

            //判断所选菜单的类型
            m_tocControl.HitTest(e.x, e.y, ref item, ref map, ref layer, ref other, ref index);
            
            //确定选定的菜单类型，Map或是图层菜单
            if (item == esriTOCControlItem.esriTOCControlItemMap)
                m_tocControl.SelectItem(map, null);
            else
                m_tocControl.SelectItem(layer, null);

            //设置CustomProperty为layer (用于自定义的Layer命令)
            m_mapControl.CustomProperty = layer;

            //弹出右键菜单
            if (item == esriTOCControlItem.esriTOCControlItemMap)
                m_menuMap.PopupMenu(e.x, e.y, m_tocControl.hWnd);
            if (item == esriTOCControlItem.esriTOCControlItemLayer)
            {
                //动态添加OpenAttributeTable菜单项
                m_menuLayer.AddItem(new OpenAttributeTable(layer), -1, 2, true, esriCommandStyles.esriCommandStyleTextOnly);
                m_menuLayer.PopupMenu(e.x, e.y, m_tocControl.hWnd);
                //移除OpenAttributeTable菜单项，以防止重复添加
                m_menuLayer.Remove(2);
            }
        }

        //主地图控件的右键响应函数
        private void axMapControl1_OnMouseDown(object sender, IMapControlEvents2_OnMouseDownEvent e)
        {
            if (e.button == 2)
            {
                //弹出右键菜单
                m_menuMap.PopupMenu(e.x, e.y, m_mapControl.hWnd);
            }
        }



    }
}