//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AutoCadLisansKontrol.DAL
{
    using System;
    using System.Collections.Generic;
    
    public partial class CheckLicenseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Ip { get; set; }
        public Nullable<int> ComputerId { get; set; }
        public string Output { get; set; }
        public Nullable<System.DateTime> CheckDate { get; set; }
        public Nullable<System.DateTime> UpdateDate { get; set; }
        public Nullable<int> OperationId { get; set; }
        public Nullable<int> FirmId { get; set; }
        public Nullable<bool> State { get; set; }
        public Nullable<bool> Installed { get; set; }
        public Nullable<bool> Uninstalled { get; set; }
        public Nullable<bool> IsFound { get; set; }
        public Nullable<System.DateTime> InstallDate { get; set; }
        public Nullable<System.DateTime> UnInstallDate { get; set; }
    
        public virtual OperationEntity Operation { get; set; }
    }
}
