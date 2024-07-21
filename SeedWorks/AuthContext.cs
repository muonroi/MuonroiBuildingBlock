namespace MuonroiBuildingBlock.SeedWorks
{
    public class AuthContext : IAuthContext
    {
        private static readonly AsyncLocal<AuthInfoContext?> LocalWorkContext = new();

        public AuthInfoContext? AuthInfoContext
        {
            get => LocalWorkContext.Value;
            set => LocalWorkContext.Value = value;
        }
    }
}