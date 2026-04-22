namespace task5.Services
{
    public static class SeedHelperService
    {
        public static int Combine(long seed, int page, int offset = 0)
        {
            return (int)(seed + page * 1000 + offset);
        }
    }
}