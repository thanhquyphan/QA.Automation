using System;

namespace TQP.QA.Automation.Shared
{
    public static class EnvironmentHelpers
    {
        private const string LocalEnv = "Local";

        public static string GetEnvironment()
        {
            var env = Environment.GetEnvironmentVariable("ENV");

#if DEBUG
            //TODO: Remove this and figure out how to have the test runner provide environment variables properly
            if (string.IsNullOrWhiteSpace(env))
                env = LocalEnv;
#endif

            return env;
        }

        public static bool IsLocal()
        {
            var env = GetEnvironment();

            if (string.IsNullOrWhiteSpace(env) || env.Equals(LocalEnv, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;

        }
    }
}
