//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CCMWebApp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class Classification
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
