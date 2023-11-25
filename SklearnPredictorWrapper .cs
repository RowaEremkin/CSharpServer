using Python.Runtime;
using System.Diagnostics;

namespace CSharpServer
{

    public class SklearnPredictorWrapper
    {
        private dynamic pythonObject;

        public SklearnPredictorWrapper(string modelPath)
        {
            try
            {
                Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", "C:\\Python311.dll");
                PythonEngine.Initialize();
                //PythonEngine.RunSimpleString("import sys\nsys.path.append('.')");
                using (Py.GIL()) // GIL - Global Interpreter Lock, обеспечивает потокобезопасный доступ к Python API
                {
                    dynamic module = Py.Import("__main__");
                    this.pythonObject = module.GetAttr("SklearnPredictor")(modelPath);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex}");
                throw; // Перебросьте исключение, чтобы обеспечить его дальнейшую обработку
            }
        }

        public dynamic Predict(dynamic inputData)
        {
            using (Py.GIL())
            {
                return this.pythonObject.predict(inputData);
            }
        }
    }
}
