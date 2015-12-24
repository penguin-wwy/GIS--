using System;
using System.Drawing;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;

namespace WindowsApplication1
{
    /// <summary>
    /// Command that works in ArcMap/Map/PageLayout
    /// </summary>
    [Guid("2d0783d4-2c96-417b-aeaa-9ff86cebeb96")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("WindowsApplication1.OpenAttributeTable")]
    public sealed class OpenAttributeTable : BaseCommand
    {
       
        private ILayer m_pLayer;

        private IHookHelper m_hookHelper = null;
        public OpenAttributeTable(ILayer pLayer)
        {
            //
            // TODO: Define values for the public properties
            //
            base.m_category = ""; //localizable text
            base.m_caption = "打开属性表";  //localizable text 
            base.m_message = "打开属性表";  //localizable text
            base.m_toolTip = "打开属性表";  //localizable text
            base.m_name = "打开属性表";   //unique id, non-localizable (e.g. "MyCategory_MyCommand")

            m_pLayer = pLayer;

            try
            {
                //
                // TODO: change bitmap name if necessary
                //
                string bitmapResourceName = GetType().Name + ".bmp";
                base.m_bitmap = new Bitmap(GetType(), bitmapResourceName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message, "Invalid Bitmap");
            }
        }

        public override void OnClick()
        {
            // TODO: Add OpenAttributeTable.OnClick implementation
            AttributeTableFrm attributeTable = new AttributeTableFrm();
            attributeTable.CreateAttributeTable(m_pLayer);
            attributeTable.ShowDialog();
        }

        public override void OnCreate(object hook)
        {
            //throw new Exception("The method or operation is not implemented.");
        }
    }
}
