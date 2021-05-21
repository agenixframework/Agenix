namespace MPP.Acceptance.Test.API.Specs.Support
{
    internal class ContentType
    {
        public static readonly string ContentTypeName = "Content-Type";
        public static readonly string AcceptName = "Accept";
        public static readonly ContentType Any = new(new[] { "*/*" });
        public static readonly ContentType Text = new(new[] { "text/plain" });
        public static readonly ContentType Json = new(new[] { "application/json", "application/javascript", "text/javascript", "text/json" });
        public static readonly ContentType Xml = new(new[] { "application/xml", "text/xml", "application/xhtml+xml" });
        public static readonly ContentType Html = new(new[] { "text/html" });
        public static readonly ContentType Urlenc = new(new[] { "application/x-www-form-urlencoded" });
        public static readonly ContentType Binary = new(new[] { "application/octet-stream" });

        private readonly string[] _ctStrings;

        private ContentType(string[] ctStrings)
        {
            _ctStrings = ctStrings;
        }

        public string[] GetContentTypeStrings => _ctStrings;

        public override string ToString()
        {
            return _ctStrings[0];
        }
    }
}