using System.Text.RegularExpressions;

namespace Factory.Infra
{
    public class Utilities
    {
        public static string GetValidColumnName(string input)
        {
            // Remove any non-alphanumeric characters except underscores
            string cleanString = Regex.Replace(input, "[^a-zA-Z0-9_]", "");

            // Trim and limit the length to a reasonable size
            string trimmedString = cleanString.Trim().Substring(0, Math.Min(cleanString.Length, 64));

            // Ensure the column name starts with a letter or underscore
            string finalColumnName = EnsureStartsWithLetterOrUnderscore(trimmedString);

            return finalColumnName;
        }

        private static string EnsureStartsWithLetterOrUnderscore(string input)
        {
            if (char.IsLetter(input[0]) || input[0] == '_')
                return input;

            return "_" + input;
        }

        public static string SafeWindowsPath(string fileName)
        {
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName;
        }

        public static string StripHTML(string input)
        {
            return Regex.Replace(input, "<.*?>", String.Empty);
        }

        public static string GetDateStr()
        {
            return string.Format("{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now);
        }

        public class Base64Converter
        {
            public static bool ConvertToFile(string bas64Str, string filePath)
            {
                try
                {
                    File.WriteAllBytes(filePath, Convert.FromBase64String(bas64Str));
                    return true;
                }
                catch (Exception ex)
                {
                    var funcName = System.Reflection.MethodBase.GetCurrentMethod().Name;
                    throw new BaseException(funcName, ex.Message);
                }
            }


        }

        public static string GetFileName(string filePath, int i = 1)
        {
            string directoryPath = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string fileExtension = Path.GetExtension(filePath);

            var outputFile = $"{directoryPath}\\{fileName}-hashed-{i}{fileExtension}";

            if (File.Exists(outputFile))
            {
                outputFile = GetFileName(filePath, ++i);
            }

            return outputFile;
        }


        public static bool IsNumeric(string input, out int result)
        {
            if (int.TryParse(input, out result))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
