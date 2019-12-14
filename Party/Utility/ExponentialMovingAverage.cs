namespace Party.Utility
{
    // implementation of N-day EMA
    // it calculates an exponential moving average roughy equivalent to the last n observations
    // https://en.wikipedia.org/wiki/Moving_average#Exponential_moving_average
    public struct ExponentialMovingAverage
    {
        public double Value { get; private set; }

        public double Var { get; private set; }

        readonly float alpha;
        bool initialized;

        public ExponentialMovingAverage(int n)
        {
            // standard N-day EMA alpha calculation
            alpha = 2.0f / (n + 1);
            initialized = false;
            Value = 0d;
            Var = 0d;
        }

        public void Add(double newValue)
        {
            // simple algorithm for EMA described here:
            // https://en.wikipedia.org/wiki/Moving_average#Exponentially_weighted_moving_variance_and_standard_deviation
            if (initialized)
            {
                double delta = newValue - Value;
                Value += alpha * delta;
                Var = (1 - alpha) * (Var + alpha * delta * delta);
            }
            else
            {
                Value = newValue;
                initialized = true;
            }
        }

    }
}
