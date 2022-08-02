using Microsoft.EntityFrameworkCore; //EfCore Eklenmesi
using ToDoApp3.Modals; //Modallar buraya taşınması
using ToDoApp3.Auth; //auhentification functionu eklenmesi

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();


// Bu fonksyon Autn.cs sinifta var olan getUserId() function kulanak, bağlanmiş olan user'in id'si gonderir
// User yoksa ya da bilgir yanlış ise 0 döndürmektedir

int getUserIdFunction(HttpContext httpContext, TodoDb db) {

    string[] userCredential = Auth.getUserId(httpContext);

    string userEmail = "";
    string password = "";
    int userId = 0;

    if (userCredential[0] != "InvalidCreadentials")
    {
        userEmail = userCredential[0];
        password = userCredential[1];
        
        var helper = async () =>
        {
            try
            {
                userId = (await db.Users.Where(user => user.Email == userEmail && user.Password == password)
                .ToListAsync())[0].Id;
            }
            catch (Exception e)
            {
                //Bir şerler gönder...
            }
        };
        helper();
    }

    return userId;

};



//Ana Sayfa-------------

app.MapGet("", (HttpContext httpContext, TodoDb db) => 
{

    //Auth.getUserId(httpContext);

    return
    @"

    --- Demo Staj ToDo list API ---
    ---------------------------------------------------------
    Bu API bir ToDo listesinin Backend'i olarak yer alabilir
    ---------------------------------------------------------

    Mevcüt endpoint'lar :
    -------------------

    GET api/todoitems 
    GET api/todoitems/id
    POST api/todoitems
    PUT api/todoitems/id
    DELETE api/todoitems/id

    --------------------------
    
    GET api/todoitems 
    GET api/todoitems/id
    POST api/todoitems
    PUT api/todoitems/id
    DELETE api/todoitems/id

    ----------------------------

    --- ©Hilal Derya Eryilmaz ------
    ";

});

//END ANASAYFA


//Users ile ilgili olan End-points

app.MapGet("api/users", async (TodoDb db) =>
    await db.Users.ToListAsync());


app.MapGet("api/users/{id}", async (int id, TodoDb db) =>
    await db.Users.FindAsync(id) is User user
            ? Results.Ok(user)
            : Results.NotFound());

app.MapPost("api/users", async (User user, TodoDb db) =>
{
    db.Users.Add(user);
    await db.SaveChangesAsync();

    return Results.Created($"/users/{user.Id}", user);
});
//Users Update EndPoint
app.MapPut("api/users/{id}", async (int id, User yeniUser, TodoDb db) =>
{
    var user = await db.Users.FindAsync(id);

    if (user is null) return Results.NotFound();

    user.Name = yeniUser.Name;
    user.Email = yeniUser.Email;
    user.Password = yeniUser.Password;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

//User delete EnnPoint
app.MapDelete("api/users/{id}", async (int id, TodoDb db) =>
{
    if (await db.Users.FindAsync(id) is User user)
    {
        db.Users.Remove(user);
        await db.SaveChangesAsync();
        return Results.Ok(user);
    }

    return Results.NotFound();
});

//END USERS------


//Todo Enpoint--------

app.MapGet("api/todoitems", 
    async (HttpContext httpContext, TodoDb db) =>
    {
        //user Id request'ten çekiyoruz...
        int userId = getUserIdFunction(httpContext, db);
        var user = await db.Users.FindAsync(userId);

        // Kullanıcı bilgileri yanlış ise
        if (user is null) return Results.NotFound(
            @"Authorization kısımda kullanıncı bilgileri girmeniz gerek. ya da girdikleriniz yanlıştır"
        );

        return Results.Ok(await db.Todos.Where(todo => todo.UserId == userId).ToListAsync());
    }
);

// GET todoitems/{id}
app.MapGet("api/todoitems/{id}",
    async (int id, HttpContext httpContext, TodoDb db) =>
    {
        //user Id request'ten çekiyoruz...
        int userId = getUserIdFunction(httpContext, db);
        var user = await db.Users.FindAsync(userId);
        var todo = await db.Todos.FindAsync(id);

        // Kullanıcı bilgileri yanlış ise
        if (user is null) return Results.NotFound(
            @"Authorization kısımda kullanıncı bilgileri girmeniz gerek. ya da girdikleriniz yanlıştır"
        );

        if (todo is null) return Results.NotFound();
        
        return Results.Ok(todo);
    }
);


// POST todoitems/{id}
app.MapPost("api/todoitems", async (Todo todo, HttpContext httpContext, TodoDb db) =>
{

    int userId = getUserIdFunction(httpContext, db);
    var user = await db.Users.FindAsync(userId);
    
    //Boyle bir kullanıcı yoksa
    if (user is null) return Results.NotFound (
        @"Authorization kısımda kullanıncı bilgileri girmeniz gerek. ya da girdikleriniz yanlıştır"
    );
    //Kullanıcı idsi eklenmesi
    todo.Id = userId;

    db.Todos.Add(todo);
    await db.SaveChangesAsync();
    return Results.Created($"/todoitems/{todo.Id}", todo);

});

//PUT /todoitems/{id}
app.MapPut("api/todoitems/{id}", async (int id, Todo inputTodo, HttpContext httpContext, TodoDb db) =>
{
    int userId = getUserIdFunction(httpContext, db);
    var user = await db.Users.FindAsync(userId);
    var todo = await db.Todos.FindAsync(id);

    if (todo is null || user is null) return Results.NotFound();

    todo.Description = inputTodo.Description;
    todo.IsCompleted = inputTodo.IsCompleted;
    todo.UserId = userId;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

//DELETE /todoitems/{id}
app.MapDelete("api/todoitems/{id}", async (int id, HttpContext httpContext, TodoDb db) =>
{

    int userId = getUserIdFunction(httpContext, db);
    var user = await db.Users.FindAsync(userId);
    var todo = await db.Todos.FindAsync(id);

    //Boyle bir kullanıcı yoksa
    if (user is null) return Results.NotFound(
        @"Authorization kısımda kullanıncı bilgileri girmeniz gerek. ya da girdikleriniz yanlıştır"
    );

    //
    if ( !(todo is null) )
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return Results.Ok(todo);
    }

    return Results.NotFound();
});

//END ----------------


//Start listening on a random port...
app.Run();

