namespace task5.Services
{
    public static class FractionalHelperService
    {
        public static int Generate(double avg, Random rng)
        {
            avg = Math.Clamp(avg, 0, 10);

            int floor = (int)Math.Floor(avg);
            double fraction = avg - floor;

            if (rng.NextDouble() < fraction)
                return floor + 1;

            return floor;
        }
    }
}