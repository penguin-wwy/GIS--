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
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;

namespace WindowsApplication1
{
    public partial class AttributeTableFrm : Form
    {

        public DataTable attributeTable;

        public AttributeTableFrm()
        {
            InitializeComponent();
        }

        private static DataTable CreaterDataTableByLayer(ILayer pLayer, string tableName)
        {
            //����һ��DataTable��
            DataTable pDataTable = new DataTable(tableName);

            //ȡ��ITable�ӿ�
            ITable pTable = pLayer as ITable;
            IField pField = null;
            DataColumn pDataColumn;

            //����ÿ���ֶε����Խ���DataColumn����
            for (int i = 0; i < pTable.Fields.FieldCount; i++)
            {
                pField = pTable.Fields.get_Field(i);
                //�½�һ��DataColumn������������
                pDataColumn = new DataColumn(pField.Name);
                if (pField.Name == pTable.OIDFieldName)
                {
                    pDataColumn.Unique = true;//�ֶ�ֵ�Ƿ�Ψһ
                }

                //�ֶ�ֵ�Ƿ�����Ϊ��
                pDataColumn.AllowDBNull = pField.IsNullable;

                //�ֶα���
                pDataColumn.Caption = pField.AliasName;

                //�ֶ���������
                pDataColumn.DataType = System.Type.GetType(ParseFieldType(pField.Type));

                //�ֶ�Ĭ��ֵ
                pDataColumn.DefaultValue = pField.DefaultValue;

                //���ֶ�ΪString�����������ֶγ���
                if (pField.VarType == 8)
                {
                    pDataColumn.MaxLength = pField.Length;
                }

                //�ֶ���ӵ�����
                pDataTable.Columns.Add(pDataColumn);
                pField = null;
                pDataColumn = null;
            }

            return pDataTable;
        }

        public static string ParseFieldType(esriFieldType fieldType)
        {
            switch (fieldType)
            {
                case esriFieldType.esriFieldTypeBlob:
                    return "System.String";
                case esriFieldType.esriFieldTypeDate:
                    return "System.DateTime";
                case esriFieldType.esriFieldTypeDouble:
                    return "System.Double";
                case esriFieldType.esriFieldTypeGeometry:
                    return "System.String";
                case esriFieldType.esriFieldTypeGlobalID:
                    return "System.String";
                case esriFieldType.esriFieldTypeGUID:
                    return "System.String";
                case esriFieldType.esriFieldTypeInteger:
                    return "System.Int32";
                case esriFieldType.esriFieldTypeOID:
                    return "System.String";
                case esriFieldType.esriFieldTypeRaster:
                    return "System.String";
                case esriFieldType.esriFieldTypeSingle:
                    return "System.Single";
                case esriFieldType.esriFieldTypeSmallInteger:
                    return "System.Int32";
                case esriFieldType.esriFieldTypeString:
                    return "System.String";
                default:
                    return "System.String";
            }
        }

        //���DataTable�е�����
        public static DataTable CreateDataTable(ILayer pLayer, string tableName)
        { 
            //������DataTable
            DataTable pDataTable = CreaterDataTableByLayer(pLayer, tableName);

            //ȡ��ͼ������
            string shapeType = getShapeType(pLayer);

            //����DataTable���ж���
            DataRow pDataRow = null;

            //��ILayer��ѯ��ITable
            ITable pTable = pLayer as ITable;
            ICursor pCursor = pTable.Search(null, false);

            //ȡ��ITable�е�����Ϣ
            IRow pRow = pCursor.NextRow();
            int n = 0;
            while (pRow != null)
            {
                //�½�DataTable���ж���
                pDataRow = pDataTable.NewRow();
                for (int i = 0; i < pRow.Fields.FieldCount; i++)
                {
                    //����ֶ�����ΪesriFieldTypeGeometry�������ͼ�����������ֶ�ֵ
                    if (pRow.Fields.get_Field(i).Type == esriFieldType.esriFieldTypeGeometry)
                    {
                        pDataRow[i] = shapeType;
                    }
                    else if (pRow.Fields.get_Field(i).Type == esriFieldType.esriFieldTypeBlob)
                    {
                        pDataRow[i] = "Element";
                    }
                    else
                    {
                        pDataRow[i] = pRow.get_Value(i);
                    }
                }
                //���DataRow��DataTable
                pDataTable.Rows.Add(pDataRow);
                pDataRow = null;
                n++;

                //Ϊ��֤Ч�ʣ�һ��ֻװ���������¼
                if (n == 2000)
                {
                    pRow = null;
                }
                else
                {
                    pRow = pCursor.NextRow();
                }
            }
            return pDataTable;
        }

        //��DataTable��DataGridView
        public void CreateAttributeTable(ILayer player)
        {
            string tableName;
            tableName = getValidFeatureClassName(player.Name);
            attributeTable = CreateDataTable(player, tableName);
            this.dataGridView1.DataSource = attributeTable;
            this.Text = "���Ա�[" + tableName + "] " + "��¼����" + attributeTable.Rows.Count.ToString();
        }

        //�滻���ݱ����еĵ�
        public static string getValidFeatureClassName(string FCname)
        {
            int dot = FCname.IndexOf(".");
            if (dot != -1)
            {
                return FCname.Replace(".", "_");
            }
            return FCname;
        }

        public static string getShapeType(ILayer pLayer)
        {
            IFeatureLayer pFeatLyr = (IFeatureLayer)pLayer;

            switch (pFeatLyr.FeatureClass.ShapeType)
            {
                case esriGeometryType.esriGeometryPoint:
                    return "Point";
                case esriGeometryType.esriGeometryPolyline:
                    return "Polyline";
                case esriGeometryType.esriGeometryPolygon:
                    return "Polygon";
                default:
                    return "";
            }
        }

    }
}