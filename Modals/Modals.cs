using Microsoft.EntityFrameworkCore;

namespace ToDoApp3.Modals
{
    //Veri Tabanı ---------------
    public class Todo
    {
        public int Id { get; set; }
        public string? Description { get; set; } 
        //public DateOnly? DueDate { get; set; }
        public bool IsCompleted { get; set; }
        public int ? UserId { get; set; }
    }

    //Ulanıcı tablosu ToDo Tablosu ile 
    public class User
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    //Veritabaninin Context'i yani Sanal bir veri tabanı olarak çalışıyor. Program kapanıca boşaltır
    //Gerçek bir veri tabanının yapısını tutan bir nesne

    public class TodoDb : DbContext
    {
        public TodoDb(DbContextOptions<TodoDb> options) : base(options) { }

        public DbSet<Todo> Todos => Set<Todo>();
        public DbSet<User> Users => Set<User>();
        
    }
}
