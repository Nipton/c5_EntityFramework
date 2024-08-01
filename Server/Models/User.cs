using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    [Index("Name", IsUnique = true)]
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<Message> MessagesToSend { get; set; }
        public virtual ICollection<Message> MessagesToReceive { get; set; }
        public User(string name) 
        {
            MessagesToSend = new List<Message>();
            MessagesToReceive = new List<Message>();
            Name = name;
        }
    }
}
