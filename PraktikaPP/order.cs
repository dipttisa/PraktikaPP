//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PraktikaPP
{
    using System;
    using System.Collections.Generic;
    
    public partial class order
    {
        public int id { get; set; }
        public int product_id { get; set; }
        public int user_id { get; set; }
        public decimal price { get; set; }
        public Nullable<int> count { get; set; }
        public Nullable<decimal> sum { get; set; }
        public DateTime date { get; set; }
    
        public virtual prodact prodact { get; set; }
        public virtual users users { get; set; }
    }
}
