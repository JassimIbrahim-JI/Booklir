namespace Booklir.ViewModels.Authentication
{
    public class FutureDateAttribute : Attribute
    {
        public string ErrorMessage { get; set; }

        public bool IsValid(object value)
        {
            if (value is DateTime dateTime)
            {
                return dateTime > DateTime.Now;
            }
            return false;
        }
    }
}