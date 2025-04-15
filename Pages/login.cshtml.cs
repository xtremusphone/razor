using Factory;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Model;


namespace OfflineFirstRazor.Pages
{
    public class loginModel : PageModel
    {
        public string Message { get; set; }
        public string Domain { get; set; }

        public loginModel()
        {
            Domain = DomainHelper.GetPCDomainName();
        }

        public void OnGet()
        {
            //Console.WriteLine("aaa");
            Message = " Hey Stranger, welcome to login page";
            if (Request.Query.ContainsKey("err"))
            {
                Message = Request.Query["err"].ToString();
            }
        }

        public async Task OnPost()
        {
            try
            {
                var loginRequest = new LoginModel()
                {
                    Domain = Request.Form["domain"],
                    UserName = Request.Form["username"],
                    Password = Request.Form["password"]
                };

                //==============================================================
                //Authentication code
                //===============================================================
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
        }

    }
}
