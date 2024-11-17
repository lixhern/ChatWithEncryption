namespace ChatWithEncryption.Models
{
    public class ContactRequest
    {
        public int Id { get; set; }
        public string RequesterId { get; set; }  // Идентификатор пользователя, который отправил запрос
        public string TargetId { get; set; }  // Идентификатор получателя запроса
        public bool IsAccepted { get; set; }  // Статус запроса (принят/отклонен)
        public DateTime RequestTime { get; set; }  // Время отправки запроса
    }
}
