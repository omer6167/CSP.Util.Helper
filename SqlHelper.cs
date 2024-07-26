using Bimser.Synergy.Entities.Shared.Business.Objects;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CSP.Util.Helper
{
    public static class SqlHelper
    {

        #region Sql

       
        public static DataTable GetSQLData(string queryString, string conStr, Dictionary<string, string>? prm = null)
        {
            using SqlConnection connection = new(conStr);
            DataTable dt = new();
            connection.Open();

            using (SqlCommand command = new(queryString, connection))
            {
                if (prm != null)
                {
                    foreach (var item in prm)
                    {
                        command.Parameters.AddWithValue("@" + item.Key, item.Value);
                    }
                }

                SqlDataAdapter adp = new()
                {
                    SelectCommand = command
                };

                adp.Fill(dt);
            }


            return dt;



        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="queryString"></param>
        /// <param name="prm"></param>
        /// <returns></returns>
        public static DataTable GetSQLData(Context context, string queryString, Dictionary<string, string> prm = null)
        {
            Configuration config = ConfigHelper.GetConfig(context);

            using (SqlConnection connection = new SqlConnection(config._ConnStr))
            {
                DataSet ds = new DataSet();
                SqlCommand command = new SqlCommand(queryString, connection);

                if (prm != null)
                {
                    foreach (var item in prm)
                    {
                        command.Parameters.AddWithValue("@" + item.Key, item.Value);
                    }
                }

                SqlDataAdapter adp = new SqlDataAdapter();

                adp.SelectCommand = command;

                command.Connection.Open();

                adp.Fill(ds);

                command.Connection.Close();

                return ds.Tables[0];
            }
        }


        public static object ExecuteScalar(string queryString, string conStr, Dictionary<string, string> prm = null, CommandType sqltyp = CommandType.Text)
        {
            using SqlConnection connection = new SqlConnection(conStr);
            connection.Open();

            using SqlCommand command = new SqlCommand(queryString, connection);

            if (prm != null)
            {
                foreach (var item in prm)
                {
                    command.Parameters.AddWithValue("@" + item.Key, item.Value);
                }
            }

            command.CommandType = sqltyp;

            return command.ExecuteScalar();
        }

        /// <summary>
        /// For Scalar Value
        /// </summary>
        /// <param name="context"></param>
        /// <param name="queryString"></param>
        /// <param name="prm"></param>
        /// <param name="sqltyp"></param>
        /// <returns></returns>
        public static object ExecuteScalar(Context context, string queryString, Dictionary<string, string> prm = null, CommandType sqltyp = CommandType.Text)
        {
            Configuration config = ConfigHelper.GetConfig(context);

            using (SqlConnection connection = new SqlConnection(config._ConnStr))
            {
                connection.Open();

                using SqlCommand command = new SqlCommand(queryString, connection);

                if (prm != null)
                {
                    foreach (var item in prm)
                    {
                        command.Parameters.AddWithValue("@" + item.Key, item.Value);
                    }
                }

                command.CommandType = sqltyp;

                return command.ExecuteScalar();
            }
        }

        /// <summary>
        /// For ExecuteNonQuery
        /// </summary>
        /// <param name="queryString"></param>
        /// <param name="ConStr"></param>
        /// <param name="prm"></param>
        public static int Execute(string queryString, string ConStr, Dictionary<string, string> prm = null) //,   CommandType sqltyp=CommandType.Text)
        {
            using (SqlConnection connection = new SqlConnection(ConStr))
            {
                SqlCommand command = new SqlCommand(queryString, connection);

                if (prm != null)
                {
                    foreach (var item in prm)
                    {
                        command.Parameters.AddWithValue("@" + item.Key, item.Value);
                    }
                }

                command.Connection.Open();
                int effective = command.ExecuteNonQuery();
                command.Connection.Close();

                return effective;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="queryString"></param>
        /// <param name="prm"></param>
        public static int Execute(Context context, string queryString, Dictionary<string, string> prm = null) //,   CommandType sqltyp=CommandType.Text)
        {
            Configuration config = ConfigHelper.GetConfig(context);

            using (SqlConnection connection = new SqlConnection(config._ConnStr))
            {
                SqlCommand command = new SqlCommand(queryString, connection);

                if (prm != null)
                {
                    foreach (var item in prm)
                    {
                        command.Parameters.AddWithValue("@" + item.Key, item.Value);
                    }
                }

                command.Connection.Open();
                int effective = command.ExecuteNonQuery();
                command.Connection.Close();

                return effective;
            }
        }


        #endregion
    }
}
