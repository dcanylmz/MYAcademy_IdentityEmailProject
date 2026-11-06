namespace MYAcademy_IdentityEmailProject.Entities
{
    public class Message
    {
        public int MessageID { get; set; }
        public string MessageSubject { get; set; }
        public string MessageDetail { get; set; }
        public string SenderEmail { get; set; }
        public string ReceiverEmail { get; set; }
        public DateTime SendDate { get; set; }

        public bool IsRead { get; set; }
        public bool IsFavourite { get; set; }
        public int CategoryID { get; set; }
        public Category Category { get; set; }
    }
}
