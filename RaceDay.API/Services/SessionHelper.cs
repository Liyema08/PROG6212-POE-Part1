namespace RaceDay.API.Services
{
    public static class SessionHelper
    {
        public static int? GetUserId(HttpContext context)
        {
            return context.Session.GetInt32("UserId");
        }

        public static string? GetUserRole(HttpContext context)
        {
            return context.Session.GetString("UserRole");
        }

        public static string? GetUserEmail(HttpContext context)
        {
            return context.Session.GetString("UserEmail");
        }

        public static bool IsLoggedIn(HttpContext context)
        {
            return GetUserId(context) != null;
        }

        public static bool IsOrganiser(HttpContext context)
        {
            return GetUserRole(context) == "Organiser";
        }

        public static bool IsParticipant(HttpContext context)
        {
            return GetUserRole(context) == "Participant";
        }
    }
}