namespace BillionSongs {
    using System;
    using System.Threading;
    public static class ThreadSafeRandom {
        static readonly ThreadLocal<Random> sources = new ThreadLocal<Random>(() => new Random());

        public static Random Instance => sources.Value;
    }
}
