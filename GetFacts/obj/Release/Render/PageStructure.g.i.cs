﻿#pragma checksum "..\..\..\Render\PageStructure.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "3D69B1ED5070EB1D3A5508B65905783E"
//------------------------------------------------------------------------------
// <auto-generated>
//     Ce code a été généré par un outil.
//     Version du runtime :4.0.30319.42000
//
//     Les modifications apportées à ce fichier peuvent provoquer un comportement incorrect et seront perdues si
//     le code est régénéré.
// </auto-generated>
//------------------------------------------------------------------------------

using GetFacts.Render;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace GetFacts.Render {
    
    
    /// <summary>
    /// PageStructure
    /// </summary>
    public partial class PageStructure : System.Windows.Controls.Page, System.Windows.Markup.IComponentConnector {
        
        
        #line 17 "..\..\..\Render\PageStructure.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid elasticGrid;
        
        #line default
        #line hidden
        
        
        #line 26 "..\..\..\Render\PageStructure.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal GetFacts.Render.ArticleDisplay pageDisplay;
        
        #line default
        #line hidden
        
        
        #line 27 "..\..\..\Render\PageStructure.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid pauseDisplay;
        
        #line default
        #line hidden
        
        
        #line 39 "..\..\..\Render\PageStructure.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Canvas pauseSymbol;
        
        #line default
        #line hidden
        
        
        #line 48 "..\..\..\Render\PageStructure.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid factsContainer;
        
        #line default
        #line hidden
        
        
        #line 49 "..\..\..\Render\PageStructure.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Border factsBorder;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/GetFacts;component/render/pagestructure.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Render\PageStructure.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 10 "..\..\..\Render\PageStructure.xaml"
            ((GetFacts.Render.PageStructure)(target)).Unloaded += new System.Windows.RoutedEventHandler(this.Page_Unloaded);
            
            #line default
            #line hidden
            return;
            case 2:
            this.elasticGrid = ((System.Windows.Controls.Grid)(target));
            
            #line 17 "..\..\..\Render\PageStructure.xaml"
            this.elasticGrid.SizeChanged += new System.Windows.SizeChangedEventHandler(this.Grid_SizeChanged);
            
            #line default
            #line hidden
            return;
            case 3:
            this.pageDisplay = ((GetFacts.Render.ArticleDisplay)(target));
            return;
            case 4:
            this.pauseDisplay = ((System.Windows.Controls.Grid)(target));
            return;
            case 5:
            this.pauseSymbol = ((System.Windows.Controls.Canvas)(target));
            return;
            case 6:
            this.factsContainer = ((System.Windows.Controls.Grid)(target));
            return;
            case 7:
            this.factsBorder = ((System.Windows.Controls.Border)(target));
            
            #line 49 "..\..\..\Render\PageStructure.xaml"
            this.factsBorder.MouseEnter += new System.Windows.Input.MouseEventHandler(this.FactsBorder_MouseEnter);
            
            #line default
            #line hidden
            
            #line 49 "..\..\..\Render\PageStructure.xaml"
            this.factsBorder.MouseLeave += new System.Windows.Input.MouseEventHandler(this.FactsBorder_MouseLeave);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

