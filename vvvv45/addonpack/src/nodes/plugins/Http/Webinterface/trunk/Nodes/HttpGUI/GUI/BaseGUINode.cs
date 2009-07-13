using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.Nodes.HttpGUI;
using VVVV.Utils.VMath;
using VVVV.Webinterface.Utilities;
using VVVV.Nodes.HttpGUI.Datenobjekte;
using VVVV.Webinterface;
using System.Diagnostics;
using System.Globalization;


namespace VVVV.Nodes.HttpGUI
{
    public abstract class BaseGUINode : IHttpGUIIO, IPluginConnections, IHttpGUIFunktionIO
    {



        #region field Definition
        

        //Host
        protected IPluginHost FHost;

       
        // Standart Pins
        public INodeOut FHttpGuiOut;


        public INodeIn FHttpGuiIn;
        public IHttpGUIIO FUpstreamInterface;


        public INodeIn FHttpStyleIn;
        public IHttpGUIStyleIO FUpstreamStyle;


        public INodeIn FFixedPain;
        public IHttpGUIIO FUpstreamInterfaceFixedPain;


        public INodeOut FFunktionOut;
        public INodeIn FFunktionOpen;
        public INodeIn FFunkttionClose;

        public ITransformIn FTransformIn;

        // Daten Liste und Objecte
        public SortedList<int, SortedList<string, string>> mValues;
        public SortedList<int, SortedList<string, string>> mStyles = new SortedList<int,SortedList<string,string>>();

        public bool mChangedStyle = false;

        public SortedList<string, string> mCssProperties;
        


        #endregion field Definition
      






        #region abstract Methods



        protected abstract void OnConfigurate(IPluginConfig Input);
        protected abstract void OnEvaluate(int SpreadMax);
        protected abstract void OnPluginHostSet();
        abstract public void GetDatenObjekt(int Index, out BaseDatenObjekt GuiDaten);
        abstract public void GetFunktionObjekt(int Index, out JsFunktion FunktionsDaten);
       

        #endregion abstract Methods

        




        #region pin creation

        //this method is called by vvvv when the node is created
        public void SetPluginHost(IPluginHost Host)
        {
            //assign host
            FHost = Host;

            this.FHost.CreateTransformInput("Transform", TSliceMode.Dynamic, TPinVisibility.True, out FTransformIn);

            FHost.CreateNodeOutput("Output", TSliceMode.Dynamic, TPinVisibility.True, out FHttpGuiOut);
            FHttpGuiOut.SetSubType(new Guid[1] { HttpGUIIO.GUID }, HttpGUIIO.FriendlyName);
            FHttpGuiOut.SetInterface(this);

            this.OnPluginHostSet();

            //Input Pins 
            FHost.CreateNodeInput("Input", TSliceMode.Dynamic, TPinVisibility.True, out FHttpStyleIn);
            FHttpStyleIn.SetSubType(new Guid[1] { HttpGUIStyleIO.GUID }, HttpGUIStyleIO.FriendlyName);
            
            //create outputs	    	
             
        }


        public virtual bool AutoEvaluate
        {
            get { return false; }
        }

        #endregion pin creation







        #region IMyNodeIO




        public void ConnectPin(IPluginIO Pin)
        {
            //cache a reference to the upstream interface when the NodeInput pin is being connected
            if (Pin == FHttpGuiIn)
            {
                if (FHttpGuiIn != null)
                {
                    INodeIOBase usI;
                    FHttpGuiIn.GetUpstreamInterface(out usI);
                    FUpstreamInterface = usI as IHttpGUIIO;
                }
                
            }
            //else if (Pin == FFunktionOpen)
            //{
            //    INodeIOBase usIJsFunktionOpen;
            //    FFunktionOpen.GetUpstreamInterface(out usIJsFunktionOpen);
            //    FUpstreamFunktionOpen = usIJsFunktionOpen as IHttpGUIFunktionIO;
            //}
            //else if (Pin == FFunkttionClose)
            //{
            //    INodeIOBase usIJsFunktionClose;
            //    FFunkttionClose.GetUpstreamInterface(out usIJsFunktionClose);
            //    FUpstreamFunktionClose = usIJsFunktionClose as IHttpGUIFunktionIO;
            //}
            else if( Pin == FHttpStyleIn)
            {
                INodeIOBase usIHttpStyle;
                FHttpStyleIn.GetUpstreamInterface(out usIHttpStyle);
                FUpstreamStyle = usIHttpStyle as IHttpGUIStyleIO;
                mChangedStyle = true;
            }
        }



        public void DisconnectPin(IPluginIO Pin)
        {
            //reset the cached reference to the upstream interface when the NodeInput is being disconnected
            if (Pin == FHttpGuiIn)
            {
                FUpstreamInterface = null;
            }
            else if (Pin == FFixedPain)
            {
                FUpstreamInterfaceFixedPain = null;
            }
            if (Pin == FFunktionOpen)
            {
                FUpstreamInterface = null;
            }
            else if (Pin == FFunkttionClose)
            {
                FUpstreamInterfaceFixedPain = null;
            }
            else if (Pin == FHttpStyleIn)
            {
                FUpstreamStyle = null;
                mChangedStyle = true;
            }

        }

        #endregion NodeIO
        

        



        #region Configurate
        public void Configurate(IPluginConfig Input)
        {
            this.OnConfigurate(Input);
        }

        #endregion







        #region Evaluate
        public void Evaluate(int SpreadMax)
        {

            //read Transform 


            //GET Style Properties
            int usS;
            if (FUpstreamStyle != null)
            {

                mStyles.Clear();
                int tNillCounter = 0;

                for (int i = 0; i < SpreadMax; i++)
                {
                    //get upstream slice index

                    FHttpStyleIn.GetUpsreamSlice(i, out usS);

                    SortedList<string,string> tStylePropertie = new SortedList<string,string>();
                    FUpstreamStyle.GetCssProperties(usS, out tStylePropertie);
                    if (tStylePropertie != null)
                    {
                        mStyles.Add(i, tStylePropertie);
                    }
                    else
                    {
                        SortedList<string, string> tDummyCssProperty;
                        mStyles.TryGetValue(tNillCounter, out tDummyCssProperty);
                        mStyles.Add(i, tDummyCssProperty);
                        if (tNillCounter  < mStyles.Count)
                        {
                            tNillCounter++;
                        }
                        else
                        {
                            tNillCounter = 0;
                        }
                    }
                    

                    bool tChangedValue;
                    FUpstreamStyle.GetInputChanged( out tChangedValue);
                    if (tChangedValue == true)
                    {
                        mChangedStyle = tChangedValue;
                    }
                    else
                    {
                        mChangedStyle = tChangedValue;
                    }
                    
                }
            }


            if (FTransformIn.PinIsChanged)
            {

            }

            this.OnEvaluate(SpreadMax);
         }

        #endregion Evaluate






        #region Node Information 

        public string GetNodeID()
        {
            string tPath;
            FHost.GetNodePath(true, out tPath);
            return tPath;
        }

        public void GetTransformation(Matrix4x4 pMatrix,out SortedList<string,string> pTransform)
        {
            SortedList<string, string> tStyles = new SortedList<string, string>();
            tStyles.Add("position", "absolute");

            double tWidth = HTMLToolkit.MapScale(pMatrix.m11, 0, 2, 0, 100);
            double tHeight = HTMLToolkit.MapScale(pMatrix.m22, 0, 2, 0, 100);

            tStyles.Add("width", ReplaceComma(string.Format("{0:0.0}", Math.Round(tWidth, 1)) + "%"));
            tStyles.Add("height", ReplaceComma(string.Format("{0:0.0}", Math.Round(tHeight, 1)) + "%"));

            double tTop = HTMLToolkit.MapTransform(pMatrix.m42, 1, -1, 0, 100, tHeight);
            double tLeft = HTMLToolkit.MapTransform(pMatrix.m41, -1, 1, 0, 100, tWidth);

            tStyles.Add("top", ReplaceComma(string.Format("{0:0.0}", Math.Round(tTop,1)) + "%"));
            tStyles.Add("left", ReplaceComma(string.Format("{0:0.0}",Math.Round(tLeft,1)) + "%"));


            tStyles.Add("z-index", Convert.ToString(Math.Round(pMatrix.m43)));
            pTransform = tStyles;

        }

        public string ReplaceComma(string tParameter)
        {
            return tParameter.Replace(",", ".");
        }

        public string GetNodeIdformSliceId(string pSliceId)
        {
            char[] delimiter ={ '/' };
            string[] Patches = pSliceId.Split(delimiter);
            int tPatchDepth = Patches.Length;

            int LengthLastPatch = Patches[tPatchDepth - 1].Length;
            return pSliceId.Substring(0, pSliceId.Length - LengthLastPatch - 1);
        }


        public string GetSliceFormSliceId(string pSliceId)
        {
            char[] delimiter ={ '/' };
            string[] Patches = pSliceId.Split(delimiter);
            int tPatchDepth = Patches.Length;
            return Patches[Patches.Length - 1];
        }

        #endregion Node Information




        
      
    }
}
