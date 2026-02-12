namespace CRM.DTOS.Customers
{
    public class CreatCustomerDto
    {
        public string CompanyName { get; set; }
        public string Industry { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string AssignedToId { get; set; }
    }
}
