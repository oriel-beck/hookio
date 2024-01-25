namespace Hookio
{
    public class DotEnv
    {
        public static void Load(string filePath)
        {
            // in production, use the docker-compose to load the env, this will skip
            if (!File.Exists(filePath))
                return;

            foreach (var line in File.ReadAllLines(filePath))
            {
                var parts = line.Split(
                    '=',
                    StringSplitOptions.RemoveEmptyEntries);
                var key = parts[0];
                var rest = parts.Skip(1).ToArray();
                Environment.SetEnvironmentVariable(key, string.Join("=", rest));
            }
        }
    }
}
