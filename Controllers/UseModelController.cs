using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Data;
using System.Xml.Linq;

namespace CSharpServer.Controllers
{
    //[ApiController]
    //[Route("api/[controller]")]
    public class UseModelController : ControllerBase
    {
        public static string modelPath = AppDomain.CurrentDomain.BaseDirectory + "tensorflow_trained120.h5";
        [HttpGet]
        public IActionResult UseModel(string statusFilter = "all")
        {
            try
            {
                SklearnPredictorWrapper predictor = new SklearnPredictorWrapper(modelPath);

                List<float> inputData = new List<float> { 1f, 2f, 3f };

                dynamic prediction = predictor.Predict(inputData);
                return Ok("Успешно!");
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка: {ex.Message}");
            }
        } 
        public static float GetProcent(List<float> inputData)
        {
            return 0;
        }
        public static List<float> GetInputData(TransactionModel transactionModel)
        {
            if(transactionModel!= null)
            {
                List<float> record = new List<float>();
                for (int i = 0; i < 9; i++)
                {
                    record.Add(0);
                    switch (i)
                    {
                        case 0:
                            record[i] = SHA.HashToNumeric(SHA.ComputeSHA256Hash(transactionModel.Step.ToString()));
                            break;
                        case 1:
                            record[i] = SHA.HashToNumeric(SHA.ComputeSHA256Hash(transactionModel.Customer.ToString()));
                            break;
                        //age
                        case 2:
                            if(int.TryParse(transactionModel.Age, out int age))
                            {
                                record[i] = MathF.Min(age / (float)6, 1f);
                            }
                            else
                            {
                                record[i] = 0;
                            }
                            break;
                        case 3:
                            record[i] = (transactionModel.Gender == "M") ? 1 : 0;
                            break;
                        case 4:
                            record[i] = SHA.HashToNumeric(SHA.ComputeSHA256Hash(transactionModel.ZipcodeOri));
                            break;
                        case 5:
                            record[i] = SHA.HashToNumeric(SHA.ComputeSHA256Hash(transactionModel.Merchant));
                            break;
                        case 6:
                            record[i] = SHA.HashToNumeric(SHA.ComputeSHA256Hash(transactionModel.ZipMerchant));
                            break;
                        case 7:
                            record[i] = SHA.HashToNumeric(SHA.ComputeSHA256Hash(transactionModel.Category));
                            break;
                        case 8:
                            record[i] = MathF.Min(transactionModel.Amount / 300000f, 1f);
                            break;
                        default:

                            break;
                    }
                }
                return record;
            }
            return null;
        }
    }
}
