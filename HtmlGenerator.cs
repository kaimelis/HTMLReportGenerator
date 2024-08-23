using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace HTMLReportGenerator
{
    public class HtmlGenerator
    {
        public string GenerateHtml(TestResults testResults)
        {
            StringBuilder html = new StringBuilder();
            html.Append(GetHTML5Header("Results"));
            html.Append(GenerateContent(testResults));
            html.Append(GetHTML5Footer());
            return html.ToString();
        }

        private string GenerateContent(TestResults testResults)
        {
            StringBuilder html = new StringBuilder();
            html.AppendLine("<div class=\"container-fluid page\">");
            html.AppendLine(GenerateSummaryPanel(testResults));
            html.Append(GenerateFixtures(testResults.Fixtures));
            html.AppendLine("</div>");
            return html.ToString();
        }

        private string GenerateSummaryPanel(TestResults testResults)
        {
            StringBuilder html = new StringBuilder();
            decimal percentage = testResults.TotalTests > 0
                ? decimal.Round(decimal.Divide(testResults.Errors, testResults.TotalTests) * 100, 1)
                : 0;

            html.AppendLine("<div class=\"row\">");
            html.AppendLine("<div class=\"col-md-12\">");
            html.AppendLine("<div class=\"panel panel-default\">");
            html.AppendLine($"<div class=\"panel-heading\">Summary - <small>{testResults.Name}</small></div>");
            html.AppendLine("<div class=\"panel-body\">");

            html.AppendLine($"<div class=\"col-md-2 col-sm-4 col-xs-6 text-center\"><div class=\"stat\">Tests</div><div class=\"val ignore-val\">{testResults.TotalTests}</div></div>");
            html.AppendLine($"<div class=\"col-md-2 col-sm-4 col-xs-6 text-center\"><div class=\"stat\">Errors</div><div class=\"val {(testResults.Errors > 0 ? "text-danger" : "")}\">{testResults.Errors}</div></div>");
            html.AppendLine($"<div class=\"col-md-2 col-sm-4 col-xs-6 text-center\"><div class=\"stat\">Date</div><div class=\"val\">{testResults.Date:d MMM}</div></div>");
            html.AppendLine($"<div class=\"col-md-2 col-sm-4 col-xs-6 text-center\"><div class=\"stat\">Version</div><div class=\"val\">{testResults.Version}</div></div>");
            html.AppendLine($"<div class=\"col-md-2 col-sm-4 col-xs-6 text-center\"><div class=\"stat\">Success Rate</div><div class=\"val\">{100 - percentage}%</div></div>");

            html.AppendLine("<div class=\"col-md-2 col-sm-4 col-xs-6 text-center\">");
            html.AppendLine("<div class=\"stat\">Filter</div>");
            html.AppendLine("<div class=\"btn-group\" data-toggle=\"buttons\">");
            html.AppendLine("<label class=\"btn btn-primary active\"><input type=\"radio\" name=\"options\" id=\"option1\" autocomplete=\"off\" checked> All</label>");
            html.AppendLine("<label class=\"btn btn-primary\"><input type=\"radio\" name=\"options\" id=\"option2\" autocomplete=\"off\"> Failed</label>");
            html.AppendLine("<label class=\"btn btn-primary\"><input type=\"radio\" name=\"options\" id=\"option3\" autocomplete=\"off\"> Success</label>");
            html.AppendLine("</div>");
            html.AppendLine("</div>");

            html.AppendLine("</div>");
            html.AppendLine("</div>");
            html.AppendLine("</div>");
            html.AppendLine("</div>");

            return html.ToString();
        }

        private string GenerateFixtures(List<TestFixture> fixtures)
        {
            StringBuilder html = new StringBuilder();
            int index = 0;

            foreach (var fixture in fixtures)
            {
                html.AppendLine($"<div class=\"col-md-3 fixture-panel {(fixture.Result.ToLower() == "success" ? "success-fixture" : "failed-fixture")}\">");
                html.AppendLine("<div class=\"panel " + GetPanelClass(fixture.Result) + "\">");
                html.AppendLine("<div class=\"panel-heading\">");
                html.AppendLine($"{fixture.Name} - <small>{fixture.Namespace}</small><small class=\"pull-right\">{fixture.Time}</small>");
                html.AppendLine($"<br><small>{fixture.FailedTests}/{fixture.TotalTests} failed</small>");

                if (!string.IsNullOrEmpty(fixture.Reason))
                {
                    html.AppendLine($"<span class=\"glyphicon glyphicon-info-sign pull-right info hidden-print\" data-toggle=\"tooltip\" title=\"{HttpUtility.HtmlEncode(fixture.Reason)}\"></span>");
                }

                html.AppendLine("</div>");
                html.AppendLine("<div class=\"panel-body\">");

                string modalId = $"modal-{HttpUtility.UrlEncode(fixture.Name)}-{index++}";
                html.AppendLine("<div class=\"text-center\" style=\"font-size: 1.5em;\">");
                html.AppendLine(GenerateResultLink(fixture.Result, modalId));
                html.AppendLine("</div>");

                html.AppendLine(GeneratePrintableView(fixture));
                html.AppendLine(GenerateFixtureModal(fixture, modalId));

                html.AppendLine("</div>");
                html.AppendLine("</div>");
                html.AppendLine("</div>");
            }

            return html.ToString();
        }

        private string GeneratePrintableView(TestFixture fixture)
        {
            StringBuilder html = new StringBuilder();

            html.AppendLine("<div class=\"visible-print printed-test-result\">");

            if (!string.IsNullOrEmpty(fixture.Reason))
            {
                html.AppendLine($"<div class=\"alert alert-warning\"><strong>Warning:</strong> {HttpUtility.HtmlEncode(fixture.Reason)}</div>");
            }

            foreach (var testCase in fixture.TestCases)
            {
                string name = testCase.Name.Substring(testCase.Name.LastIndexOf('.') + 1);

                html.AppendLine($"<div class=\"panel {GetPanelClass(testCase.Result)}\">");
                html.AppendLine("<div class=\"panel-heading\">");
                html.AppendLine("<h4 class=\"panel-title\">");
                html.AppendLine(HttpUtility.HtmlEncode(name));
                html.AppendLine("</h4>");
                html.AppendLine("</div>");
                html.AppendLine("<div class=\"panel-body\">");

                if (!string.IsNullOrEmpty(testCase.FailureMessage))
                {
                    html.AppendLine($"<div><strong>Error:</strong> <pre>{HttpUtility.HtmlEncode(testCase.FailureMessage)}</pre></div>");
                }

                html.AppendLine("</div>");
                html.AppendLine("</div>");
            }

            html.AppendLine("</div>");

            return html.ToString();
        }

        private string GenerateFixtureModal(TestFixture fixture, string modalId)
        {
            StringBuilder html = new StringBuilder();

            html.AppendLine($"<div class=\"modal fade\" id=\"{modalId}\" tabindex=\"-1\" role=\"dialog\" aria-labelledby=\"myModalLabel\" aria-hidden=\"true\">");
            html.AppendLine("<div class=\"modal-dialog\">");
            html.AppendLine("<div class=\"modal-content\">");
            html.AppendLine("<div class=\"modal-header\">");
            html.AppendLine("<button type=\"button\" class=\"close\" data-dismiss=\"modal\" aria-hidden=\"true\">&times;</button>");
            html.AppendLine($"<h4 class=\"modal-title\" id=\"myModalLabel\">{HttpUtility.HtmlEncode(fixture.Name)}</h4>");
            html.AppendLine("</div>");
            html.AppendLine("<div class=\"modal-body\">");

            html.AppendLine($"<div class=\"panel-group no-bottom-margin\" id=\"{modalId}-accordion\">");

            if (!string.IsNullOrEmpty(fixture.Reason))
            {
                html.AppendLine($"<div class=\"alert alert-warning\"><strong>Warning:</strong> {HttpUtility.HtmlEncode(fixture.Reason)}</div>");
            }

            int i = 0;
            foreach (var testCase in fixture.TestCases)
            {
                string name = testCase.Name.Substring(testCase.Name.LastIndexOf('.') + 1);

                html.AppendLine($"<div class=\"panel {GetPanelClass(testCase.Result)}\">");
                html.AppendLine("<div class=\"panel-heading\">");
                html.AppendLine("<h4 class=\"panel-title\">");
                html.AppendLine($"<a data-toggle=\"collapse\" data-parent=\"#{modalId}-accordion\" href=\"#{modalId}-accordion-{i}\">{HttpUtility.HtmlEncode(name)}</a>");
                html.AppendLine("</h4>");
                html.AppendLine("</div>");
                html.AppendLine($"<div id=\"{modalId}-accordion-{i}\" class=\"panel-collapse collapse\">");
                html.AppendLine("<div class=\"panel-body\">");

                html.AppendLine(GenerateHtmlTables(testCase.Data, $"{modalId}-{i}"));

                if (!string.IsNullOrEmpty(testCase.FailureMessage))
                {
                    html.AppendLine("<div class=\"panel panel-danger\">");
                    html.AppendLine("<div class=\"panel-heading\">");
                    html.AppendLine("<h4 class=\"panel-title\">");
                    html.AppendLine($"<a data-toggle=\"collapse\" href=\"#{modalId}-stacktrace-{i}\">Error Details</a>");
                    html.AppendLine("</h4>");
                    html.AppendLine("</div>");
                    html.AppendLine($"<div id=\"{modalId}-stacktrace-{i}\" class=\"panel-collapse collapse\">");
                    html.AppendLine("<div class=\"panel-body\">");
                    html.AppendLine($"<pre>{HttpUtility.HtmlEncode(testCase.FailureMessage)}</pre>");
                    html.AppendLine("</div>");
                    html.AppendLine("</div>");
                    html.AppendLine("</div>");
                }

                html.AppendLine("</div>");
                html.AppendLine("</div>");
                html.AppendLine("</div>");
                i++;
            }

            html.AppendLine("</div>");
            html.AppendLine("</div>");
            html.AppendLine("<div class=\"modal-footer\">");
            html.AppendLine("<button type=\"button\" class=\"btn btn-primary\" data-dismiss=\"modal\">Close</button>");
            html.AppendLine("</div>");
            html.AppendLine("</div>");
            html.AppendLine("</div>");
            html.AppendLine("</div>");

            return html.ToString();
        }

        private string GenerateHtmlTables(Dictionary<string, Dictionary<string, Dictionary<string, string>>> data, string modalId)
        {
            if (data == null || data.Count == 0) return string.Empty;

            var html = new StringBuilder();
            int sectionIndex = 0;

            foreach (var section in data)
            {
                string sectionId = $"{modalId}-section-{sectionIndex}";
                html.AppendLine("<div class=\"panel panel-default\">");
                html.AppendLine("<div class=\"panel-heading\">");
                html.AppendLine("<h4 class=\"panel-title\">");
                html.AppendFormat("<a data-toggle=\"collapse\" href=\"#{0}\">{1}</a>", sectionId, HttpUtility.HtmlEncode(section.Key));
                html.AppendLine("</h4>");
                html.AppendLine("</div>");
                html.AppendFormat("<div id=\"{0}\" class=\"panel-collapse collapse\">", sectionId);
                html.AppendLine("<div class=\"panel-body\">");
                html.AppendLine(GenerateHtmlTable(section.Value));
                html.AppendLine("</div>");
                html.AppendLine("</div>");
                html.AppendLine("</div>");
                sectionIndex++;
            }

            return html.ToString();
        }

        private string GenerateHtmlTable(Dictionary<string, Dictionary<string, string>> data)
        {
            if (data == null || data.Count == 0) return string.Empty;

            var html = new StringBuilder();
            html.AppendLine("<table class='table table-bordered'>");
            html.AppendLine("<thead><tr><th>Value</th><th>Before</th><th>After</th></tr></thead>");
            html.AppendLine("<tbody>");

            var allKeys = data["before"].Keys.Union(data["after"].Keys).Distinct();
            foreach (var key in allKeys)
            {
                html.AppendLine("<tr>");
                html.AppendFormat("<td>{0}</td>", HttpUtility.HtmlEncode(key));
                html.AppendFormat("<td>{0}</td>", data["before"].TryGetValue(key, out var beforeValue) ? HttpUtility.HtmlEncode(beforeValue) : "");
                html.AppendFormat("<td>{0}</td>", data["after"].TryGetValue(key, out var afterValue) ? HttpUtility.HtmlEncode(afterValue) : "");
                html.AppendLine("</tr>");
            }

            html.AppendLine("</tbody></table>");
            return html.ToString();
        }

        private string GenerateResultLink(string result, string modalId)
        {
            StringBuilder html = new StringBuilder();
            switch (result.ToLower())
            {
                case "success":
                    html.AppendLine($"<a href=\"#{modalId}\" role=\"button\" data-toggle=\"modal\" class=\"text-success no-underline\">");
                    html.AppendLine("<span class=\"glyphicon glyphicon-ok-sign\"></span>");
                    html.AppendLine("<span class=\"test-result\">Success</span>");
                    html.AppendLine("</a>");
                    break;
                case "failure":
                case "error":
                    html.AppendLine($"<a href=\"#{modalId}\" role=\"button\" data-toggle=\"modal\" class=\"text-danger no-underline\">");
                    html.AppendLine("<span class=\"glyphicon glyphicon-exclamation-sign\"></span>");
                    html.AppendLine("<span class=\"test-result\">Failed</span>");
                    html.AppendLine("</a>");
                    break;
                default:
                    break;
            }
            return html.ToString();
        }

        private string GetPanelClass(string result)
        {
            switch (result.ToLower())
            {
                case "success":
                    return "panel-success";
                case "failure":
                case "error":
                    return "panel-danger";
                default:
                    return "panel-default";
            }
        }

        private static string GetHTML5Header(string title)
        {
            StringBuilder header = new StringBuilder();
            header.AppendLine("<!doctype html>");
            header.AppendLine("<html lang=\"en\">");
            header.AppendLine("  <head>");
            header.AppendLine("    <meta charset=\"utf-8\">");
            header.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1, maximum-scale=1\" />"); // Set for mobile
            header.AppendLine(string.Format("    <title>{0}</title>", title));

            // Add custom scripts
            header.AppendLine("    <script>");

            // Include jQuery in the page
            header.AppendLine(Properties.Resources.jQuery);
            header.AppendLine("    </script>");
            header.AppendLine("    <script>");

            // Include Bootstrap in the page
            header.AppendLine(Properties.Resources.BootstrapJS);
            header.AppendLine("    </script>");
            header.AppendLine("    <script type=\"text/javascript\">");
            header.AppendLine("    $(document).ready(function() { ");
            header.AppendLine("        $('[data-toggle=\"tooltip\"]').tooltip({'placement': 'bottom'});");

            // Add filtering functionality
            header.AppendLine("        $('.btn-group input[type=\"radio\"]').change(function() {");
            header.AppendLine("            var selectedFilter = $(this).parent().text().trim().toLowerCase();");
            header.AppendLine("            if (selectedFilter === 'all') {");
            header.AppendLine("                $('.fixture-panel').show();");
            header.AppendLine("            } else if (selectedFilter === 'failed') {");
            header.AppendLine("                $('.fixture-panel').hide();");
            header.AppendLine("                $('.failed-fixture').show();");
            header.AppendLine("            } else if (selectedFilter === 'success') {");
            header.AppendLine("                $('.fixture-panel').hide();");
            header.AppendLine("                $('.success-fixture').show();");
            header.AppendLine("            }");
            header.AppendLine("        });");

            header.AppendLine("    });");
            header.AppendLine("    </script>");

            // Add custom styles
            header.AppendLine("    <style>");
            header.AppendLine("    .panel-title { font-size: 16px; }");
            header.AppendLine("    .panel-title a { display: block; padding: 10px 15px; }");
            header.AppendLine("    .panel-title a:hover, .panel-title a:focus { text-decoration: none; }");
            header.AppendLine("    .panel-heading { padding: 0; }");

            // Include Bootstrap CSS in the page
            header.AppendLine(Properties.Resources.BootstrapCSS);
            header.AppendLine("    .page { margin: 15px 0; }");
            header.AppendLine("    .no-bottom-margin { margin-bottom: 0; }");
            header.AppendLine("    .printed-test-result { margin-top: 15px; }");
            header.AppendLine("    .reason-text { margin-top: 15px; }");
            header.AppendLine("    .scroller { overflow: scroll; }");
            header.AppendLine("    @media print { .panel-collapse { display: block !important; } }");
            header.AppendLine("    .val { font-size: 38px; font-weight: bold; margin-top: -10px; }");
            header.AppendLine("    .stat { font-weight: 800; text-transform: uppercase; font-size: 0.85em; color: #6F6F6F; }");
            header.AppendLine("    .test-result { display: block; }");
            header.AppendLine("    .no-underline:hover { text-decoration: none; }");
            header.AppendLine("    .text-default { color: #555; }");
            header.AppendLine("    .text-default:hover { color: #000; }");
            header.AppendLine("    .info { color: #888; }");
            header.AppendLine("    </style>");
            header.AppendLine("  </head>");
            header.AppendLine("  <body>");

            return header.ToString();
        }

        private static string GetHTML5Footer()
        {
            StringBuilder footer = new StringBuilder();
            footer.AppendLine("  </body>");
            footer.AppendLine("</html>");

            return footer.ToString();
        }
    }
}
