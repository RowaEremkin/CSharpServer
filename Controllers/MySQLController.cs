using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MySql.Data.MySqlClient;
using System.Data;
using System.Diagnostics;
using Microsoft.ML;
using Microsoft.ML.Transforms.Onnx;
//using Microsoft.ML.OnnxRuntime.Tensors;
//using Microsoft.ML.OnnxRuntime;
using OneOf.Types;
using Newtonsoft.Json;
using Tensorflow;
using TensorFlow;
using System;
using MySqlX.XDevAPI;
using Keras;
using Keras.Models;
using System.Runtime.InteropServices;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.AspNetCore.Http;

namespace CSharpServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MySQLController : ControllerBase
    {
        [HttpGet(Name = "GetDataFromDB")]
        public IActionResult GetDataFromDatabase(string statusFilter = "all", string customerFilter = "all", string categoryFilter = "all", int limit = 100)
        {
            try
            {
                string connectionString = "Server=26.149.132.151;Database=transactions;User Id=hack;Password=QWErty0987;";

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "";
                    if (true)
                    {
                        query = "SELECT * FROM operationsfraud join categorytable on categorytable.id = category_id";
                        string endQuery = "";
                        bool where = false;
                        #region Status
                        if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "all")
                        {
                            where = true;
                            string[] splitted = statusFilter.Split(';');
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
                            string[] splitted = categoryFilter.Split(';');
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
                        if(limit > 0)
                        {
                            query += " LIMIT " + limit;
                        }
                        query += ";";

                    }
                    else
                    {
                        query = $"call sameOperation('{customerFilter}', '{categoryFilter}', '{statusFilter}', {limit});";
                    }
                    //return Ok(query);
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);

                            foreach (DataRow row in dataTable.Rows)
                            {
                                foreach (DataColumn col in dataTable.Columns)
                                {
                                    //Debug.Write(row[col] + " ");
                                }
                                //Debug.WriteLine("");
                            }
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

                return Ok(prediction);
                if (false)
                {
                //    var context = new MLContext();

                //    // Загрузка ONNX модели
                //    //var onnxModelPath = "path/to/your/sklearn_model.onnx";
                //    //var model = context.Model.Load(UseModelController.modelPath, out var modelSchema);
                //    InferenceSession onnxSession = new InferenceSession(UseModelController.modelPath);
                //    // Преобразуйте входные данные в тензор ONNX
                //    var inputTensor = new DenseTensor<float>(new[] { 1, inputData.Count });
                //    // Заполните тензор значениями из input

                //    // Выполните предсказание
                //    var inputs = new List<NamedOnnxValue>
                //{
                //    NamedOnnxValue.CreateFromTensor("float_input", inputTensor)
                //};
                //    if (false)
                //    {
                //        using (var result = onnxSession.Run(inputs).FirstOrDefault())
                //        {
                //            if (result != null)
                //            {
                //                // Проверяем, является ли результат картой (map)
                //                if (result.ValueType == OnnxValueType.ONNX_TYPE_MAP)
                //                {
                //                    var map = result.AsDictionary<string, float>();

                //                    foreach (var kvp in map)
                //                    {
                //                        string key = kvp.Key;
                //                        float value = kvp.Value;

                //                        Debug.WriteLine($"key name: {kvp.Key}, value: {kvp.Value}");
                //                        // Ваш код для обработки ключа и значения
                //                    }
                //                }
                //                if (result.ValueType == OnnxValueType.ONNX_TYPE_TENSOR)
                //                {
                //                    //var outputTensor = result.AsTensor<float>();
                //                    {
                //                        //prediction = outputTensor[0];

                //                    }
                //                }
                //            }
                //        }
                //    }
                //    var results = onnxSession.Run(inputs);
                //    // Обработайте результаты и верните ответ
                //    var outputTensor = results.First().AsTensor<float>();
                //    if (outputTensor != null && outputTensor.Count() > 0)
                //    {
                //        prediction = outputTensor.GetValue(0);
                //    }
                //    var outputProbabilityTensor = results.FirstOrDefault(r => r.Name == "output_probability");
                //    if (outputProbabilityTensor != null)
                //    {
                //        IEnumerable<DisposableNamedOnnxValue> disposableNamedOnnxValues = (IEnumerable<DisposableNamedOnnxValue>)outputProbabilityTensor.Value;
                //        if (disposableNamedOnnxValues != null && disposableNamedOnnxValues.Count() > 0)
                //        {
                //            Debug.WriteLine(disposableNamedOnnxValues.ElementAt(0).Value.ToString());
                //            Dictionary<long, float> keyValuePairs = (Dictionary<long, float>)disposableNamedOnnxValues.ElementAt(0).Value;
                //            if (keyValuePairs != null)
                //            {
                //                Debug.WriteLine("Dictinary: " + keyValuePairs.Count);
                //            }
                //            foreach (var pair in keyValuePairs)
                //            {
                //                if (pair.Key == 0)
                //                {
                //                    prediction = pair.Value;
                //                }
                //            }
                //        }
                //        // Ваш код для обработки outputProbabilityTensor
                //    }
                }
                else
                {
                    var modelPath = UseModelController.modelPath;

                    if (false)
                    {
                        using (var session = new TFSession())
                        {
                            System.IO.File.ReadAllBytes(modelPath);
                        }
                    }
                    else
                    {
                        using (var graph = new TFGraph())
                        {
                            Debug.WriteLine("new TFGraph()");
                            var tensor = new TFTensor(inputData.ToArray());
                            Debug.WriteLine("new TFTensor(inputData.ToArray());");
                            // Load the model
                            Debug.WriteLine("modelPath: " + modelPath);
                            //graph = TFGraph.FromGraphDef(File.ReadAllBytes(modelPath));
                            graph.Import(System.IO.File.ReadAllBytes(modelPath), status: TFStatus.Default);
                            Debug.WriteLine("graph.Import(System.IO.File.ReadAllBytes(modelPath));");
                            using (var session = new TFSession(graph))
                            {
                                Debug.WriteLine("session = new TFSession(graph)");
                                // Setup the runner
                                var runner = session.GetRunner();
                                Debug.WriteLine("var runner = session.GetRunner();");
                                runner.AddInput(graph["dense"][0], tensor);
                                Debug.WriteLine("runner.AddInput(graph[\"dense\"][0], tensor);");
                                runner.Fetch(graph["dense_1"][0]);
                                Debug.WriteLine("runner.Fetch(graph[\"dense_1\"][0]);");

                                // Run the model
                                var output = runner.Run();
                                Debug.WriteLine("var output = runner.Run();");

                                // Fetch the results from output:
                                TFTensor result = output[0];
                                if (result != null)
                                {
                                    prediction = (float)result.GetValue();
                                }
                            }
                        }
                    }
                }
                #region Python
                //Debug.WriteLine("UseModelController.GetInputData(data)");
                //SklearnPredictorWrapper predictor = new SklearnPredictorWrapper(UseModelController.modelPath);
                //Debug.WriteLine("predictor");
                //prediction = predictor.Predict(inputData);
                //string s = "";
                //if(data != null)
                //{
                //    s += (" Step: " +data.Step);
                //    s += (" Customer: " + data.Customer);
                //    s += (" Age: " + data.Age);
                //    s += (" Gender: " + data.Gender);
                //    s += (" ZipcodeOri: " + data.ZipcodeOri);
                //    s += (" Merchant: " + data.Merchant);
                //    s += (" ZipMerchant: " + data.ZipMerchant);
                //    s += (" Category: " + data.Category);
                //    s += (" Amount: " + data.Amount);
                //}
                #endregion
                return Ok(prediction);
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка подключения: {ex.Message}");
            }
        }
    }
}

