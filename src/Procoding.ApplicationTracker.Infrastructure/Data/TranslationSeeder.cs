using Microsoft.EntityFrameworkCore;
using Procoding.ApplicationTracker.Domain.Entities;

namespace Procoding.ApplicationTracker.Infrastructure.Data;

/// <summary>
/// Seeds the base UI translations (Croatian + English). Runs on every startup (not dev-only) and
/// inserts any base key that is missing, so newly added keys show up without wiping the table
/// (admin edits to existing keys are preserved).
/// </summary>
public static class TranslationSeeder
{
    private static readonly (string Key, string Hr, string En)[] BaseTranslations =
    {
        // ---- App navigation / layout ----
        ("nav.home", "Početna", "Home"),
        ("nav.board", "Ploča", "Board"),
        ("nav.myApplications", "Moje prijave", "My applications"),
        ("nav.newApplication", "Nova prijava", "New application"),
        ("nav.jobApplications", "Prijave za posao", "Job applications"),
        ("nav.candidates", "Kandidati", "Candidates"),
        ("nav.companies", "Tvrtke", "Companies"),
        ("nav.sources", "Izvori prijava", "Job application sources"),
        ("nav.employees", "Zaposlenici", "Employees"),
        ("nav.translations", "Prijevodi", "Translations"),
        ("layout.adminView", "Admin pogled", "Admin view"),
        ("layout.candidateView", "Pogled kandidata", "Candidate view"),
        ("common.logout", "Odjava", "Logout"),
        ("common.newApplication", "Nova prijava", "New application"),
        ("common.details", "Detalji", "Details"),

        // ---- Dashboard ----
        ("dashboard.welcome", "Dobrodošao natrag", "Welcome back"),
        ("dashboard.subtitle", "Evo kako ide tvoja potraga za poslom.", "Here's how your job search is going."),
        ("dashboard.applications", "Prijave", "Applications"),
        ("dashboard.inProcess", "U procesu", "In process"),
        ("dashboard.offers", "Ponude", "Offers"),
        ("dashboard.responseRate", "Stopa odgovora", "Response rate"),
        ("dashboard.pipeline", "Tijek", "Pipeline"),
        ("dashboard.recentActivity", "Nedavna aktivnost", "Recent activity"),

        // ---- Statuses ----
        ("status.applied", "Prijavljeno", "Applied"),
        ("status.inProcess", "U procesu", "In process"),
        ("status.offer", "Ponuda", "Offer"),
        ("status.accepted", "Prihvaćeno", "Accepted"),
        ("status.rejected", "Odbijeno", "Rejected"),
        ("status.withdrawed", "Povučeno", "Withdrawn"),

        // ---- Board ----
        ("board.title", "Ploča prijava", "Job applications board"),
        ("board.listView", "Prikaz popisa", "List view"),
        ("board.move", "Premjesti u", "Move to"),

        // ---- Job types (JobType value object values) ----
        ("jobType.FullTime", "Puno radno vrijeme", "Full-time"),
        ("jobType.PartTime", "Skraćeno radno vrijeme", "Part-time"),
        ("jobType.Contract", "Ugovor", "Contract"),
        ("jobType.Temporary", "Privremeno", "Temporary"),
        ("jobType.Volunteer", "Volontiranje", "Volunteer"),

        // ---- Work location types ----
        ("workLocation.Remote", "Na daljinu", "Remote"),
        ("workLocation.OnSite", "U uredu", "On-site"),
        ("workLocation.Hybrid", "Hibridno", "Hybrid"),

        // ---- Interview step types ----
        ("stepType.Applied", "Prijavljeno", "Applied"),
        ("stepType.PhoneScreen", "Telefonski / HR razgovor", "Phone / HR screen"),
        ("stepType.TechnicalInterview", "Tehnički intervju", "Technical interview"),
        ("stepType.TakeHomeTask", "Zadatak za doma", "Take-home task"),
        ("stepType.SystemDesign", "Dizajn sustava", "System design"),
        ("stepType.Behavioral", "Bihevioralni intervju", "Behavioral"),
        ("stepType.Final", "Završni razgovor", "Final"),
        ("stepType.Offer", "Ponuda", "Offer"),
        ("stepType.Other", "Ostalo", "Other"),

        // ---- Interview step outcomes ----
        ("outcome.Pending", "U tijeku", "Pending"),
        ("outcome.Passed", "Prošao", "Passed"),
        ("outcome.Failed", "Nije prošao", "Failed"),

        // ---- Application detail page ----
        ("detail.addDetails", "Dodaj detalje", "Add details"),
        ("detail.editDetails", "Uredi detalje", "Edit details"),
        ("detail.newApplication", "Nova prijava", "New application"),
        ("detail.interviewProcess", "Proces intervjua", "Interview process"),
        ("detail.addStep", "Dodaj korak", "Add step"),
        ("detail.noSteps", "Još nema koraka — klikni \"Dodaj korak\" da zabilježiš prvu fazu intervjua.", "No steps yet — click \"Add step\" to log your first interview stage."),
        ("detail.details", "Detalji", "Details"),
        ("detail.company", "Tvrtka", "Company"),
        ("detail.source", "Izvor", "Source"),
        ("detail.workLocation", "Lokacija rada", "Work location"),
        ("detail.jobType", "Vrsta posla", "Job type"),
        ("detail.jobAd", "Oglas za posao", "Job ad"),
        ("detail.openPosting", "Otvori oglas", "Open posting"),
        ("detail.description", "Opis", "Description"),
        ("detail.letsAdd", "Dodajmo ovu prijavu", "Let's add this application"),
        ("detail.letsAddSubtitle", "Ispuni detalje da je počneš pratiti.", "Fill in the details to start tracking it."),
        ("detail.jobApplication", "Prijava za posao", "Job application"),
        ("detail.fillDetailsSave", "Ispuni detalje i spremi", "Fill in the details and save"),
        ("detail.type", "Vrsta", "Type"),
        ("detail.date", "Datum", "Date"),
        ("detail.outcome", "Ishod", "Outcome"),
        ("detail.notes", "Bilješke / komentar", "Notes / comment"),
        ("detail.jobPositionTitle", "Naziv radnog mjesta", "Job position title"),
        ("detail.jobApplicationSource", "Izvor prijave", "Job application source"),
        ("detail.jobAdLink", "Poveznica na oglas", "Job ad link"),
        ("detail.createNewCompany", "Kreiraj novu tvrtku", "Create new company"),
        ("detail.createNew", "Kreiraj novu", "Create new"),
        ("detail.createCompany", "Kreiraj tvrtku", "Create company"),
        ("detail.companyNotExist", "Tvrtka s tim imenom ne postoji. Želiš li je kreirati?", "Company with that name does not exist. Would you like to create it?"),
        ("detail.name", "Naziv", "Name"),
        ("detail.officialWebsite", "Poveznica na službenu web stranicu", "Official website link"),
        ("detail.chooseOrAdd", "Odaberi s popisa ili dodaj novu", "Choose from the list or add new"),
        ("detail.egPosition", "Na primjer .NET developer", "For example .NET developer"),
        ("detail.egLink", "Nešto poput https://www.link.com", "Something like https://www.link.com"),

        // ---- Common actions ----
        ("common.save", "Spremi", "Save"),
        ("common.cancel", "Odustani", "Cancel"),

        // ---- Applications list ----
        ("list.title", "Prijave za posao", "Job applications"),
        ("list.board", "Ploča", "Board"),
        ("list.col.position", "Pozicija", "Position"),
        ("list.col.company", "Tvrtka", "Company"),
        ("list.col.source", "Izvor", "Source"),
        ("list.col.type", "Vrsta", "Type"),
        ("list.col.location", "Lokacija", "Location"),
        ("list.col.status", "Status", "Status"),
        ("list.col.jobAd", "Oglas", "Job ad"),

        // ---- Marketing / landing (homepage) ----
        ("mkt.nav.features", "Značajke", "Features"),
        ("mkt.nav.how", "Kako radi", "How it works"),
        ("mkt.nav.signin", "Prijava", "Sign in"),
        ("mkt.nav.getStarted", "Započni", "Get started"),
        ("mkt.hero.eyebrow", "Organizirana potraga za poslom", "Job search, organized"),
        ("mkt.hero.titlePre", "Ovladaj svojom potragom za poslom", "Land your next job without"),
        ("mkt.hero.titleHighlight", "od prijave do ponude", "losing track"),
        ("mkt.hero.subtitle",
            "JobTrek pretvara tvoju neurednu tablicu prijava u pregledu vizualnu ploču — prati svaku prijavu, intervju i ponudu na jednom mjestu.",
            "JobTrek turns your messy spreadsheet of applications into a clean visual board — track every application, interview and offer in one place."),
        ("mkt.hero.ctaPrimary", "Započni besplatno", "Start for free"),
        ("mkt.hero.ctaSecondary", "Prijava", "Sign in"),
        ("mkt.hero.trust", "Besplatno za početak · Bez kreditne kartice", "Free to start · No credit card required"),
        ("mkt.features.title", "Sve što trebaš za vođenje potrage za poslom", "Everything you need to run your job search"),
        ("mkt.features.lead", "Prestani nagađati gdje je koja prijava. Vidi cijeli tijek u jednom pogledu.", "Stop guessing where each application stands. See your whole pipeline at a glance."),
        ("mkt.feature1.title", "Vizualna ploča", "Visual board"),
        ("mkt.feature1.desc", "Povlači prijave kroz Prijavljeno → U procesu → Ponuda → Prihvaćeno. Tvoj tijek, u jednom pogledu.", "Drag applications across Applied → In process → Offer → Accepted. Your pipeline, at a glance."),
        ("mkt.feature2.title", "Prati svaki korak", "Track every stage"),
        ("mkt.feature2.desc", "Bilježi intervjue i ishode kako ništa ne bi propalo između rundi.", "Log interviews and outcomes so nothing slips through the cracks between rounds."),
        ("mkt.feature3.title", "Vidi svoj napredak", "See your progress"),
        ("mkt.feature3.desc", "Znaj koliko je prijava aktivno, koliko ih je dovelo do intervjua i što slijedi.", "Know how many applications are live, how many led to interviews, and what's next."),
        ("mkt.how.title", "Spreman za par minuta", "Up and running in minutes"),
        ("mkt.how.lead", "Bez postavljanja, bez tablica. Samo dodaj prvu prijavu i kreni.", "No setup, no spreadsheets. Just add your first application and go."),
        ("mkt.step1.title", "Kreiraj račun", "Create your account"),
        ("mkt.step1.desc", "Registriraj se besplatno u nekoliko sekundi — bez kreditne kartice.", "Sign up free in seconds — no credit card required."),
        ("mkt.step2.title", "Dodaj prijave", "Add your applications"),
        ("mkt.step2.desc", "Ubaci pozicije na koje si se prijavio, s tvrtkom, izvorom i poveznicama.", "Drop in the roles you've applied to, with company, source and links."),
        ("mkt.step3.title", "Pomiči ih po ploči", "Move them across the board"),
        ("mkt.step3.desc", "Ažuriraj statuse kako napreduješ i uvijek znaj gdje stojiš.", "Update statuses as you progress and always know where you stand."),
        ("mkt.cta.title", "Spreman preuzeti kontrolu nad potragom za poslom?", "Ready to take control of your job search?"),
        ("mkt.cta.subtitle", "Pridruži se i pretvori potragu za poslom u jasan proces koji vodi do cilja.", "Join and turn your job hunt into a clear, winnable process."),
        ("mkt.cta.button", "Kreiraj besplatni račun", "Create your free account"),
    };

    public static async Task SeedAsync(ApplicationDbContext dbContext)
    {
        var existing = await dbContext.Translations
            .Select(t => new { t.Key, t.LanguageCode })
            .ToListAsync();

        var existingSet = existing
            .Select(x => $"{x.Key}|{x.LanguageCode}")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var added = false;

        foreach (var (key, hr, en) in BaseTranslations)
        {
            if (!existingSet.Contains($"{key}|hr"))
            {
                await dbContext.Translations.AddAsync(Translation.Create(Guid.NewGuid(), key, "hr", hr));
                added = true;
            }

            if (!existingSet.Contains($"{key}|en"))
            {
                await dbContext.Translations.AddAsync(Translation.Create(Guid.NewGuid(), key, "en", en));
                added = true;
            }
        }

        if (added)
        {
            await dbContext.SaveChangesAsync();
        }
    }
}
