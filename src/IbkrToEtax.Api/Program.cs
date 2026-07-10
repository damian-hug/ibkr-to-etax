using System;
using System.Linq;
using System.Xml.Linq;
using CommandLine;
using IbkrToEtax.IbkrReport;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace IbkrToEtax
{
    class Program
    {
        private static ILoggerFactory? _loggerFactory;
        private static ILogger<Program>? _logger;

        // Command-line option classes
        [Verb("convert", HelpText = "Convert IBKR XML export to eCH-0196 format (XML + PDF)")]
        class ConvertOptions
        {
            [Value(0, Required = true, MetaName = "file", HelpText = "IBKR XML export file to convert")]
            public string InputFile { get; set; } = "";
        }

        [Verb("validate", HelpText = "Validate eCH-0196 PDF and extract embedded XML")]
        class ValidateOptions
        {
            [Value(0, Required = true, MetaName = "file", HelpText = "eCH-0196 PDF file to validate")]
            public string InputFile { get; set; } = "";

            [Option('s', "schema", Required = false, HelpText = "XSD schema file for validation (auto-detects eCH-0196-2-2.xsd if not specified)")]
            public string? SchemaFile { get; set; }
        }

        [Verb("genpdf", HelpText = "[Debug] Generate PDF from existing eCH-0196 XML")]
        class GenPdfOptions
        {
            [Value(0, Required = true, MetaName = "xmlFile", HelpText = "eCH-0196 XML file to convert to PDF")]
            public string XmlFile { get; set; } = "";

            [Option('o', "output", Required = false, HelpText = "Output PDF file name inside data/outputs")]
            public string? OutputPdf { get; set; }
        }

        [Verb("serve-frontend", HelpText = "Serve the Angular frontend")]
        class ServeFrontendOptions
        {
            [Option('u', "url", Required = false, Default = "http://0.0.0.0:8080", HelpText = "URL Kestrel should bind to")]
            public string Url { get; set; } = "http://0.0.0.0:8080";

            [Option('r', "root", Required = false, HelpText = "Directory containing the compiled Angular frontend")]
            public string? Root { get; set; }
        }

        static int Main(string[] args)
        {
            // Configure logging with NLog
            _loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.ClearProviders();
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddNLog();
            });
            _logger = _loggerFactory.CreateLogger<Program>();

            // Parse with CommandLineParser
            int result = Parser.Default.ParseArguments<ConvertOptions, ValidateOptions, GenPdfOptions, ServeFrontendOptions>(args)
                .MapResult(
                    (ConvertOptions opts) => RunConvert(opts),
                    (ValidateOptions opts) => RunValidate(opts),
                    (GenPdfOptions opts) => RunGenPdf(opts),
                    (ServeFrontendOptions opts) => RunServeFrontend(opts),
                    errs => 1);

            // Dispose logger factory
            _loggerFactory?.Dispose();

            return result;
        }

        static int RunConvert(ConvertOptions opts)
        {
            if (!File.Exists(opts.InputFile))
            {
                _logger!.LogError("File not found: {FilePath}", opts.InputFile);
                return 2;
            }

            try
            {
                string outputXmlPath = GetOutputPath(opts.InputFile, ".output.xml");
                string outputPdfPath = GetOutputPath(opts.InputFile, ".output.pdf");

                return ConvertIbkrToEch(opts.InputFile, outputXmlPath, outputPdfPath);
            }
            catch (InvalidOperationException ex)
            {
                _logger!.LogError("{Message}", ex.Message);
                return 2;
            }
        }

        static int RunServeFrontend(ServeFrontendOptions opts)
        {
            string frontendRoot = GetFrontendRoot(opts.Root);

            if (!Directory.Exists(frontendRoot))
            {
                _logger!.LogError("Frontend build directory not found: {FrontendRoot}", frontendRoot);
                return 2;
            }

            var fileProvider = new PhysicalFileProvider(frontendRoot);
            var builder = WebApplication.CreateBuilder();

            builder.WebHost.UseUrls(opts.Url);
            builder.Logging.ClearProviders();
            builder.Logging.AddNLog();

            var app = builder.Build();
            var contentTypeProvider = new FileExtensionContentTypeProvider();

            app.UseDefaultFiles(new DefaultFilesOptions
            {
                FileProvider = fileProvider
            });
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = fileProvider,
                ContentTypeProvider = contentTypeProvider
            });
            app.MapFallbackToFile("index.html", new StaticFileOptions
            {
                FileProvider = fileProvider,
                ContentTypeProvider = contentTypeProvider
            });

            _logger!.LogInformation("Serving frontend from {FrontendRoot} on {Url}", frontendRoot, opts.Url);
            app.Run();
            return 0;
        }

        static int RunValidate(ValidateOptions opts)
        {
            if (!File.Exists(opts.InputFile))
            {
                _logger!.LogError("File not found: {FilePath}", opts.InputFile);
                return 2;
            }
            return ValidateEchPdf(opts.InputFile, opts.SchemaFile);
        }

        static int RunGenPdf(GenPdfOptions opts)
        {
            if (!File.Exists(opts.XmlFile))
            {
                _logger!.LogError("XML file not found: {FilePath}", opts.XmlFile);
                return 2;
            }

            try
            {
                string outputPdf = GetOutputPath(opts.OutputPdf ?? opts.XmlFile, ".pdf");

                _logger!.LogInformation("=== PDF Generation (Debug Mode) ===");
                Console.WriteLine();
                _logger!.LogInformation("Input XML: {XmlFile}", opts.XmlFile);
                _logger!.LogInformation("Output PDF: {OutputPdf}", outputPdf);
                Console.WriteLine();

                PdfBarcodeGenerator.GeneratePdfWithBarcodes(opts.XmlFile, outputPdf, _logger);

                Console.WriteLine();
                _logger!.LogInformation("✓ PDF generated successfully");
                return 0;
            }
            catch (InvalidOperationException ex)
            {
                _logger!.LogError("{Message}", ex.Message);
                return 2;
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "Error generating PDF");
                return 3;
            }
        }

        static int ConvertIbkrToEch(string inputFilePath, string outputXmlPath, string outputPdfPath)
        {
            _logger!.LogInformation("=== IBKR to eCH-0196 Converter ===");
            Console.WriteLine();

            if (!File.Exists(inputFilePath))
            {
                _logger!.LogError("File not found: {FilePath}", inputFilePath);
                return 2;
            }

            try
            {
                _logger!.LogInformation("Loading {FilePath}...", inputFilePath);
                var doc = XDocument.Load(inputFilePath);

                // Extract data from IBKR XML
                var ibkrReport = new IbkrFlexReport(doc, _loggerFactory!);

                // display suimmary of loaded data
                ibkrReport.PrintDataLoadSummary();

                // Build eCH tax statement
                var echStatement = new EchStatementBuilder(ibkrReport, _loggerFactory!).BuildEchTaxStatement();

                // Display financial summary
                var summaryLogger = _loggerFactory!.CreateLogger<FinancialSummaryPrinter>();
                var financialSummaryPrinter = new FinancialSummaryPrinter(summaryLogger);
                financialSummaryPrinter.PrintFinancialSummary(ibkrReport);

                // Generate output
                EchXmlGenerator.SaveAndDisplayOutput(_logger!, echStatement, outputXmlPath, outputPdfPath);

                Console.WriteLine();
                _logger!.LogInformation("✓ Conversion completed successfully");
                return 0;
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "Error during conversion");
                return 3;
            }
        }

        static int ValidateEchPdf(string pdfPath, string? xsdPath)
        {
            _logger!.LogInformation("=== eCH-0196 PDF Validator ===");
            Console.WriteLine();

            if (!File.Exists(pdfPath))
            {
                _logger!.LogError("File not found: {FilePath}", pdfPath);
                return 2;
            }

            // Auto-detect XSD if not provided
            if (string.IsNullOrEmpty(xsdPath))
            {
                string[] possibleXsdPaths = {
                    "eCH-0196-2-2.xsd",
                    Path.Combine(Path.GetDirectoryName(pdfPath) ?? "", "eCH-0196-2-2.xsd")
                };

                foreach (var path in possibleXsdPaths)
                {
                    if (File.Exists(path))
                    {
                        xsdPath = path;
                        break;
                    }
                }
            }

            try
            {
                var result = PdfValidator.ValidatePdf(pdfPath, xsdPath);

                Console.WriteLine();
                _logger!.LogInformation("=== Validation Summary ===");
                _logger!.LogInformation("PDF: {PdfFileName}", Path.GetFileName(pdfPath));
                _logger!.LogInformation("Status: {Status}", result.IsValid ? "✓ VALID" : "✗ INVALID");
                Console.WriteLine();

                // Display metadata
                if (result.Metadata.Count > 0)
                {
                    _logger!.LogInformation("Metadata:");
                    foreach (var kvp in result.Metadata)
                    {
                        _logger!.LogInformation("  {Key}: {Value}", kvp.Key, kvp.Value);
                    }
                    Console.WriteLine();
                }

                // Display errors
                if (result.Errors.Count > 0)
                {
                    _logger!.LogError("Errors ({ErrorCount}):", result.Errors.Count);
                    foreach (var error in result.Errors)
                    {
                        _logger!.LogError("  ✗ {Error}", error);
                    }
                    Console.WriteLine();
                }

                // Display warnings
                if (result.Warnings.Count > 0)
                {
                    _logger!.LogWarning("Warnings ({WarningCount}):", result.Warnings.Count);
                    foreach (var warning in result.Warnings)
                    {
                        _logger!.LogWarning("  ⚠ {Warning}", warning);
                    }
                    Console.WriteLine();
                }

                // Display extracted XML summary
                if (!string.IsNullOrEmpty(result.ExtractedXml))
                {
                    _logger!.LogInformation("Extracted XML Summary:");

                    try
                    {
                        var xmlDoc = XDocument.Parse(result.ExtractedXml);
                        var root = xmlDoc.Root;

                        if (root != null)
                        {
                            var ns = root.Name.Namespace;

                            // Extract key information
                            var taxPeriod = root.Attribute("taxPeriod")?.Value;
                            var periodFrom = root.Attribute("periodFrom")?.Value;
                            var periodTo = root.Attribute("periodTo")?.Value;
                            var canton = root.Attribute("canton")?.Value;
                            var totalTaxValue = root.Attribute("totalTaxValue")?.Value;
                            var totalGrossRevenueB = root.Attribute("totalGrossRevenueB")?.Value;
                            var totalWithHoldingTaxClaim = root.Attribute("totalWithHoldingTaxClaim")?.Value;

                            _logger!.LogInformation("  Tax Period: {TaxPeriod}", taxPeriod);
                            _logger!.LogInformation("  Period: {PeriodFrom} to {PeriodTo}", periodFrom, periodTo);
                            _logger!.LogInformation("  Canton: {Canton}", canton);
                            _logger!.LogInformation("  Total Tax Value: {TotalTaxValue} CHF", totalTaxValue);
                            _logger!.LogInformation("  Total Gross Revenue: {TotalGrossRevenue} CHF", totalGrossRevenueB);
                            _logger!.LogInformation("  Total Withholding Tax Claim: {TotalWithHoldingTaxClaim} CHF", totalWithHoldingTaxClaim);

                            // Count securities
                            var securities = xmlDoc.Descendants(ns + "security").Count();
                            var payments = xmlDoc.Descendants(ns + "payment").Count();
                            var stocks = xmlDoc.Descendants(ns + "stock").Count();

                            _logger!.LogInformation("  Securities: {Securities}", securities);
                            _logger!.LogInformation("  Payments: {Payments}", payments);
                            _logger!.LogInformation("  Stock Mutations: {Stocks}", stocks);
                        }
                    }
                    catch
                    {
                        _logger!.LogInformation("  XML Length: {XmlLength} characters", result.ExtractedXml.Length);
                    }

                    try
                    {
                        string extractedXmlPath = GetOutputPath(pdfPath, "-extracted.xml");
                        File.WriteAllText(extractedXmlPath, result.ExtractedXml);
                        Console.WriteLine();
                        _logger!.LogInformation("✓ Extracted XML saved to: {ExtractedXmlPath}", extractedXmlPath);
                    }
                    catch (InvalidOperationException ex)
                    {
                        _logger!.LogError("{Message}", ex.Message);
                        return 2;
                    }
                }

                Console.WriteLine();
                _logger!.LogInformation("=========================");

                return result.IsValid ? 0 : 3;
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "Error during validation");
                return 3;
            }
        }

        static string GetOutputPath(string inputPath, string suffix)
        {
            string outputDirectory = GetOutputDirectory();
            Directory.CreateDirectory(outputDirectory);

            string fileName = Path.GetFileNameWithoutExtension(inputPath) + suffix;
            return Path.Combine(outputDirectory, fileName);
        }

        static string GetOutputDirectory()
        {
            string? configuredDataDirectory = Environment.GetEnvironmentVariable("IBKR_TO_ETAX_DATA_DIR");
            if (string.IsNullOrWhiteSpace(configuredDataDirectory))
            {
                throw new InvalidOperationException("IBKR_TO_ETAX_DATA_DIR must be set to the data directory path.");
            }

            return Path.Combine(configuredDataDirectory, "outputs");
        }

        static string GetFrontendRoot(string? configuredRoot)
        {
            if (!string.IsNullOrWhiteSpace(configuredRoot))
            {
                return Path.GetFullPath(configuredRoot);
            }

            string? environmentRoot = Environment.GetEnvironmentVariable("IBKR_TO_ETAX_FRONTEND_ROOT");
            if (!string.IsNullOrWhiteSpace(environmentRoot))
            {
                return Path.GetFullPath(environmentRoot);
            }

            string publishedRoot = Path.Combine(AppContext.BaseDirectory, "frontend");
            if (Directory.Exists(publishedRoot))
            {
                return publishedRoot;
            }

            return Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory,
                "..",
                "..",
                "..",
                "frontend",
                "dist",
                "ibkr-to-etax-frontend",
                "browser"));
        }
    }
}
