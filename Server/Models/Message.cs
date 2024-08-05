using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Server.Models
{
    public enum Command
    {
        Login,
        Message,
        Confirmation,
        ServerAnswer     
    }
    public class Message
    {
        [NotMapped]
        public Command Command { get; set; }
        public int? Id { get; set; }
        public string? Text { get; set; }
        public DateTime TimeMessage { get; set; }
        public bool ReceivedStatus { get; set; } = false;

        public int ToUserId { get; set; }
        public User? ToUser { get; set; }

        public int FromUserId { get; set; }
        public User? FromUser { get; set; }

        public static Message? FromJson(string json)
        {
            return JsonSerializer.Deserialize<Message>(json);
        }
        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }
        public override string ToString()
        {
            return $"[{TimeMessage.ToShortTimeString()}] {FromUser?.Name}: {Text}";
        }
    }
}
