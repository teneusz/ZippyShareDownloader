﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TenekDownloader.Properties
{
    using System;


    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources
    {

        private static global::System.Resources.ResourceManager resourceMan;

        private static global::System.Globalization.CultureInfo resourceCulture;

        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources()
        {
        }

        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(resourceMan, null))
                {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("TenekDownloader.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }

        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture
        {
            get
            {
                return resourceCulture;
            }
            set
            {
                resourceCulture = value;
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to AppTitle.
        /// </summary>
        public static string AppTitle
        {
            get
            {
                return ResourceManager.GetString("AppTitle", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Dare you motherfucker. I double dare you.
        /// </summary>
        public static string dare_you
        {
            get
            {
                return ResourceManager.GetString("dare_you", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to DLC files (*.dlc)|*.dlc.
        /// </summary>
        public static string DlcFileFilter
        {
            get
            {
                return ResourceManager.GetString("DlcFileFilter", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Dare you. I double dare you Motherfucker.
        /// </summary>
        public static string Error_I_double_dare_you
        {
            get
            {
                return ResourceManager.GetString("Error_I_double_dare_you", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to 7-z library|7z.dll.
        /// </summary>
        public static string SevenZipFileFilter
        {
            get
            {
                return ResourceManager.GetString("SevenZipFileFilter", resourceCulture);
            }
        }
    }
}
