namespace ShounenGaming.Business.Helpers
{
    public static class StringExtensions
    {
        public static string NormalizeStringToDirectory(this string folderName)
        {
            if (string.IsNullOrEmpty(folderName)) return folderName;

            var newName = folderName.ToLower().Replace(".", "-").Replace(" ", "-")
                .Replace(":", "").Replace("\\", "").Replace("/", "")
                .Replace(";", "").Replace("?", "").Replace("\"", "");
            

            foreach (var c in Path.GetInvalidFileNameChars())
                newName = newName.Replace(c.ToString(), string.Empty);

            foreach (var c in Path.GetInvalidPathChars())
                newName = newName.Replace(c.ToString(), string.Empty);

            return newName;
        }
    }
}
