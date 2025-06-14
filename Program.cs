var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

string dataPath = Path.Combine(Directory.GetCurrentDirectory(), "Data");
string imagePath = Path.Combine(app.Environment.WebRootPath, "uploads");

if (!Directory.Exists(dataPath)) Directory.CreateDirectory(dataPath);
if (!Directory.Exists(imagePath)) Directory.CreateDirectory(imagePath);

app.MapGet("/", async context =>
{
    await context.Response.SendFileAsync("index.html");
});

// ดึงข้อความของวัน
app.MapGet("/api/message/{day}", (string day) =>
{
    var file = Path.Combine(dataPath, $"{day.ToLower()}.txt");
    return File.Exists(file) ? File.ReadAllText(file) : "";
});

// บันทึกข้อความใหม่
app.MapPost("/api/message/{day}", async (HttpRequest request, string day) =>
{
    var text = await new StreamReader(request.Body).ReadToEndAsync();
    File.WriteAllText(Path.Combine(dataPath, $"{day.ToLower()}.txt"), text);
    return Results.Ok("ข้อความถูกบันทึก");
});

// อัปโหลดภาพใหม่
app.MapPost("/api/image/{day}", async (HttpRequest request, string day) =>
{
    var form = await request.ReadFormAsync();
    var file = form.Files.FirstOrDefault();
    if (file != null)
    {
        var savePath = Path.Combine(imagePath, $"{day.ToLower()}.jpg");
        using var stream = new FileStream(savePath, FileMode.Create);
        await file.CopyToAsync(stream);
        return Results.Ok("ภาพถูกอัปโหลด");
    }
    return Results.BadRequest("ไม่พบไฟล์");
});

// ดูภาพปัจจุบัน
app.MapGet("/uploads/{filename}", async (HttpContext ctx, string filename) =>
{
    var file = Path.Combine(imagePath, filename);
    if (File.Exists(file))
        await ctx.Response.SendFileAsync(file);
    else
        ctx.Response.StatusCode = 404;
});

app.Run();
