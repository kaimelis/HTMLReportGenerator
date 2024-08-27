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

            html.AppendLine("<div class=\"row\">");
            html.AppendLine("<div class=\"col-md-12\">");
            html.AppendLine("<input type=\"text\" id=\"searchBar\" class=\"form-control\" placeholder=\"Search by fixture or test name...\" onkeyup=\"searchInPage()\">");
            html.AppendLine("</div>");
            html.AppendLine("</div>");

            html.AppendLine(GenerateSummaryPanel(testResults));
            html.AppendLine(GenerateErrorGroupCollapsibleList(testResults));
            html.Append(GenerateFixtures(testResults.Fixtures));
            html.AppendLine("</div>");
            return html.ToString();
        }


        private string GenerateErrorGroupCollapsibleList(TestResults testResults)
        {
            StringBuilder html = new StringBuilder();
            var errorGroups = testResults.ErrorList.GroupBy(e => e.Type)
                                         .Select(g => new { Type = g.Key, Count = g.Count(), Tests = g.Select(e => e.Test).Distinct() });

            html.AppendLine("<div class=\"panel panel-danger\" id=\"errorContainer\">");
            html.AppendLine("<div class=\"panel-heading\" style=\"cursor: pointer;\" data-toggle=\"collapse\" data-target=\"#errorsList\">");
            html.AppendLine("<h4 class=\"panel-title\">");
            html.AppendLine("Grouped Errors List");
            html.AppendLine("<span class=\"pull-right\"><i class=\"glyphicon glyphicon-chevron-down\"></i></span>");
            html.AppendLine("</h4>");
            html.AppendLine("</div>");
            html.AppendLine("<div class=\"panel-collapse collapse\" id=\"errorsList\">"); // Removed 'in' class to make it closed by default
            html.AppendLine("<div class=\"panel-body\">");

            foreach (var group in errorGroups)
            {
                html.AppendLine("<div class=\"error-group\">");
                html.AppendLine($"<h4 class=\"error-type\">{HttpUtility.HtmlEncode(group.Type)} failed. (Count: {group.Count})</h4>");
               // html.AppendLine($"<p class=\"error-count\">Count: {group.Count}</p>");
                html.AppendLine("<p class=\"affected-tests\"><strong>Affected Tests:</strong></p>");
                html.AppendLine("<ul>");
                foreach (var test in group.Tests)
                {
                    html.AppendLine($"<li>{HttpUtility.HtmlEncode(test)}</li>");
                }
                html.AppendLine("</ul>");
                html.AppendLine("</div>");
                html.AppendLine("<hr>");
            }

            html.AppendLine("</div>"); // panel-body
            html.AppendLine("</div>"); // panel-collapse
            html.AppendLine("</div>"); // panel

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
                string sanitizedName = SanitizeIdName(fixture.Name);
                string modalId = $"modal-{sanitizedName}-{index}";

                html.AppendLine($"<div class=\"col-md-3 fixture-panel {(fixture.Result.ToLower() == "success" ? "success-fixture" : "failed-fixture")}\">");
                html.AppendLine($"<div class=\"panel {GetPanelClass(fixture.Result)}\" style=\"cursor: pointer;\" data-toggle=\"modal\" data-target=\"#{modalId}\">");

                html.AppendLine("<div class=\"panel-heading\">");
                html.AppendLine("<div class=\"row\">");
                html.AppendLine("<div class=\"col-xs-9\">");
                html.AppendLine($"<strong class=\"fixture-name\">{fixture.Name}</strong>");
                html.AppendLine($"<br><small>{fixture.FailedTests}/{fixture.TotalTests} failed</small>");
                html.AppendLine("</div>");
                html.AppendLine("<div class=\"col-xs-3 text-right\">");
                html.AppendLine($"<span class=\"fixture-time\">{fixture.Time}</span>");
                html.AppendLine("</div>");
                html.AppendLine("</div>");

                // Move content from panel body to panel heading
                html.AppendLine("<div class=\"text-center\" style=\"font-size: 1.5em; margin-top: 2px;\">");
                html.AppendLine("</div>");

                if (!string.IsNullOrEmpty(fixture.Reason))
                {
                    html.AppendLine($"<div class=\"reason-text\" data-toggle=\"tooltip\" title=\"{HttpUtility.HtmlEncode(fixture.Reason)}\">");
                    html.AppendLine($"<small><strong>Reason:</strong> {HttpUtility.HtmlEncode(fixture.Reason)}</small>");
                    html.AppendLine("</div>");
                }

                html.AppendLine("</div>"); // End of Panel Heading

                // Empty panel body
                html.AppendLine("<div class=\"panel-body\"></div>");

                html.AppendLine("</div>"); // End of Panel

                html.AppendLine(GeneratePrintableView(fixture));
                html.AppendLine(GenerateFixtureModal(fixture, modalId));

                html.AppendLine("</div>");
                index++;
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
                html.AppendLine($"<div class=\"alert alert-warning\"><strong>Warning:</strong> <span class=\"searchable-content\">{HttpUtility.HtmlEncode(fixture.Reason)}</span></div>");
            }

            foreach (var testCase in fixture.TestCases)
            {
                string testCaseId = SanitizeIdName(testCase.Name);
                string name = testCase.Name.Substring(testCase.Name.LastIndexOf('.') + 1);

                html.AppendLine($"<div class=\"panel {GetPanelClass(testCase.Result)} test-case-panel\" data-test-result=\"{testCase.Result.ToLower()}\">");
                html.AppendLine("<div class=\"panel-heading\">");
                html.AppendLine("<h4 class=\"panel-title\">");
                html.AppendLine($"<a data-toggle=\"collapse\" data-parent=\"#{modalId}-accordion\" href=\"#{modalId}-accordion-{testCaseId}\"><span class=\"searchable-content\">{HttpUtility.HtmlEncode(name)}</span></a>");
                html.AppendLine("</h4>");
                html.AppendLine("</div>");
                html.AppendLine($"<div id=\"{modalId}-accordion-{testCaseId}\" class=\"panel-collapse collapse\">");
                html.AppendLine("<div class=\"panel-body\">");

                html.AppendLine(GenerateHtmlTables(testCase.Data, $"{modalId}-{testCaseId}"));

                if (!string.IsNullOrEmpty(testCase.FailureMessage))
                {
                    html.AppendLine("<div class=\"panel panel-danger\">");
                    html.AppendLine("<div class=\"panel-heading\">");
                    html.AppendLine("<h4 class=\"panel-title\">");
                    html.AppendLine($"<a data-toggle=\"collapse\" href=\"#{modalId}-stacktrace-{testCaseId}\">Error Details</a>");
                    html.AppendLine("</h4>");
                    html.AppendLine("</div>");
                    html.AppendLine($"<div id=\"{modalId}-stacktrace-{testCaseId}\" class=\"panel-collapse collapse\">");
                    html.AppendLine("<div class=\"panel-body\">");
                    html.AppendLine($"<pre class=\"searchable-content\">{HttpUtility.HtmlEncode(testCase.FailureMessage)}</pre>");
                    html.AppendLine("</div>");
                    html.AppendLine("</div>");
                    html.AppendLine("</div>");
                }

                html.AppendLine("</div>");
                html.AppendLine("</div>");
                html.AppendLine("</div>");
            }

            html.AppendLine("</div>");
            html.AppendLine("</div>");
            html.AppendLine("<div class=\"modal-footer\">");
            html.AppendLine("<button type=\"button\" class=\"btn btn-primary\" data-dismiss=\"modal\">Close</button>");
            html.AppendLine("</div>");
            html.AppendLine("</div>");
            html.AppendLine("<div class=\"ui-resizable-handle ui-resizable-se\"></div>");
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
            html.AppendLine("<div class=\"table-responsive\">");
            html.AppendLine("<table class='table table-bordered equal-width-columns'>");
            html.AppendLine("<thead><tr><th class='col-value'>Value</th><th class='col-before'>Before</th><th class='col-after'>After</th></tr></thead>");
            html.AppendLine("<tbody>");

            var allKeys = data["before"].Keys.Union(data["after"].Keys).Distinct();
            foreach (var key in allKeys)
            {
                string rowClass = data["before"].TryGetValue(key, out var beforeValue) && beforeValue.EndsWith("[FAILED]") ? "class=\"danger\"" : "";
                html.AppendLine($"<tr {rowClass}>");
                html.AppendFormat("<td class=\"searchable-content\">{0}</td>", HttpUtility.HtmlEncode(key));
                html.AppendFormat("<td class=\"searchable-content\">{0}</td>", data["before"].TryGetValue(key, out beforeValue) ? HttpUtility.HtmlEncode(beforeValue.Replace("[FAILED]", "")) : "");
                html.AppendFormat("<td class=\"searchable-content\">{0}</td>", data["after"].TryGetValue(key, out var afterValue) ? HttpUtility.HtmlEncode(afterValue.Replace("[FAILED]", "")) : "");
                html.AppendLine("</tr>");
            }

            html.AppendLine("</tbody></table>");
            html.AppendLine("</div>");
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

        private string GetHTML5Header(string title)
        {
            StringBuilder header = new StringBuilder();
            header.AppendLine("<!doctype html>");
            header.AppendLine("<html lang=\"en\">");
            header.AppendLine("  <head>");
            header.AppendLine("    <meta charset=\"utf-8\">");
            header.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1, maximum-scale=1\" />"); // Set for mobile
            header.AppendLine(string.Format("    <title>{0}</title>", title));


            header.AppendLine("<script src=\"https://code.jquery.com/jquery-3.6.0.min.js\"></script>");
            header.AppendLine("<script src=\"https://code.jquery.com/ui/1.12.1/jquery-ui.min.js\"></script>");
            header.AppendLine("<script src=\"https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/js/bootstrap.min.js\"></script>");

            string projectDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string jsFilePath = Path.Combine(projectDirectory, "ReportScripts.js");
            string jsContent = File.ReadAllText(jsFilePath);
            header.AppendLine($"<script>{jsContent}</script>");

            // Add custom styles
            header.AppendLine("    <style>");
            header.AppendLine("    .panel-title { font-size: 16px; }");
            header.AppendLine("    .panel-title a { display: block; padding: 10px 15px; }");
            header.AppendLine("    .panel-title a:hover, .panel-title a:focus { text-decoration: none; }");
            header.AppendLine("    .panel-heading { padding: 0; }");


            // Specify the relative path to the Bootstrap.css file
            string bootstrapFilePath = Path.Combine(projectDirectory, "BootstrapCSS.css");

            // Read the content of the Bootstrap.css file
            string bootstrapCssContent = File.ReadAllText(bootstrapFilePath);
            // Include Bootstrap CSS in the page
            header.AppendLine(bootstrapCssContent);
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

        private string GetHTML5Footer()
        {
            StringBuilder footer = new StringBuilder();
            footer.AppendLine("  </body>");
            footer.AppendLine("</html>");

            return footer.ToString();
        }

        private string SanitizeIdName(string name)
        {
            // Remove any characters that are not alphanumeric, underscore, or hyphen
            string sanitized = System.Text.RegularExpressions.Regex.Replace(name, @"[^\w-]", "_");
            // Ensure the ID doesn't start with a number
            if (char.IsDigit(sanitized[0]))
            {
                sanitized = "_" + sanitized;
            }
            return sanitized;
        }
    }
}
