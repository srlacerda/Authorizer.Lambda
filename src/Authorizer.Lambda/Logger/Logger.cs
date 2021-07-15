using Amazon.Lambda.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Authorizer.Lambda.Logger
{
    public class Logger : ILogger
    {
        private StringBuilder sb;
        public void Debug(string message)
        {
            sb = new StringBuilder();
            sb.AppendLine("Level: [Debug]");
            sb.AppendLine($"Message: [{message}]");
            LambdaLogger.Log(sb.ToString());
        }

        public void Error(string message)
        {
            sb = new StringBuilder();
            sb.AppendLine("Level: [Error]");
            sb.AppendLine($"Message: [{message}]");
            LambdaLogger.Log(sb.ToString());
        }

        public void Error(Exception exception, string message)
        {
            sb = new StringBuilder();
            sb.AppendLine("Level: [Error]");
            sb.AppendLine($"Message: [{message}]");
            sb.AppendLine($"Exception: [{exception}]");
            LambdaLogger.Log(sb.ToString());
        }

        public void Info(string message)
        {
            sb = new StringBuilder();
            sb.AppendLine("Level: [Information]");
            sb.AppendLine($"Message: [{message}]");
            LambdaLogger.Log(sb.ToString());
        }

        public void Warning(string message)
        {
            sb = new StringBuilder();
            sb.AppendLine("Level: [Warning]");
            sb.AppendLine($"Message: [{message}]");
            LambdaLogger.Log(sb.ToString());
        }
    }
}
