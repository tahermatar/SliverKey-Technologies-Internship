//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;
//using EdgeDB;
//using System.Collections;

//namespace ContactDatabase.Pages
//{
//    public class IndexModel : PageModel
//    {
//        private readonly EdgeDBClient _client;

//        [BindProperty]
//        public string FirstName { get; set; }

//        [BindProperty]
//        public string LastName { get; set; }

//        [BindProperty]
//        public string Email { get; set; }

//        [BindProperty]
//        public string Title { get; set; }

//        [BindProperty]
//        public string Description { get; set; }

//        [BindProperty]
//        public DateTime BirthDate { get; set; }

//        [BindProperty]
//        public bool MaritalStatus { get; set; }
//        public int counter { get; set; } = 0;

//        public List<Contact> Contacts { get; set; }
//        //public int counter { get; set; }
//        public IndexModel(EdgeDBClient client)
//        {
//            _client = client;
//        }

//        public async Task OnGetAsync()
//        {
//            await LoadContacts();
//        }

//        public async Task<IActionResult> OnPostAsync()
//        {
//            counter++;

//            var query = @"
//        INSERT Contact {
//            contact_id := <int64>$contact_id,
//            first_name := <str>$first_name,
//            last_name := <str>$last_name,
//            email := <str>$email,
//            title := <str>$title,
//            description := <str>$description,
//            birth_date := <str>$birth_date,
//            marital_status := <bool>$marital_status
//        }";

//            await _client.ExecuteAsync(query, new Dictionary<string, object?>
//        {
//            {"contact_id", Convert.ToInt64(counter)},
//            {"first_name", FirstName},
//            {"last_name", LastName},
//            {"email", Email},
//            {"title", Title},
//            {"description", Description},
//            {"birth_date", BirthDate.ToString("yyyy-MM-dd")},
//            {"marital_status", MaritalStatus}
//        });

//            await LoadContacts();

//            return Page();
//        }

//        public async Task<IActionResult> OnGetSearchContactsAsync(string search)
//        {
//            FirstName = LastName = Email = search;
//            await LoadContacts(search);
//            return Page();
//        }

//        private async Task LoadContacts(string searchQuery = "")
//        {
//            var query = $@"
//                SELECT Contact {{
//                    contact_id,
//                    first_name,
//                    last_name,
//                    email,
//                    title,
//                    description,
//                    birth_date,
//                    marital_status
//                }}
//                FILTER
//                    Contact.first_name ILIKE '%{searchQuery}%' OR
//                    Contact.last_name ILIKE '%{searchQuery}%' OR
//                    Contact.email ILIKE '%{searchQuery}%'
//                ORDER BY Contact.first_name ASC";

//            var result = await _client.QueryAsync<Contact>(query);
//            Contacts = result.ToList() ?? new List<Contact>();
//            //Contacts = result.ToList();
//        }

//        public class Contact
//        {
//            public int? ContactId { get; set; }
//            public string FirstName { get; set; }
//            public string LastName { get; set; }
//            public string Email { get; set; }
//            public string Title { get; set; }
//            public string Description { get; set; }
//            public string BirthDate { get; set; }
//            public bool MaritalStatus { get; set; }

//            public Contact(int contactId, string firstName, string lastName, string email, string title, string description, string birthDate, bool maritalStatus)
//            {
//                ContactId = contactId;
//                FirstName = firstName;
//                LastName = lastName;
//                Email = email;
//                Title = title;
//                Description = description;
//                BirthDate = birthDate;
//                MaritalStatus = maritalStatus;
//            }
//        }
//    }
//}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EdgeDB;

namespace ContactDatabase.Pages;

public class IndexModel : PageModel
{
    [BindProperty]
    public Contact NewContact { get; set; } = new();
    public int counter { get; set; } = 0;
    private readonly EdgeDBClient _client;

    public IndexModel(EdgeDBClient client)
    {
        _client = client;
    }

    public void OnGet()
    {

    }

    public async Task<IActionResult> OnPost()
    {
        int counter = HttpContext.Session.GetInt32("ContactCounter") ?? 0;
        counter++;

        var query = "INSERT Contact {contact_id := <int64>$contact_id, first_name := <str>$first_name, last_name := <str>$last_name, email := <str>$email, title := <str>$title, description := <str>$description, birth_date := <str>$birth_date, marital_status := <bool>$marital_status}";

        await _client.ExecuteAsync(query, new Dictionary<string, object?>
        {
            {"contact_id", Convert.ToInt64(counter)},
            {"first_name", NewContact.FirstName},
            {"last_name", NewContact.LastName},
            {"email", NewContact.Email},
            {"title", NewContact.Title},
            {"description", NewContact.Description},
            {"birth_date", NewContact.BirthDate},
            {"marital_status", NewContact.MaritalStatus}
        });

        HttpContext.Session.SetInt32("ContactCounter", counter);

        return RedirectToPage("/ContactsList");
    }
}

public class Contact
{
    public int Id { get; set; }
    public String FirstName { get; set; }
    public String LastName { get; set; }
    public String Email { get; set; }
    public String Title { get; set; }
    public String Description { get; set; } = "";
    public String BirthDate { get; set; }
    public bool MaritalStatus { get; set; }

    public Contact()
    {

    }

    public Contact(int id, string firstName, string lastName, string email, string title, string description, string birthDate, bool maritalStatus)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Title = title;
        Description = description;
        BirthDate = birthDate;
        MaritalStatus = maritalStatus;
    }
}