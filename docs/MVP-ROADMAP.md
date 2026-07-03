# MVP Roadmap — Job Application Tracker

Plan kako od trenutne (solidne) tehničke osnove doći do **upotrebljivog proizvoda** i zatim do
**MVP-a koji se može prodavati**. Pisano na temelju stvarnog stanja koda (vidi `CLAUDE.md`).

---

## 0. Prvo: odluka o pozicioniranju (ovo mijenja sve ostalo)

Kod je **candidate-centričan**: kandidat prati svoje prijave, `JobApplication` ima global query
filter da kandidat vidi samo svoje. Iz toga slijede dvije mogućnosti:

| Opcija | Kupac | Što je proizvod | Naplata |
|---|---|---|---|
| **A — B2C (preporuka)** | Pojedinac koji traži posao | Osobni tracker prijava (kao Huntr / Teal / Simplify) | Mjesečna pretplata po korisniku, freemium |
| B — B2B / mini-ATS | Mali recruiting tim / agencija | Praćenje kandidata i pozicija | Pretplata po timu/sjedalu |

**Preporuka: opcija A.** Manje truda do prodaje, model već pristaje (per-candidate izolacija),
tržište je veliko i jasno. Ostatak ovog dokumenta pretpostavlja A. Za B onda `Employee` postaje
"recruiter", `Candidate` postaje entitet kojim se upravlja, i treba pravi multi-tenant (organizacije).

> **Ovo je jedina odluka koju trebam od tebe prije nego se krene ozbiljno graditi.**

---

## Što znači "upotrebljivo" vs "MVP za prodaju"

- **Upotrebljivo** = ja kao korisnik mogu se registrirati, dodati prijavu s pravim podacima,
  mijenjati status kroz faze, dodavati intervjue i bilješke, i to radi bez bugova. (Faze 1–2)
- **MVP za prodaju** = sve gore + plaćanje, sigurnost, e-mail, landing stranica, pravni okvir,
  i deploy koji izdrži prave korisnike. (Faze 3–4)

---

## FAZA 1 — Popraviti pokvarenu jezgru (bez ovoga ništa ne vrijedi)

Trenutno se **osnovna radnja "dodaj prijavu" ne izvršava ispravno**. Ovo je must-fix.

1. ~~**Popraviti `ApplyForJobCommandHandler`**~~ ✅ **Gotovo** — handler sad koristi stvarne
   vrijednosti iz komande (naslov, link, `JobType`, `WorkLocationType`, opis) umjesto hardkodiranih.
2. ~~**Statusni tijek prijave**~~ ✅ **Gotovo** — dodan `JobApplication.ChangeStatus(...)` sa state
   machineom (`Applied → InProcess → Rejected/Withdrawed`, terminalni statusi), domain event
   `JobApplicationStatusChangedDomainEvent`, exception za nevaljani prijelaz, CQRS komanda
   `ChangeJobApplicationStatusCommand` + endpoint `PATCH job-applications/status`, `Status` u DTO-u,
   te domenski testovi. **Preostaje**: proširiti statuse (npr. `Offer`/`Accepted`) i UI/Kanban (Faza 2).
3. **Interview steps end-to-end** — domena ima `CreateNewInterview` i `NewInterviewAddedDomainEvent`,
   ali **nema API endpointa ni UI-a**. Dodati: dodaj/uredi/obriši intervju, prikaz na detalju prijave.
4. **Brisanje prijava** — soft-delete infrastruktura postoji (`ISoftDeletableEntity`, auto u
   `SaveChangesAsync`), ali **nema delete use-casea ni endpointa**. Dodati.
5. **Uključiti dispatch domain eventova** — sad je zakomentiran u `ApplicationDbContext.SaveChangesAsync`.
   Treba za podsjetnike/notifikacije kasnije.
6. **Očistiti mrtvi kod** — `Something.cs`, `ISomething.cs`, `WeatherForecast.cs`, `Counter.razor`,
   `Weather.razor`, prazan `Api.Tests` template. Sitno ali čisti dojam.

**Rezultat faze:** aplikacija se ponaša kao ispravan tracker za jednog korisnika.

---

## FAZA 2 — Napraviti da vrijedi platiti (MVP feature set)

Ovo su značajke zbog kojih netko plaća osobni tracker:

1. **Bogatiji model prijave** — plaća/raspon + valuta (već postoji `Currency`), lokacija, kontakt
   osoba, verzija CV-a, izvor, rok, prioritet. Proširiti `JobApplication` + DTO-e + formu.
2. ~~**Kanban ploča po statusu**~~ ✅ **Gotovo** — MudBlazor drag-and-drop ploča na
   `/my-job-applications/board` sa 6 kolona (`Applied → In process → Offer → Accepted`, te
   `Rejected`/`Withdrawn`). Povlačenje kartice zove `PATCH job-applications/status`; nevaljani
   prijelaz server odbija (400) pa se kartica vrati i prikaže greška. Statusi prošireni s
   `Offer`/`Accepted`. **Preostaje**: "Wishlist" stupac (prije prijave) ako zatreba.
3. **Bilješke i timeline** po prijavi (što se dogodilo, kad).
4. **Podsjetnici / follow-up** — "javi se za 5 dana" → e-mail/notifikacija (koristi domain evente iz Faze 1).
5. **Dashboard** — broj prijava/tjedno, response rate, funnel po statusu, prosječno vrijeme do odgovora.
6. **Import/Export** (CSV) — nizak trud, korisnicima bitno za povjerenje (mogu izaći s podacima).

**Rezultat faze:** proizvod koji rješava stvaran problem bolje od Excela.

---

## FAZA 3 — SaaS omotač (da se uopće MOŽE prodavati)

1. **Naplata (Stripe)** — pretplate, freemium limit (npr. besplatno do 15 prijava), webhookovi,
   customer portal. Bez ovoga nema "prodaje".
2. **Sigurnost autentikacije:**
   - JWT `SecretKey` **izvan `appsettings.json`** → environment / secrets manager.
   - Reset lozinke + verifikacija e-maila (trenutno ne postoje).
   - Rotacija i opoziv refresh tokena (tablica postoji — dovršiti logiku).
   - Rate limiting na login/signup, CORS politika, lockout.
3. **Transakcijski e-mail** — provider (Postmark/SendGrid/SES; već ste na AWS-u → SES). Verifikacija,
   reset, podsjetnici.
4. **Onboarding + landing stranica** — javna marketing stranica, cijene, registracija, prazni ekrani
   s uputama (empty states).
5. **Pravni okvir (EU/GDPR, jer je deploy `eu-west-1`)** — privacy policy, ToS, cookie consent,
   brisanje računa i podataka na zahtjev.

**Rezultat faze:** stranac se može registrirati, platiti i sigurno koristiti proizvod.

---

## FAZA 4 — Produkcijska spremnost i pouzdanost

1. **Observability** — strukturirani logovi, error tracking (Sentry), metrika (Aspire/OpenTelemetry
   je već djelomično tu preko `ServiceDefaults`), health checks izloženi.
2. **Baza u produkciji** — managed Postgres (AWS RDS), backupi, migracijska strategija na deployu
   (pokretati EF migracije kontrolirano, ne `EnsureCreated`).
3. **CI/CD popravci** — workflow instalira **.NET 7 SDK** iako je sve `net8.0` → podignuti na `8.0.x`;
   dodati pokretanje integracijskih testova (trebaju Docker/Postgres na runneru).
4. **Sigurnosni pregled** — pokrenuti `/security-review`, provjeriti secrets, HTTPS/HSTS, dependency scan.
5. **Testovi** — popuniti prazan `Api.Tests`, pokriti nove status-prijelaze i naplatu.

**Rezultat faze:** izdrži prave korisnike i ne gubi podatke.

---

## FAZA 5 — Rast (nakon MVP-a, opcionalno)

- **Browser ekstenzija** za hvatanje oglasa jednim klikom (najjači diferencijator u ovoj niši).
- **Mobilna aplikacija** (README je već spominje kao plan; API je spreman za to).
- AI značajke: auto-parsiranje oglasa, prijedlog CV-a po oglasu, praćenje odgovora iz maila.

---

## Prioritet — čime početi (redom)

1. Fix `ApplyForJobCommandHandler` (Faza 1.1)
2. Statusni prijelazi + Kanban (Faza 1.2 + 2.2) — najveća vrijednost po jedinici truda
3. Interview steps end-to-end (Faza 1.3)
4. Bogatiji model prijave + bilješke (Faza 2.1, 2.3)
5. Auth hardening + reset/verifikacija e-maila (Faza 3.2, 3.3)
6. Stripe naplata (Faza 3.1)
7. Landing + onboarding + pravni okvir (Faza 3.4, 3.5)
8. Produkcijski deploy + observability + CI fix (Faza 4)

Grubo: **Faze 1–2 = "upotrebljivo", Faze 1–3 = "MVP za prodaju", Faza 4 = "spremno za prave korisnike".**

---

## Napomena o tehničkom dugu

Osnova je zdrava (Clean Arch, DDD, CQRS, arhitekturni testovi) pa nema potrebe za velikim
refaktorom. Fokus je **dovršiti započeto** (status, intervjui, eventi, brisanje) i **dodati SaaS
sloj**, a ne prepisivati. Držati se postojećih obrazaca iz `CLAUDE.md`.
