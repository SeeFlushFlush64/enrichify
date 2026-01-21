namespace Enrichify.Services
{
    public interface IHunterService
    {
        Task<string> FindEmail(string domain, string firstName, string lastName);
    }
}