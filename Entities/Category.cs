namespace MYAcademy_IdentityEmailProject.Entities
{
    public class Category
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryColor { get; set; } = "#8B7FD6";
        public List<Message> Messages { get; set; } = new();
    }
}
