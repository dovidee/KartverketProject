# Kartverket Project

Registrer og se hindringer i kartet, også uten nettforbindelse.

## Innholdsfortegnelse

### Oppsett
1. [Kom i gang](#kom-i-gang)
2. [Offlinekart](#offlinekart)
3. [Testbrukere](#testbrukere)

### Feil
4. [Tilbakestille migrasjoner](#tilbakestille-migrasjoner)
5. [An error occurred using the connection to the database](#an-error-occurred-using-the-connection-to-the-database)

### Arkitektur
6. [Model View Controller](#model-view-controller)
7. [Docker](#docker)
8. [Frontend](#frontend)
9. [Backend](#backend)
10. [Systemkontekstdiagram](#systemkontekstdiagram)
11. [Mermaiddiagram](#mermaiddiagram)

### Testing
12. [Enhetstest](#enhetstest)
13. [Systemtest](#systemtest)
14. [Sikkerhetstest](#sikkerhetstest)
15. [Brukertest](#brukertest)

### Bidragsytere
16. [Bidragsytere](#bidragsytere-1)

## Kom i gang


1. Åpne cmd og klon prosjektet


![CMD](images/cmd1.png)


2. Åpne solution-filen i Visual Studio


![SOL](images/solution2.png)


3. Høyreklikk på docker-compose, hold musen over Add, og klikk New Item.


![dockercompose](images/add3.png)


4. Gi filen navnet .env


![ENV](images/env4.png)


5. I .env skriver du DBPASSWORD= og deretter et vilkårlig passord. Sørg for at appsettings.json også inneholder passordet i Pwd=, ellers vil det ikke fungere.


![PASS](images/apppass5.png)


6. I Visual Studio:


Tools -> NuGet Package Manager -> Package Manager Console


7. Kjør følgende kommando


docker compose up --build


8. Gå til http://localhost:8082 og logg inn med testbrukerne.

9. For å se kartet, last ned offlinekartet fra Releases-siden eller med lenken under.

## Offlinekart

For å kunne se kartet, last ned ZIP-filen på 1,17 GB under.

http://github.com/dovidee/KartverketProject/releases/latest/download/norway.zip

Pakk ut mappen og legg filen norway.mbtiles i KartverketProject/KartverketProject/wwwroot/

## Testbrukere

brukernavn:passord

1. johnd:admin (NLA, admin)
  
2. janed:admin (NLA, reviewer)
   
3. bobs:admin (NLA, user)

4. janiced:admin (Luftsforsvaret, reviewer)

## Tilbakestille migrasjoner


1. Slett migrations-mappen


![MIG](images/migrations15.png)

2. Bytt til KartverketProject 


![SEL](images/selectdockercompose8.png)


3. I Visual Studio:


Tools -> NuGet Package Manager -> Package Manager Console


4. Kjør følgende kommandoer

Add-Migration NewMigration

Update-Database

## An error occurred using the connection to the database 

1. Åpne cmd, list opp med «docker volume ls» og kjør deretter «docker volume rm {VOLUMENAME HERE}». Hvis den sier at volumet er i bruk, gå til Docker Desktop og slett containeren.

![VOL](images/volume6.png)


![DEL](images/deletecompose7.png)

2. Kjør prosjektet som docker-compose for å sette opp volumet på nytt.


## Systemarkitektur

### Model View Controller

MVC gjør det enklere å kode, feilsøke og teste noe som kun har en oppgave.

![MVC](images/mvc14.png)

Modellen representerer forretningslogikken eller operasjonene. Dette kan være i form av feilmeldinger eller lagring av dataoverføringsobjekter.

Viewet har ansvar for å presentere innhold gjennom brukergrensesnittet. Dette omfatter layout og sider.

Controlleren håndterer brukerinteraksjon og styrer hvordan applikasjonen svarer på en gitt forespørsel.

Brukeren ønsker å registrere en bruker. POST-forespørselen treffer controlleren, som deretter mottar modellen. Hvis modellvalideringen feiler, lagrer modellstatusen feilen. Controlleren sjekker så om modellstatusen er gyldig og returnerer viewet.

### Docker

Docker er en plattform som pakker applikasjonen og avhengighetene dens inn i en container.

Dockerfilen inneholder instruksjonene for å bygge et Docker-image.

Imaget brukes deretter til å bygge applikasjonen.

docker-compose.yml er en konfigurasjonsfil som setter opp containerne, der den henter passordet fra .env-filen.

Applikasjonen monterer så volumene fra verten til containeren.

### Frontend

Statiske filer serveres fra wwwroot til brukerens nettleser.

https://github.com/dovidee/KartverketProject/blob/c7bc85a6db046f4227ac6778df9241b47b521a0c/KartverketProject/Program.cs#L102

CSS brukes til å utforme nettsiden. Prosjektet bruker Tailwind CSS for å forenkle dette.

JS brukes til å gjøre siden interaktiv. Prosjektet bruker Leaflet til å lage kartet.

### Backend

ApplicationDbContext bruker dependency injection for å hente tjenester som ASP.NET Core Identity til å logge inn og registrere brukere.

Rollene admin, reviewer og user opprettes. Brukeren opprettes med et hashet passord, ettersom lagring av passord i klartekst er en sikkerhetsrisiko.

IdentityUser er tilpasset fra User-modellen med tilleggsattributter som Department og Active, slik interessentene krevde.

Når modellene er definert, opprettes tabellene ved å migrere og oppdatere databasen gjennom objektrelasjonell mapping.

Prosjektet bruker Entity Framework, som støtter LINQ-spørringer som utfører Create-, Read-, Update- og Delete-operasjoner på databasen.

Controlleren har deretter ansvar for å returnere views, model binding, modellvalidering og modellfeil.

### Mermaiddiagram

![MMD](images/mermaiddiagram28.png)

Laget med https://mermaid.live

### Systemkontekstdiagram

![SCD](images/systemcontextdiagram11_v2.png)

Basert på C4-modellen: https://c4model.com/diagrams/system-context

## Enhetstest

### Validering av modellstatus
Sjekker om modellstatusen er gyldig
https://github.com/dovidee/KartverketProject/blob/9073420b0a123a217a8d737adba32ce542875756/KartverketTest/Test1.cs#L17-L34

### Innsending av hindring
Sjekker om hindringen blir lagret
https://github.com/dovidee/KartverketProject/blob/9073420b0a123a217a8d737adba32ce542875756/KartverketTest/Test1.cs#L40-L75

### Videresending ved innlogging
Sjekker om brukeren blir videresendt når hen er logget inn
https://github.com/dovidee/KartverketProject/blob/9073420b0a123a217a8d737adba32ce542875756/KartverketTest/Test1.cs#L81-L122

### Resultater
 
![UNI](images/unittesting13.png)

## Systemtest

### Verdiområde

Under systemtesting førte redigering av rapporthøyden med en stor verdi til dette problemet:

"Value was either too large or too small for an Int32."

![ONL](images/range26.png)

Verdiområdet ble rettet fra [Range(0, 200)] til [Range(0.0, 200.0)]

### Tomt skjema

Brukeren sender inn tomme data i skjemaet.

![EMP](images/empty24.png)

Utkastet kan nå redigeres med de tomme dataene.

![FIL](images/filled25.png)

### Offlinekart

Kartet vises online (uten struping) med grønn HTTP-status (200)

![ONL](images/online22.png)

Kartet vises offline uten HTTP-status.

![OFL](images/offline23.png)

## Sikkerhetstest

### ZAP

ZAP avdekket Content Security Policy som en høy risiko.
Bruken av Tailwind CDN, HTTP og uspesifisert Content-Type er en sikkerhetsrisiko.
I produksjon ville dataene blitt lagret lokalt i stedet.
I tillegg ville HTTP blitt migrert til HTTPS for å unngå at passord i klartekst er synlige over nettverket.

[Se ZAP-rapporten](security/zapscan.html)

Last ned ZAP-rapporten over for å se sikkerhetsproblemene.

### CIA-triaden

#### Konfidensialitet

Reviewere er begrenset ut fra disse kriteriene:

1. Om de eier rapporten
2. Om rapporten er delt med dem
3. Om de tilhører samme avdeling

https://github.com/dovidee/KartverketProject/blob/9073420b0a123a217a8d737adba32ce542875756/KartverketProject/Controllers/AccountController.cs#L398-L401

Hvis en rapport er delt med dem, kan de ikke dele den videre.

https://github.com/dovidee/KartverketProject/blob/9073420b0a123a217a8d737adba32ce542875756/KartverketProject/Controllers/AccountController.cs#L470-L472

Dette ivaretar konfidensialitet, ettersom reviewere ikke kan dele rapporten videre til standardbrukere.
I tillegg krever need-to-know-prinsippet at kun brukere som trenger informasjonen skal ha tilgang til den.

#### Integritet

Root-brukere har full tilgang til filsystemet.

https://github.com/dovidee/KartverketProject/blob/4dfe9b01d0d3ad47ad11f4ed9ea5672a0cce5419/docker-compose.yml#L19-L24

Å sette opp en appuser isolerer containeren, slik at det blir vanskeligere for angripere å kartlegge systemet for sårbarheter.
I tillegg krever prinsippet om minste privilegium at brukere skal ha minst mulig tilgang for å utføre en oppgave.

#### Tilgjengelighet

Angripere kan oversvømme databasen med forespørsler for å ta ned tjenesten.

https://github.com/dovidee/KartverketProject/blob/4dfe9b01d0d3ad47ad11f4ed9ea5672a0cce5419/docker-compose.yml#L30-L35

Helsesjekken sørger for at mariadb-tjenesten holdes i gang.

### OWASP: Security Misconfiguration

#### Stack Trace

Stack trace kan avsløre feil som kan brukes til feilbasert SQL eller XSS.

![STACK](images/stacktrace16.png)

En exception handler videresender brukeren til en feilside i stedet for å vise stack tracen.
Under utvikling trenger utviklere stack tracen for å finne problemer.

### OWASP: Identification and Authentication Failures 

#### Brute Force

På usikre nettsider kan angripere avdekke gyldige brukernavn fordi feilmeldingene skiller mellom dem:

1. "Username/email already taken" bekrefter at brukernavnet allerede finnes.
2. "Incorrect password" bekrefter at brukernavnet er riktig, men at passordet er feil.

Denne applikasjonen viser i stedet en generisk melding, "Invalid login attempt", uansett hva som er feil.
Etter 5 mislykkede forsøk låses kontoen i 15 minutter.

![BRUTE](images/brute20.png)

Dette gjør brute force i praksis ubrukelig.

### OWASP: Injection

#### XSS

XSS kan injisere JavaScript på andre brukeres sider.
La oss si at angriperen bruker {}; alert(0); // i BurpSuite.
Deretter URL-enkodes nyttelasten for videre forespørsler:

![XSS](images/xss17.png)

De kan så vise varselet på siden.

![ALERT](images/alert18.png)

Det er trygt å hente modellen og parse HTML-en som textContent, så lenge den ikke legges inn i innerHTML.

https://github.com/dovidee/KartverketProject/blob/433e47255b20bc2ca6cc992841a94e9dc0285d14/KartverketProject/wwwroot/js/mapoverview.js#L14-L20

Objektet parses deretter for å opprette et GeoJSON-objekt som viser markøren i kartet.

![REG](images/register19.png)

### OWASP: Broken Access Control

#### IDOR

Autentiserte brukere kan se sine egne rapporter.
Men brukere kunne potensielt endre ID-en i headeren for å endre andre brukeres rapporter.
Koden under hindrer en bruker i å hente en rapport som ikke er deres, ut fra ID-en.

https://github.com/dovidee/KartverketProject/blob/9073420b0a123a217a8d737adba32ce542875756/KartverketProject/Controllers/AccountController.cs#L184-L188

De blir videresendt til "Access Denied"

![IDOR](images/idor21.png)

Slik kan ikke brukere manipulere URL-en for å endre andres rapporter.

#### CSRF

CSRF lurer en autentisert bruker til å utføre en utilsiktet handling.
Angriperen lager en URL med skjemaet som brukeren klikker på.
Dette kan være ødeleggende hvis brukeren er admin.

https://github.com/dovidee/KartverketProject/blob/fb0fb4271ddc0f080dec6b35b7023c38041efda0/KartverketProject/Views/Obstacle/DataForm.cshtml#L57-L58

Anti forgery token legges inn i skjemaet

https://github.com/dovidee/KartverketProject/blob/fb0fb4271ddc0f080dec6b35b7023c38041efda0/KartverketProject/Controllers/ObstacleController.cs#L34-L35

Controlleren validerer deretter hver forespørsel.

https://github.com/dovidee/KartverketProject/blob/fb0fb4271ddc0f080dec6b35b7023c38041efda0/KartverketProject/Program.cs#L81

Det ondsinnede nettstedet vil ikke ha en matchende CSRF-token, noe som stopper angriperen.

### Content Security Policy

https://github.com/dovidee/KartverketProject/blob/fb0fb4271ddc0f080dec6b35b7023c38041efda0/KartverketProject/Program.cs#L81-L84

X-Frame-Options er satt til DENY for å hindre at <iframe> vises i et annet origin.

X-Content-Type-Options hindrer angripere i å kjøre ondsinnet kode som XSS hvis nettleseren gjetter feil Content-Type.

Referrer-Policy hindrer at URL-informasjon som stier sendes videre til et annet origin, noe som utnyttes ved CSRF.

## Brukertest

Brukervennligheten i applikasjonen ble testet på et nært familiemedlem.

https://youtu.be/Tqa0U8SsCfY

Videoen over viser at brukeren er usikker på hvordan hen skal:

1. Samhandle med kartet.
2. Tegne en markør.
3. Se skjemaet.

![IDOR](images/draw27.png)

Skjemaet ble plassert til høyre, samt på oversiktssiden.
I tillegg ble det lagt til en tegnemodus for å slå tegning av og på.

## Bidragsytere

En spesiell takk til DAkintola94 for hjelpen, og for at vi fikk gjenbruke koden hans for Core Identity.

Du finner prosjektet hans her:

https://github.com/DAkintola94/MatFrem/tree/main

Generativ KI ble brukt til å generere Tailwind CSS-sider og til å forbedre eksisterende kode.
