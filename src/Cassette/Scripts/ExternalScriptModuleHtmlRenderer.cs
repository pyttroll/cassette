﻿using System;
using System.Linq;
using System.Web;

namespace Cassette.Scripts
{
    public class ExternalScriptModuleHtmlRenderer : IModuleHtmlRenderer<ExternalScriptModule>
    {
        readonly IModuleHtmlRenderer<ScriptModule> fallbackScriptRenderer;

        public ExternalScriptModuleHtmlRenderer(IModuleHtmlRenderer<ScriptModule> fallbackScriptRenderer)
        {
            this.fallbackScriptRenderer = fallbackScriptRenderer;
        }

        public IHtmlString Render(ExternalScriptModule module)
        {
            var externalScriptHtml = string.Format(HtmlConstants.ScriptHtml, module.Url);

            if (string.IsNullOrEmpty(module.FallbackCondition))
            {
                return new HtmlString(externalScriptHtml);
            }
            else
            {
                return new HtmlString(
                    string.Format(
                        "{1}{0}<script type=\"text/javascript\">{0}if({2}){{{0}{3}{0}}}{0}</script>",
                        Environment.NewLine,
                        externalScriptHtml,
                        module.FallbackCondition,
                        CreateFallbackScripts(module)
                    )
                );
            }
        }

        string CreateFallbackScripts(ExternalScriptModule module)
        {
            var scripts = fallbackScriptRenderer.Render(module).ToHtmlString();
            return ConvertToDocumentWriteCalls(scripts);
        }

        string ConvertToDocumentWriteCalls(string scriptElements)
        {
            var scripts = scriptElements.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            
            return string.Join(
                Environment.NewLine,
                from script in scripts
                select "document.write(unescape('" + Escape(script) + "'));"
            );
        }

        string Escape(string script)
        {
            return script.Replace("<", "%3C").Replace(">", "%3E");
        }
    }
}