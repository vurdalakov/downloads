namespace Vurdalakov
{
    using System;
    using System.Collections.Generic;

    static public class Tracer
    {
        static public void Trace(String text)
        {
#if DEBUG
            Console.WriteLine(text);
#endif
        }

        static public void Trace(String format, params Object[] args)
        {
#if DEBUG
            Trace(String.Format(format, args));
#endif
        }

        static public void Trace(Dictionary<String, String> dictionary)
        {
#if DEBUG
            foreach (var entry in dictionary)
            {
                Trace("{0}={1}", entry.Key, entry.Value);
            }
#endif
        }
    }
}
