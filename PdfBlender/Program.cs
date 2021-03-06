/*
PDF Blender - A web based pdf merger powered by iText
Copyright (C) 2022 freshbit

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as
published by the Free Software Foundation, either version 3 of the
License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using Microsoft.AspNetCore.HttpOverrides;
using PdfBlender.Core;
using PdfBlender.Services;

var builder = WebApplication.CreateBuilder(args);

var logFilePath = builder.Configuration["LogFilePath"] ?? "/var/log/pdf-blender.log";
var logActive = bool.Parse(builder.Configuration["Log"] ?? "false");
var enableCors = bool.Parse(builder.Configuration["EnableCors"] ?? "false");

builder.Services.AddSingleton(new AppLogger(logActive, logFilePath));
builder.Services.AddScoped<IPdfManager, PdfManager>();

if (enableCors)
{
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(
            policyBuilder =>
            {
                policyBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            });
    });
}

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
}
else
{
    builder.Services.AddControllersWithViews();
}

builder.Services.Configure<RouteOptions>(c =>
{
    c.LowercaseUrls = true;
    c.AppendTrailingSlash = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | 
                           ForwardedHeaders.XForwardedProto
    });
    
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Main}/{action=Index}/{id?}");

app.Run();