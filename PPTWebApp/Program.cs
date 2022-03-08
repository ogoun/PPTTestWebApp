using Microsoft.AspNetCore.Http.Features;
using PPTDataLibrary;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<PPTFilesStorage>(new PPTFilesStorage("pptfolder"));
builder.Services.Configure<FormOptions>(x =>
{
    x.ValueLengthLimit = int.MaxValue;
    x.MultipartBodyLengthLimit = int.MaxValue;
    x.MultipartHeadersLengthLimit = int.MaxValue;
});
builder.Services.AddControllers();

var app = builder.Build();
app.UseAuthorization();
app.MapControllers();
app.Run();