using System;
using System.Data;
using System.Data.SqlClient;
using MySql.Data;
using MySql.Data.MySqlClient;
using Npgsql;


public class DbAccess
{


    protected string _connStringMySql = "place connection string here";
    protected string _connStringNpgsql = "place connection string here";



    /// <summary>
    /// Execute the query and return the data as DataTable
    /// </summary>
    /// <param name="query"></param>
    /// <returns>DataTable</returns>
    public DataTable MySqlExecuteQuery(string query)
    {
        DataTable dt = new DataTable();

        MySqlConnection conn = new MySqlConnection(_connStringMySql);
        try
        {

            conn.Open();


            MySqlCommand cmd = new MySqlCommand(query, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();


            dt.Load(rdr);

            rdr.Close();
            conn.Close();
        }
        catch (Exception ex)
        {
            // unable to execute query
        }

        conn.Close();

        return dt;


    }


        /// <summary>
        /// Execute the given query and return the data as DataTable
        /// NpgSQL is used by AWS RedShift
        /// </summary>
        /// <param name="query"></param>
        /// <returns>DataTable</returns>
        public DataTable NpgsqlExecuteQuery(string query)
        {

            DataSet ds = new DataSet();
            DataTable dt = new DataTable();

            using (var conn = new NpgsqlConnection(_connStringNpgsql))
            {

                try
                {
                    conn.Open();
                    var cmd = new NpgsqlCommand(query, conn);
                    var reader = cmd.ExecuteReader();

                    dt.Load(reader);

                    conn.Close();
                }
                catch (Exception ex)
                {
                    // Unable to execute query
                }
            }
            return dt;
        }




}
