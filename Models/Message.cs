using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace REST_API_ProiectUTCN.Models
{
    public class Message
    {
        public long Id { get; set; }
        public string User { get; set; }
        public string Text { get; set; }
    }
}
