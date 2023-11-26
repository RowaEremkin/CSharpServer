using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Data;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Globalization;

namespace CSharpServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MySQLController : ControllerBase
    {
        [HttpGet(Name = "GetDataFromDB")]
        public IActionResult GetDataFromDatabase(string statusFilter = "all", string customerFilter = "all", string categoryFilter = "all", int limit = 100, int offset = 0)
        {
            try
            {
                statusFilter = Validator(statusFilter);
                customerFilter = Validator(customerFilter);
                categoryFilter = Validator(categoryFilter);
                string connectionString = $"Server={Params.databaseServerIp};Database=transactions;User Id={Params.databaseServerUser};Password={Params.databaseServerPassword};";

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "";
                    query = "WITH category_table AS (select id AS cat_id, category FROM categorytable) SELECT * FROM operationsfraud JOIN category_table ON category_table.cat_id = category_id";
                    string endQuery = "";
                    bool where = false;
                    #region Status
                    if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "all")
                    {
                        where = true;
                        string[] splitted = statusFilter.Split(':');
                        for (int i = 0; i < splitted.Length; i++)
                        {
                            if (i == 0) { endQuery += " ("; } else { endQuery += " OR "; }
                            switch (splitted[i])
                            {
                                case "a":
                                default:
                                    endQuery += "fraud = 0";
                                    break;
                                case "f":
                                    endQuery += "fraud = 1";
                                    break;
                                case "s":
                                    endQuery += "(fraud > 0 AND fraud < 1)";
                                    break;
                            }
                            if (i == splitted.Length - 1) { endQuery += ")"; }
                        }
                    }
                    #endregion
                    #region Customer
                    if (!string.IsNullOrEmpty(customerFilter) && customerFilter != "all")
                    {
                        where = true;
                        if(endQuery.Length  > 0) { endQuery += " AND "; }
                        endQuery += $"customer like '%{customerFilter}%'";
                    }
                    #endregion
                    #region Category
                    if (!string.IsNullOrEmpty(categoryFilter) && categoryFilter != "all")
                    {
                        where = true;
                        if (endQuery.Length > 0) { endQuery += " AND "; }
                        string[] splitted = categoryFilter.Split(':');
                        for (int i = 0; i < splitted.Length; i++)
                        {
                            if (i == 0) { endQuery += " ("; } else { endQuery += " OR "; }
                            endQuery += $"category = '{splitted[i]}'";
                            if (i == splitted.Length - 1) { endQuery += ")"; }
                        }
                    }
                    #endregion
                    if (where)
                    {
                        query += " WHERE " + endQuery;
                    }
                    query += " ORDER BY id DESC ";
                    if (limit > 0)
                    {
                        query += " LIMIT " + limit;
                        if (offset > 0)
                        {
                            offset = Math.Max(offset, 0);
                            query += " OFFSET " + (offset * limit);
                        }
                    }
                    query += ";";
                    Debug.WriteLine("query: " + query);
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);
                            return Ok(ConvertDataTableToJson(dataTable));
                        }
                    }
                    return Ok("Подключение успешно установлено!");
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка подключения: {ex.Message}");
            }
        }
        private string Validator(string s)
        {
            s = s.Replace(';', ' ');
            s = s.Replace('\'', ' ');
            return s;
        }
        public static string ConvertDataTableToJson(DataTable dataTable)
        {
            string jsonString = JsonConvert.SerializeObject(dataTable, Formatting.Indented);
            return jsonString;
        }
        [HttpPost]
        public IActionResult PostData([FromBody] TransactionModel data)
        {
            try
            {
                float prediction = -1;
                List<float> inputData = UseModelController.GetInputData(data);
                try
                {
                    using (var wb = new WebClient())
                    {
                        string url = $"http://{Params.pythonServerAddress}/predict";
                        string json = JsonConvert.SerializeObject(inputData);
                        var data1 = new NameValueCollection();
                        ;
                        data1["data"] = json;

                        Debug.WriteLine(" json: " + json);
                        var response = wb.UploadValues(url, "POST", data1);
                        string responseInString = Encoding.UTF8.GetString(response);
                        Debug.WriteLine("responseInString: " + responseInString + " json: " + json);
                        if (float.TryParse(responseInString, NumberStyles.Any, null, out float f1))
                        {
                            prediction = System.MathF.Min( f1 * 10, 0.97f);
                            Debug.WriteLine(" New float: " + prediction);
                        }
                    }
                }
                catch(Exception ex)
                {
                    Debug.WriteLine("Olegs error!: " + ex.Message);
                }
                #region SQL

                try
                {
                    data.Age = Validator(data.Age);
                    data.Category = Validator(data.Category);
                    data.Customer = Validator(data.Customer);
                    data.Gender = Validator(data.Gender);
                    data.Merchant = Validator(data.Merchant);
                    data.ZipcodeOri = Validator(data.ZipcodeOri);
                    data.ZipMerchant = Validator(data.ZipMerchant);
                    string connectionString = $"Server={Params.databaseServerIp};Database=transactions;User Id={Params.databaseServerUser};Password={Params.databaseServerPassword};";

                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        string query = "";
                        if (true)
                        {
                            string fl = prediction.ToString();
                            fl = fl.Replace(',', '.');
                            string fa = data.Amount.ToString();
                            fa = fa.Replace(',', '.');
                            Debug.WriteLine("Prediction float string!: " + fl);
                            query = $"INSERT INTO operationsfraud(id, step, customer, age, gender, zipcodeOri, merchant, zipMerchant, category_id, amount, fraud)\r\n    VALUES (GetMaxID(), '{data.Step}', '{data.Customer}', '{data.Age}', '{data.Gender}', '{data.ZipcodeOri}', '{data.Merchant}', '{data.ZipMerchant}', GetCategoryId('{data.Category}'), '{fa}', '{fl}');";
                        }
                        else
                        {
                            query = $"call newCustomer('{data.Step}', '{data.Customer}', '{data.Age}', '{data.Gender}', '{data.ZipcodeOri}', '{data.Merchant}', '{data.ZipMerchant}', '{data.Category}', '{data.Amount}', '{prediction}');";
                        }
                        using (MySqlCommand command = new MySqlCommand(query, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("SQL send error!: " + ex.Message);
                }
                #endregion
                return Ok(prediction);
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка подключения: {ex.Message}");
            }
        }
    }
    [ApiController]
    [Route("api/[controller]")]
    public class CheckerController : ControllerBase
    {
        [HttpPost]
        public IActionResult PostCheckData([FromBody] CheckModel data)
        {
            try
            {
                #region SQL
                try
                {
                    string connectionString = $"Server={Params.databaseServerIp};Database=transactions;User Id={Params.databaseServerUser};Password={Params.databaseServerPassword};";

                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        string query = "";
                        if (true)
                        {
                            query = $"call UpdateFraudById('{data.Id}', '{(data.Check?0:1)}');";
                        }
                        else
                        {

                        }
                        using (MySqlCommand command = new MySqlCommand(query, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("SQL send error!: " + ex.Message);
                }
                #endregion
                return Ok(true);
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка подключения: {ex.Message}");
            }
        }
    }
}

