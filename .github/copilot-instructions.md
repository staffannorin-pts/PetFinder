GitHub Copilot – gemensamma instruktioner för .NET-projekt
Övergripande regler
- Följ dessa instruktioner när du skapar, ändrar, granskar och föreslår kod i detta repository.
- Svara och förklara på svenska om inget annat efterfrågas. Använd engelska för kodidentifierare, klassnamn och metoder, i linje med befintlig kodbas.
- Följ befintliga kodkonventioner och gör små, tydliga ändringar. Föreslå inte onödiga ramverk eller arkitekturer.
- Säkerhet, underhållbarhet, testbarhet, tydlig felhantering och strukturerad loggning är obligatoriska.
- Generera komplett, kompilerbar kod med nödvändiga registreringar i dependency injection, konfiguration och tester när relevant.
- Gör inga tysta undantag från kraven. Om ett krav inte kan följas ska du påtala det och ange en säker lösning.
.NET och beroenden
- Nya applikationer och API:er ska använda senaste stabila, produktionsklara och Microsoft-supporterade LTS-versionen av .NET, för närvarande .NET 10 (november 2026). Använd inte preview/RC eller en STS-version enbart för att den har högre versionsnummer.
- Använd aktuell säkerhetspatch av vald .NET-version i drift och byggmiljö. Välj paketversioner som är kompatibla med projektets TargetFramework.
- Uppgradera inte befintliga äldre projekt eller stora paketberoenden utan att först analysera kompatibilitet och påverkan. Vid nyutveckling ska modern ASP.NET Core användas.
- För framework-dependent IIS-deployment: hårdkoda inte RuntimeFrameworkVersion eller TargetLatestRuntimePatch om det låser applikationen till en specifik patch. Förutsätt att driften håller installerad runtime uppdaterad. Använd inte self-contained deployment utan uttryckligt beslut och separat patchstrategi.
- Använd async/await och CancellationToken för I/O-operationer där det är lämpligt. Aktivera nullable reference types i nya projekt.
Arkitektur – Controller -> Service -> Repository
Följ strikt ansvarsfördelningen:
1. Controller: HTTP-endpoints, route, model binding, validering av indata, auth-policy, korrekt HTTP-status och svar (DTO/ProblemDetails). Ingen affärslogik eller direkt EF/DbContext-åtkomst.
2. Service: Affärslogik, domänregler, orkestrering, transaktionsgränser vid behov och mappning mellan entiteter/DTO:er. Anropar repositories via interface.
3. Repository: Databasfrågor och persistens med EF Core eller befintligt datalager. Inga HTTP-begrepp, controller-beslut eller API-responsmodeller här.
   - Använd constructor injection och DI för controllers, services, repositories och andra beroenden.
   - Föreslå enhetliga namn som IPermitService/`PermitService` och IPermitRepository/`PermitRepository`.
   - Returnera DTO:er från API:er, inte EF-entiteter. Undvik att exponera känsliga interna fält.
   - Använd AsNoTracking() vid läsningar som inte ska uppdateras och parametriserade databasfrågor.
   - Skapa inte ett extra generiskt repository-lager om det inte tillför verkligt värde; återanvänd etablerade repository-mönster.
API-säkerhet – API-key obligatoriskt
- Alla HTTP-API:er och endpoints som projektet äger ska som utgångspunkt skyddas med API-key, inklusive nya controller-actions och minimal API-endpoints. Autentisering ska vara deny-by-default och tillämpas centralt via ett testat filter, middleware eller autentiseringsschema/policy. Ett misstag eller glömd attributdekoration får inte lämna endpoints oskyddade.
- API-nyckeln ska normalt skickas i headern X-API-Key över HTTPS. Acceptera aldrig nyckeln i query string eller URL.
- Nycklar får aldrig hårdkodas eller committas i källkod, appsettings.json, testdata eller loggar. Läs dem från godkänd hemlighetshantering eller miljövariabler. Exempelkonfiguration får endast innehålla platshållare.
- Validera nyckeln säkert (t.ex. jämförelse i konstant tid där direkt jämförelse av hemlighet används). Stöd byte/rotation av nycklar utan kodändringar när möjligt.
- Saknad/ogiltig API-key ska leda till 401 Unauthorized med säkert, enhetligt ProblemDetails-svar utan att avslöja giltiga nycklar. 403 Forbidden används när en redan autentiserad klient saknar behörighet.
- En API-key är autentisering av anropande klient, inte en ersättning för användaridentitet, auktorisering eller resursbehörigheter. Kontrollera dessa separat där de behövs.
- All extern anropstrafik ska använda HTTPS; kontrollera proxy-/forwarded-header-konfiguration så att HTTPS-kravet fungerar korrekt bakom IIS/reverse proxy.
- Eventuella undantag, t.ex. specifik health check eller preflight, ska vara explicit godkända, dokumenterade och begränsade. Exponera inte administrativ statusinformation anonymt.
- För icke-HTTP-projekt gäller skyddet deras eventuella HTTP-gränssnitt. Bibliotek, batchjobb och rena datalager ska inte förses med meningslösa API-key-kontroller internt.
Felhantering och ProblemDetails
- Använd AddProblemDetails() och central, global exception handling, t.ex. IExceptionHandler tillsammans med UseExceptionHandler(), eller motsvarande stöd för vald ASP.NET Core-version.
- Returnera konsekventa ProblemDetails-svar (RFC 7807, eller aktuell efterföljande RFC 9457) för fel. Använd lämplig HTTP-status och en säker, begriplig titel och detaljtext.
- Lägg med traceId i ProblemDetails och koppla det till loggningen.
- Ingen stack trace, intern sökväg, SQL-fråga, exception-text med hemligheter eller annan intern information får exponeras i produktionssvar.
- Detailed errors ska vara aktiverade i lokal utveckling/test där åtkomst är kontrollerad (t.ex. Development) för felsökning och avaktiverade i produktion. Använd UseDeveloperExceptionPage() endast i Development och aktivera detaljerade fel i relevanta komponenter endast miljöstyrt.
- Fånga exception lokalt bara om det finns ett konkret återhämtningsbehov. Annars låter du den globala hanteraren hantera och logga felet en gång.
Loggning – Serilog + Seq
- Använd Serilog som gemensamt loggningsramverk, integrerat med ASP.NET Core och ILogger<T>.
- Skicka strukturerade logghändelser till Seq via konfigurerbar sink. Stöd även konsol-/fillogg där befintlig driftstandard kräver det.
- Skriv strukturerade meddelanden med namngivna fält, t.ex. logger.LogInformation("Permit {PermitId} updated", permitId) – inte stränginterpolering eller konkatenering i loggmallar.
- Använd lämpliga nivåer (Information, Warning, Error) och undvik dubbelloggning av samma exception.
- Använd Activity.Current?.TraceId / ASP.NET Cores HttpContext.TraceIdentifier och korrelationsinformation. Se till att TraceId läggs automatiskt till i loggar och ProblemDetails samt följer vidare i utgående HTTP-anrop via standardiserad distribuerad tracing när det är möjligt.
- Lägg till Serilog request logging med UseSerilogRequestLogging() eller en likvärdig lösning. Logga method, path utan hemliga query-parametrar, statuskod, duration och TraceId.
- RequestBody-loggning är ett krav men måste utföras säkert: använd central middleware med explicit kontroll över vilka endpoint-typer och content types som får loggas, storleksgräns, korrekt buffering/återställning av request stream och maskering av känsliga fält.
- Logga aldrig API-nycklar, Authorization/Cookie-headers, lösenord, tokens, personnummer, personuppgifter eller andra hemligheter. Logga inte multipart/filer eller binära payloads. För känsliga endpoints: logga endast metadata, body-storlek eller en redigerad/allowlistad sammanfattning.
- Begränsa mängden body-data och loggvolym, samt använd miljö-/konfigurationsstyrning och lämpliga åtkomst- och gallringsregler i Seq.
Rate limiting
- Använd ASP.NET Cores inbyggda rate limiting eller en likvärdig beprövad lösning för alla exponerade API:er.
- Välj policy efter klient/identifierad API-key när möjligt; använd inte enbart IP-adress bakom proxy utan genomtänkt forwarded-header-policy.
- Sätt lämpliga gränser per endpoint/klient, konfigurera köbeteende och returnera 429 Too Many Requests med Retry-After när det är möjligt. Använd ProblemDetails för konsekvent felformat.
- Dokumentera valda gränser. Bedöm särskilt inloggning, tunga sökningar och filuppladdningar.
Mapster
- Använd Mapster för mappning mellan entiteter, domänmodeller och DTO:er. Introducera inte AutoMapper i nya lösningar.
- Lägg komplexa mappningar i gemensam, tydlig TypeAdapterConfig/registrering i stället för utspridd logik.
- Behåll befintlig IMapper/mapper-wrapper om projektet använder detta för att minska ändringspåverkan.
- Skriv tester för mappningar med anpassade fält, nästlade objekt och enhetskonverteringar.
API-design, konfiguration och test
- Använd tydliga routes, DTO:er, validering, korrekta statuskoder, OpenAPI/Swagger (skydda swagger i miljöer där det krävs) och versionshantering där relevant.
- Håll hemligheter utanför repository. Skriv tydliga options-klasser, validera obligatorisk konfiguration vid uppstart och konfigurera olika miljöer korrekt.
- Tillför enhets-/integrationstester för Controller -> Service -> Repository, Mapster-mappning, API-key-godkännande/avslag, ProblemDetails, felhantering, TraceId, säker RequestBody-loggning och rate limiting.
- Verifiera att nya endpoints inte går att nå utan API-key, att hemligheter inte hamnar i loggar och att stack traces inte visas i produktionssvar.
- Utgå från befintliga bibliotek och registreringar i projektet innan du föreslår nya paket. Kontrollera paketens kompatibilitet och säkerhetsvarningar.
Databas
- Använd dapper för databasfrågor
- Skapa default en lokal databas.
Checklista för varje ändring
Innan du presenterar en lösning, kontrollera:
- [ ] Senaste godkända stabila .NET LTS och kompatibla paket används.
- [ ] Controller -> Service -> Repository respekteras.
- [ ] Alla nya HTTP-endpoints har centralt påtvingad API-key-kontroll, om inte ett godkänt undantag finns.
- [ ] ProblemDetails, global exception handling och säkra produktionsfel används.
- [ ] Detailed errors finns endast i kontrollerad utvecklings-/testmiljö.
- [ ] Serilog, Seq, strukturerad loggning och automatiskt TraceId används.
- [ ] RequestBody-loggning är redigerad, begränsad och utan hemligheter/personuppgifter.
- [ ] Rate limiting och korrekta 429-svar finns.
- [ ] Mapster används för mappning.
- [ ] DI, konfiguration, relevanta tester och dokumentation är uppdaterade.