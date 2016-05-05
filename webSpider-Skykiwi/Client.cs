using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webSpider_Skykiwi
{
    class Client
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Zone { get; set; }
        public string Address { get; set; }
        public string WebPage { get; set; }
        public string HaveHRV { get; set; }



        public Client(string name, string phone, string zone, string address, string webPage, string haveHRV)
        {
            Name = name;
            Phone = phone;
            Zone = zone;
            Address = address;
            WebPage = webPage;
            HaveHRV = haveHRV;
        }






    }
}
