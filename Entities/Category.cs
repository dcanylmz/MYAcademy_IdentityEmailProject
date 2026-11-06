namespace MYAcademy_IdentityEmailProject.Entities
{
    public class Category
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public List<Message> Messages { get; set; }
    }
}
