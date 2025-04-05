using System.Text.Json.Serialization;
using PlayTimeManager;
using PlayTimeManager.Models.Database;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<AppDbContext>();
// builder.Services.AddAuthentication("Basic").AddScheme<AuthenticationSchemeOptions, TokenSchemeHandler>("Basic", x => { });
// builder.Services.AddAuthorization(conf =>
// {
//     conf.AddPolicy("rw", builder => builder.AddAuthenticationSchemes("Basic").RequireRole("rw"));
//     conf.AddPolicy("ro", builder => builder.AddAuthenticationSchemes("Basic").RequireRole("ro"));
// });

var app = builder.Build();

// app.UseExceptionHandler();
app.UseHttpsRedirection();
// app.UseAuthentication();
// app.UseAuthorization();
app.MapControllers();

app.Run();

// типы получаемые/отправляемые в контроллерах
[JsonSerializable(typeof(List<PlayTime>))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}