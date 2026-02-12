namespace CRM.Models
{
    public class CreateCustomerViewModel
    {
        public Customer Customer { get; set; } = new();
        public IEnumerable<ApplicationUser> SalesExecutives { get; set; }
        public string SelectedSalesExecutiveId { get; set; }
    }
}
