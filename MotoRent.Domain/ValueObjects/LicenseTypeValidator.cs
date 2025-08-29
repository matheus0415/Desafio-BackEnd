namespace MotoRent.Domain.ValueObjects
{
    public static class LicenseTypeValidator
    {
        public static bool IsValid(string licenseType)
        {
            return licenseType switch
            {
                "A" => true,
                "B" => true,
                "A+B" => true,
                _ => false
            };
        }
    }
}
