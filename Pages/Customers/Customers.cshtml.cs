using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace jekirdekCase.Pages.Customers
{
    public class CustomersModel : PageModel
    {
        // Müşteri verilerini burada alabilirsiniz
        public List<Customer> Customers { get; set; }

        public async Task OnGetAsync()
        {
            // API'den veri çekme işlemi burada yapılabilir
            // Şu an statik bir örnek verelim:
            Customers = new List<Customer>
            {
                new Customer { Id = 1, FirstName = "Ahmet", LastName = "Yılmaz", Email = "ahmet@example.com", Region = "İstanbul" },
                new Customer { Id = 2, FirstName = "Mehmet", LastName = "Kara", Email = "mehmet@example.com", Region = "Ankara" }
            };
        }
    }

    public class Customer
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Region { get; set; }
    }
}