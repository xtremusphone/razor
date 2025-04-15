namespace Factory.DB.Interface
{
    interface ISSOUser
    {
        int Id { get; set; }

        string UserName { get; set; }

        string Token { get; set; }

        string ExpiryDate { get; set; }

        string LogDate { get; set; }
    }
}
