namespace Procoding.ApplicationTracker.Web.Services;

public static class UrlConstants
{
    public static class JobApplicationSources
    {

        public const string GET_ALL_URL = "job-application-sources";

        public static string GetOne(Guid id)
        {
            return $"{GET_ALL_URL}/{id}";
        }

        public static string UpdateUrl()
        {
            return $"{GET_ALL_URL}";
        }

        public static string InsertUrl()
        {
            return $"{GET_ALL_URL}";
        }
    }

    public static class Candidates
    {

        public const string GET_ALL_URL = "candidates";


        public static string LoginCandidate()
        {
            return $"{GET_ALL_URL}/login";
        }

        public static string LoginRefreshCandidate()
        {
            return $"{GET_ALL_URL}/login/refresh";
        }

        public static string SignupCandidate()
        {
            return $"{GET_ALL_URL}/signup";
        }

        public static string GetOne(Guid id)
        {
            return $"{GET_ALL_URL}/{id}";
        }

        public static string UpdateUrl()
        {
            return $"{GET_ALL_URL}";
        }

        public static string InsertUrl()
        {
            return $"{GET_ALL_URL}";
        }

    }

    public static class Employees
    {

        public const string GET_ALL_URL = "employees";

        public static string GetOne(Guid id)
        {
            return $"{GET_ALL_URL}/{id}";
        }

        public static string UpdateUrl()
        {
            return $"{GET_ALL_URL}";
        }

        public static string LoginEmployee()
        {
            return $"{GET_ALL_URL}/login";
        }

        public static string LoginRefreshEmployee()
        {
            return $"{GET_ALL_URL}/login/refresh";
        }

        public static string InsertUrl()
        {
            return $"{GET_ALL_URL}";
        }

    }

    public static class Companies
    {

        public const string GET_ALL_URL = "companies";

        public static string GetOne(Guid id)
        {
            return $"{GET_ALL_URL}/{id}";
        }

        public static string UpdateUrl()
        {
            return $"{GET_ALL_URL}";
        }

        public static string InsertUrl()
        {
            return $"{GET_ALL_URL}";
        }
    }


    public static class JobApplications
    {

        public const string GET_ALL_URL = "job-applications";

        public static string ExtractFromText()
        {
            return $"{GET_ALL_URL}/extract-from-text";
        }

        public static string Archive(Guid id)
        {
            return $"{GET_ALL_URL}/{id}/archive";
        }

        public static string Unarchive(Guid id)
        {
            return $"{GET_ALL_URL}/{id}/unarchive";
        }

        public static string Delete(Guid id)
        {
            return $"{GET_ALL_URL}/{id}";
        }

        public static string GetOne(Guid id)
        {
            return $"{GET_ALL_URL}/{id}";
        }

        public static string UpdateUrl()
        {
            return $"{GET_ALL_URL}";
        }

        public static string InsertUrl()
        {
            return $"{GET_ALL_URL}";
        }

        public static string ChangeStatusUrl()
        {
            return $"{GET_ALL_URL}/status";
        }

        public static string InterviewStepsUrl()
        {
            return $"{GET_ALL_URL}/interview-steps";
        }

        public static string DeleteInterviewStepUrl(Guid jobApplicationId, Guid interviewStepId)
        {
            return $"{GET_ALL_URL}/{jobApplicationId}/interview-steps/{interviewStepId}";
        }
    }

    public static class JobTypes
    {
        public const string GET_ALL_URL = "job-types";

    }

    public static class Translations
    {
        public const string GET_ALL_URL = "translations";
    }

    public static class WorkLocationTypes
    {
        public const string GET_ALL_URL = "work-location-types";
    }

}
