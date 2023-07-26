using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EdgeDB;
using System;

namespace ContactDatabase.Pages;

public class IndexModel : PageModel
{
    [BindProperty]
    public Contact NewContact { get; set; } = new();
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
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Title { get; set; }
    public string Description { get; set; } = "";
    public string BirthDate { get; set; }
    public bool MaritalStatus { get; set; }


    public Contact(string firstName, string lastName, string email, string title, string description, string birthDate, bool maritalStatus)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Title = title;
        Description = description;
        BirthDate = birthDate;
        MaritalStatus = maritalStatus;
    }

    public Contact()
    {
    }
}