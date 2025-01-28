// MarkdownRenderer.cs
using Markdig;

namespace WindowsFormsApp1
{
    public class MarkdownRenderer
    {
        private readonly MarkdownPipeline pipeline;

        public MarkdownRenderer()
        {
            pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();
        }

        /// <summary>
        /// Converts Markdown text to HTML.
        /// </summary>
        public string ConvertToHtml(string markdown)
        {
            return Markdown.ToHtml(markdown, pipeline);
        }
    }
}
