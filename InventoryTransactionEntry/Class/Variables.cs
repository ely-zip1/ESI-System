using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryTransactionEntry
{
    class Variables
    {
        private static string v_userLogged = "";

        public static string userLogged
        {
            get { return v_userLogged; }
            set { v_userLogged = value; }
        }
    }
}
