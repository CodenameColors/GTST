﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EditingTimeStampTool.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.9.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("#FF000000")]
        public global::System.Windows.Media.Color AppBackgroundColor {
            get {
                return ((global::System.Windows.Media.Color)(this["AppBackgroundColor"]));
            }
            set {
                this["AppBackgroundColor"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("#FFFFFFFF")]
        public global::System.Windows.Media.Color AppOutlineColor {
            get {
                return ((global::System.Windows.Media.Color)(this["AppOutlineColor"]));
            }
            set {
                this["AppOutlineColor"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("#FF707070")]
        public global::System.Windows.Media.Color AppBoxFillColor {
            get {
                return ((global::System.Windows.Media.Color)(this["AppBoxFillColor"]));
            }
            set {
                this["AppBoxFillColor"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string LogoURLPath {
            get {
                return ((string)(this["LogoURLPath"]));
            }
            set {
                this["LogoURLPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("#FFFFFFFF")]
        public global::System.Windows.Media.Color AppTextColor {
            get {
                return ((global::System.Windows.Media.Color)(this["AppTextColor"]));
            }
            set {
                this["AppTextColor"] = value;
            }
        }
    }
}
