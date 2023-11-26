using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Ocsp;
using System.Data;
using System.Formats.Asn1;
using System.Globalization;
using System.Security.Cryptography;
using System.Xml.Linq;
using Tensorflow;
using Tensorflow.Keras;
using Tensorflow.Keras.Layers;
using static Tensorflow.Binding;

namespace CSharpServer.Controllers
{
    //[ApiController]
    //[Route("api/[controller]")]
    public class TensorFlowModelController : ControllerBase
    {

        [HttpGet]
        public IActionResult TrainModel()
        {
            try
            {
                List<float[]> floats = ReadCSV();
                string s = "";
                for (int i = 0; i < floats.Count; i++)
                {
                    for (int j = 0; j < floats[i].Length; j++)
                    {
                        s += floats[i][j];
                        if (j != floats[i].Length - 1)
                        {
                            s += " - ";
                        }
                    }
                    s += "\n";
                }
                return Ok("Успешно! - " + floats.Count);
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка: {ex.Message}");
            }
        }
        static List<float[]> ReadCSV()
        {
            var records = new List<float[]>();
            var labels = new List<float>();
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                MissingFieldFound = null
            };
            using (var reader = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "/fraud_dataset.csv"))
            using (var csv = new CsvReader(reader, config))
            {

                csv.Read();
                csv.ReadHeader();

                while (csv.Read())
                {
                    var record = new float[csv.ColumnCount];
                    for (int i = 0; i < record.Length; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                record[i] = SHA.HashToNumeric(SHA.ComputeSHA256Hash(csv.GetField<int>(i).ToString()));
                                break;
                            case 1:
                                record[i] = SHA.HashToNumeric(SHA.ComputeSHA256Hash(csv.GetField<string>(i)));
                                break;
                                //age
                            case 2:
                                record[i] = MathF.Min(csv.GetField<int>(i) / (float)6,1f);
                                break;
                            case 3:
                                record[i] = (csv.GetField<string>(i) == "M")?1:0;
                                break;
                            case 4:
                            case 5:
                            case 6:
                            case 7:
                                record[i] = SHA.HashToNumeric(SHA.ComputeSHA256Hash(csv.GetField<string>(i)));
                                break;
                            case 8:
                                record[i] = MathF.Min(csv.GetField<float>(i) / 300000f, 1f);
                                break;
                            default:

                                break;
                        }
                    }

                    records.Add(record);
                    labels.Add(csv.GetField<float>("fraud"));
                }
            }
            return records;
        }
    }
    class FraudDetectionModel
    {
        static float[][] OneHotEncode(string[] categories)
        {
            var uniqueCategories = categories.Distinct().ToArray();

            float[][] encodedData = new float[categories.Length][];

            for (int i = 0; i < categories.Length; i++)
            {
                encodedData[i] = new float[uniqueCategories.Length];

                int categoryIndex = Array.IndexOf(uniqueCategories, categories[i]);

                encodedData[i][categoryIndex] = 1.0f;
            }

            return encodedData;
        }
    }
}
