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

        private string sMapUnits = null;            /*ȫ�����굥λ����*/

        private ITOCControl2 m_tocControl = null;    /*TOCControl�ؼ�����*/
        private IToolbarMenu m_menuMap = null;      /*TOCControl��Map�˵�*/
        private IToolbarMenu m_menuLayer = null;    /*TOCControl��ͼ��˵�*/
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            m_mapControl = (IMapControl3)this.axMapControl1.Object;
            m_pageLayoutControl = (IPageLayoutControl2)this.axPageLayoutControl1.Object;

            //��ʼ��controls synchronization calss
            m_controlsSynchronizer = new ControlsSynchronizer(m_mapControl, m_pageLayoutControl);

            //��MapControl��PageLayoutControl������(������ָ��ͬһ��Map),Ȼ������MapControlΪ���Control
            m_controlsSynchronizer.BindControls(true);

            //Ϊ�����л�MapControl��PageLayoutControl��ͼͬ����Ҫ���Framework Control
            m_controlsSynchronizer.AddFrameworkControl(this.axToolbarControl1.Object);
            m_controlsSynchronizer.AddFrameworkControl(this.axTOCControl1.Object);

            // ��Ӵ����ť��������
            OpenNewMapDocument openMapDoc = new OpenNewMapDocument(m_controlsSynchronizer);
            axToolbarControl1.AddItem(openMapDoc, -1, 0, false, -1, esriCommandStyles.esriCommandStyleIconOnly);

            //ȫ�����굥λ����
            sMapUnits = "Unknown";

            m_tocControl = (ITOCControl2)this.axTOCControl1.Object;

            m_menuMap = new ToolbarMenuClass();
            m_menuLayer = new ToolbarMenuClass();

            //����Զ���˵��TOCCOntrol��Map�˵���
            m_menuMap.AddItem(new OpenNewMapDocument(m_controlsSynchronizer), -1, 0,
                false, esriCommandStyles.esriCommandStyleIconAndText);              /*���ĵ��˵�*/
            m_menuMap.AddItem(new ControlsAddDataCommandClass(), -1, 1,
                false, esriCommandStyles.esriCommandStyleIconAndText);              /*������ݲ˵�*/
            m_menuMap.AddItem(new LayerVisibility(), 1, 2,
                false, esriCommandStyles.esriCommandStyleTextOnly);                 /*��ȫ��ͼ��˵�*/
            m_menuMap.AddItem(new LayerVisibility(), 2, 3,
                false, esriCommandStyles.esriCommandStyleTextOnly);                 /*�ر�ȫ��ͼ��˵�*/
            m_menuMap.AddSubMenu("esriControls.ControlsFeatureSelectionMenu",
                4, true);                                                           /*�Զ����˵�����ʽ������õġ�ѡ�񡱲˵�*/
            m_menuMap.AddSubMenu("esriControls.ControlsMapViewMenu",
                5, true);                                                           /*�Զ����˵�����ʽ������õġ���ͼ������˵�*/

            //����Զ���˵��TOCCOntrol��ͼ��˵���
            m_menuLayer.AddItem(new RemoveLayer(), -1, 0, 
                false, esriCommandStyles.esriCommandStyleTextOnly);                 /*��ӡ��Ƴ�ͼ�㡱�˵���*/
            m_menuLayer.AddItem(new ZoomToLayer(), -1, 1,
                true, esriCommandStyles.esriCommandStyleTextOnly);                  /*��ӡ��Ŵ�����ͼ�㡱�˵���*/

            //���ò˵���Hook
            m_menuLayer.SetHook(m_mapControl);
            m_menuMap.SetHook(m_mapControl);
        }

        private void New_MenuItem_Click(object sender, EventArgs e)
        {
            if (this.axMapControl1.LayerCount > 0)
            {
                //ѯ���Ƿ񱣴浱ǰ��ͼ
                DialogResult res = MessageBox.Show("�Ƿ񱣴浱ǰ��ͼ?", "��ʾ", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (DialogResult.Yes == res)
                {
                    //���Ҫ���棬�������Ϊ�Ի���
                    ICommand command = new ControlsSaveAsDocCommandClass();
                    if (m_mapControl != null)
                        command.OnCreate(m_controlsSynchronizer.MapControl.Object);
                    else
                        command.OnCreate(m_controlsSynchronizer.PageLayoutControl.Object);
                    command.OnClick();
                }
            }
 
            //�����µĵ�ͼʵ��
            IMap map = new MapClass();
            map.Name = "Map";
            m_controlsSynchronizer.MapControl.DocumentFilename = null;
            //�����½���ͼʵ���Ĺ����ͼ�ĵ�
            m_controlsSynchronizer.ReplaceMap(map);

        }

        private void Open_MenuItem_Click(object sender, EventArgs e)
        {
            if (this.axMapControl1.LayerCount > 0)
            {
                DialogResult result = MessageBox.Show("�Ƿ񱣴浱ǰ��ͼ��", "��ʾ", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
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
            //���������AddDataʱ������ҲҪ���е�ͼ����
            IMap pMap = this.axMapControl1.Map;
            this.m_controlsSynchronizer.ReplaceMap(pMap);

        }

        private void Save_MenuItem_Click(object sender, EventArgs e)
        {
            //ȷ�ϵ�ǰ�ĵ��Ƿ�����
            if (null != m_pageLayoutControl.DocumentFilename &&
                m_mapControl.CheckMxFile(m_pageLayoutControl.DocumentFilename))
            {
                //����һ���µ��ĵ��ռ�class MapDocumentClass : MapDocument, IMapDocument, IDocumentVersion
                IMapDocument mapDoc = new MapDocumentClass();
                //�򿪵�ǰ�ĵ�
                mapDoc.Open(m_pageLayoutControl.DocumentFilename, string.Empty);
                //�滻
                mapDoc.ReplaceContents((IMxdContents)m_pageLayoutControl.PageLayout);
                //����
                mapDoc.Save(mapDoc.UsesRelativePaths, false);
                //�ر�
                mapDoc.Close();
            }
        }

        private void SaveAS_MenuItem_Click(object sender, EventArgs e)
        {
            //�����ǰ��ͼΪMapControlʱ����ʾ�û����Ϊ��������ʧPageLayoutControl�е�����
            if (m_controlsSynchronizer.ActiveControl is IMapControl3)
            {
                if (MessageBox.Show("���Ϊ��ͼ�ĵ�����ʧ�ư���ͼ������\r\n��Ҫ������?", "��ʾ", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    return;
            }
            //�������Ϊ����
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
                //����MapControl
                m_controlsSynchronizer.ActivateMap();
            }
            else
            {
                //����PageLayoutControl
                m_controlsSynchronizer.ActivatePageLayout();
            }
        }

        private void axToolbarControl1_OnMouseMove(object sender, IToolbarControlEvents_OnMouseMoveEvent e)
        {
            //ȡ��������ڹ��ߵ�������
            int index = axToolbarControl1.HitTest(e.x, e.y, false);

            if (-1 != index)
            {
                //ȡ��������ڹ��ߵ� ToolbarItem
                IToolbarItem toolbarTerm = axToolbarControl1.GetItem(index);

                //����״̬����Ϣ
                MessageLabel.Text = toolbarTerm.Command.Message;
            }
            else
            {
                MessageLabel.Text = "����";
            }

            
        }

        private void axMapControl1_OnMouseMove_1(object sender, IMapControlEvents2_OnMouseMoveEvent e)
        {
            //��ʾ��ǰ������
            ScaleLabel.Text = " ������ 1:" + ((long)this.axMapControl1.MapScale).ToString();

            // ��ʾ��ǰ����
            DinateLabel.Text = " ��ǰ���� X = " + e.mapX.ToString() + " Y = " + e.mapY.ToString() + " " + this.axMapControl1.MapUnits;

            
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

            //��ӥ������ӵ�ͼ
            //������ͼ��ʾ�ؼ��ĵ�ͼ����ʱ��ӥ���еĵ�ͼҲ�������
            this.axMapControl2.Map = new MapClass();

            //�������ͼ�ؼ��е�����ͼ�㵽ӥ�ۿؼ���
            for (int i = 0; i < this.axMapControl1.LayerCount; i++)
            {
                this.axMapControl2.AddLayer(this.axMapControl1.get_Layer(i));
            }

            //���� MapControl ��ʾ��Χ�����ݵ�ȫ�ַ�Χ
            this.axMapControl2.Extent = this.axMapControl1.FullExtent;

            //ˢ��ӥ�ۿؼ���ͼ
            this.axMapControl2.Refresh();

            this.axMapControl2.Refresh();
        }

        private void axMapControl1_OnExtentUpdated(object sender, IMapControlEvents2_OnExtentUpdatedEvent e)
        {//����ӥ�۵ľ��ο�
            //�õ��·�Χ
            IEnvelope pEnv = (IEnvelope)e.newEnvelope;
            IGraphicsContainer pGra = axMapControl2.Map as IGraphicsContainer;
            IActiveView pAv = pGra as IActiveView;

            //�ڻ���ǰ����� axMapControl2 �е��κ�ͼ��Ԫ��
            pGra.DeleteAllElements();
            IRectangleElement pRectangleEle = new RectangleElementClass();
            IElement pEle = pRectangleEle as IElement;
            pEle.Geometry = pEnv;

            //����ӥ��ͼ�еĺ��߿�
            IRgbColor pColor = new RgbColorClass();
            pColor.Red = 255;
            pColor.Green = 0;
            pColor.Blue = 0;
            pColor.Transparency = 255;

            //����һ���߷��Ŷ���
            ILineSymbol pOutline = new SimpleLineSymbolClass();
            pOutline.Width = 2;
            pOutline.Color = pColor;

            //������ɫ����
            pColor = new RgbColorClass();
            pColor.Red = 255;
            pColor.Green = 0;
            pColor.Blue = 0;
            pColor.Transparency = 0;

            //���������ŵ�����
            IFillSymbol pFillSymbol = new SimpleFillSymbolClass();
            pFillSymbol.Color = pColor;
            pFillSymbol.Outline = pOutline;
            IFillShapeElement pFillShapeEle = pEle as IFillShapeElement;
            pFillShapeEle.Symbol = pFillSymbol;
            pGra.AddElement((IElement)pFillShapeEle, 0);

            //ˢ��
            pAv.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);

        }

        private void axMapControl2_OnMouseDown(object sender, IMapControlEvents2_OnMouseDownEvent e)
        {
            if(0 != this.axMapControl2.Map.LayerCount)
            {
                //�����������ƶ����ο�
                if (1 == e.button)
                {
                    IPoint pPoint = new PointClass();
                    pPoint.PutCoords(e.mapX, e.mapY);
                    IEnvelope pEnvelope = this.axMapControl1.Extent;
                    pEnvelope.CenterAt(pPoint);
                    this.axMapControl1.Extent = pEnvelope;
                    this.axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
                }
                //��������Ҽ����ƾ��ο�
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
            //�������������¾�ֱ�ӷ���
            if (e.button != 1) 
                return;
            
            IPoint pPoint = new PointClass();
            pPoint.PutCoords(e.mapX, e.mapY);
            this.axMapControl1.CenterAt(pPoint);
            this.axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
        }

        //axTOCControl1�Ҽ���Ӧ����
        private void axTOCControl1_OnMouseDown(object sender, ITOCControlEvents_OnMouseDownEvent e)
        {
            //��������Ҽ�����ֱ�ӷ���
            if (e.button != 2) 
                return;

            esriTOCControlItem item = esriTOCControlItem.esriTOCControlItemNone;
            IBasicMap map = null;
            ILayer layer = null;
            object other = null;
            object index = null;

            //�ж���ѡ�˵�������
            m_tocControl.HitTest(e.x, e.y, ref item, ref map, ref layer, ref other, ref index);
            
            //ȷ��ѡ���Ĳ˵����ͣ�Map����ͼ��˵�
            if (item == esriTOCControlItem.esriTOCControlItemMap)
                m_tocControl.SelectItem(map, null);
            else
                m_tocControl.SelectItem(layer, null);

            //����CustomPropertyΪlayer (�����Զ����Layer����)
            m_mapControl.CustomProperty = layer;

            //�����Ҽ��˵�
            if (item == esriTOCControlItem.esriTOCControlItemMap)
                m_menuMap.PopupMenu(e.x, e.y, m_tocControl.hWnd);
            if (item == esriTOCControlItem.esriTOCControlItemLayer)
            {
                //��̬���OpenAttributeTable�˵���
                m_menuLayer.AddItem(new OpenAttributeTable(layer), -1, 2, true, esriCommandStyles.esriCommandStyleTextOnly);
                m_menuLayer.PopupMenu(e.x, e.y, m_tocControl.hWnd);
                //�Ƴ�OpenAttributeTable�˵���Է�ֹ�ظ����
                m_menuLayer.Remove(2);
            }
        }

        //����ͼ�ؼ����Ҽ���Ӧ����
        private void axMapControl1_OnMouseDown(object sender, IMapControlEvents2_OnMouseDownEvent e)
        {
            if (e.button == 2)
            {
                //�����Ҽ��˵�
                m_menuMap.PopupMenu(e.x, e.y, m_mapControl.hWnd);
            }
        }



    }
}