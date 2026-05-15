## Blue10 Stage-Opdracht: CLI Password Manager

Deze repository bevat mijn uitwerking voor de backend stage-opdracht bij Blue10. Het project is een command-line gebaseerde Password Manager, gebouwd in C# / .NET, met een sterke focus op security en een schone, schaalbare architectuur.

Kenmerken & Opdracht Eisen

-   Master Password Authenticatie: Toegang tot de kluis wordt beveiligd via een master password.

-   Veilige Opslag: Inloggegevens (wachtwoorden, usernames) worden zwaar versleuteld in de database opgeslagen.

-   Database Integratie: Gebruik van PostgreSQL via Entity Framework Core.

-   Gestructureerde Opzet: De applicatie volgt het principe van Clean Architecture / N-Tier, waardoor de CLI volledig ontkoppeld is van de database en business logica.

Technologie Stack

-   Taal & Framework: C# 12 / .NET 10 (Console App)

-   Database: PostgreSQL

-   ORM: Entity Framework Core (Code-First Migrations)

-   Cryptografie: Konscious.Security.Cryptography.Argon2 en System.Security.Cryptography.AesGcm

Security & Architectuur (Hoe het werkt) Tijdens de ontwikkeling stonden veiligheid en structuur voorop:

1.  Clean Architecture: Het project is opgedeeld in 4 lagen (Domain, Application, Infrastructure, ConsoleUI).

2.  Authenticatie (Argon2id): Het master password wordt gehasht opgeslagen in de database met behulp van Argon2id inclusief Salt en Pepper.

3.  Encryptie (AES-256-GCM): Wachtwoorden in de kluis worden nooit in leesbare tekst opgeslagen. PBKDF2 leidt de sleutel af en AES-256-GCM versleutelt de data.

Hoe start je de applicatie lokaal?

1.  Vereisten .NET SDK geïnstalleerd en een draaiende PostgreSQL server.

2.  Configuratie instellen Zorg ervoor dat in het project PasswordManager.ConsoleUI een bestand genaamd appsettings.json staat met de volgende inhoud:

{ "ConnectionStrings": { "DefaultConnection": "Host=localhost;Database=PasswordManagerDb;Username=JOUW_USERNAME;Password=JOUW_WACHTWOORD" }, "Security": { "Pepper": "Zet_Hier_Een_Lange_Willekeurige_String_Neer" } }

1.  Database Migraties Toepassen De database wordt automatisch aangemaakt via Entity Framework Core Migrations wanneer de applicatie voor het eerst start.

2.  Applicatie Runnen Navigeer met je terminal naar de root van de solution en voer dit commando uit:

dotnet run --project PasswordManager.ConsoleUI/PasswordManager.ConsoleUI.csproj

De CLI zal opstarten, de database klaarmaken (indien nodig), en het menu openen.****
