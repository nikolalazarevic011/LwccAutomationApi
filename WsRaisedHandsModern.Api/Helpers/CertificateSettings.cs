namespace WsRaisedHandsModern.Api.Helpers
{
    public class CertificateSettings
    {
        public string TemplateImagePath { get; set; } = string.Empty;
        public string TempStoragePath { get; set; } = string.Empty;
        public string FontFamily { get; set; } = "Arial";
        public int NameFontSize { get; set; } = 24;
        public int DateFontSize { get; set; } = 16;
        public string FontColor { get; set; } = "#000000";

        // Coordinates for text placement on certificate (in pixels or points)
        public int NameXPosition { get; set; } = 0;
        public int NameYPosition { get; set; } = 300;
        public int DateXPosition { get; set; } = 0;
        public int DateYPosition { get; set; } = 400;

        // Certificate text
        public string CertificateText { get; set; } = "has satisfactorily completed FOUNDATIONS TRAINING";

        // Email settings specific to certificates
        public string CertificateEmailSubject { get; set; } = "Your Foundations Training Certificate";
        public string CertificateEmailBody { get; set; } = "Congratulations on completing your Foundations Training! Please find your certificate attached.";

        // Processing limits
        public int MaxBatchSize { get; set; } = 100;
        public int MaxFileSizeMB { get; set; } = 10;

        // Path where generated certificates will be stored
        public string GeneratedCertificatesPath { get; set; } = string.Empty;

    }
}