using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace InventoryTransactionEntry
{
    class Global
    {
        private MySqlConnection con = new MySqlConnection("server= 'localhost'; database = 'esidb2'; uid = 'root'; password = '';");
        private MySqlCommand cmd = new MySqlCommand();
        public MySqlDataReader dr;


        public void openConnection()
        {
            if (con.State == System.Data.ConnectionState.Closed)
            {
                con.Open();
            }
        }
        public void closeConnection()
        {
            if (con.State == System.Data.ConnectionState.Open)
            {
                con.Close();
            }
        }
        public void InUpDel(string query)
        {
            cmd.Connection = con;
            cmd.CommandText = query;
            cmd.ExecuteNonQuery();
        }
        public void fetch(string query)
        {
            cmd.Connection = con;
            cmd.CommandText = query;
            dr = cmd.ExecuteReader();
        }
    }
}
