﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GetFacts {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.5.0.0")]
    internal sealed partial class Rendering : global::System.Configuration.ApplicationSettingsBase {
        
        private static Rendering defaultInstance = ((Rendering)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Rendering())));
        
        public static Rendering Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("#FF696969")]
        public global::System.Windows.Media.Color TimeOfDayColor {
            get {
                return ((global::System.Windows.Media.Color)(this["TimeOfDayColor"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("#FFA9A9A9")]
        public global::System.Windows.Media.Color ClockFrameColor {
            get {
                return ((global::System.Windows.Media.Color)(this["ClockFrameColor"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("#FF404040")]
        public global::System.Windows.Media.Color ArticleBorderColor {
            get {
                return ((global::System.Windows.Media.Color)(this["ArticleBorderColor"]));
            }
        }
    }
}
