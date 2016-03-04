using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RushHourTests.MockClasses
{
    // Idea borrowed from http://forum.unity3d.com/threads/security-exception-ecall-methods-must-be-packaged-into-a-system-module.98230/
    // See also http://forum.unity3d.com/threads/networkserver-in-console-application.362012/
    // and http://forum.unity3d.com/threads/c-console-server-ecall-error.50163/
    // http://www.amitloaf.com/2014/02/continuous-integration-and-unit-tests_12.html
    // and http://stackoverflow.com/questions/11286004/securityexception-ecall-methods-must-be-packaged-into-a-system-module
    // and http://answers.unity3d.com/questions/628962/why-cant-we-use-unityenginedll-outside-of-unity.html
    // :(
    public class Debug
    {
        public static void Log(string s) { Console.WriteLine(s); }
        public static void LogWarning(string s) { Console.WriteLine(s); }
        public static void LogError(string s) { Console.WriteLine(s); }
    }

    public class MonoBehaviour
    {

    }
}
