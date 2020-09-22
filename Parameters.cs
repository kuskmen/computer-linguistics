namespace computer_linguistics
{
    using CommandLine;

    public class Parameters
    {
        [Option('u', "url", Required = true, HelpText = "Set url to start searching from.")]
        public string Url { get; set; }

        [Option('p', "pattern", Required = true, HelpText = "Set reg ex pattern to start searching with.")]
        public string RegEx { get; set; }

        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }
    }
}
