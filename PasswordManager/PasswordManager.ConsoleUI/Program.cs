using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PasswordManager.Application.Services;
using PasswordManager.Domain.Interfaces;
using PasswordManager.Infrastructure.Data;
using PasswordManager.Infrastructure.Repositories;
using PasswordManager.Infrastructure.Services;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    })
    .ConfigureServices((context, services) =>
    {
        var connectionString = context.Configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));
        
        services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<UserService>();
        services.AddSingleton<IEncryptionService, AesGcmEncryptionService>();
        services.AddScoped<ICredentialRepository, CredentialRepository>();
        services.AddScoped<VaultService>();
    });

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();    

    var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
    var userService = scope.ServiceProvider.GetRequiredService<UserService>();

    Console.Clear();
    Console.WriteLine("=======================================");
    Console.WriteLine("     B L U E 1 0   P A S S W O R D S   ");
    Console.WriteLine("=======================================\n");

    bool hasUser = await userRepository.HasAnyUsersAsync();

    if (!hasUser)
    {
        // --- REGISTRATIE FLOW ---
        Console.WriteLine("Welkom! Het lijkt erop dat je nieuw bent.");
        Console.WriteLine("Laten we eerst je Master Password instellen.");
        Console.WriteLine("Eisen: Minimaal 6 tekens, minimaal 1 hoofdletter, minimaal 1 cijfer.\n");
        
        while (true)
        {
            Console.Write("Voer je nieuwe Master Password in: ");
            string password = ReadPassword();

            Console.Write("Bevestig je Master Password:       ");
            string confirmPassword = ReadPassword();

            if (password != confirmPassword)
            {
                Console.WriteLine("\n[FOUT] Wachtwoorden komen niet overeen. Probeer het opnieuw.\n");
                continue;
            }

            try
            {
                Console.WriteLine("\nEen moment geduld, je wachtwoord wordt veilig opgeslagen...");
                await userService.SetupMasterPasswordAsync(password);
                Console.WriteLine("[SUCCES] Master Password succesvol ingesteld! Start de app opnieuw om in te loggen.");
                break; 
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"\n[FOUT] {ex.Message}\n");
            }
        }
    }
    else
    {
        // --- LOGIN FLOW ---
        Console.WriteLine("Welkom terug! Log in om je kluis te openen.\n");
        
        while (true)
        {
            Console.Write("Master Password: ");
            string password = ReadPassword();

            Console.WriteLine("Controleren...");
            
            var user = await userService.VerifyMasterPasswordAsync(password);

            if (user != null)
            {
                Console.WriteLine("\n[SUCCES] Inloggen succesvol!");
                
                var vaultService = scope.ServiceProvider.GetRequiredService<VaultService>();
                vaultService.UnlockVault(user, password); 
                
                bool keepRunning = true;
                while (keepRunning)
                {
                    Console.WriteLine("\n=== JOUW KLUIS ===");
                    Console.WriteLine("1. Bekijk opgeslagen wachtwoorden");
                    Console.WriteLine("2. Voeg nieuw wachtwoord toe");
                    Console.WriteLine("3. Wachtwoord verwijderen");
                    Console.WriteLine("4. Afsluiten");
                    Console.Write("\nKies een optie (1-4): ");
                    
                    var choice = Console.ReadLine();

                    switch (choice)
                    {
                        case "1":
                            var passwords = await vaultService.GetAllCredentialsAsync();
                            Console.WriteLine("\n--- OPGESLAGEN WACHTWOORDEN ---");
                            if (!passwords.Any())
                            {
                                Console.WriteLine("Je kluis is nog leeg.");
                            }
                            else
                            {
                                int counter = 1;
                                foreach (var p in passwords)
                                {
                                    Console.WriteLine($"[{counter}] Titel: {p.Title} | Gebruiker: {p.Username}");
                                    Console.WriteLine($"    Wachtwoord: {p.PlainPassword}");
                                    counter++;
                                }
                            }
                            break;

                        case "2":
                            Console.WriteLine("\n--- NIEUW WACHTWOORD TOEVOEGEN ---");
                            Console.Write("Titel (bijv. Google): ");
                            string title = Console.ReadLine() ?? "";
                            
                            Console.Write("Gebruikersnaam / Email: ");
                            string username = Console.ReadLine() ?? "";
                            
                            Console.Write("URL (optioneel): ");
                            string url = Console.ReadLine() ?? "";
                            
                            Console.Write("Wachtwoord: ");
                            string newPassword = ReadPassword(); 

                            try
                            {
                                await vaultService.AddCredentialAsync(title, username, url, newPassword);
                                Console.WriteLine("[SUCCES] Wachtwoord succesvol en versleuteld opgeslagen!");
                            }
                            catch (ArgumentException ex)
                            {
                                Console.WriteLine($"\n[FOUT] Mislukt: {ex.Message}");
                            }
                            break;

                        case "3":
                            Console.WriteLine("\n--- WACHTWOORD VERWIJDEREN ---");
                            
                            var deleteList = await vaultService.GetAllCredentialsAsync();
                            if (!deleteList.Any()) 
                            {
                                Console.WriteLine("Je kluis is leeg. Er valt niets te verwijderen."); 
                                break;
                            }
                            
                            for (int i = 0; i < deleteList.Count; i++)
                            {
                                Console.WriteLine($"{i + 1}. {deleteList[i].Title} ({deleteList[i].Username})");
                            }

                            Console.Write("\nTyp het nummer van het wachtwoord dat je wilt verwijderen (of 0 om te annuleren): ");
                            
                            if (int.TryParse(Console.ReadLine(), out int deleteChoice))
                            {
                                if (deleteChoice == 0)
                                {
                                    Console.WriteLine("Verwijderen geannuleerd.");
                                    break;
                                }

                                if (deleteChoice > 0 && deleteChoice <= deleteList.Count)
                                {
                                    var idToDelete = deleteList[deleteChoice - 1].Id;
                                    var titleToDelete = deleteList[deleteChoice - 1].Title;

                                    Console.Write($"Weet je zeker dat je '{titleToDelete}' wilt verwijderen? (j/n): ");
                                    if (Console.ReadLine()?.ToLower() == "j")
                                    {
                                        try
                                        {
                                            await vaultService.DeleteCredentialAsync(idToDelete);
                                            Console.WriteLine("[SUCCES] Wachtwoord is verwijderd uit je kluis.");
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine($"[FOUT] Kon niet verwijderen: {ex.Message}");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Verwijderen geannuleerd.");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("[FOUT] Dat nummer staat niet in de lijst.");
                                }
                            }
                            else
                            {
                                Console.WriteLine("[FOUT] Vul een geldig getal in.");
                            }
                            break;

                        case "4":
                            keepRunning = false;
                            Console.WriteLine("Kluis wordt gesloten. Tot ziens!");
                            break;

                        default:
                            Console.WriteLine("[FOUT] Ongeldige keuze, probeer het opnieuw.");
                            break;
                    }
                }
                break; 
            }
            else
            {
                Console.WriteLine("[FOUT] Incorrect Master Password. Probeer het opnieuw.\n");
            }
        }
    }
}

static string ReadPassword()
{
    string password = "";
    ConsoleKeyInfo info = Console.ReadKey(true);
    
    while (info.Key != ConsoleKey.Enter)
    {
        if (info.Key != ConsoleKey.Backspace)
        {
            Console.Write("*");
            password += info.KeyChar;
        }
        else if (info.Key == ConsoleKey.Backspace)
        {
            if (!string.IsNullOrEmpty(password))
            {
                password = password.Substring(0, password.Length - 1);
                
                int pos = Console.CursorLeft;
                Console.SetCursorPosition(pos - 1, Console.CursorTop);
                Console.Write(" ");
                Console.SetCursorPosition(pos - 1, Console.CursorTop);
            }
        }
        info = Console.ReadKey(true);
    }
    Console.WriteLine();
    return password;
}