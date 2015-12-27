using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.SystemUI;

namespace how_trouble
{
    public sealed class LayerVisibility : BaseCommand, ICommandSubType
    {
        private HookHelper m_hookHelper = new HookHelperClass();
        private long m_subType;
        public LayerVisibility()
        { }

        public override void OnClick()
        {
            for (int i = 0; i <= m_hookHelper.FocusMap.LayerCount - 1; i++)
            {
                if (1 == m_subType)
                    m_hookHelper.FocusMap.get_Layer(i).Visible = true;
                if (2 == m_subType)
                    m_hookHelper.FocusMap.get_Layer(i).Visible = false;
            }

            m_hookHelper.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
        }

        public override void OnCreate(object hook)
        {
            m_hookHelper.Hook = hook;
        }

        public int GetCount()
        {
            return 2;
        }

        public void SetSubType(int SubType)
        {
            m_subType = SubType;
        }

        public override string Caption
        {
            get
            {
                if (m_subType == 1) return "Turn All Layers On";
                else return "Turn All Layers Off";
            }
        }

        public override bool Enabled
        {
            get
            {
                bool enable = false;
                int i;
                if (1 == m_subType)
                {
                    for (i = 0; i <= m_hookHelper.FocusMap.LayerCount - 1; i++)
                    {
                        if (m_hookHelper.ActiveView.FocusMap.get_Layer(i).Visible == false)
                        {
                            enable = true;
                            break;
                        }
                    }
                }

                else
                {
                    for (i = 0; i <= m_hookHelper.FocusMap.LayerCount - 1; i++)
                    {
                        if (m_hookHelper.ActiveView.FocusMap.get_Layer(i).Visible == true)
                        {
                            enable = true;
                            break;
                        }
                    }
                }
                return enable;
            }
        }
    }
}
