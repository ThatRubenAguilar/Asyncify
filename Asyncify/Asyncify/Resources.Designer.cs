﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Asyncify {
    using System;
    using System.Reflection;
    
    
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
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Asyncify.Resources", typeof(Resources).GetTypeInfo().Assembly);
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
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Consider using await on the Task over synchronous blocking method GetResult().
        /// </summary>
        internal static string GetResultDescription {
            get {
                return ResourceManager.GetString("GetResultDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Consider awaiting Task &apos;{0}&apos;.
        /// </summary>
        internal static string GetResultMessageFormat {
            get {
                return ResourceManager.GetString("GetResultMessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Use await over synchronous method GetResult().
        /// </summary>
        internal static string GetResultTitle {
            get {
                return ResourceManager.GetString("GetResultTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Consider removing Task call to synchronous blocking method Wait().
        /// </summary>
        internal static string RemoveWaitDescription {
            get {
                return ResourceManager.GetString("RemoveWaitDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Consider removing Wait() call on Task &apos;{0}&apos;.
        /// </summary>
        internal static string RemoveWaitMessageFormat {
            get {
                return ResourceManager.GetString("RemoveWaitMessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Remove extraneous call to synchronous method Wait().
        /// </summary>
        internal static string RemoveWaitTitle {
            get {
                return ResourceManager.GetString("RemoveWaitTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Consider using await on the Task over synchronous blocking property Result.
        /// </summary>
        internal static string ResultDescription {
            get {
                return ResourceManager.GetString("ResultDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Consider awaiting Task &apos;{0}&apos;.
        /// </summary>
        internal static string ResultMessageFormat {
            get {
                return ResourceManager.GetString("ResultMessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Use await over synchronous property Result.
        /// </summary>
        internal static string ResultTitle {
            get {
                return ResourceManager.GetString("ResultTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Consider using await on the Task over synchronous blocking method Wait().
        /// </summary>
        internal static string WaitDescription {
            get {
                return ResourceManager.GetString("WaitDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Consider awaiting Task &apos;{0}&apos;.
        /// </summary>
        internal static string WaitMessageFormat {
            get {
                return ResourceManager.GetString("WaitMessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Use await over synchronous method Wait().
        /// </summary>
        internal static string WaitTitle {
            get {
                return ResourceManager.GetString("WaitTitle", resourceCulture);
            }
        }
    }
}
